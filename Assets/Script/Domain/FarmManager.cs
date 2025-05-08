using System;
using System.Collections.Generic;
using System.Linq;

public class FarmManager
{
    public int Gold { get => _gold; 
        private set
        {
            _gold = value;
            if(_gold >= 1000000)
            {
                isGameWon = true;
            }
        }  
    } 

    public int EquipmentLevel { get; private set; } = 1;
    public List<Plot> Plots { get; private set; } = new List<Plot>();
    public List<Worker> Workers { get; private set; } = new List<Worker>();
    public Dictionary<string, int> InventoryOnHand { get; private set; } = new Dictionary<string, int>();
    public Dictionary<string, int> Products { get; private set; } = new Dictionary<string, int>();
    public bool isGameWon { get; private set; } = false;
    private readonly GameConfig _config;
    private DateTime _lastUpdate;
    public GameConfig Config => _config;

    private int _gold = 0;
    
    public FarmManager(GameConfig config)
    {
        _config = config;
        Initialize();
        _lastUpdate = DateTime.Now;
    }

    private void Initialize()
    {
        for (int i = 0; i < 3; i++)
            Plots.Add(new Plot());
        InventoryOnHand["Tomato"] = 10;
        InventoryOnHand["Blueberry"] = 10;
        InventoryOnHand["Strawberry"] = 0;
        InventoryOnHand["Cow"] = 2;
        Products["Tomato"] = 0;
        Products["Blueberry"] = 0;
        Products["Strawberry"] = 0;
        Products["Cow"] = 0;
        Workers.Add(new Worker(this));
        Gold = _config.StartGold;
    }
    public void LoadSaveData(SaveData saveData)
    {
        Gold = saveData.Gold;
        EquipmentLevel = saveData.EquipmentLevel;
        _lastUpdate = saveData.LastUpdate;
    }


    public void Update(double deltaTime)
    {
        double deltaTimeScaled = deltaTime;
        foreach (var worker in Workers)
        {
            worker.Update(deltaTimeScaled);
        }
        foreach (var plot in Plots)
        {
            plot.Update(deltaTimeScaled, EquipmentLevel);
        }
        _lastUpdate = DateTime.Now;
    }




    public bool PlantCrop(int plotIndex, string cropName, DateTime now)
    {
        if (plotIndex < 0 || plotIndex >= Plots.Count || !Plots[plotIndex].IsEmpty || !InventoryOnHand.ContainsKey(cropName) || InventoryOnHand[cropName] <= 0)
            return false;
        InventoryOnHand[cropName]--;
        Plots[plotIndex].Plant(_config.Crops[cropName], now);
        return true;
    }

    public bool Harvest(int plotIndex)
    {
        if (plotIndex < 0 || plotIndex >= Plots.Count || Plots[plotIndex].IsEmpty)
            return false;
        var plot = Plots[plotIndex];
        if (plot.CanHarvest)
        {
            var product = plot.Harvest();
            Products[product.Name] = Products.GetValueOrDefault(product.Name, 0) + product.Amount;
            return true;
        }
        return false;
    }

    public void SellProduct(string productName)
    {
        if (Products.ContainsKey(productName) && Products[productName] > 0)
        {
            Gold += Products[productName] * _config.Crops[productName].SellPrice;
            Products[productName] = 0;
        }
    }

    public bool UpgradeEquipment()
    {
        if (Gold >= _config.UpgradeCost)
        {
            Gold -= _config.UpgradeCost;
            EquipmentLevel++;
            return true;
        }
        return false;
    }

    public bool HireWorker()
    {
        if (Gold >= _config.WorkerCost)
        {
            Gold -= _config.WorkerCost;
            Workers.Add(new Worker(this));
            return true;
        }
        return false;
    }

    public bool BuyPlot()
    {
        if (Gold >= _config.PlotCost)
        {
            Gold -= _config.PlotCost;
            Plots.Add(new Plot());
            return true;
        }
        return false;
    }

    public bool BuySeeds(string cropName, int amount)
    {
        var cost = _config.Crops[cropName].SeedCost * amount;
        if (Gold >= cost)
        {
            Gold -= cost;
            InventoryOnHand[cropName] = InventoryOnHand.GetValueOrDefault(cropName, 0) + amount;
            return true;
        }
        return false;
    }

    public int GetIdleWorkerCount()
    {
        return Workers.Count(w => w.IsIdle);
    }
    //test
    public void AddGold(int gold)
    {
        Gold += gold;
    }

}