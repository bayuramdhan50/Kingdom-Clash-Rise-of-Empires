# Building Placement System

This system allows players to place buildings in the game world. The process is:

1. Select a building from the Shop Building Panel
2. Buy the building
3. Place the building by clicking on the ground
4. Right-click to cancel placement

## Setup Instructions

1. Add the `BuildingPlacementSystem` component to a GameObject in your scene
2. Set the `Ground Layer` to the layer that contains your terrain/ground objects
3. Optionally create a GameObject with the `BuildingPlacementMaterials` component to manage materials
4. Make sure your building prefabs have:
   - A collider for proper placement detection
   - The `Building` component attached
   - The "Building" tag assigned

## Components

### Building.cs
Base class for all buildings. Manages:
- Building stats (health, name, etc.)
- Resource production
- Building destruction

### BuildingPlacementSystem.cs
Manages the placement of buildings:
- Shows a preview of the building being placed
- Checks valid placement locations
- Handles mouse interaction for placing buildings
- Changes appearance based on valid/invalid placement locations

### BuildingPlacementMaterials.cs
Creates and manages materials used for building placement:
- Green transparent material for valid placement
- Red transparent material for invalid placement

### BuildingTypes.cs
Defines the types of buildings that can be placed in the game.

## Integration with ShopBuildingPanel

The `ShopBuildingPanel.cs` script has been updated to:
1. Start the building placement process when a building is purchased
2. Close the panel during placement for better visibility

## Usage Example

```csharp
// Example code to start placing a building
GameObject buildingPrefab = ...; // Reference to your building prefab
BuildingPlacementSystem.Instance.StartPlacement(buildingPrefab);

// To cancel placement
BuildingPlacementSystem.Instance.CancelPlacement();
```

## Tips

- Buildings should include the `Building` component and use the "Building" tag for placement detection.
- Customize the appearance of placement preview by modifying the materials.
- Add additional check methods in `BuildingPlacementSystem` for specific placement rules.
- Use `EventSystem.current.IsPointerOverGameObject()` to prevent placing buildings while clicking UI elements.
