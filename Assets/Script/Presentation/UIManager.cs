using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI goldText, equipmentText, workersText, seedsText, plotsText, productsText;
    [SerializeField] private GameObject plotPrefab;
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private GameObject farmBackground;
    [SerializeField] private Transform plotHolder; 
    [SerializeField] private Transform workerHolder; 
    [SerializeField]
    private Button plantTomatoButton, plantBlueberryButton, plantStrawberryButton, cowFarming, harvestButton, sellTomatoButton,
        sellBlueberryButton,sellStrawberryButton, sellMilkButton, upgradeButton, hireWorkerButton, buyPlotButton,
        buyTomatoButton, buyBlueberryButton, buyCowButton, buyStrawberryButton;
    [Header("Config")]
    [SerializeField] private int maxPlots = 500; // Số lượng ô đất tối đa
    [SerializeField] private int maxWorkers = 50; // Số lượng công nhân tối đa
    [SerializeField] private Vector2 plotSpacing = new Vector2(2f, 2f); // Khoảng cách giữa các ô đất
    [SerializeField] private int plotsPerRow = 10; // Số ô đất mỗi hàng

    private FarmManager _farmManager;
    private GameConfig _config;
    private SaveSystem _saveSystem;
    private CameraManager _cameraManager;
    private List<PlotWorld> _plotWorlds = new List<PlotWorld>(); // Ô đất đang hoạt động
    private List<PlotWorld> poolingPlots = new List<PlotWorld>(); // Pool ô đất
    private List<WorkerAnimation> _workerWorlds = new List<WorkerAnimation>(); // Công nhân đang hoạt động
    private List<WorkerAnimation> poolingWorkers = new List<WorkerAnimation>(); // Pool công nhân
    private int _selectedPlotIndex = -1;

    private void Awake()
    {
        _cameraManager = Camera.main.GetComponent<CameraManager>();
        if (_cameraManager == null)
        {
            Debug.LogError("CameraManager component is missing on MainCamera");
        }
    }

    private void Start()
    {
        _config = new GameConfig();
        _config.LoadFromCSV(Application.dataPath + "/Resources/config.csv");
        Debug.Log(Application.dataPath + "/Resources/config.csv");
        _config.LoadGlobalConfigFromCSV(Application.dataPath + "/Resources/global_config.csv");
        _saveSystem = new SaveSystem(Application.persistentDataPath + "/save.json");
        _farmManager = new FarmManager(_config);

        var saveData = _saveSystem.Load();

        if (saveData != null)
        {
            _farmManager = new FarmManager(_config);
            _farmManager.LoadSaveData(saveData);
            _farmManager.InventoryOnHand.Clear();
            foreach (var seed in saveData.InventoryOnHand)
                _farmManager.InventoryOnHand[seed.Key] = seed.Value;
            _farmManager.Products.Clear();
            foreach (var product in saveData.Products)
                _farmManager.Products[product.Key] = product.Value;
            _farmManager.Plots.Clear();
            foreach (var plotData in saveData.Plots)
            {
                var plot = new Plot();
                if (!string.IsNullOrEmpty(plotData.CropName))
                {
                    plot.Plant(_config.Crops[plotData.CropName], saveData.LastUpdate);
                    plot.LoadSaveData(plotData);
                }
                _farmManager.Plots.Add(plot);
            }
            _farmManager.Workers.Clear();
            foreach (var workerData in saveData.Workers)
            {
                var worker = new Worker(_farmManager);
                worker.LoadSaveData(workerData);
                _farmManager.Workers.Add(worker);
            }
            
        }

        // Thiết lập button listeners
        plantTomatoButton.onClick.AddListener(() => PlantCrop("Tomato"));
        plantBlueberryButton.onClick.AddListener(() => PlantCrop("Blueberry"));
        plantStrawberryButton.onClick.AddListener(() => PlantCrop("Strawberry"));
        cowFarming.onClick.AddListener(() => PlantCrop("Cow"));
        harvestButton.onClick.AddListener(Harvest);
        sellTomatoButton.onClick.AddListener(() => _farmManager.SellProduct("Tomato"));
        sellBlueberryButton.onClick.AddListener(() => _farmManager.SellProduct("Blueberry"));
        sellMilkButton.onClick.AddListener(() => _farmManager.SellProduct("Cow"));
        upgradeButton.onClick.AddListener(() => _farmManager.UpgradeEquipment());
        hireWorkerButton.onClick.AddListener(HireWorker);
        buyPlotButton.onClick.AddListener(() => BuyPlot());
        buyTomatoButton.onClick.AddListener(() => _farmManager.BuySeeds("Tomato", 10));
        buyBlueberryButton.onClick.AddListener(() => _farmManager.BuySeeds("Blueberry", 10));
        buyCowButton.onClick.AddListener(() => _farmManager.BuySeeds("Cow", 1));
        buyStrawberryButton.onClick.AddListener(() => _farmManager.BuySeeds("Strawberry", 10));

        // Khởi tạo pool ô đất và công nhân
        InitializePlotGrid();
        InitializeWorkerPool();
        UpdatePlotWorld();

        // Thiết lập giới hạn camera
        if (_cameraManager != null && farmBackground != null)
        {
            var spriteRenderer = farmBackground.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Vector2 backgroundSize = spriteRenderer.bounds.size;
                _cameraManager.SetBounds(backgroundSize);
            }
        }
    }

    private void Update()
    {
        _farmManager.Update(Time.deltaTime);
        UpdateUI();
        UpdateWorkerWorld();
        _saveSystem.Save(_farmManager);
    }

    private void InitializePlotGrid()
    {
        if (plotHolder == null)
        {
            Debug.LogError("plotHolder is not assigned in UIManager");
            return;
        }

        poolingPlots.Clear();
        for (int i = 0; i < maxPlots; i++)
        {
            var plotObj = Instantiate(plotPrefab, plotHolder);
            var plotWorld = plotObj.GetComponent<PlotWorld>();
            poolingPlots.Add(plotWorld);
            plotObj.SetActive(false);
        }
    }

    private void InitializeWorkerPool()
    {
        if (workerHolder == null)
        {
            Debug.LogError("workerHolder is not assigned in UIManager");
            return;
        }

        poolingWorkers.Clear();
        for (int i = 0; i < maxWorkers; i++)
        {
            var workerObj = Instantiate(workerPrefab, workerHolder);
            var workerWorld = workerObj.GetComponent<WorkerAnimation>();
            poolingWorkers.Add(workerWorld);
            workerObj.SetActive(false);
        }
    }

    private void UpdatePlotWorld()
    {
        
        while (_plotWorlds.Count < _farmManager.Plots.Count)
        {
            if (_plotWorlds.Count >= poolingPlots.Count)
            {
                Debug.LogWarning("Not enough pooled PlotWorld objects");
                break;
            }

            var plotIndex = _plotWorlds.Count;
            var plotWorld = poolingPlots[plotIndex];
            plotWorld.gameObject.SetActive(true);
            plotWorld.Initialize(_farmManager, plotIndex, OnPlotSelected);
            _plotWorlds.Add(plotWorld);

            int row = plotIndex / plotsPerRow;
            int col = plotIndex % plotsPerRow;
            Vector3 newPosition = new Vector3(col * plotSpacing.x, -row * plotSpacing.y, 0f);
            plotWorld.transform.position = newPosition;

            
        }

        for (int i = _farmManager.Plots.Count; i < _plotWorlds.Count; i++)
        {
            _plotWorlds[i].gameObject.SetActive(false);
        }
    }

    private void UpdateWorkerWorld()
    {
        // Đảm bảo số lượng công nhân hoạt động khớp với Workers
        while (_workerWorlds.Count < _farmManager.Workers.Count)
        {
            if (_workerWorlds.Count >= poolingWorkers.Count)
            {
                Debug.LogWarning("Not enough pooled WorkerAnimation objects");
                break;
            }

            var workerWorld = poolingWorkers[_workerWorlds.Count];
            workerWorld.gameObject.SetActive(true);
            workerWorld.Initialize(_farmManager.Workers[_workerWorlds.Count], GetPlotWorldPosition);
            _workerWorlds.Add(workerWorld);
        }

        // Cập nhật công nhân và tắt các công nhân không dùng
        for (int i = 0; i < poolingWorkers.Count; i++)
        {
            if (i < _farmManager.Workers.Count)
            {
                poolingWorkers[i].gameObject.SetActive(true);
                poolingWorkers[i].UpdateWorld(_farmManager.Workers[i]);
            }
            else
            {
                poolingWorkers[i].gameObject.SetActive(false);
            }
        }
    }

    private Vector3 GetPlotWorldPosition(int plotIndex)
    {
        if (plotIndex < 0 || plotIndex >= _plotWorlds.Count) return Vector3.zero;
        return _plotWorlds[plotIndex].transform.position;
    }

    private void UpdateUI()
    {
        goldText.text = $"Gold: {_farmManager.Gold}";
        equipmentText.text = $"Equipment: Lv {_farmManager.EquipmentLevel}";
        workersText.text = $"Workers: {_farmManager.Workers.Count} (Idle: {_farmManager.GetIdleWorkerCount()})";
        seedsText.text = $"Inventory on hand: T:{_farmManager.InventoryOnHand["Tomato"]} B:{_farmManager.InventoryOnHand["Blueberry"]} S:{_farmManager.InventoryOnHand["Strawberry"]} C:{_farmManager.InventoryOnHand["Cow"]}";
        plotsText.text = $"Plots: {_farmManager.Plots.Count} (Free: {_farmManager.Plots.Count(p => p.IsEmpty)})";
        productsText.text = $"Products: T:{_farmManager.Products["Tomato"]} B:{_farmManager.Products["Blueberry"]} S:{_farmManager.Products["Strawberry"]} M:{_farmManager.Products["Cow"]}";

        for (int i = 0; i < _farmManager.Plots.Count && i < _plotWorlds.Count; i++)
        {
            _plotWorlds[i].UpdateWorld(_farmManager.Plots[i], _config);
        }
    }

    private void OnPlotSelected(int index)
    {
        _selectedPlotIndex = index;
        foreach (var plotWorld in _plotWorlds)
            plotWorld.SetSelected(plotWorld.PlotIndex == index);
    }

    private void PlantCrop(string cropName)
    {
        if (_selectedPlotIndex >= 0)
            _farmManager.PlantCrop(_selectedPlotIndex, cropName, DateTime.Now);
    }

    private void Harvest()
    {
        if (_selectedPlotIndex >= 0)
            _farmManager.Harvest(_selectedPlotIndex);
    }

    private void HireWorker()
    {
        _farmManager.HireWorker();
        UpdateWorkerWorld();
    }

    private void BuyPlot()
    {
        _farmManager.BuyPlot();
        UpdatePlotWorld();
    }


}