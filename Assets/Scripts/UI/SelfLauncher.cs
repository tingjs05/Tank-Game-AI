using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class SelfLauncher : MonoBehaviour
    {
        public GameObject prefab;
        private List<GameObject> objectPool = new List<GameObject>();

        public void LaunchCopy()
        {
            GameObject obj = FindObject();
            obj.SetActive(true);
        }

        GameObject FindObject()
        {
            if (objectPool.Count > 0)
            {
                foreach (GameObject obj in objectPool)
                {
                    if (obj.gameObject.activeSelf) continue;
                    obj.transform.position = transform.position;
                    obj.transform.rotation = transform.rotation;
                    return obj;
                }
            }

            objectPool.Add(Instantiate(prefab, transform.position, transform.rotation));
            return objectPool[^1];
        }
    }
}
