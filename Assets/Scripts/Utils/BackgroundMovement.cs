using DG.Tweening;
using UnityEngine;

namespace Utils
{
    public class BackgroundMovement : MonoBehaviour
    {
        public RectTransform bg1;
        public RectTransform bg2;
        public float speed = 100f;

        private const float ResetY = -1280f;
        private const float TopY = 1280f;

        private void Start()
        {
            MoveLoop(bg1);
            MoveLoop(bg2, true);
            
        }

        private void MoveLoop(RectTransform bg, bool offset = false)
        {
            if (offset)
                bg.anchoredPosition = new Vector2(0, ResetY);

            MoveNext(bg);
        }

        private void MoveNext(RectTransform bg)
        {
            var currentY = bg.anchoredPosition.y;
            var distance = TopY - currentY;

            var duration = Mathf.Abs(distance) / speed;

            bg.DOAnchorPosY(TopY, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    bg.anchoredPosition = new Vector2(0, ResetY);
                    MoveNext(bg);
                });
        }
    }
}