using System.Collections;
using UnityEngine;
using EasyButtons;

namespace UI
{
    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public class CameraPositionController : MonoBehaviour
    {
        public Vector3 startPos, endPos, targetPoint;
        public float moveSpeed = 0.5f;
        public float stopThreshold = 0.01f;
        public bool inverseDirection = false;

        private Coroutine moveCoroutine = null;

        void Update()
        {
            transform.LookAt(targetPoint);
        }

        [Button]
        void TeleportToStart()
        {
            transform.position = startPos;
        }

        [Button]
        void TeleportToEnd()
        {
            transform.position = endPos;
        }

        [Button]
        public void MoveToStart()
        {
            StopMove();
            moveCoroutine = StartCoroutine(MoveToTarget(true));
        }

        [Button]
        public void MoveToEnd()
        {
            StopMove();
            moveCoroutine = StartCoroutine(MoveToTarget(false));
        }

        [Button]
        void StopMove()
        {
            if (moveCoroutine == null) return;
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        IEnumerator MoveToTarget(bool to_start)
        {
            Vector3 midPoint = new Vector3(
                    (startPos.x + endPos.x) / 2f, 
                    (startPos.y + endPos.y) / 2f, 
                    (startPos.z + endPos.z) / 2f
                );
            Vector3 direction;

            while (Vector3.Distance(transform.position, (to_start ? startPos : endPos)) > stopThreshold)
            {
                direction = (midPoint - transform.position).normalized;
                direction = new Vector3(direction.z * (inverseDirection ? -1f : 1f), direction.y, 
                    direction.x * (inverseDirection ? 1f : -1f));// * (to_start ? 1 : -1);

                Debug.DrawRay(transform.position, direction, Color.red);

                transform.position += direction * moveSpeed * Time.deltaTime;

                yield return null;
            }

            transform.position = to_start ? startPos : endPos;
            moveCoroutine = null;
        }

        void OnDrawGizmos() 
        {
            Gizmos.DrawSphere(targetPoint, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPos, 0.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPos, 0.05f);
        }
    }
}
