using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml;
using Newtonsoft.Json;

public class SaveData
{
    public int Gold { get; set; }
    public int EquipmentLevel { get; set; }
    public List<PlotSaveData> Plots { get; set; }
    public List<WorkerSaveData> Workers { get; set; }
    public Dictionary<string, int> InventoryOnHand { get; set; }
    public Dictionary<string, int> Products { get; set; }
    public DateTime LastUpdate { get; set; }
}

public class PlotSaveData
{
    public string CropName { get; set; }
    public double TimeSincePlanted { get; set; }
    public int HarvestsDone { get; set; }
    public double LastHarvestTime { get; set; }
}

public class WorkerSaveData
{
    public double TaskTimeRemaining { get; set; }
    public bool IsIdle { get; set; }
    public Vector3 LastPostion { get; set; }
}

public class SaveSystem
{
    private readonly string _savePath;

    public SaveSystem(string savePath)
    {
        _savePath = savePath;
    }

    public void Save(FarmManager farm)
    {
        var saveData = new SaveData
        {
            Gold = farm.Gold,
            EquipmentLevel = farm.EquipmentLevel,
            Plots = farm.Plots.Select(p => new PlotSaveData
            {
                CropName = p.CropName,
                TimeSincePlanted = p.IsEmpty ? 0 : p.TimeSincePlanted,
                HarvestsDone = p.IsEmpty ? 0 : p.HarvestsDone,
                LastHarvestTime = p.IsEmpty ? 0 : p.LastHarvestTime
            }).ToList(),
            Workers = farm.Workers.Select(w => new WorkerSaveData
            {
                TaskTimeRemaining = w.TaskTimeRemaining,
                IsIdle = w.IsIdle,
                LastPostion = w.UpdatedLastPositon
            }).ToList(),
            InventoryOnHand = farm.InventoryOnHand,
            Products = farm.Products,
            LastUpdate = DateTime.Now
        };
        var json = JsonConvert.SerializeObject(saveData, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(_savePath, json);
    }

    public SaveData Load()
    {
        if (File.Exists(_savePath))
        {
            Console.WriteLine("Loading save data from " + _savePath);
            var json = File.ReadAllText(_savePath);
            return JsonConvert.DeserializeObject<SaveData>(json);
        }
        return null;
    }
}