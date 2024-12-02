using System.Collections;
using UnityEngine;
using EasyButtons;
using UnityEngine.Events;

namespace UI
{
    [RequireComponent(typeof(Collider))]
    public class Button3D : MonoBehaviour
    {
        public Transform targetTransform;
        public Vector3 floatingPosition;
        public Vector3 floatingRotation;
        public float floatSpeed = 0.5f;

        [Space] 
        public UnityEvent OnClick;

        private Vector3? originalPosition = null;
        private Quaternion? originalRotation = null;
        private Coroutine moveCoroutine = null;

        void Awake()
        {
            SetOriginalTransform();
        }

        void OnMouseEnter() 
        {
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveTo(floatingPosition, 
                Quaternion.Euler(floatingRotation.x, floatingRotation.y, floatingRotation.z)));
        }

        void OnMouseOver()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            OnClick?.Invoke();
        }

        void OnMouseExit() 
        {
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveTo((Vector3) originalPosition, (Quaternion) originalRotation));
        }
        
        [Button]
        void ApplyOriginalPositionToFloatingPosition()
        {
            floatingPosition = targetTransform.position;
            floatingRotation = new Vector3(targetTransform.eulerAngles.x, targetTransform.eulerAngles.y, targetTransform.eulerAngles.z);
        }

        [Button]
        void TestFloatPosition()
        {
            if (originalPosition == null || originalRotation == null)
                SetOriginalTransform();
            
            targetTransform.position = floatingPosition;
            targetTransform.rotation = Quaternion.Euler(floatingRotation.x, floatingRotation.y, floatingRotation.z);
        }

        [Button]
        public void SetOriginalTransform()
        {
            originalPosition = targetTransform.position;
            originalRotation = targetTransform.rotation;
        }

        [Button]
        public void ResetPosition()
        {
            if (originalPosition == null || originalRotation == null) return;
            targetTransform.position = (Vector3) originalPosition;
            targetTransform.rotation = (Quaternion) originalRotation;
        }

        IEnumerator MoveTo(Vector3 targetPos, Quaternion targetRot)
        {
            float timeElasped = 0f;

            while (timeElasped < floatSpeed)
            {
                targetTransform.position = Vector3.Lerp(targetTransform.position, targetPos, timeElasped);
                targetTransform.rotation = Quaternion.Lerp(targetTransform.rotation, targetRot, timeElasped);
                timeElasped += Time.deltaTime;
                yield return timeElasped;
            }

            targetTransform.position = targetPos;
            targetTransform.rotation = targetRot;
            moveCoroutine = null;
        }
    }
}
