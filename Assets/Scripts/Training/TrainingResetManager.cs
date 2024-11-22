using System.Collections;
using UnityEngine;
using Astar;
using AI;

namespace Training
{
    public class TrainingResetManager : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;
        [SerializeField] KeyCode resetKey = KeyCode.Alpha0;
        [SerializeField] float resetDelay = 0.01f;
        [SerializeField] bool testReset = false;

        [Header("Training Scripts")]
        [SerializeField] RandomObstacle randomObstacle;
        [SerializeField] ComplexMapManager complexMapManager;
        [SerializeField] RandomPosition randomPosition;
        [SerializeField] RandomGroundTilt randomGroundTilt;
        [SerializeField] GroundMaterialManager groundMaterialManager;

        Coroutine resetCoroutine;

        // Start is called before the first frame update
        void Start()
        {
            agentAI.OnNewEpisode += OnNewEpisodeReset;
        }

        // Update is called once per frame
        void Update()
        {
            if (!testReset || !Input.GetKeyDown(resetKey)) return;
            OnNewEpisodeReset();
        }

        void OnNewEpisodeReset()
        {
            if (resetCoroutine != null) StopCoroutine(resetCoroutine);
            resetCoroutine = StartCoroutine(ResetTraining());
        }

        IEnumerator ResetTraining()
        {
            randomGroundTilt.ResetTilt();
            randomObstacle.SetNewEpisode();
            complexMapManager.SetMap();
            randomPosition.SetNewEpisode();

            yield return new WaitForSeconds(resetDelay);

            // recalculate pathfinding nodes on reset
            if (NodeManager.Instance != null)
                NodeManager.Instance.UpdateObstructedNodes();
            
            yield return new WaitForSeconds(resetDelay);
            
            randomGroundTilt.TiltGround();
            groundMaterialManager.NewEpisodeReset();

            resetCoroutine = null;
        }
    }
}
