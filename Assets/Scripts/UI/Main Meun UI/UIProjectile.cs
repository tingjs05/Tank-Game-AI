using System.Collections;
using UnityEngine;

namespace UI
{
    public class UIProjectile : MonoBehaviour
    {
        public float duration;
        public float moveSpeed;
        public Vector3 moveDirection;
        public Vector3 rotationSpeed;
        private Coroutine coroutine;

        void Start()
        {
            StopAllCoroutines();
            coroutine = StartCoroutine(HideAfterLifetime());
        }

        void Update()
        {
            // move self
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);

            // check if need to restart timer
            if (coroutine != null) return;
            Start();
        }

        IEnumerator HideAfterLifetime()
        {
            yield return new WaitForSeconds(duration);
            gameObject.SetActive(false);
            coroutine = null;
        }

        void OnDrawGizmosSelected() 
        {
            Debug.DrawRay(transform.position, moveDirection.normalized * moveSpeed, Color.red);
        }
    }
}
