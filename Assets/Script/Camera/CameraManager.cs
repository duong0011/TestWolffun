using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Tốc độ di chuyển camera (units/giây)
    [SerializeField] private Vector2 minBounds = new Vector2(-10f, -10f); // Giới hạn tọa độ X/Y tối thiểu
    [SerializeField] private Vector2 maxBounds = new Vector2(10f, 10f); // Giới hạn tọa độ X/Y tối đa

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraManager must be attached to a GameObject with a Camera component");
        }
    }

    private void Update()
    {
        if (mainCamera == null) return;

        // Lấy input từ phím mũi tên
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            moveY = 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            moveY = -1f;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            moveX = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
        }

        // Tính vector di chuyển
        Vector3 moveDirection = new Vector3(moveX, moveY, 0f).normalized;
        Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;

        // Giới hạn vị trí camera
        newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y, maxBounds.y);
        newPosition.z = transform.position.z; // Giữ nguyên Z (thường là -10)

        // Cập nhật vị trí camera
        transform.position = newPosition;
    }

    // Thiết lập giới hạn dựa trên kích thước background
    public void SetBounds(Vector2 backgroundSize)
    {
        minBounds = new Vector2(-backgroundSize.x / 2f, -backgroundSize.y / 2f);
        maxBounds = new Vector2(backgroundSize.x / 2f, backgroundSize.y / 2f);
    }

    private void OnValidate()
    {
        if (moveSpeed < 0f)
        {
            Debug.LogWarning("moveSpeed should be positive in CameraManager", this);
            moveSpeed = 0f;
        }
        if (minBounds.x > maxBounds.x || minBounds.y > maxBounds.y)
        {
            Debug.LogWarning("minBounds should be less than maxBounds in CameraManager", this);
        }
    }
}