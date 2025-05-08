using NUnit.Framework;
using System;
using System.IO;

[TestFixture]
public class GameConfigTests
{
    private string configCsvPath = "D:/Unity/TestWolffun/Assets/Resources/config.csv";
    private string globalConfigCsvPath = "D:/Unity/TestWolffun/Assets/Resources/global_config.csv";

    [SetUp]
    public void SetUp()
    {
        
    }

    [TearDown]
    public void TearDown()
    {
       // if (File.Exists(configCsvPath)) File.Delete(configCsvPath);
        //if (File.Exists(globalConfigCsvPath)) File.Delete(globalConfigCsvPath);
    }

    [Test]
    public void LoadFromCSV_ValidFile_LoadsCrops()
    {
        var config = new GameConfig();
        config.LoadFromCSV(configCsvPath);

        Assert.That(config.Crops, Has.Count.EqualTo(4));
        Assert.That(config.Crops["Tomato"].Name, Is.EqualTo("Tomato"));
        Assert.That(config.Crops["Tomato"].SeedCost, Is.EqualTo(30));
        Assert.That(config.Crops["Tomato"].GrowthTime, Is.EqualTo(600));
        Assert.That(config.Crops["Tomato"].Yield, Is.EqualTo(1));
        Assert.That(config.Crops["Tomato"].Lifespan, Is.EqualTo(40));
        Assert.That(config.Crops["Tomato"].SellPrice, Is.EqualTo(5));
        Assert.That(config.Crops["Tomato"].DecayTime, Is.EqualTo(3600));
        Assert.That(config.Crops["Tomato"].IsAnimal, Is.False);

        Assert.That(config.Crops["Cow"].Name, Is.EqualTo("Cow"));
        Assert.That(config.Crops["Cow"].DecayTime, Is.EqualTo(3600));
        Assert.That(config.Crops["Cow"].IsAnimal, Is.True);

        Assert.That(config.Crops["Tomato"].SellPrice, Is.EqualTo(5));
        Assert.That(config.Crops["Blueberry"].SellPrice, Is.EqualTo(8));
        Assert.That(config.Crops["Cow"].SellPrice, Is.EqualTo(15));

    }

    [Test]
    public void LoadGlobalConfigFromCSV_InvalidKey_SkipsKey()
    {
        var config = new GameConfig();
        config.LoadGlobalConfigFromCSV(globalConfigCsvPath);
        Assert.That(config.WorkerSpeed, Is.EqualTo(3f));
        Assert.That(config.WorkerTaskDuration, Is.EqualTo(120f));
    }
}