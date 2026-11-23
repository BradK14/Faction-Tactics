using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MapDesigner : EditorWindow
{
    private MapData mapData;
    private Vector2 scrollPosition;
    private List<int> selectedIndeces = new();
    private string mapName = "NewMapDesign";
    private int mapHeight = 10;
    private int mapWidth = 10;

    [MenuItem("Tools/Map Designer")]
    public static void ShowWindow()
    {
        GetWindow<MapDesigner>("Map Designer");
    }

    private void OnGUI()
    {
        // Assign or create a working file
        mapData = (MapData)EditorGUILayout.ObjectField("Current Map:", mapData, typeof(MapData), false);

        if (mapData == null)
        {
            // Draw input fields for user configuration
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Map Configuration", EditorStyles.boldLabel);

            mapName = EditorGUILayout.TextField("Map Name", mapName);
            mapWidth = EditorGUILayout.IntField("Width", mapWidth);
            mapHeight = EditorGUILayout.IntField("Height", mapHeight);

            // Ensure the width and height are in a valid (and not rediculous) range
            mapWidth = Mathf.Clamp(mapWidth, 3, 30);
            mapHeight = Mathf.Clamp(mapHeight, 3, 30);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create New Map"))
            {
                // Ensure the target directory exists
                if (!Directory.Exists("Assets/ScriptableObjects"))
                {
                    Directory.CreateDirectory("Assets/ScriptableObjects");
                }

                // Create an empty map
                MapData newMap = CreateInstance<MapData>();

                // Set the map's height and width
                newMap.width = mapWidth;
                newMap.height = mapHeight;

                // Set name, use a default name for a fall back.  
                string safeMapName = string.IsNullOrEmpty(mapName) ? "NewMapDesign" : mapName;

                string mapPath = $"Assets/ScriptableObjects/{safeMapName}.asset";
                mapPath = AssetDatabase.GenerateUniqueAssetPath(mapPath);

                AssetDatabase.CreateAsset(newMap, mapPath);
                AssetDatabase.SaveAssets();
                mapData = newMap;
            }
            return;
        }

        // Synchronize grid size
        int totalCells = mapData.width * mapData.height;
        while (mapData.tiles.Count < totalCells) mapData.tiles.Add(new TileData());

        EditorGUILayout.BeginHorizontal();

        // LEFT SIDE: Draw the Visual Grid Selection Matrix
        EditorGUILayout.BeginVertical(GUILayout.Width(900));

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(900));

        float tileSize = 45f;

        // Custom style to eliminate spacing between tiles
        GUIStyle removedDefaultStyle = new GUIStyle(GUI.skin.box);
        removedDefaultStyle.margin = new RectOffset(0, 0, 0, 0);
        removedDefaultStyle.padding = new RectOffset(0, 0, 0, 0);
        removedDefaultStyle.border = new RectOffset(0, 0, 0, 0);

        for (int y = mapData.height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal(removedDefaultStyle);
            for (int x = 0; x < mapData.width; x++)
            {
                int index = y * mapData.width + x;
                mapData.tiles[index].x = x;
                mapData.tiles[index].y = y;

                // Reserve a fixed-size layout rect for this tile
                Rect tileRect = EditorGUILayout.GetControlRect(false, tileSize, removedDefaultStyle, GUILayout.Width(tileSize));

                // Draw each layer of the preview
                // Terrain layer
                Texture2D groundTex = GetGroundTexFromTile(mapData.tiles[index]);
                if (groundTex != null)
                {
                    GUI.DrawTexture(tileRect, groundTex, ScaleMode.ScaleToFit, true);
                }

                // Cliff layer
                Texture2D cliffTex = GetCliffTexFromTile(mapData.tiles[index]);
                if (cliffTex != null)
                {
                    GUI.DrawTexture(tileRect, cliffTex, ScaleMode.ScaleToFit, true);
                }

                // Resource layer
                Texture2D resourceTex = GetResourceTexFromTile(mapData.tiles[index]);
                if (resourceTex != null)
                {
                    GUI.DrawTexture(tileRect, resourceTex, ScaleMode.ScaleToFit, true);
                }
                

                // Structure layer
                Texture2D structureTex = GetStructureTexFromTile(mapData.tiles[index]);
                if (structureTex != null)
                {
                    GUI.DrawTexture(tileRect, structureTex, ScaleMode.ScaleToFit, true);
                }

                // Unit layer
                Texture2D unitTex = GetUnitTexFromTile(mapData.tiles[index]);
                if (unitTex != null)
                {
                    GUI.DrawTexture(tileRect, unitTex, ScaleMode.ScaleToFit, true);
                }

                // Selection highlight layer
                Texture2D highlightTex = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Images/Other/GameCursor.png").texture;
                if (selectedIndeces.Contains(index))
                {
                    GUI.DrawTexture(tileRect, highlightTex, ScaleMode.ScaleToFit, true);
                }

                // Tile Selection
                Event e = Event.current;
                if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown))
                {
                    if (e.button == 1)
                    {
                        selectedIndeces.Clear();
                        GUI.FocusControl(null);
                        e.Use();
                    }
                    else if (e.button == 0 && tileRect.Contains(e.mousePosition) && !selectedIndeces.Contains(index))
                    {
                        selectedIndeces.Add(index);
                        GUI.FocusControl(null);
                        e.Use();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // RIGHT SIDE: Render the Form Fields for the Selected Tile
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (selectedIndeces.Count == 1)
        {
            TileData tile = mapData.tiles[selectedIndeces[0]];
            GUILayout.Label($"Editing Tile ({tile.x}, {tile.y})", EditorStyles.boldLabel);
            tile.elevation = EditorGUILayout.IntField("Elevation", tile.elevation);
            tile.elevation = Mathf.Clamp(tile.elevation, 1, 5);
            tile.cliffNorth = EditorGUILayout.Toggle("Cliff North", tile.cliffNorth);
            tile.cliffEast = EditorGUILayout.Toggle("Cliff East", tile.cliffEast);
            tile.cliffSouth = EditorGUILayout.Toggle("Cliff South", tile.cliffSouth);
            tile.cliffWest = EditorGUILayout.Toggle("Cliff West", tile.cliffWest);
            tile.resourceType = (TileData.ResourceType)EditorGUILayout.EnumPopup("Resource Type", tile.resourceType);
            tile.startingStructure = (TileData.StartingStructure)EditorGUILayout.EnumPopup("Structure", tile.startingStructure);
            if (tile.startingStructure != TileData.StartingStructure.None)
            {
                EditorGUI.indentLevel++;

                tile.startingStructurePlayer = EditorGUILayout.IntField("Player:", tile.startingStructurePlayer);
                tile.startingStructurePlayer = Mathf.Clamp(tile.startingStructurePlayer, 1, 2);

                EditorGUI.indentLevel--;
            }
            else
            {
                tile.startingStructurePlayer = 1;
            }
            tile.startingUnit = (TileData.StartingUnit)EditorGUILayout.EnumPopup("Unit", tile.startingUnit);
            if (tile.startingUnit != TileData.StartingUnit.None)
            {
                EditorGUI.indentLevel++;

                tile.startingUnitPlayer = EditorGUILayout.IntField("Player:", tile.startingUnitPlayer);
                tile.startingUnitPlayer = Mathf.Clamp(tile.startingUnitPlayer, 1, 2);

                EditorGUI.indentLevel--;
            }
            else
            {
                tile.startingUnitPlayer = 1;
            }

            // Mark object dirty so Unity updates changes automatically
            EditorUtility.SetDirty(mapData);
        }
        else if (selectedIndeces.Count > 0)
        {
            TileData tile = mapData.tiles[selectedIndeces[0]];  // First tile used as an example in input fields
            TileData prevTile = tile;  // This is only used in the next loop for checking for multiple values
            string tileCoordinates = "";
            bool multElevation = false;
            bool multCN = false;
            bool multCE = false;
            bool multCS = false;
            bool multCW = false;
            bool multResourceType = false;
            bool multStructure = false;
            bool multStructurePlayer = false;
            bool multUnit = false;
            bool multUnitPlayer = false;
            bool anyStructures = false;
            bool anyUnits = false;
            foreach (int ind in selectedIndeces)
            {
                TileData t = mapData.tiles[ind];
                tileCoordinates += "(" + t.x + "," + t.y + ")";

                // Check for multiple different types of each map element across selected tiles
                if (t.elevation != prevTile.elevation) multElevation = true;
                if (t.cliffNorth != prevTile.cliffNorth) multCN = true;
                if (t.cliffEast != prevTile.cliffEast) multCE = true;
                if (t.cliffSouth != prevTile.cliffSouth) multCS = true;
                if (t.cliffWest != prevTile.cliffWest) multCW = true;
                if (t.resourceType != prevTile.resourceType) multResourceType = true;
                if (t.startingStructure != prevTile.startingStructure) multStructure = true;
                if (t.startingStructurePlayer != prevTile.startingStructurePlayer) multStructurePlayer = true;
                if (t.startingUnit != prevTile.startingUnit) multUnit = true;
                if (t.startingUnitPlayer != prevTile.startingUnitPlayer) multUnitPlayer = true;
                if (t.startingStructure != TileData.StartingStructure.None) anyStructures = true;
                if (t.startingUnit != TileData.StartingUnit.None) anyUnits = true;

                prevTile = t;
            }
            GUILayout.Label($"Editing Selected Tiles: {tileCoordinates}", EditorStyles.boldLabel);  // Display coordinates of all selected tiles

            // Elevation
            if (multElevation) EditorGUI.showMixedValue = true;
            EditorGUI.BeginChangeCheck();
            int elevation = Mathf.Clamp(EditorGUILayout.IntField("Elevation", tile.elevation), 1, 5);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (int ind in selectedIndeces)
                    mapData.tiles[ind].elevation = elevation;
            }
            EditorGUI.showMixedValue = false;

            // Cliff North
            if (multCN) EditorGUI.showMixedValue = true;
            EditorGUI.BeginChangeCheck();
            bool cliffNorth = EditorGUILayout.Toggle("Cliff North", tile.cliffNorth);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (int ind in selectedIndeces)
                    mapData.tiles[ind].cliffNorth = cliffNorth;
            }
            EditorGUI.showMixedValue = false;

            // Cliff East
            if (multCE) EditorGUI.showMixedValue = true;
            EditorGUI.BeginChangeCheck();
            bool cliffEast = EditorGUILayout.Toggle("Cliff East", tile.cliffEast);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (int ind in selectedIndeces)
                    mapData.tiles[ind].cliffEast = cliffEast;
            }
            EditorGUI.showMixedValue = false;

            // Cliff South
            if (multCS) EditorGUI.showMixedValue = true;
            EditorGUI.BeginChangeCheck();
            bool cliffSouth = EditorGUILayout.Toggle("Cliff South", tile.cliffSouth);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (int ind in selectedIndeces)
                    mapData.tiles[ind].cliffSouth = cliffSouth;
            }
            EditorGUI.showMixedValue = false;

            // Cliff West
            if (multCW) EditorGUI.showMixedValue = true;
            EditorGUI.BeginChangeCheck();
            bool cliffWest = EditorGUILayout.Toggle("Cliff West", tile.cliffWest);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (int ind in selectedIndeces)
                    mapData.tiles[ind].cliffWest = cliffWest;
            }
            EditorGUI.showMixedValue = false;

            // Resource Type
            if (multResourceType) EditorGUI.showMixedValue = true;
            EditorGUI.BeginChangeCheck();
            TileData.ResourceType resourceType = (TileData.ResourceType)EditorGUILayout.EnumPopup("Resource Type", tile.resourceType);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (int ind in selectedIndeces)
                    mapData.tiles[ind].resourceType = resourceType;
            }
            EditorGUI.showMixedValue = false;

            // Starting Structure
            if (multStructure) EditorGUI.showMixedValue = true;
            EditorGUI.BeginChangeCheck();
            TileData.StartingStructure startingStructure = (TileData.StartingStructure)EditorGUILayout.EnumPopup("Structure", tile.startingStructure);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (int ind in selectedIndeces)
                {
                    mapData.tiles[ind].startingStructure = startingStructure;
                    if (startingStructure == TileData.StartingStructure.None) mapData.tiles[ind].startingStructurePlayer = 1;
                }
                    
            }
            EditorGUI.showMixedValue = false;

            // Starting Structure Player
            if (anyStructures)
            {
                EditorGUI.indentLevel++;
                if (multStructurePlayer) EditorGUI.showMixedValue = true;
                EditorGUI.BeginChangeCheck();
                int startingStructurePlayer = Mathf.Clamp(EditorGUILayout.IntField("Player:", tile.startingStructurePlayer), 1, 2);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (int ind in selectedIndeces)
                        mapData.tiles[ind].startingStructurePlayer = startingStructurePlayer;
                }
                EditorGUI.showMixedValue = false;
                EditorGUI.indentLevel--;
            }

            // Starting Unit
            if (multUnit) EditorGUI.showMixedValue = true;
            EditorGUI.BeginChangeCheck();
            TileData.StartingUnit startingUnit = (TileData.StartingUnit)EditorGUILayout.EnumPopup("Unit", tile.startingUnit);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (int ind in selectedIndeces)
                {
                    mapData.tiles[ind].startingUnit = startingUnit;
                    if (startingUnit == TileData.StartingUnit.None) mapData.tiles[ind].startingUnitPlayer = 1;
                }
                    
            }
            EditorGUI.showMixedValue = false;

            // Starting Unit Player
            if (anyUnits)
            {
                EditorGUI.indentLevel++;
                if (multUnitPlayer) EditorGUI.showMixedValue = true;
                EditorGUI.BeginChangeCheck();
                int startingUnitPlayer = Mathf.Clamp(EditorGUILayout.IntField("Player:", tile.startingUnitPlayer), 1, 2);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (int ind in selectedIndeces)
                        mapData.tiles[ind].startingUnitPlayer = startingUnitPlayer;
                }
                EditorGUI.showMixedValue = false;
                EditorGUI.indentLevel--;
            }

            // Mark object dirty so Unity updates changes automatically
            EditorUtility.SetDirty(mapData);
        }
        else
        {
            GUILayout.Label("Select tiles on the map to configure their properties.");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private Texture2D GetGroundTexFromTile(TileData tile)
    {
        Texture2D tex = null;

        tex = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Images/Terrain/Ground{tile.elevation}.png").texture;

        return tex;
    }

    private Texture2D GetCliffTexFromTile(TileData tile)
    {
        Texture2D tex = null;

        string n = (tile.cliffNorth) ? "N" : "";
        string e = (tile.cliffEast) ? "E" : "";
        string s = (tile.cliffSouth) ? "S" : "";
        string w = (tile.cliffWest) ? "W" : "";
        string all = n + e + s + w;

        if (all != "")
        {
            tex = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Images/Terrain/Cliff{all}.png").texture;
        }

        return tex;
    }

    private Texture2D GetResourceTexFromTile(TileData tile)
    {
        Texture2D tex = null;

        if (tile.resourceType != TileData.ResourceType.None)
        {
            tex = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Images/Terrain/{tile.resourceType}Resource.png").texture;
        }

        return tex;
    }

    private Texture2D GetStructureTexFromTile(TileData tile)
    {
        Texture2D tex = null;

        if (tile.startingStructure != TileData.StartingStructure.None)
        {
            tex = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Images/Structures/{tile.startingStructure}{tile.startingStructurePlayer}-1.png").texture;
        }

        return tex;
    }

    private Texture2D GetUnitTexFromTile(TileData tile)
    {
        Texture2D tex = null;

        if (tile.startingUnit != TileData.StartingUnit.None)
        {
            tex = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Images/Units/{tile.startingUnit}{tile.startingUnitPlayer}-1.png").texture;
        }

        return tex;
    }
}
