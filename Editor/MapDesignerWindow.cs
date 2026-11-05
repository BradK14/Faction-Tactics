using UnityEngine;
using UnityEditor;
using System.IO;

public class MapDesignerWindow : EditorWindow
{
    private MapDesignContainer designData;
    private Vector2 scrollPosition;
    private int selectedIndex = -1;

    [MenuItem("Tools/Map Designer")]
    public static void ShowWindow()
    {
        GetWindow<MapDesignerWindow>("Map Designer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Map Design Exporter", EditorStyles.boldLabel);

        // 1. Assign or create a working file
        designData = (MapDesignContainer)EditorGUILayout.ObjectField("Design Data Asset", designData, typeof(MapDesignContainer), false);

        if (designData == null)
        {
            if (GUILayout.Button("Create New Map Design Asset"))
            {
                MapDesignContainer newAsset = CreateInstance<MapDesignContainer>();
                AssetDatabase.CreateAsset(newAsset, "Assets/ScriptableObjects/NewMapDesign.asset");
                AssetDatabase.SaveAssets();
                designData = newAsset;
            }
            return;
        }

        // Synchronize grid size
        int totalCells = designData.width * designData.height;
        while (designData.tiles.Count < totalCells) designData.tiles.Add(new SerializableTile());

        EditorGUILayout.BeginHorizontal();

        // 2. LEFT SIDE: Draw the Visual Grid Selection Matrix
        EditorGUILayout.BeginVertical(GUILayout.Width(900));
        GUILayout.Label("Click a cell to edit its properties:", EditorStyles.miniLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(900));
        for (int y = designData.height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < designData.width; x++)
            {
                int index = y * designData.width + x;
                designData.tiles[index].x = x;
                designData.tiles[index].y = y;

                // Color code button states based on passability/type
                Color originalColor = GUI.color;
                if (index == selectedIndex) GUI.color = Color.yellow;
                else if (!designData.tiles[index].isPassable) GUI.color = Color.red;

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
            SerializableTile tile = designData.tiles[selectedIndex];
            GUILayout.Label($"Editing Tile ({tile.x}, {tile.y})", EditorStyles.boldLabel);

            tile.tileName = EditorGUILayout.TextField("Tile Name", tile.tileName);
            tile.terrainType = (byte)EditorGUILayout.IntField("Terrain Type ID", tile.terrainType);
            tile.isPassable = EditorGUILayout.Toggle("Is Passable?", tile.isPassable);

            tile.maxHealth = EditorGUILayout.IntField("Max Health", tile.maxHealth);
            tile.currentHealth = (int)EditorGUILayout.Slider("Current Health", tile.currentHealth, 0, tile.maxHealth);

            tile.factionOwner = EditorGUILayout.TextField("Faction Owner", tile.factionOwner);
            tile.remainingResources = EditorGUILayout.IntField("Resources Remaining", tile.remainingResources);
            tile.trapDamage = EditorGUILayout.IntField("Trap Damage Pool", tile.trapDamage);

            // Mark object dirty so Unity updates changes automatically
            EditorUtility.SetDirty(designData);
        }
        else
        {
            GUILayout.Label("Select a tile on the left grid to configure properties.");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }
}
