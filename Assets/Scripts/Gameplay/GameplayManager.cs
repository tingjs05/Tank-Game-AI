using UnityEngine;
using UI.Gameplay;

namespace Gameplay
{
    public class GameplayManager : MonoBehaviour
    {
        public TankController player, ML_AI, FSM_AI;
        public PauseMenu pauseMenu;

        public static GameplayManager Instance { get; private set; }

        void Awake()
        {
            // set singleton
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        void Start()
        {
            // subscribe to death
            player.Died += () => pauseMenu.EndGame(false);
            ML_AI.Died += () => pauseMenu.EndGame(true);
            FSM_AI.Died += () => pauseMenu.EndGame(true);

            // set AI
            ActivateAI();
            SetAICharacter((GameManager.Instance.TypeOfAI == GameManager.AIType.MACHINE_LEARNING ? ML_AI : FSM_AI));
        }

        void ActivateAI()
        {
            if (GameManager.Instance == null) return;
            ML_AI.gameObject.SetActive(GameManager.Instance.TypeOfAI == GameManager.AIType.MACHINE_LEARNING);
            FSM_AI.gameObject.SetActive(GameManager.Instance.TypeOfAI == GameManager.AIType.FSM);
        }

        void SetAICharacter(TankController AI)
        {
            if (GameManager.Instance == null) return;
            CharacterController charaController = AI.GetComponent<CharacterController>();
            charaController.SetCharacter(GameManager.Instance.characterValue == 0 ? 1 : 0);
        }

        public void HideObject(bool win)
        {
            if (!win)
            {
                player.gameObject.SetActive(false);
                return;
            }

            ML_AI.gameObject.SetActive(false);
            FSM_AI.gameObject.SetActive(false);
        }

        public void ResetGame()
        {
            // reset player
            player.Reset();
            // reset AI
            if (ML_AI.gameObject.activeInHierarchy) 
                ML_AI.Reset();
            if (FSM_AI.gameObject.activeInHierarchy) 
                FSM_AI.Reset();
            // show player and AI after reset
            player.gameObject.SetActive(true);
            ActivateAI();
        }
    }
}
