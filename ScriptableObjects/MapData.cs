using UnityEngine;
using System.Collections.Generic;

// This class matches the structure the server expects
[System.Serializable]
public class TileData
{
    public int x;
    public int y;

    public int elevation;
    public bool cliffNorth;
    public bool cliffEast;
    public bool cliffSouth;
    public bool cliffWest;
    public enum ResourceType { None, Purple, Red, Blue }
    public ResourceType resourceType;

    // These are used to tell whose assets should be spawned in.  0 for none, 1 for player 1, 2 for player 2, ...
    public enum StartingStructure { None, Base, SmallBase, BasicProduction, AdvProduction, Wall, StaticDefense }
    public StartingStructure startingStructure;
    public int startingStructurePlayer;
    public enum StartingUnit { None, Builder, Fighter, Scout, Supporter, Harasser, Destroyer, Powerhouse }
    public StartingUnit startingUnit;
    public int startingUnitPlayer;
}

public class MapData : ScriptableObject
{
    public int width = 10;
    public int height = 10;
    public List<TileData> tiles = new();
}
