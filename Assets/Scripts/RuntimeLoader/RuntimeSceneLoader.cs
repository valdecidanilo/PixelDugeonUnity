using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
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
            bundleId = "tower";
#endif
            StartCoroutine(DownloadBundle(bundleId, () =>
            {
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
        private static IEnumerator DownloadBundle(string gameId, Action callback = null)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                Debug.LogError("GameId é nulo ou vazio");
                yield break;
            }

            var request = UnityWebRequest.Get($"{URL}remote/getscene.php?bundle={gameId}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Erro ao baixar bundle: {request.error}");
                yield break;
            }

            var data = JsonUtility.FromJson<BundleList>(request.downloadHandler.text);

            foreach (var file in data.files)
            {
                var dl = UnityWebRequest.Get($"{URL}remote/{gameId}/" + file);
                yield return dl.SendWebRequest();
        
                if (dl.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Erro ao baixar arquivo {file}: {dl.error}");
                }
                dl.Dispose();
            }

            var catalogHandle = Addressables.LoadContentCatalogAsync(
                $"https://oxentegames.com.br/remote/{gameId}/catalog_1.0.2.json"
            );
            yield return catalogHandle;

            if (catalogHandle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Falha ao carregar catálogo");
                yield break;
            }

            var scenes = new[] { "layer0", "layer1", "layer2" };
            foreach (var sceneName in scenes)
            {
                var sceneHandle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                yield return sceneHandle;
        
                if (sceneHandle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"Falha ao carregar cena: {sceneName}");
                }
            }

            callback?.Invoke();
    
            Addressables.Release(catalogHandle);
            request.Dispose();
        }
    }
}
