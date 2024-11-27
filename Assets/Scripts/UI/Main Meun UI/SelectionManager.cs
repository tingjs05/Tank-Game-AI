using UnityEngine;

namespace UI
{
    public class SelectionManager : MonoBehaviour
    {
        public GameObject[] selectors;

        public void Select(int index)
        {
            if (selectors == null || index < 0 || index >= selectors.Length) return;

            for (int i = 0; i < selectors.Length; i++)
                selectors[i].SetActive(i == index);
        }
    }
}
