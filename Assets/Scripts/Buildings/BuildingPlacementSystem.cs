using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
        [SerializeField] private Vector3 buildingRotation = new Vector3(0, 0, 0); // Rotasi default        // Warna untuk preview
        [SerializeField] private Color validPlacementColor = new Color(0.0f, 1.0f, 0.0f, 0.5f); // Hijau transparan
        [SerializeField] private Color invalidPlacementColor = new Color(1.0f, 0.0f, 0.0f, 0.5f); // Merah transparan

        // References
        private GameObject currentBuildingPreview;
        private GameObject buildingPrefab;
        private bool isPlacingBuilding = false;
        private Camera mainCamera;
        
        // Untuk menyimpan material asli
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        private void Awake()
        {
            // Setup singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            mainCamera = Camera.main;
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
        }        /// <summary>
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
            
            // Simpan material asli dan buat preview transparan
            SaveOriginalMaterials(currentBuildingPreview);
            MakeObjectTransparent(currentBuildingPreview, 0.5f);
            
            // Nonaktifkan collider untuk preview
            ToggleColliders(currentBuildingPreview, false);
        }
        
        /// <summary>
        /// Simpan material asli dari objek
        /// </summary>
        private void SaveOriginalMaterials(GameObject obj)
        {
            originalMaterials.Clear();
            
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                // Duplikasi material asli agar bisa dikembalikan nanti
                Material[] originalMats = renderer.materials;
                Material[] matsCopy = new Material[originalMats.Length];
                
                for (int i = 0; i < originalMats.Length; i++)
                {
                    matsCopy[i] = new Material(originalMats[i]);
                }
                
                originalMaterials.Add(renderer, matsCopy);
            }
        }        /// <summary>
        /// Update posisi preview bangunan berdasarkan posisi mouse
        /// </summary>
        private void UpdateBuildingPreviewPosition()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Pertama, cek apakah raycast mengenai tanah
            if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
            {
                // Pindahkan preview ke titik hit
                currentBuildingPreview.transform.position = hit.point;
                currentBuildingPreview.transform.rotation = Quaternion.Euler(buildingRotation);
                
                // Cek apakah posisi valid (tidak tumpang tindih)
                bool validPosition = !IsOverlappingWithOtherBuildings(hit.point);
                
                // Atur warna sesuai validitas posisi
                Color previewColor = validPosition ? validPlacementColor : invalidPlacementColor;
                ApplyColorToObject(currentBuildingPreview, previewColor);
            }
        }        /// <summary>
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
            }
        }        /// <summary>
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
        }/// <summary>
        /// Periksa apakah bangunan akan tumpang tindih dengan bangunan lain
        /// </summary>
        private bool IsOverlappingWithOtherBuildings(Vector3 position)
        {
            Collider buildingCollider = buildingPrefab.GetComponentInChildren<Collider>();
            if (buildingCollider == null)
            {
                return false;
            }

            // Posisi kotak pengecekan relatif terhadap posisi penempatan
            Vector3 checkPosition = position;
            // Ukuran kotak pengecekan berdasarkan bounds collider
            Vector3 checkSize = buildingCollider.bounds.extents * 0.9f;
            
            // Periksa apakah ada collider lain di area penempatan
            Collider[] colliders = Physics.OverlapBox(
                checkPosition, 
                checkSize, 
                Quaternion.Euler(buildingRotation)
            );

            // Uji setiap collider yang ditemukan
            foreach (var collider in colliders)
            {
                // Lewati preview dan child-nya
                if (currentBuildingPreview != null && 
                    (collider.gameObject == currentBuildingPreview || 
                     collider.transform.IsChildOf(currentBuildingPreview.transform)))
                {
                    continue;
                }
                
                // Pengecekan untuk semua jenis collider yang mungkin mengganggu penempatan
                // Misalnya bangunan lain, terrain features, dll.
                if (collider.CompareTag("Building") || collider.GetComponent<Building>() != null)
                {
                    return true; // Ada tumpang tindih
                }
            }
            
            return false; // Tidak ada tumpang tindih
        }        /// <summary>
        /// Buat objek transparan dengan mengubah warna
        /// </summary>
        private void MakeObjectTransparent(GameObject obj, float alpha)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    // Aktifkan transparansi untuk shader standar
                    mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mats[i].SetInt("_ZWrite", 0);
                    mats[i].DisableKeyword("_ALPHATEST_ON");
                    mats[i].EnableKeyword("_ALPHABLEND_ON");
                    mats[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mats[i].renderQueue = 3000;
                    
                    // Buat transparan
                    Color color = mats[i].color;
                    mats[i].color = new Color(color.r, color.g, color.b, alpha);
                }
                renderer.materials = mats;
            }
        }        /// <summary>
        /// Terapkan warna ke semua renderer di objek
        /// </summary>
        private void ApplyColorToObject(GameObject obj, Color color)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                Material[] mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    // Pastikan warna alpha dipertahankan
                    float alpha = mats[i].color.a;  // Simpan alpha asli
                    mats[i].color = new Color(color.r, color.g, color.b, alpha);
                    
                    // Alternatif cara 2: gunakan properti _Color langsung
                    mats[i].SetColor("_Color", color);
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
            
            // Reset warna ke warna normal
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    // Kembalikan warna ke normal (tidak transparan)
                    Color color = mats[i].color;
                    mats[i].color = new Color(color.r, color.g, color.b, 1.0f);
                    
                    // Matikan transparansi
                    mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mats[i].SetInt("_ZWrite", 1);
                    mats[i].DisableKeyword("_ALPHATEST_ON");
                    mats[i].DisableKeyword("_ALPHABLEND_ON");
                    mats[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mats[i].renderQueue = -1;
                }
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
