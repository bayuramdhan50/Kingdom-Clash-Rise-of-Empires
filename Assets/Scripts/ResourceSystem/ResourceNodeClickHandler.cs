using UnityEngine;
using KingdomClash; // Tambahkan namespace KingdomClash

/// <summary>
/// Simple click behavior untuk ResourceNode
/// </summary>
public class ResourceNodeClickHandler : MonoBehaviour
{    private ResourceNode resourceNode;
    
    private void Start()
    {
        resourceNode = GetComponent<ResourceNode>();
        if (resourceNode == null)
        {
            resourceNode = GetComponentInParent<ResourceNode>();
        }
    }
    
    private void OnMouseDown()
    {
        if (resourceNode == null) return;
        
        // Find selected units
        KingdomClash.Characters.Unit[] allUnits = FindObjectsOfType<KingdomClash.Characters.Unit>();
        bool workerSent = false;
        
        foreach (KingdomClash.Characters.Unit unit in allUnits)
        {
            // Only consider player units that are selected and are workers
            if (unit.IsSelected() && unit.gameObject.CompareTag("PlayerUnit"))
            {
                KingdomClash.WorkerUnit worker = unit.GetComponent<KingdomClash.WorkerUnit>();
                if (worker != null)
                {
                    // Send worker to gather this resource
                    worker.GatherResourceAt(resourceNode);
                    workerSent = true;
                }
            }
        }
        
        if (workerSent)
        {
            Debug.Log($"Worker(s) sent to gather {resourceNode.GetResourceType()} from node");
        }
    }
}
