using System;
using System.Collections;
using Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Gameplay
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject targetMenu, resumeButton;
        public GameObject[] titles;
        public GameObject loadingScreen;
        public float gameEndDelay = 1f;
        public float switchSceneDelay = 0.5f;
        public KeyCode pauseKey = KeyCode.Escape;

        public bool Paused { get; private set; } = false;
        private bool gameEnd = false;
        private Coroutine coroutine;

        void Update()
        {
            if (!Input.GetKeyDown(pauseKey)) return;
            TogglePause();
        }

        public void Restart()
        {
            if (GameplayManager.Instance == null) return;
            // reset tanks
            GameplayManager.Instance.ResetGame();
            // reset variables
            gameEnd = false;
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = null;
            // reset pause
            if (!Paused) return;
            ToggleMenu();
        }

        public void LoadScene(int index)
        {
            Time.timeScale = 1f;
            loadingScreen.SetActive(true);

            StartCoroutine(DelayedAction(switchSceneDelay, () => 
                {
                    SceneManager.LoadSceneAsync(index);
                }
            ));
        }

        public void EndGame(bool win)
        {
            if (coroutine != null) return;
            gameEnd = true;
            coroutine = StartCoroutine(DelayedAction(gameEndDelay, () => 
                {
                    // show end game menu
                    ToggleMenu();
                    SetTitle(win ? 1 : 2);
                    resumeButton.SetActive(false);
                    coroutine = null;
                    // hide object when showing menu
                    if (GameplayManager.Instance == null) return;
                    GameplayManager.Instance.HideObject(win);
                }
            ));
        }

        public void TogglePause()
        {
            if (gameEnd) return;
            ToggleMenu();
            SetTitle(0);
            resumeButton.SetActive(true);
        }

        public void ToggleMenu()
        {
            Paused = !Paused;
            targetMenu.SetActive(Paused);
            Time.timeScale = Paused ? 0f : 1f;
        }

        void SetTitle(int index)
        {
            for (int i = 0; i < titles.Length; i++)
            {
                titles[i].SetActive(i == index);
            }
        }

        IEnumerator DelayedAction(float duration, Action callback)
        {
            yield return new WaitForSeconds(duration);
            callback?.Invoke();
        }
    }
}
