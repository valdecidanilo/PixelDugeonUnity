using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMap.Editor
{
    [CustomEditor(typeof(TileMapLoader))]
    public class TileMapLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var loader = (TileMapLoader)target;

            if (GUILayout.Button("🔄 Preencher tileMappings automaticamente"))
            {
                var tiles = Resources.LoadAll<TileBase>("Tiles");

                var mappings = tiles.Select(t =>
                    {
                        string[] parts = t.name.Split('_');
                        int id = -1;

                        if (parts.Length > 1 && int.TryParse(parts[^1], out id))
                        {
                            return new TileMapping { id = id, tile = t };
                        }
                        Debug.LogWarning($"Ignorado: nome inválido para ID → {t.name}");
                        return null;
                    })
                    .Where(m => m != null)
                    .OrderBy(m => m.id)
                    .ToArray();

                loader.tileMappings = mappings;
                EditorUtility.SetDirty(loader);
            }
        }
    }
}
