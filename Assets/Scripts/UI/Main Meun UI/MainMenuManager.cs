using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] float delay = 0.5f;
        [SerializeField] SelectionManager characterSelect, typeOfAISelect;

        void Start()
        {
            if (GameManager.Instance == null) return;
            characterSelect.Select(GameManager.Instance.characterValue);
            typeOfAISelect.Select((int) GameManager.Instance.TypeOfAI);
        }

        public void Quit()
        {
            StartCoroutine(DelayedAction(delay, () => 
                {
                    Debug.Log("Quit Game");
                    Application.Quit();
                }
            ));
        }

        public void PlayGame(int sceneIndex)
        {
            StartCoroutine(DelayedAction(delay, () => 
                {
                    SceneManager.LoadSceneAsync(sceneIndex);
                }
            ));
        }

        public void SetCharacter(int index)
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.characterValue = index;
        }

        public void SetAIType(int typeOfAI)
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.TypeOfAI = (GameManager.AIType) typeOfAI;
        }

        IEnumerator DelayedAction(float duration, Action callback)
        {
            yield return new WaitForSeconds(duration);
            callback?.Invoke();
        }
    }
}
