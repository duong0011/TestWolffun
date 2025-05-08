using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlotWorld : MonoBehaviour
{
    [SerializeField] private SpriteRenderer plotSprite; // Sprite cho cây/vật nuôi
    [SerializeField] private GameObject progressBar; // Thanh tiến trình (world space)
    [SerializeField] private SpriteRenderer progressFill; // Phần fill của thanh tiến trình
    [SerializeField] private SpriteRenderer border;
    [SerializeField] private TextMeshPro currentAmountProduct; // Hiện số lượng sản phẩm hiện có
    [SerializeField] private Sprite emptySprite, tomatoSprite, blueberrySprite, strawberrySprite, cowSprite;

    private FarmManager _farmManager;
    private int _plotIndex;
    private System.Action<int> _onPlotSelected;

    public int PlotIndex => _plotIndex;

    public void Initialize(FarmManager farmManager, int plotIndex, System.Action<int> onPlotSelected)
    {
        if (farmManager == null)
        {
            Debug.LogError("FarmManager is null in PlotWorld.Initialize");
            return;
        }
        _farmManager = farmManager;
        _plotIndex = plotIndex;
        _onPlotSelected = onPlotSelected;

        var collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 1f);
        gameObject.AddComponent<PlotClickHandler>().Initialize(() => _onPlotSelected?.Invoke(_plotIndex));
    }

    public void UpdateWorld(Plot plot, GameConfig config)
    {
        if (plot == null || config == null || _farmManager == null)
        {
            Debug.LogError("Plot, config, or farmManager is null in PlotWorld.UpdateWorld");
            return;
        }

        if (plotSprite == null || progressBar == null || progressFill == null)
        {
            Debug.LogError("plotSprite, progressBar, or progressFill is null in PlotWorld.UpdateWorld");
            return;
        }

        if (plot.IsEmpty)
        {
            plotSprite.sprite = emptySprite ?? null;
            progressBar.SetActive(false);
            currentAmountProduct.gameObject.SetActive(false);
        }
        else
        {
            if (string.IsNullOrEmpty(plot.CropName) || !config.Crops.ContainsKey(plot.CropName) || config.Crops[plot.CropName] == null)
            {
                Debug.LogError($"Invalid CropName: {plot.CropName} or crop config is null in PlotWorld.UpdateWorld");
                plotSprite.sprite = emptySprite;
                progressBar.SetActive(false);
                currentAmountProduct.gameObject.SetActive(false);
                return;
            }

            // Cập nhật sprite
            switch (plot.CropName)
            {
                case "Tomato":
                    plotSprite.sprite = tomatoSprite ?? emptySprite;
                    break;
                case "Blueberry":
                    plotSprite.sprite = blueberrySprite ?? emptySprite;
                    break;
                case "Strawberry":
                    plotSprite.sprite = strawberrySprite ?? emptySprite;
                    break;
                case "Cow":
                    plotSprite.sprite = cowSprite ?? emptySprite;
                    break;
                default:
                    plotSprite.sprite = emptySprite;
                    Debug.LogWarning($"Unknown CropName: {plot.CropName}");
                    break;
            }
            progressBar.SetActive(true);
            currentAmountProduct.gameObject.SetActive(true);
            var equipmentLevel = _farmManager.EquipmentLevel;
            if (equipmentLevel < 1)
            {
                equipmentLevel = 1;
            }
            var growthTime = config.Crops[plot.CropName].GrowthTime / (1 + 0.1 * (equipmentLevel - 1));
            var timeSincePlanted = plot.TimeSincePlanted;
            var lastHarvestTime = plot.LastHarvestTime;
            var progress = Mathf.Clamp01((float)((timeSincePlanted - lastHarvestTime) / growthTime));
            progressFill.transform.localScale = new Vector3(progress, 1f, 1f);
            currentAmountProduct.text = plot.AmountProduct.ToString();
        }
    }

    public void SetSelected(bool isSelected)
    {
        border.gameObject.SetActive(isSelected);    
    }
}


public class PlotClickHandler : MonoBehaviour
{
    private System.Action _onClick;

    public void Initialize(System.Action onClick)
    {
        _onClick = onClick;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        _onClick?.Invoke();
    }
}