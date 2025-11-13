using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace RuntimeLoader
{
    public class RuntimeSceneLoader : MonoBehaviour
    {
        private const string URL = "https://oxentegames.com.br/";
        
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private float delayToNextStep = 0.4f;

        private float _timer;
        private int _dotCount = 0;
        private const string BaseText = "Carregando";
        private static string _currentGameId; // ✅ Static para usar no Transform

        private void Start()
        {
            LoadAdditiveScene();
            loadingScreen.SetActive(true);
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= delayToNextStep)
            {
                _timer = 0f;
                _dotCount = (_dotCount + 1) % 4;
                loadingText.text = BaseText + new string('.', _dotCount);
            }
        }

        private void LoadAdditiveScene()
        {
            var url = Application.absoluteURL;
            var bundleId = GetUrlParam(url, "bundle");
            
#if UNITY_EDITOR
            bundleId = "gold";
#endif

            _currentGameId = bundleId; // ✅ Salvar gameId para uso global
            
            Debug.Log($"[Init] GameId definido: {_currentGameId}");

            StartCoroutine(DownloadBundle(bundleId, () => {
                loadingScreen.SetActive(false);
            }));
        }

        private static string GetUrlParam(string url, string param)
        {
            if (!url.Contains("?"))
                return null;

            var query = url.Split('?')[1];
            var parts = query.Split('&');

            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length == 2 && kv[0] == param)
                    return kv[1];
            }

            return null;
        }

        private IEnumerator DownloadBundle(string gameId, Action callback = null)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                Debug.LogError("❌ GameId é nulo ou vazio");
                yield break;
            }

            // ✅ Limpar cache
            Debug.Log("🧹 [Cache] Limpando cache do Addressables...");
            
            yield return Addressables.ClearDependencyCacheAsync((object) null, false);

            // ✅ Buscar lista de arquivos
            var request = UnityWebRequest.Get($"{URL}remote/getscene.php?bundle={gameId}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Erro ao buscar lista de bundles: {request.error}");
                yield break;
            }

            // ✅ Fazer download de todos os arquivos
            var data = JsonUtility.FromJson<BundleList>(request.downloadHandler.text);
            Debug.Log($"📦 [Download] {data.files.Length} arquivos para baixar");
            
            foreach (var file in data.files)
            {
                var dl = UnityWebRequest.Get($"{URL}remote/{gameId}/{file}");
                yield return dl.SendWebRequest();
                
                if (dl.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"❌ Erro ao baixar {file}: {dl.error}");
                else
                    Debug.Log($"✅ Download: {file}");
                    
                dl.Dispose();
            }

            // ✅ REGISTRAR TRANSFORM FUNCTION (antes de carregar catálogo)
            Addressables.InternalIdTransformFunc = TransformInternalId;
            Debug.Log("🔄 [Transform] Função de transformação registrada");

            // ✅ Carregar catálogo
            var catalogUrl = $"{URL}remote/{gameId}/catalog_1.0.2.json";
            Debug.Log($"📚 [Catalog] Carregando: {catalogUrl}");
            
            var catalogHandle = Addressables.LoadContentCatalogAsync(catalogUrl, false);
            yield return catalogHandle;

            if (catalogHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("❌ Falha ao carregar catálogo");
                yield break;
            }

            Debug.Log("✅ [Catalog] Catálogo carregado com sucesso");
            yield return null; // Aguardar um frame

            // ✅ Carregar cenas
            var scenes = new[] { "layer0", "layer1", "layer2" };
            foreach (var sceneName in scenes)
            {
                Debug.Log($"🎬 [Scene] Carregando: {sceneName}");
                var sceneHandle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                yield return sceneHandle;

                if (sceneHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"❌ Falha ao carregar: {sceneName}");
                    if (sceneHandle.OperationException != null)
                        Debug.LogError($"Exception: {sceneHandle.OperationException}");
                }
                else
                {
                    Debug.Log($"✅ [Scene] {sceneName} carregada!");
                }
            }

            callback?.Invoke();
            Addressables.Release(catalogHandle);
            request.Dispose();
            
            Debug.Log("🎉 [Complete] Todas as cenas foram processadas!");
        }

        // ✅ TRANSFORM FUNCTION - Intercepta todos os paths de bundles
        private static string TransformInternalId(
            UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation location)
        {
            string id = location.InternalId;

            // Verificar se é um bundle remoto
            if (id.Contains("oxentegames.com.br") && id.Contains(".bundle"))
            {
                // Extrair apenas o nome do arquivo
                string fileName = System.IO.Path.GetFileName(id);
                
                // ✅ CONSTRUIR PATH COM GAMEID
                string newPath = $"https://oxentegames.com.br/remote/{_currentGameId}/{fileName}";
                
                Debug.Log($"🔄 [Transform] {fileName} → remote/{_currentGameId}/{fileName}");
                return newPath;
            }

            // Retornar path original se não for bundle
            return id;
        }
    }
}
