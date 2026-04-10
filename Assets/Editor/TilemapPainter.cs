// Editor-only tool: Tools → Paint Scene Tiles
// Run once per tileset to create Tile assets from PixelArtLibrary sprites.
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapPainterWindow : EditorWindow
{
    Tilemap _tilemap;
    TileID  _selectedTile = TileID.FloorWood;

    [MenuItem("Tools/Paint Scene Tiles")]
    static void Open() => GetWindow<TilemapPainterWindow>("Paint Tiles");

    void OnGUI()
    {
        GUILayout.Label("Tile Asset Generator", EditorStyles.boldLabel);
        GUILayout.Space(4);

        _tilemap      = (Tilemap)EditorGUILayout.ObjectField(
            "Target Tilemap", _tilemap, typeof(Tilemap), true);
        _selectedTile = (TileID)EditorGUILayout.EnumPopup("Tile Type", _selectedTile);

        GUILayout.Space(8);

        if (GUILayout.Button("Create Tile Asset"))
            CreateTileAsset(_selectedTile);

        GUILayout.Space(4);

        if (GUILayout.Button("Create ALL Tile Assets"))
            foreach (TileID id in System.Enum.GetValues(typeof(TileID)))
                CreateTileAsset(id);
    }

    static void CreateTileAsset(TileID id)
    {
        string dir  = "Assets/Art/Tiles";
        string path = $"{dir}/{id}.asset";
        Directory.CreateDirectory(dir);

        // Reuse existing asset if present
        var existing = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (existing != null)
        {
            existing.sprite = PixelArtLibrary.BuildTile(id);
            EditorUtility.SetDirty(existing);
            Debug.Log($"Updated tile asset: {path}");
        }
        else
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = PixelArtLibrary.BuildTile(id);
            AssetDatabase.CreateAsset(tile, path);
            Debug.Log($"Created tile asset: {path}");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
