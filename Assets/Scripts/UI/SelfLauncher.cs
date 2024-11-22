using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class SelfLauncher : MonoBehaviour
    {
        public GameObject prefab;
        public Vector3 launchDirection;
        public float launchForce;
        public bool isKinematic = true;

        private List<Copy> objectPool = new List<Copy>();
        private class Copy
        {
            public GameObject gameObject;
            public Rigidbody rb;
            public float duration;

            public Copy(GameObject gameObject)
            {
                this.gameObject = gameObject;
                rb = gameObject.GetComponent<Rigidbody>();
                duration = 0f;
            }
        }

        public void LaunchCopy()
        {
            Copy copiedObject = FindCopy();
            if (copiedObject.rb == null) return;
            copiedObject.duration = 0f;
            copiedObject.gameObject.SetActive(true);
            copiedObject.rb.isKinematic = isKinematic;
            copiedObject.rb.AddForce(launchDirection.normalized * launchForce, ForceMode.Impulse);
        }

        Copy FindCopy()
        {
            if (objectPool.Count > 0)
            {
                foreach (Copy obj in objectPool)
                {
                    if (obj.gameObject.activeSelf) continue;
                    obj.gameObject.transform.position = transform.position;
                    obj.gameObject.transform.rotation = transform.rotation;
                    return obj;
                }
            }

            objectPool.Add(new Copy(Instantiate(prefab, transform.position, transform.rotation)));
            return objectPool[^1];
        }

        void OnDrawGizmosSelected() 
        {
            Debug.DrawRay(transform.position, launchDirection.normalized * launchForce, Color.red);
        }
    }
}
