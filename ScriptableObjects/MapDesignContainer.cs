using UnityEngine;
using System.Collections.Generic;

// This class matches the structure the server expects
[System.Serializable]
public class SerializableTile
{
    public int x;
    public int y;
    public string tileName;
    public byte terrainType;
    public bool isPassable;
    public int currentHealth;
    public int maxHealth;
    public string factionOwner;
    public int remainingResources;
    public int trapDamage;
    public List<string> itemsDroppedOnTile = new List<string>();

    /* Implement this information instead of the above information
    public int elevation;
    public bool cliffNorth;
    public bool cliffEast;
    public bool cliffSouth;
    public bool cliffWest;
    public bool purpleResource;
    public bool redResource;
    public bool blueResource;

    // These are used to tell whose assets should be spawned in.  0 for none, 1 for player 1, 2 for player 2, ...
    public int playerBase;
    public int playerWorker;
    */
}

public class MapDesignContainer : ScriptableObject
{
    public int width = 10;
    public int height = 10;
    public List<SerializableTile> tiles = new List<SerializableTile>();
}
