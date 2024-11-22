using System.Collections;
using UnityEngine;

namespace UI
{
    public class HideAfter : MonoBehaviour
    {
        [SerializeField] float duration;
        Coroutine coroutine;

        void Start()
        {
            coroutine = StartCoroutine(HideAfterLifetime());
        }

        void Update()
        {
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
    }
}
