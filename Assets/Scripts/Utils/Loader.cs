using TileMap;
using UnityEngine;

namespace Utils
{
    public class Loader : MonoBehaviour
    {
        private readonly Checklist _currentChecklist = new(1);
        
        [SerializeField] private TileMapLoader tileMapLoader;
        public GameObject menuScreen;
        public GameObject selectCaracterScreen;
        public GameObject loaderScreen;
        public GameObject gameScreen;
        private void Awake()
        {
            tileMapLoader.OnFinishLoaderMap += _currentChecklist.FinishStep;
            _currentChecklist.OnCompleted += OnFinishLoader;
        }

        public void StartGetMap()
        {
            loaderScreen.SetActive(true);
            selectCaracterScreen.SetActive(false);
            tileMapLoader.GetMapServer();
        }
        private void OnFinishLoader()
        {
            loaderScreen.SetActive(false);
            gameScreen.SetActive(true);
        }
    }
}