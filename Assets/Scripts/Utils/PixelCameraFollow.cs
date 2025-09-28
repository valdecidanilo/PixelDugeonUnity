using UnityEngine;

namespace Utils
{
    public class PixelCameraFollow : MonoBehaviour
    {
        public Transform target;
        public float followDelay = 0.1f;
        public float followSpeed = 5f;

        private Vector3 _targetPosition;
        private float _delayTimer = 0f;
        private bool _waitingToMove = false;

        private Vector3 _lastTargetPos;

        private void Start()
        {
            if (target == null) return;

            _lastTargetPos = target.position;

            var startPosition = target.position;
            startPosition.z = transform.position.z;
            transform.position = startPosition;

            _targetPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            if (target.position != _lastTargetPos)
            {
                _waitingToMove = true;
                _delayTimer = followDelay;
                _lastTargetPos = target.position;
            }

            if (_waitingToMove)
            {
                _delayTimer -= Time.deltaTime;

                if (_delayTimer <= 0f)
                {
                    _targetPosition = new Vector3(
                        target.position.x,
                        target.position.y,
                        transform.position.z 
                    );
                    _waitingToMove = false;
                }
            }

            transform.position = Vector3.Lerp(transform.position, _targetPosition, followSpeed * Time.deltaTime);
        }
    }
}