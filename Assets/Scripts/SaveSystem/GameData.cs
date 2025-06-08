using UnityEngine;
using System.Collections.Generic;

namespace KingdomClash
{
    /// <summary>
    /// Stores all game data for saving and loading
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        public string playerName;
        public int level;
        public Resources resources;
        public Characters.CharacterType selectedCharacter;
        public string dateTime = System.DateTime.Now.ToString();
        
        // Camera data
        public Vector3Data cameraPosition;
        public QuaternionData cameraRotation;
        public float cameraZoom;
        
        // Building data - list of all placed buildings
        public List<BuildingData> placedBuildings = new List<BuildingData>();
        
        // Training data - list of all training processes
        public List<TrainingData> trainingProcesses = new List<TrainingData>();
        
        // Unit data - list of all units
        public List<UnitData> units = new List<UnitData>();
    }
    
    /// <summary>
    /// Stores information about a placed building
    /// </summary>
    [System.Serializable]
    public class BuildingData
    {
        public string buildingName;   // Nama bangunan
        public string prefabName;     // Nama prefab untuk diinisialisasi
        public Vector3Data position;  // Posisi bangunan di dunia game
        public QuaternionData rotation; // Rotasi bangunan
        public int health;            // Health saat ini
        public int maxHealth;         // Health maksimum
        public bool producesResources; // Apakah menghasilkan resource
        public string resourceType;    // Tipe resource yang dihasilkan
        public int productionAmount;   // Jumlah produksi
        
        // Konstruktor default
        public BuildingData() { }
        
        // Konstruktor untuk membuat data dari bangunan yang sudah ada
        public BuildingData(Building building, string prefabName)
        {
            this.buildingName = building.GetBuildingName();
            this.prefabName = prefabName;
            this.position = new Vector3Data(building.transform.position);
            this.rotation = new QuaternionData(building.transform.rotation);
            this.health = building.GetHealth();
            this.maxHealth = building.GetMaxHealth();
            
            // Get resource production info
            var resourceInfo = building.GetResourceProductionInfo();
            this.producesResources = resourceInfo.producesResources;
            this.resourceType = resourceInfo.resourceType;
            this.productionAmount = resourceInfo.amount;
        }
    }
    
    /// <summary>
    /// Stores resource information for the game
    /// </summary>
    [System.Serializable]
    public class Resources
    {
        public int wood;  // Kayu
        public int stone; // Batu
        public int iron;  // Besi
        public int food;  // Makanan
    }
    
    /// <summary>
    /// Helper class for serializing Vector3 data
    /// </summary>
    [System.Serializable]
    public class Vector3Data
    {
        public float x, y, z;
        
        public Vector3Data(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        public Vector3Data(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
        
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
    
    /// <summary>
    /// Helper class for serializing Quaternion data
    /// </summary>
    [System.Serializable]
    public class QuaternionData
    {
        public float x, y, z, w;
        
        public QuaternionData(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        
        public QuaternionData(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }
        
        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
    }
    
    /// <summary>
    /// Stores training process information for saving/loading
    /// </summary>
    [System.Serializable]
    public class TrainingData
    {
        public string buildingName;    // Nama bangunan yang melakukan training
        public string unitType;        // Tipe unit yang dilatih
        public int count;              // Jumlah unit dalam antrian
        public float progress;         // Progress training saat ini (0-1)
        public float trainingTime;     // Total waktu yang dibutuhkan untuk melatih satu unit
        
        public TrainingData() { }
        
        public TrainingData(string buildingName, string unitType, int count, float progress, float trainingTime)
        {
            this.buildingName = buildingName;
            this.unitType = unitType;
            this.count = count;
            this.progress = progress;
            this.trainingTime = trainingTime;
        }
    }
    
    /// <summary>
    /// Stores information about a unit (troop)
    /// </summary>
    [System.Serializable]
    public class UnitData
    {
        public string unitType;        // Tipe unit (Infantry, Archer, Cavalry)
        public int health;             // Health saat ini
        public int maxHealth;          // Health maksimum
        public int attack;             // Damage serangan
        public int defense;            // Pertahanan
        public Vector3Data position;   // Posisi unit
        
        public UnitData() { }
        
        public UnitData(string unitType, int health, int maxHealth, int attack, int defense, Vector3 position)
        {
            this.unitType = unitType;
            this.health = health;
            this.maxHealth = maxHealth;
            this.attack = attack;
            this.defense = defense;
            this.position = new Vector3Data(position);
        }
    }
}
