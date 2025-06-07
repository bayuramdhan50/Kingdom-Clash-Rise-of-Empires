using UnityEngine;
using UnityEngine.EventSystems;

namespace KingdomClash
{
    /// <summary>
    /// Handles the placement of buildings in the game world
    /// </summary>
    public class BuildingPlacementSystem : MonoBehaviour
    {
        public static BuildingPlacementSystem Instance { get; private set; }

        [Header("Placement Settings")]
        [SerializeField] private LayerMask groundLayer; // Layer for the ground where buildings can be placed
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        [SerializeField] private float raycastDistance = 1000f;
        [SerializeField] private Vector3 buildingRotation = new Vector3(0, 0, 0); // Default rotation for buildings

        // References
        private GameObject currentBuildingPreview;
        private GameObject buildingPrefab;
        private bool isPlacingBuilding = false;
        private Camera mainCamera;

        private void Awake()
        {
            // Singleton pattern setup
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

            // Move the preview building to the mouse position on the ground
            UpdateBuildingPreviewPosition();

            // Check if the player clicks to place the building
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                PlaceBuilding();
            }
            // Cancel building placement with right-click
            else if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
        
        /// <summary>
        /// Start the placement process for a new building
        /// </summary>
        /// <param name="prefab">The building prefab to place</param>
        public void StartPlacement(GameObject prefab)
        {
            if (prefab == null)
                return;

            // Cancel any current placement
            if (isPlacingBuilding)
            {
                CancelPlacement();
            }

            buildingPrefab = prefab;
            isPlacingBuilding = true;

            // Create the preview building
            currentBuildingPreview = Instantiate(buildingPrefab);
            
            // Make the preview semi-transparent
            SetPreviewTransparency(currentBuildingPreview, 0.5f);
            
            // Disable any colliders for the preview
            DisableCollidersRecursively(currentBuildingPreview);
        }
        
        /// <summary>
        /// Updates the position of the building preview based on mouse position
        /// </summary>
        private void UpdateBuildingPreviewPosition()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if we hit the ground
            bool validPosition = Physics.Raycast(ray, out hit, raycastDistance, groundLayer) && 
                                !IsOverlappingWithOtherBuildings(hit.point);

            // Move the preview building to the hit point
            if (validPosition)
            {
                currentBuildingPreview.transform.position = hit.point;
                currentBuildingPreview.transform.rotation = Quaternion.Euler(buildingRotation);
                
                // Set to valid material
                SetPreviewMaterial(currentBuildingPreview, validPlacementMaterial);
            }
            else if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
            {
                // We hit the ground, but the position is invalid
                currentBuildingPreview.transform.position = hit.point;
                currentBuildingPreview.transform.rotation = Quaternion.Euler(buildingRotation);
                
                // Set to invalid material
                SetPreviewMaterial(currentBuildingPreview, invalidPlacementMaterial);
            }
        }

        /// <summary>
        /// Place the building at the current position
        /// </summary>
        private void PlaceBuilding()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if we hit the ground and the position is valid
            if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer) && 
                !IsOverlappingWithOtherBuildings(hit.point))
            {
                // Create the actual building
                GameObject placedBuilding = Instantiate(buildingPrefab, hit.point, Quaternion.Euler(buildingRotation));
                
                // Reset the material and make it fully visible
                ResetMaterialsRecursively(placedBuilding);
                
                // Enable colliders
                EnableCollidersRecursively(placedBuilding);
                
                // End the placement mode
                isPlacingBuilding = false;
                
                // Destroy the preview
                Destroy(currentBuildingPreview);
                currentBuildingPreview = null;
            }
        }

        /// <summary>
        /// Cancel the building placement
        /// </summary>
        public void CancelPlacement()
        {
            if (currentBuildingPreview != null)
            {
                Destroy(currentBuildingPreview);
                currentBuildingPreview = null;
            }
            isPlacingBuilding = false;
        }

        /// <summary>
        /// Check if the building would overlap with other buildings
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>True if there's overlap, false otherwise</returns>
        private bool IsOverlappingWithOtherBuildings(Vector3 position)
        {
            // Get the bounds of the current building prefab (use a box collider or other means)
            Collider buildingCollider = buildingPrefab.GetComponentInChildren<Collider>();
            if (buildingCollider == null)
                return false;

            // Adjust the position to match where the building would be placed
            Vector3 checkPosition = position;

            // Check if there are any colliders in the area where the building would be
            Collider[] colliders = Physics.OverlapBox(
                checkPosition + buildingCollider.bounds.center, 
                buildingCollider.bounds.extents * 0.9f, 
                Quaternion.Euler(buildingRotation)
            );

            // Filter out the preview itself and check for other buildings
            foreach (var collider in colliders)
            {
                // Skip the preview and its children
                if (currentBuildingPreview != null && 
                    (collider.gameObject == currentBuildingPreview || 
                     collider.transform.IsChildOf(currentBuildingPreview.transform)))
                {
                    continue;
                }

                // If there's another object with a Building tag or component
                if (collider.CompareTag("Building") || collider.GetComponent<Building>() != null)
                {
                    return true; // Overlapping with another building
                }
            }
            
            return false; // No overlap
        }

        /// <summary>
        /// Set preview transparency for all renderers in the object
        /// </summary>
        private void SetPreviewTransparency(GameObject obj, float alpha)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                Material[] mats = renderer.materials;
                
                for (int i = 0; i < mats.Length; i++)
                {
                    Color color = mats[i].color;
                    mats[i].color = new Color(color.r, color.g, color.b, alpha);
                    
                    // Enable transparency
                    mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mats[i].SetInt("_ZWrite", 0);
                    mats[i].DisableKeyword("_ALPHATEST_ON");
                    mats[i].EnableKeyword("_ALPHABLEND_ON");
                    mats[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mats[i].renderQueue = 3000;
                }
                
                renderer.materials = mats;
            }
        }
        
        /// <summary>
        /// Set preview material for all renderers in the object
        /// </summary>
        private void SetPreviewMaterial(GameObject obj, Material material)
        {
            if (material == null)
                return;
                
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
        /// Reset materials to their original state
        /// </summary>
        private void ResetMaterialsRecursively(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                // Reset materials to their default state if needed
                Material[] mats = renderer.materials;
                
                for (int i = 0; i < mats.Length; i++)
                {
                    // Reset transparency
                    Color color = mats[i].color;
                    mats[i].color = new Color(color.r, color.g, color.b, 1.0f);
                    
                    // Reset blend mode to opaque
                    mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mats[i].SetInt("_ZWrite", 1);
                    mats[i].DisableKeyword("_ALPHATEST_ON");
                    mats[i].DisableKeyword("_ALPHABLEND_ON");
                    mats[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mats[i].renderQueue = -1;
                }
                
                renderer.materials = mats;
            }
        }
        
        /// <summary>
        /// Disable all colliders in the object recursively
        /// </summary>
        private void DisableCollidersRecursively(GameObject obj)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
        }
        
        /// <summary>
        /// Enable all colliders in the object recursively
        /// </summary>
        private void EnableCollidersRecursively(GameObject obj)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }
        }
    }
}
