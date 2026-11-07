using UnityEngine;
using UnityEditor;
using System.IO;

public class MapDesignerWindow : EditorWindow
{
    private MapData designData;
    private Vector2 scrollPosition;
    private int selectedIndex = -1;
    private string mapName = "NewMapDesign";
    private int mapHeight = 10;
    private int mapWidth = 10;

    [MenuItem("Tools/Map Designer")]
    public static void ShowWindow()
    {
        GetWindow<MapDesignerWindow>("Map Designer");
    }

    private void OnGUI()
    {
        // 1. Assign or create a working file
        designData = (MapData)EditorGUILayout.ObjectField("Current Map:", designData, typeof(MapData), false);

        if (designData == null)
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
                designData = newMap;
            }
            return;
        }

        // Synchronize grid size
        int totalCells = designData.width * designData.height;
        while (designData.tiles.Count < totalCells) designData.tiles.Add(new TileData());

        EditorGUILayout.BeginHorizontal();

        // 2. LEFT SIDE: Draw the Visual Grid Selection Matrix
        EditorGUILayout.BeginVertical(GUILayout.Width(900));
        GUILayout.Label("Click a tile to edit its properties:", EditorStyles.miniLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(900));
        for (int y = designData.height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < designData.width; x++)
            {
                int index = y * designData.width + x;
                designData.tiles[index].x = x;
                designData.tiles[index].y = y;

                // Color code button states
                Color originalColor = GUI.color;
                if (index == selectedIndex) GUI.color = Color.green;

                if (GUILayout.Button($"{x},{y}", GUILayout.Width(45), GUILayout.Height(45)))
                {
                    selectedIndex = index;
                }
                GUI.color = originalColor;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // 3. RIGHT SIDE: Render the Form Fields for the Selected Tile
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (selectedIndex >= 0 && selectedIndex < designData.tiles.Count)
        {
            TileData tile = designData.tiles[selectedIndex];
            GUILayout.Label($"Editing Tile ({tile.x}, {tile.y})", EditorStyles.boldLabel);
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

                EditorGUI.indentLevel--;
            }
            tile.startingUnit = (TileData.StartingUnit)EditorGUILayout.EnumPopup("Unit", tile.startingUnit);
            if (tile.startingUnit != TileData.StartingUnit.None)
            {
                EditorGUI.indentLevel++;

                tile.startingUnitPlayer = EditorGUILayout.IntField("Player:", tile.startingUnitPlayer);

                EditorGUI.indentLevel--;
            }

            // Mark object dirty so Unity updates changes automatically
            EditorUtility.SetDirty(designData);
        }
        else
        {
            GUILayout.Label("Select a tile on the map to configure properties.");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }
}
