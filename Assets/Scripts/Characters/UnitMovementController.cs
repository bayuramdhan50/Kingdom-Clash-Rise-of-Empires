using UnityEngine;
using System.Collections.Generic;

namespace KingdomClash.Characters
{
    /// <summary>
    /// Handles unit selection and movement commands
    /// </summary>
    public class UnitMovementController : MonoBehaviour
    {
        private List<Unit> selectedUnits = new List<Unit>();
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleUnitSelection();
            HandleMovementCommand();
        }

        private void HandleUnitSelection()
        {
            // Single unit selection with left click
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Unit unit = hit.collider.GetComponent<Unit>();
                    if (unit != null)
                    {
                        // If not holding shift, deselect all units first
                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            DeselectAllUnits();
                        }
                        
                        SelectUnit(unit);
                    }
                    else if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        // If clicking empty space and not holding shift, deselect all units
                        DeselectAllUnits();
                    }
                }
            }
        }

        private void HandleMovementCommand()
        {
            // Move selected units with right click
            if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // Move all selected units to the clicked position
                    foreach (Unit unit in selectedUnits)
                    {
                        unit.MoveTo(hit.point);
                    }
                }
            }
        }

        private void SelectUnit(Unit unit)
        {
            if (!selectedUnits.Contains(unit))
            {
                selectedUnits.Add(unit);
                unit.Select();
            }
        }

        private void DeselectAllUnits()
        {
            foreach (Unit unit in selectedUnits)
            {
                unit.Deselect();
            }
            selectedUnits.Clear();
        }
    }
}
