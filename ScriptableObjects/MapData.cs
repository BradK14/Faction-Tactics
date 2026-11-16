using UnityEngine;
using System.Collections.Generic;

// This class matches the structure the server expects
[System.Serializable]
public class TileData
{
    public int x;
    public int y;

    public int elevation = 1;
    public bool cliffNorth;
    public bool cliffEast;
    public bool cliffSouth;
    public bool cliffWest;
    public enum ResourceType { None, Purple, Red, Blue }
    public ResourceType resourceType;

    // These are used to tell whose assets should be spawned in.  1 for player 1, 2 for player 2, ...
    public enum StartingStructure { None, Base, SmallBase, BasicProduction, AdvProduction, Wall, StaticDefense }
    public StartingStructure startingStructure;
    public int startingStructurePlayer = 1;
    public enum StartingUnit { None, Builder, Fighter, Scout, Support, Harasser, Destroyer, Powerhouse }
    public StartingUnit startingUnit;
    public int startingUnitPlayer = 1;
}

public class MapData : ScriptableObject
{
    public int width = 10;
    public int height = 10;
    public List<TileData> tiles = new();

    // Access a tile using x, y coordinates
    public TileData GetTile(int x, int y)
    {
        // Return null if out of bounds
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return null;
        }

        int index = y * width + x;
        return tiles[index];
    }
}
