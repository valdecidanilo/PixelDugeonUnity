using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

namespace TileMap
{
    public class TileMapLoader : MonoBehaviour
    {
        public Tilemap tilemap;
        public TileMapping[] tileMappings;
        private Dictionary<int, TileBase> _tileDictionary;

        public Action OnFinishLoaderMap;
        
        private void Awake()
        {
            LoadTileDictionaryFromResources();
        }

        private void Start()
        {
            _tileDictionary = tileMappings.ToDictionary(t => t.id, t => t.tile);
        }
/*#if UNITY_EDITOR
        [ContextMenu("🔁 Testar carregamento de mapa JSON")]
        private void TestarCarregamento()
        {
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, "map.json");
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError("map.json não encontrado em StreamingAssets!");
                return;
            }
            var json = System.IO.File.ReadAllText(path);
            LoadMapFromJson(json);
        }
#endif
*/
        private void LoadMapFromJson(string json)
        {
            var mapData = JsonUtility.FromJson<TileMapData>(json);
            for (var y = 0; y < mapData.height; y++)
            {
                for (var x = 0; x < mapData.width; x++)
                {
                    var index = y * mapData.width + x;

                    if (index >= mapData.tiles.Count)
                    {
                        Debug.LogWarning($"Índice fora do intervalo no JSON: {index}");
                        continue;
                    }
                    var tileId = mapData.tiles[index];

                    if (_tileDictionary.TryGetValue(tileId, out var tile))
                    {
                        var pos = new Vector3Int(x, -y, 0);
                        tilemap.SetTile(pos, tile);
                    }
                }
            }
            tilemap.RefreshAllTiles();
        }
        private void LoadTileDictionaryFromResources()
        {
            _tileDictionary = new Dictionary<int, TileBase>();
            var tiles = Resources.LoadAll<TileBase>("Tiles");

            foreach (var tile in tiles)
            {
                var nameTile = tile.name;
                var parts = nameTile.Split('_');

                if (parts.Length > 1 && int.TryParse(parts[^1], out var id))
                {
                    if (_tileDictionary.TryAdd(id, tile))
                    {
                        Debug.Log($"Tile ID {id} -> {nameTile}");
                    }
                    else
                    {
                        Debug.LogWarning($"ID duplicado: {id} em {nameTile}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Tile ignorado: nome mal formatado ({nameTile})");
                }
            }
            Debug.Log($"Tiles carregados: {_tileDictionary.Count}");
        }

        [ContextMenu("Pegar mapa via PHP")]
        public void GetMapServer()
        {
            StartCoroutine(LoadMap());
        }
        private IEnumerator LoadMap()
        {
            var www = UnityWebRequest.Get("https://oxentegames.com.br/map-editor/map.json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao carregar mapa: " + www.error);
            }
            else
            {
                var json = www.downloadHandler.text;
                Debug.Log("Mapa carregado: " + json);
                LoadMapFromJson(json);
                OnFinishLoaderMap?.Invoke();
                
            }
        }
    }
}