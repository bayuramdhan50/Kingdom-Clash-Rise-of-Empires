using UnityEngine;
using UnityEngine.EventSystems;

namespace KingdomClash
{
    /// <summary>
    /// Menangani penempatan bangunan di dunia game
    /// </summary>
    public class BuildingPlacementSystem : MonoBehaviour
    {
        // Singleton instance
        public static BuildingPlacementSystem Instance { get; private set; }

        [Header("Pengaturan Penempatan")]
        [SerializeField] private LayerMask groundLayer; // Layer untuk tanah
        [SerializeField] private float raycastDistance = 1000f;
        [SerializeField] private Vector3 buildingRotation = new Vector3(0, 0, 0); // Rotasi default

        // Materials untuk preview
        private Material validPlacementMaterial;
        private Material invalidPlacementMaterial;

        // References
        private GameObject currentBuildingPreview;
        private GameObject buildingPrefab;
        private bool isPlacingBuilding = false;
        private Camera mainCamera;

        private void Awake()
        {
            // Setup singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Buat material default
            CreateDefaultMaterials();
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        /// <summary>
        /// Buat material default untuk preview
        /// </summary>
        private void CreateDefaultMaterials()
        {
            // Material valid (hijau)
            validPlacementMaterial = new Material(Shader.Find("Standard"));
            validPlacementMaterial.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);
            SetupTransparentMaterial(validPlacementMaterial);

            // Material tidak valid (merah)
            invalidPlacementMaterial = new Material(Shader.Find("Standard"));
            invalidPlacementMaterial.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            SetupTransparentMaterial(invalidPlacementMaterial);
        }

        /// <summary>
        /// Setup material untuk transparansi
        /// </summary>
        private void SetupTransparentMaterial(Material material)
        {
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        private void Update()
        {
            if (!isPlacingBuilding || currentBuildingPreview == null)
                return;

            // Pindahkan preview bangunan ke posisi mouse
            UpdateBuildingPreviewPosition();

            // Periksa jika pemain mengklik untuk menempatkan bangunan
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                PlaceBuilding();
            }
            // Batalkan penempatan dengan klik kanan
            else if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
        
        /// <summary>
        /// Mulai proses penempatan untuk bangunan baru
        /// </summary>
        public void StartPlacement(GameObject prefab)
        {
            if (prefab == null)
                return;

            // Batalkan penempatan yang sedang berlangsung
            if (isPlacingBuilding)
            {
                CancelPlacement();
            }

            buildingPrefab = prefab;
            isPlacingBuilding = true;

            // Buat preview bangunan
            currentBuildingPreview = Instantiate(buildingPrefab);
            
            // Buat preview transparan
            MakeObjectTransparent(currentBuildingPreview, 0.5f);
            
            // Nonaktifkan collider untuk preview
            ToggleColliders(currentBuildingPreview, false);

            Debug.Log($"Placing building: {buildingPrefab.name}");
        }
        
        /// <summary>
        /// Update posisi preview bangunan berdasarkan posisi mouse
        /// </summary>
        private void UpdateBuildingPreviewPosition()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Cek apakah raycast mengenai tanah
            bool validPosition = Physics.Raycast(ray, out hit, raycastDistance, groundLayer) && 
                               !IsOverlappingWithOtherBuildings(hit.point);

            // Pindahkan preview ke titik hit
            if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
            {
                currentBuildingPreview.transform.position = hit.point;
                currentBuildingPreview.transform.rotation = Quaternion.Euler(buildingRotation);
                
                // Atur material sesuai validitas posisi
                Material previewMaterial = validPosition ? validPlacementMaterial : invalidPlacementMaterial;
                ApplyMaterialToObject(currentBuildingPreview, previewMaterial);
            }
        }

        /// <summary>
        /// Tempatkan bangunan pada posisi saat ini
        /// </summary>
        private void PlaceBuilding()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Periksa apakah posisi valid
            if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer) && 
                !IsOverlappingWithOtherBuildings(hit.point))
            {
                // Buat bangunan actual
                GameObject placedBuilding = Instantiate(buildingPrefab, hit.point, Quaternion.Euler(buildingRotation));
                
                // Kembalikan material normal dan aktifkan collider
                ResetObject(placedBuilding);
                
                // Hentikan mode penempatan
                isPlacingBuilding = false;
                
                // Hapus preview
                Destroy(currentBuildingPreview);
                currentBuildingPreview = null;

                Debug.Log($"Building placed: {buildingPrefab.name}");
            }
        }

        /// <summary>
        /// Batalkan penempatan bangunan
        /// </summary>
        public void CancelPlacement()
        {
            if (currentBuildingPreview != null)
            {
                Destroy(currentBuildingPreview);
                currentBuildingPreview = null;
            }
            isPlacingBuilding = false;
            
            Debug.Log("Building placement canceled");
        }

        /// <summary>
        /// Periksa apakah bangunan akan tumpang tindih dengan bangunan lain
        /// </summary>
        private bool IsOverlappingWithOtherBuildings(Vector3 position)
        {
            Collider buildingCollider = buildingPrefab.GetComponentInChildren<Collider>();
            if (buildingCollider == null)
                return false;

            // Periksa apakah ada collider lain di area penempatan
            Collider[] colliders = Physics.OverlapBox(
                position + buildingCollider.bounds.center, 
                buildingCollider.bounds.extents * 0.9f, 
                Quaternion.Euler(buildingRotation)
            );

            foreach (var collider in colliders)
            {
                // Lewati preview dan child-nya
                if (currentBuildingPreview != null && 
                    (collider.gameObject == currentBuildingPreview || 
                     collider.transform.IsChildOf(currentBuildingPreview.transform)))
                {
                    continue;
                }

                // Jika ada objek dengan tag Building atau komponen Building
                if (collider.CompareTag("Building") || collider.GetComponent<Building>() != null)
                {
                    return true; // Ada tumpang tindih
                }
            }
            
            return false; // Tidak ada tumpang tindih
        }

        /// <summary>
        /// Buat objek transparan
        /// </summary>
        private void MakeObjectTransparent(GameObject obj, float alpha)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Color color = mats[i].color;
                    mats[i].color = new Color(color.r, color.g, color.b, alpha);
                    SetupTransparentMaterial(mats[i]);
                }
                renderer.materials = mats;
            }
        }
        
        /// <summary>
        /// Terapkan material ke semua renderer di objek
        /// </summary>
        private void ApplyMaterialToObject(GameObject obj, Material material)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] mats = new Material[renderer.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = material;
                }
                renderer.materials = mats;
            }
        }
        
        /// <summary>
        /// Reset objek kembali ke keadaan normal
        /// </summary>
        private void ResetObject(GameObject obj)
        {
            // Aktifkan collider
            ToggleColliders(obj, true);
            
            // Reset material jika diperlukan
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                // Reset material dilakukan secara otomatis karena kita membuat objek baru
            }
        }
        
        /// <summary>
        /// Aktifkan/nonaktifkan collider pada objek
        /// </summary>
        private void ToggleColliders(GameObject obj, bool enabled)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = enabled;
            }
        }
    }
}
