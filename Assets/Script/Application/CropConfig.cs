using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class CropConfig
{
    public string Name { get; set; }
    public int SeedCost { get; set; }
    public double GrowthTime { get; set; } // Seconds
    public int Yield { get; set; }
    public int Lifespan { get; set; }
    public int SellPrice { get; set; }
    public bool IsAnimal { get; set; }
    public double DecayTime { get; set; } // Seconds
}

public class GameConfig
{
    public Dictionary<string, CropConfig> Crops { get; private set; } = new Dictionary<string, CropConfig>();
    public float WorkerSpeed { get; private set; }
    public float WorkerTaskDuration { get; private set; }
    public int StartGold { get; private set; }
    public int WorkerCost { get; private set; }
    public int UpgradeCost { get; private set; }
    public int PlotCost { get; private set; }

    private readonly Dictionary<string, Action<float>> _globalConfigSetters;

    public GameConfig()
    {
        _globalConfigSetters = new Dictionary<string, Action<float>>
        {
            { "WorkerSpeed", value => WorkerSpeed = value },
            { "WorkerTaskDuration", value => WorkerTaskDuration = value },
            { "StartGold", value => StartGold = (int)value },
            { "WorkerCost", value => WorkerCost = (int)value },
            { "UpgradeCost", value => UpgradeCost = (int)value },
            { "PlotCost", value => PlotCost = (int)value }
        };
    }

    public void LoadFromCSV(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Logger.Error($"Config file not found: {filePath}");
            throw new FileNotFoundException($"Config file not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath).Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line)).ToArray();
        if (lines.Length <= 1)
        {
            Logger.Error("Config file is empty or missing data");
            throw new InvalidOperationException("Config file is empty or missing data");
        }

        try
        {
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length < 8)
                {
                    Logger.Warning($"Skipping invalid line in config.csv: {line}");
                    continue;
                }

                var config = new CropConfig
                {
                    Name = parts[1],
                    SeedCost = int.Parse(parts[2]),
                    GrowthTime = double.Parse(parts[3]),
                    Yield = int.Parse(parts[4]),
                    Lifespan = int.Parse(parts[5]),
                    SellPrice = int.Parse(parts[6]),
                    IsAnimal = parts[0].Equals("Animal", StringComparison.OrdinalIgnoreCase),
                    DecayTime = double.Parse(parts[7])
                };

                Crops[config.Name] = config;
                Logger.Info($"Loaded crop: {config.Name}, SellPrice: {config.SellPrice}");
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to load config.csv: {e.Message}");
            throw;
        }
    }

    public void LoadGlobalConfigFromCSV(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Logger.Error($"Global config file not found: {filePath}");
            throw new FileNotFoundException($"Global config file not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath).Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line)).ToArray();
        if (lines.Length <= 1)
        {
            Logger.Error("Global config file is empty or missing data");
            throw new InvalidOperationException("Global config file is empty or missing data");
        }

        try
        {
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length < 2)
                {
                    Logger.Warning($"Skipping invalid line in global_config.csv: {line}");
                    continue;
                }

                string key = parts[0];
                float value = float.Parse(parts[1]);

                if (_globalConfigSetters.TryGetValue(key, out var setter))
                {
                    setter(value);
                    Logger.Info($"Loaded global config: {key}={value}");
                }
                else
                {
                    Logger.Warning($"Unknown config key: {key}");
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to load global_config.csv: {e.Message}");
            throw;
        }
    }
}