using System;

public class Plot
{
    public bool IsEmpty { get; private set; } = true;
    public bool IsAsignedWorker { get; private set; } = false;
    public bool CanHarvest { get; private set; }
    public string CropName { get; private set; }
    public int AmountProduct { get; private set; } = 0;
    private CropConfig _config;
    private double _timeSincePlanted;
    private int _harvestsDone;
    private double _lastHarvestTime;
    private double _decayTime = 3600; // 1 hour
    

    public double TimeSincePlanted => _timeSincePlanted;
    public int HarvestsDone => _harvestsDone;
    public double LastHarvestTime => _lastHarvestTime;
    public void Plant(CropConfig config, DateTime now)
    {
        _config = config;
        CropName = config.Name;
        _decayTime = config.DecayTime;
        IsEmpty = false;
        CanHarvest = false;
        _timeSincePlanted = 0;
        _harvestsDone = 0;
        _lastHarvestTime = 0;
    }
    public void LoadSaveData(PlotSaveData plotSaveData)
    {
        _timeSincePlanted = plotSaveData.TimeSincePlanted;
        _harvestsDone = plotSaveData.HarvestsDone;
        _lastHarvestTime = plotSaveData.LastHarvestTime;
    }
    public void Update(double deltaTime, int equipmentLevel)
    {
        if (IsEmpty) return;

        _timeSincePlanted += deltaTime;

        var growthTime = _config.GrowthTime / (1 + 0.1 * (equipmentLevel - 1));

        if (_timeSincePlanted >= _lastHarvestTime + growthTime && _harvestsDone < _config.Lifespan)
        {
            CanHarvest = true;
            _lastHarvestTime += growthTime;
           
            _harvestsDone++;
            AmountProduct += _config.Yield;
        }
        if (_harvestsDone >= _config.Lifespan && _timeSincePlanted >= _lastHarvestTime + _decayTime)
        {
            Clear();
        }
    }

    public (string Name, int Amount) Harvest()
    {
        if (!CanHarvest) return (null, 0);
        CanHarvest = false;
        string name = _config.Name;
        int amount = AmountProduct;
        if (_harvestsDone >= _config.Lifespan)
            Clear();
        AmountProduct = 0;
        return (name, amount);
    }

    private void Clear()
    {
        IsEmpty = true;
        CropName = null;
        _config = null;
        CanHarvest = false;
        AmountProduct = 0;
    }
    public void AssignWorker(bool isAsign)
    {
        IsAsignedWorker = isAsign;
    }

}