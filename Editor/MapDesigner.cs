using UnityEngine;
using UnityEditor;
using System.IO;

public class MapDesigner : EditorWindow
{
    private MapData mapData;
    private Vector2 scrollPosition;
    private int selectedIndex = -1;
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
        GUILayout.Label("Click a tile to edit its properties:", EditorStyles.miniLabel);

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
                Sprite terrainSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Terrain/Ground1-1.png");
                Texture2D terrainTex = terrainSprite.texture;
                if (mapData.tiles[index].elevation == 1)
                {
                    //EditorGUI.DrawPreviewTexture(tileRect, terrainTex, null, ScaleMode.ScaleToFit, 0);
                    GUI.DrawTexture(tileRect, terrainTex, ScaleMode.ScaleToFit, true);
                }

                // Cliff layer
                // You can check data here to decide which texture to draw
                //Texture2D cliffTex = Resources.Load<Texture2D>("Tiles/TreeOverlay");
                //if (cliffTex != null)
                //{
                //    GUI.DrawTexture(tileRect, cliffTex);
                //}

                // Resource layer
                // Structure layer
                // Unit layer
                // Selection highlight layer
                /*
                if (index == selectedIndex)
                {
                    Texture2D selectTex = Resources.Load<Texture2D>("UI/GreenHighlightOverlay");
                    if (selectTex != null)
                    {
                        GUI.DrawTexture(tileRect, selectTex);
                    }
                }
                // Color code button states
                
                Color originalColor = GUI.color;
                if (index == selectedIndex) GUI.color = Color.green;

                if (GUILayout.Button($"{x},{y}", tileStyle, GUILayout.Width(45), GUILayout.Height(45)))
                {
                    selectedIndex = index;
                }
                GUI.color = originalColor;
                */

                if (GUI.Button(tileRect, GUIContent.none, GUIStyle.none))
                {
                    selectedIndex = index;
                }
                
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // RIGHT SIDE: Render the Form Fields for the Selected Tile
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (selectedIndex >= 0 && selectedIndex < mapData.tiles.Count)
        {
            TileData tile = mapData.tiles[selectedIndex];
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
            tile.startingUnit = (TileData.StartingUnit)EditorGUILayout.EnumPopup("Unit", tile.startingUnit);
            if (tile.startingUnit != TileData.StartingUnit.None)
            {
                EditorGUI.indentLevel++;

                tile.startingUnitPlayer = EditorGUILayout.IntField("Player:", tile.startingUnitPlayer);
                tile.startingUnitPlayer = Mathf.Clamp(tile.startingUnitPlayer, 1, 2);

                EditorGUI.indentLevel--;
            }

            // Mark object dirty so Unity updates changes automatically
            EditorUtility.SetDirty(mapData);
        }
        else
        {
            GUILayout.Label("Select a tile on the map to configure properties.");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }
}
