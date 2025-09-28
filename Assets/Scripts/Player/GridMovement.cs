using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utils
{
    public class GridMovement : MonoBehaviour
    {
        public float tileSize = .16f;
        public float moveTime = 0.1f;

        private Vector2Int moveInput = Vector2Int.zero;
        private bool isMoving = false;
        private Vector3 targetPos;

        private void Start()
        {
            targetPos = transform.position;
        }

        private void Update()
        {
            if (!isMoving && moveInput != Vector2Int.zero)
            {
                TryMove(moveInput);
                moveInput = Vector2Int.zero;
            }
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            var raw = ctx.ReadValue<Vector2>();
            moveInput = new Vector2Int(Mathf.RoundToInt(raw.x), Mathf.RoundToInt(raw.y));
        }

        private void TryMove(Vector2Int dir)
        {
            var newPos = transform.position + new Vector3(dir.x, dir.y, 0) * tileSize;
            StartCoroutine(MoveTo(newPos));
        }

        private IEnumerator MoveTo(Vector3 dest)
        {
            isMoving = true;

            var start = transform.position;
            var elapsed = 0f;

            while (elapsed < moveTime)
            {
                transform.position = Vector3.Lerp(start, dest, elapsed / moveTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = dest;
            isMoving = false;
        }
    }
}