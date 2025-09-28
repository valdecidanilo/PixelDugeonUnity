using UnityEngine;

namespace Utils
{
    public class LoadingCircle : MonoBehaviour
    {
        private RectTransform _rectComponent;
        [Range(200f, 1000f)] public float rotateSpeed = 200f;
        public bool isClockWise;

        private void Start()
        {
            _rectComponent = GetComponent<RectTransform>();
        }

        private void FixedUpdate()
        {
            if(isClockWise){
                _rectComponent.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);
            }else{
                _rectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
            }
        }
    }
}