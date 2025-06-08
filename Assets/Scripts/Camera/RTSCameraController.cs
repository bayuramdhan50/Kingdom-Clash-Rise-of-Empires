using UnityEngine;
using KingdomClash;

/// <summary>
/// Simple RTS camera controller that handles movement with WASD keys and zoom with scroll wheel.
/// </summary>
public class RTSCameraController : MonoBehaviour
{    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float borderThickness = 10f;
    [SerializeField] private bool useScreenEdge = false;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoomHeight = 10f;
    [SerializeField] private float maxZoomHeight = 50f;
    
    [Header("Rotation Settings")]
    [SerializeField] private bool smoothRotation = true;
    
    [Header("Smoothing")]
    [SerializeField] private float moveSmoothTime = 0.1f;
    [SerializeField] private float zoomSmoothTime = 0.2f;
    [SerializeField] private float rotationSmoothTime = 0.2f;
      // Internal movement variables
    private Vector3 targetPosition;
    private float targetZoom;
    private Quaternion targetRotation;
    private Vector3 moveVelocity = Vector3.zero;
    private float zoomVelocity = 0f;
    private float rotationVelocity = 0f;
    public Camera cam { get; private set; }    private void Awake()
    {
        // Dapatkan komponen Camera
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = gameObject.AddComponent<Camera>();
        }
        
        // Set nilai default jika posisi di origin
        if (transform.position == Vector3.zero)
        {
            transform.position = new Vector3(20f, 20f, 20f);
        }
        
        // Set nilai default jika zoom tidak valid
        if (cam.orthographicSize <= 0)
        {
            cam.orthographicSize = 30f;
        }
        
        // Inisialisasi target values
        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;
        targetRotation = transform.rotation;
    }private void Start()
    {
        // Pastikan kamera memiliki posisi awal yang valid
        if (transform.position == Vector3.zero)
        {
            // Set posisi default yang valid
            transform.position = new Vector3(20f, 20f, 20f);
        }
        
        // Pastikan targetPosition dan targetZoom memiliki nilai yang valid
        targetPosition = transform.position;
        targetZoom = Mathf.Clamp(cam.orthographicSize, minZoomHeight, maxZoomHeight);
        
        // Pastikan orthographic size juga diatur dengan benar
        cam.orthographicSize = targetZoom;
        
        // Coba load camera state setelah kamera diinisialisasi
        Invoke("LoadCameraState", 0.5f);
        
        // Simpan state kamera awal jika GameData masih kosong
        Invoke("EnsureCameraStateSaved", 1.0f);
        
        // Save camera state on scene unload
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    
    private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        // Save camera state when scene is unloaded
        SaveCameraState();
    }    private void OnDestroy()
    {
        // Clean up event subscriptions
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
      private void Update()
    {
        HandleMovementInput();
        HandleZoomInput();
        HandleMiddleMouseDrag();
    }
      private void LateUpdate()
    {
        // Apply movement with smoothing
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref moveVelocity, moveSmoothTime);
        
        // Apply zoom with smoothing
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
        
        // Apply rotation with smoothing if enabled
        if (smoothRotation)
        {
            // Smooth rotation using SmoothDampAngle for each axis
            float currentYAngle = transform.eulerAngles.y;
            float targetYAngle = targetRotation.eulerAngles.y;
            float newYAngle = Mathf.SmoothDampAngle(currentYAngle, targetYAngle, ref rotationVelocity, rotationSmoothTime);
            
            // Apply rotation around Y axis (horizontal rotation)
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, newYAngle, transform.eulerAngles.z);
        }
        else
        {
            // Apply rotation immediately
            transform.rotation = targetRotation;
        }
    }
      private void HandleMovementInput()
    {
        Vector3 moveDirection = Vector3.zero;
        
        // Keyboard (WASD) movement - using local space directions
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveDirection += Vector3.right;
            
        // Screen edge movement
        if (useScreenEdge)
        {
            Vector3 mousePos = Input.mousePosition;
            if (mousePos.x < borderThickness)
                moveDirection += Vector3.left;
            else if (mousePos.x > Screen.width - borderThickness)
                moveDirection += Vector3.right;
            
            if (mousePos.y < borderThickness)
                moveDirection += Vector3.back;
            else if (mousePos.y > Screen.height - borderThickness)
                moveDirection += Vector3.forward;
        }
        
        // Apply movement
        if (moveDirection != Vector3.zero)
        {
            // Normalize to prevent faster diagonal movement
            moveDirection.Normalize();
            
            // Convert direction from local space to world space based on camera rotation
            Vector3 worldSpaceDirection = transform.TransformDirection(moveDirection);
            
            // Keep movement on the horizontal plane
            worldSpaceDirection.y = 0;
            worldSpaceDirection.Normalize();
            
            // Adjust movement speed based on zoom level (move faster when zoomed out)
            float speedMultiplier = cam.orthographicSize / minZoomHeight;
            float actualMoveSpeed = moveSpeed * speedMultiplier;
            
            targetPosition += worldSpaceDirection * actualMoveSpeed * Time.deltaTime;
        }
    }
      private void HandleZoomInput()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput != 0)
        {
            // Negative scroll value = scroll down = zoom out
            // Positive scroll value = scroll up = zoom in
            float zoomChange = scrollInput * zoomSpeed;
            
            // Invert the direction so scrolling down zooms out
            targetZoom -= zoomChange;
            
            // Clamp zoom to min/max values
            targetZoom = Mathf.Clamp(targetZoom, minZoomHeight, maxZoomHeight);
        }
    }    private void HandleMiddleMouseDrag()
    {
        // Check if middle mouse button (scroll wheel) is being pressed
        if (Input.GetMouseButton(2)) // 0=left, 1=right, 2=middle
        {
            // Get mouse position and create a direction from screen center to mouse
            Vector3 mousePos = Input.mousePosition;
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Vector3 direction = (mousePos - screenCenter).normalized;
            
            // Calculate the target rotation based on mouse position
            if (direction.sqrMagnitude > 0.1f) // Only rotate if mouse is far enough from center
            {
                // Calculate angle in degrees from mouse direction
                float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                
                // Create rotation quaternion (rotate around Y axis for standard camera orientation)
                targetRotation = Quaternion.Euler(transform.eulerAngles.x, angle, transform.eulerAngles.z);
                
                // If not using smooth rotation, apply it immediately
                if (!smoothRotation)
                {
                    transform.rotation = targetRotation;
                }
                
                // Position is not changed when rotating with middle mouse
            }
        }
    }
      /// <summary>
    /// Immediately move the camera to a specific position without smoothing
    /// </summary>
    /// <param name="position">The world position to move to</param>
    public void SetPosition(Vector3 position)
    {
        targetPosition = position;
        transform.position = position;
        moveVelocity = Vector3.zero;
    }
    
    /// <summary>
    /// Set the camera rotation to look in a specific direction
    /// </summary>
    /// <param name="rotation">The rotation to set</param>
    public void SetRotation(Quaternion rotation)
    {
        targetRotation = rotation;
        transform.rotation = rotation;
        rotationVelocity = 0f;
    }
    
    /// <summary>
    /// Rotate the camera to look at a specific world position
    /// </summary>
    /// <param name="worldPosition">The world position to look at</param>
    public void LookAt(Vector3 worldPosition)
    {
        Vector3 directionToTarget = worldPosition - transform.position;
        directionToTarget.y = 0; // Keep camera level
        
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            SetRotation(targetRotation);
        }
    }
      /// <summary>
    /// Set the zoom level directly without smoothing
    /// </summary>
    /// <param name="zoomLevel">The orthographic size to set</param>
    public void SetZoom(float zoomLevel)
    {
        targetZoom = Mathf.Clamp(zoomLevel, minZoomHeight, maxZoomHeight);
        cam.orthographicSize = targetZoom;
        zoomVelocity = 0f;
    }    /// <summary>
    /// Save the current camera state to the game data
    /// </summary>
    public void SaveCameraState()
    {
        if (GameManager.Instance != null)
        {
            GameData gameData = GameManager.Instance.GetCurrentGameData();
            if (gameData != null)
            {
                // Pastikan kamera sudah diinisialisasi
                if (cam == null)
                {
                    cam = GetComponent<Camera>();
                    if (cam == null)
                    {
                        return;
                    }
                }
                
                // PENTING: Selalu gunakan posisi aktual kamera saat ini, bukan target values
                Vector3 currentPosition = transform.position;
                Quaternion currentRotation = transform.rotation;
                float currentZoom = cam.orthographicSize;
                
                // Validasi data kamera
                bool positionValid = currentPosition != Vector3.zero;
                bool zoomValid = currentZoom > 0;
                
                if (!positionValid || !zoomValid)
                {
                    return; // Jangan simpan data yang tidak valid
                }
                
                // Simpan langsung ke GameData
                gameData.cameraPosition = new Vector3Data(currentPosition);
                gameData.cameraRotation = new QuaternionData(currentRotation);
                gameData.cameraZoom = currentZoom;
            }
        }
    }    /// <summary>
    /// Load the camera state from the game data
    /// </summary>
    public void LoadCameraState()
    {
        if (GameManager.Instance != null)
        {
            GameData gameData = GameManager.Instance.GetCurrentGameData();
            
            // Periksa data kamera di GameData
            if (gameData != null && gameData.cameraPosition != null)
            {
                // Validasi data kamera yang tersimpan
                bool isPositionValid = 
                    (gameData.cameraPosition.x != 0 || gameData.cameraPosition.y != 0 || gameData.cameraPosition.z != 0);
                
                bool isZoomValid = gameData.cameraZoom > 0;
                
                if (isPositionValid && isZoomValid)
                {
                    // Konversi data kamera ke format Unity
                    Vector3 savedPosition = gameData.cameraPosition.ToVector3();
                    Quaternion savedRotation = gameData.cameraRotation != null ? 
                        gameData.cameraRotation.ToQuaternion() : Quaternion.Euler(30f, 0f, 0f);
                    float savedZoom = gameData.cameraZoom;
                    
                    // Terapkan data kamera ke objek kamera aktual
                    transform.position = savedPosition;
                    transform.rotation = savedRotation;
                    cam.orthographicSize = savedZoom;
                    
                    // Perbarui juga target values
                    targetPosition = savedPosition;
                    targetRotation = savedRotation;
                    targetZoom = savedZoom;
                }
            }
            
            // VALIDASI: Pastikan kamera memiliki posisi dan zoom yang valid setelah load
            if (transform.position == Vector3.zero || cam.orthographicSize <= 0)
            {
                if (transform.position == Vector3.zero)
                {
                    transform.position = new Vector3(20f, 20f, 20f);
                    targetPosition = transform.position;
                }
                
                if (cam.orthographicSize <= 0)
                {
                    cam.orthographicSize = 30f;
                    targetZoom = cam.orthographicSize;
                }
            }
            
            // Update moveVelocity dan zoomVelocity ke nol untuk mencegah smooth dampening saat pertama kali load
            moveVelocity = Vector3.zero;
            zoomVelocity = 0f;
        }
    }    /// <summary>
    /// Memastikan state kamera tersimpan dalam GameData
    /// </summary>
    private void EnsureCameraStateSaved()
    {
        if (GameManager.Instance == null) return;
        
        GameData gameData = GameManager.Instance.GetCurrentGameData();
        if (gameData == null) return;
        
        // Cek apakah data kamera sudah ada dan valid
        bool cameraDataIsValid = 
            gameData.cameraPosition != null && 
            (gameData.cameraPosition.x != 0 || gameData.cameraPosition.y != 0 || gameData.cameraPosition.z != 0) &&
            gameData.cameraZoom > 0;
        
        if (!cameraDataIsValid)
        {
            SaveCameraState();
        }
    }
}
