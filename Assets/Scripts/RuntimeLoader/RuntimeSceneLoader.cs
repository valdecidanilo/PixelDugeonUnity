using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace RuntimeLoader
{
    public class RuntimeSceneLoader : MonoBehaviour
    {
        private const string URL = "https://oxentegames.com.br/remote/";
        [SerializeField] private GameObject loadingScreen;
        private void Start()
        {
            LoadAdditiveScene();
            loadingScreen.SetActive(true);
        }
        private void LoadAdditiveScene()
        {
            var url = Application.absoluteURL;
            var bundleId = GetUrlParam(url, "bundle");
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
            var request = UnityWebRequest.Get($"{URL}getscene.php?bundle={gameId}");
            yield return request.SendWebRequest();

            var data = JsonUtility.FromJson<BundleList>(request.downloadHandler.text);

            foreach (var file in data.files)
            {
                var dl = UnityWebRequest.Get($"{URL}/remote" + file);
                yield return dl.SendWebRequest();
            }

            yield return Addressables.InitializeAsync();
            callback?.Invoke();
            var scenes = new [] { $"layer0", $"layer1", $"layer2" };

            foreach (var s in scenes)
            {
                Addressables.LoadSceneAsync(s, LoadSceneMode.Additive);
            }
        }
    }
}
