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
        public float smoothDistance = 0.5f;
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
            int dirScale = (inverseDirection ? -1 : 1) * (!to_start ? 1 : -1);
            float totalDistance = Vector3.Distance(startPos, endPos);
            float remainingDistance = totalDistance;
            float speedScale = 1f;

            Vector3 midPoint = new Vector3(
                    (startPos.x + endPos.x) / 2f, 
                    (startPos.y + endPos.y) / 2f, 
                    (startPos.z + endPos.z) / 2f
                );
            
            // calculate vertical vector
            Vector3 direction = (endPos - startPos).normalized;
            Vector3 prepDir = new Vector3(direction.z, direction.y, -direction.x);
            Vector3 upVector = Vector3.Cross(direction, prepDir) * dirScale;

            while (remainingDistance > stopThreshold)
            {
                remainingDistance = Vector3.Distance(transform.position, (to_start ? startPos : endPos));
                if (remainingDistance <= smoothDistance) speedScale = remainingDistance / totalDistance;
                transform.RotateAround(midPoint, upVector, Time.deltaTime * moveSpeed * speedScale);
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
