using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace Utils
{
    public class DebrisManager : MonoBehaviour
    {
        [Header("Detritos")]
        public GameObject[] detritos;
        public Sprite[] spritePool;

        [Header("Configurações")]
        public float speed = 100f;
        public float resetY = -1280f;
        public float topY = 1280f;
        public float minX = -300f;
        public float maxX = 300f;

        private void Start()
        {
            foreach (var debris in detritos)
            {
                InitDebris(debris);
            }
        }

        private void InitDebris(GameObject obj)
        {
            var rect = obj.GetComponent<RectTransform>();
            var image = obj.GetComponent<Image>();
            AnimateDebris(rect, image);
        }

        private void ResetDebris(RectTransform rect, Image image)
        {
            var randomX = Random.Range(minX, maxX);
            rect.anchoredPosition = new Vector2(randomX, resetY);

            if (spritePool != null && spritePool.Length > 0 && image != null)
            {
                image.sprite = spritePool[Random.Range(0, spritePool.Length)];
            }
        }

        private void AnimateDebris(RectTransform rect, Image image)
        {
            var distance = topY - rect.anchoredPosition.y;
            var duration = Mathf.Abs(distance) / speed;

            rect.DOAnchorPosY(topY, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    ResetDebris(rect, image);
                    AnimateDebris(rect, image); // Recursivo
                });
        }
    }
}