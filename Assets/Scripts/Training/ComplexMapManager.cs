using System.Collections.Generic;
using UnityEngine;
using AI;

namespace Training
{
    public class ComplexMapManager : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;
        [SerializeField] TankController trainerAI;
        [SerializeField] KeyCode resetKey = KeyCode.Alpha3;
        [SerializeField] bool testReset = false;
        [SerializeField] bool curricularTraining = false;

        [Header("Position Settings")]
        [SerializeField] Vector3 newAIResetPosition;

        [Header("General Settings")]
        [SerializeField] RandomPosition randomPositionManager;
        [SerializeField] Transform ground;
        [SerializeField] Vector2 extendedGroundScale = new Vector2(2f, 2f);
        [SerializeField] bool useComplexMap = false;
        [SerializeField] float lessonValue = 4f;
        
        TankController agentController;
        Vector3 originalAIResetPosition, originalGroundScale;
        List<GameObject> obstacleLayouts = new List<GameObject>();

        Vector3 originalTrainerPos => new Vector3(originalAIResetPosition.x, originalAIResetPosition.y, 
            originalAIResetPosition.z - newAIResetPosition.z);
        Vector3 newTrainerPos => transform.parent.position + 
            new Vector3(newAIResetPosition.x, newAIResetPosition.y, -newAIResetPosition.z);
        float prog => EnvParamManager.Instance.prog;

        void Awake()
        {
            originalAIResetPosition = agentAI.transform.position;
            originalGroundScale = ground.transform.localScale;
            agentController = agentAI.GetComponent<TankController>();

            // set obstacle layouts to list
            foreach (Transform child in transform)
            {
                obstacleLayouts.Add(child.gameObject);
            }

            if (curricularTraining) useComplexMap = false;

            if (agentAI == null || trainerAI == null) return;
            agentAI.OnNewEpisode += SetMap;
        }

        void Update()
        {
            // update use complex map based on prog
            if (curricularTraining) useComplexMap = prog >= lessonValue;

            // check for test reset
            if (!testReset || !Input.GetKeyDown(resetKey)) return;
            agentController.Reset();
            SetMap();
        }

        void SetMap()
        {
            // set ground scale
            ground.transform.localScale = useComplexMap ? 
                new Vector3(extendedGroundScale.x, originalGroundScale.y, extendedGroundScale.y) : 
                originalGroundScale;
            
            // set original reset positions
            agentController.SetResetPosition(useComplexMap ? (transform.parent.position + newAIResetPosition) : originalAIResetPosition);
            trainerAI.SetResetPosition(useComplexMap ? (newTrainerPos) : (originalTrainerPos));

            if (!useComplexMap)
            {
                // check if turned on automatic reset
                randomPositionManager.reset_position = true;
                return;
            }

            // check if manual reset has been turned off
            randomPositionManager.reset_position = false;
            // call random position manually
            randomPositionManager.SetNewEpisode();

            // show random obstacle
            foreach (GameObject obj in obstacleLayouts)
            {
                obj.SetActive(false);
            }

            obstacleLayouts[Random.Range(0, obstacleLayouts.Count)].SetActive(true);
        }

        void OnDrawGizmosSelected() 
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.parent.position + newAIResetPosition, 0.5f);
            Gizmos.DrawSphere(newTrainerPos, 0.5f);
        }
    }
}
