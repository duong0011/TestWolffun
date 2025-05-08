using NUnit.Framework;
using System;

[TestFixture]
public class FarmManagerTests
{
    private FarmManager _farmManager;
    private GameConfig _config;

    [SetUp]
    public void Setup()
    {
        _config = new GameConfig();
        _config.LoadGlobalConfigFromCSV("D:/Unity/TestWolffun/Assets/Resources/global_config.csv");
        _config.Crops["Tomato"] = new CropConfig { Name = "Tomato", SeedCost = 30, GrowthTime = 600, Yield = 1, Lifespan = 40, SellPrice = 5, DecayTime = 3600};
        _config.Crops["Cow"] = new CropConfig { Name = "Cow", SeedCost = 30, GrowthTime = 1800, Yield = 1, Lifespan = 100, SellPrice = 15, DecayTime = 3600 };
        _farmManager = new FarmManager(_config);
    }
    //Test start product
    [Test]
    public void StartGame_ProductGameInNewGame()
    {
        Assert.AreEqual(3, _farmManager.Plots.Count);
        Assert.AreEqual(10, _farmManager.InventoryOnHand["Tomato"]);
        Assert.AreEqual(10, _farmManager.InventoryOnHand["Blueberry"]);
        Assert.AreEqual(2, _farmManager.InventoryOnHand["Cow"]);
        Assert.AreEqual(1, _farmManager.Workers.Count);
        Assert.AreEqual(1, _farmManager.EquipmentLevel);
    }

    //Plant seeds test
    [Test]
    public void PlantCrop_ReducesSeedCount()
    {
        _farmManager.PlantCrop(0, "Tomato", DateTime.Now);
        Assert.AreEqual(9, _farmManager.InventoryOnHand["Tomato"]);
    }
    //Sell product test
    [Test]
    public void SellProduct_IncreasesGold()
    {
        _farmManager.PlantCrop(0, "Tomato", DateTime.Now);
        _farmManager.Update(600);
        _farmManager.Harvest(0);
        _farmManager.SellProduct("Tomato");
        Assert.AreEqual(5, _farmManager.Gold);
    }
    //Upgrade equipment 
    [Test]
    public void UpgradeEquipment_IncreasesLevel()
    {
        _farmManager.AddGold(1000);
        _farmManager.UpgradeEquipment();
        Assert.AreEqual(2, _farmManager.EquipmentLevel);
        _farmManager.PlantCrop(0, "Tomato", DateTime.Now);
        _farmManager.Update(550);
        _farmManager.Harvest(0);
        _farmManager.SellProduct("Tomato");
        Assert.AreEqual(505, _farmManager.Gold);
    }
    //Buy plot test
    [Test]
    public void BuyPlot_Increases5Plots()
    {
        _farmManager.AddGold(5000);
        for (int i = 0; i < 5; i++)
        {
            _farmManager.BuyPlot();
        }
        Assert.AreEqual(8, _farmManager.Plots.Count);
        _farmManager.PlantCrop(7, "Tomato", DateTime.Now);
        Assert.AreEqual(false, _farmManager.Plots[7].IsEmpty);
    }
    //Test product biodegradable
    [Test]
    public void PlantCrop_TestProductBiodegradable()
    {
        _farmManager.PlantCrop(1, "Tomato", DateTime.Now);
        Assert.AreEqual(false, _farmManager.Plots[1].IsEmpty);
        for (int i = 0; i < _config.Crops["Tomato"].Lifespan + 1; ++i)
        {
            _farmManager.Update(_config.Crops["Tomato"].GrowthTime);
        }
        _farmManager.Update(_config.Crops["Tomato"].DecayTime + 1f);
        Assert.AreEqual(true, _farmManager.Plots[1].IsEmpty);
 
        //Cow
        _farmManager.PlantCrop(2, "Cow", DateTime.Now);
        Assert.AreEqual(false, _farmManager.Plots[2].IsEmpty);
        for (int i = 0; i < _config.Crops["Cow"].Lifespan + 1; ++i)
        {
            _farmManager.Update(_config.Crops["Cow"].GrowthTime);
        }
        _farmManager.Update(_config.Crops["Cow"].DecayTime + 1f);
        Assert.AreEqual(true, _farmManager.Plots[2].IsEmpty);
    }
    //Win condition test
    [Test]
    public void Win_WinCondition()
    {
        _farmManager.AddGold(1000000);
        Assert.AreEqual(true, _farmManager.isGameWon);
    }
}