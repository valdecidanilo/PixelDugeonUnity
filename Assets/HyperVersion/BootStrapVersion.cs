// AUTO-GERADO PELO HYPERVERSION
using UnityEngine;
using HyperVersion.Core;

namespace HyperVersion.Runtime
{
    internal class BootStrapVersion : MonoBehaviour
    {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void EditorPing()
        {
            Debug.Log("[BootStrapVersion] Forçando inclusão de CustomVersion.Core.");
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RuntimePing()
        {
            Debug.Log("[BootStrapVersion] Inclusão do Hyperversion.");
            var dummy = new VersionData
            {
                release = "0",
                build = "0",
                data = "0",
                environment = "dev"
            };
            Debug.Log($"[BootStrapVersion] Dummy version: {dummy.release}.{dummy.build}");
        }
    }
}
