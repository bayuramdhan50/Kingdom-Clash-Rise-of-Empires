using UnityEngine;

namespace KingdomClash
{
    /// <summary>
    /// Creates default materials for building placement indicators
    /// </summary>
    public class BuildingPlacementMaterials : MonoBehaviour
    {
        // Singleton instance
        public static BuildingPlacementMaterials Instance { get; private set; }
        
        // Materials for placement preview
        public Material validPlacementMaterial { get; private set; }
        public Material invalidPlacementMaterial { get; private set; }
        
        private void Awake()
        {
            // Setup singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            // Create materials if they don't exist
            CreateMaterials();
        }
        
        /// <summary>
        /// Create default materials for building placement
        /// </summary>
        private void CreateMaterials()
        {
            // Create valid placement material (green)
            validPlacementMaterial = new Material(Shader.Find("Standard"));
            validPlacementMaterial.color = new Color(0.0f, 1.0f, 0.0f, 0.5f); // Semi-transparent green
            validPlacementMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            validPlacementMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            validPlacementMaterial.SetInt("_ZWrite", 0);
            validPlacementMaterial.DisableKeyword("_ALPHATEST_ON");
            validPlacementMaterial.EnableKeyword("_ALPHABLEND_ON");
            validPlacementMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            validPlacementMaterial.renderQueue = 3000;
            
            // Create invalid placement material (red)
            invalidPlacementMaterial = new Material(Shader.Find("Standard"));
            invalidPlacementMaterial.color = new Color(1.0f, 0.0f, 0.0f, 0.5f); // Semi-transparent red
            invalidPlacementMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            invalidPlacementMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            invalidPlacementMaterial.SetInt("_ZWrite", 0);
            invalidPlacementMaterial.DisableKeyword("_ALPHATEST_ON");
            invalidPlacementMaterial.EnableKeyword("_ALPHABLEND_ON");
            invalidPlacementMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            invalidPlacementMaterial.renderQueue = 3000;
        }
    }
}
