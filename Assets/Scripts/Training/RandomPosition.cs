using UnityEngine;
using AI;

namespace Training
{
    public class RandomPosition : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;
        [SerializeField] TankController trainerAI;
        [SerializeField] KeyCode resetKey = KeyCode.Alpha1;
        [SerializeField] bool testReset = false;
        [SerializeField] bool curricularTraining = false;

        [Header("Position Settings")]
        [SerializeField] bool changeX;
        [SerializeField] bool changeZ;
        [SerializeField] float xBounds;
        [SerializeField] float zBounds;

        [Header("Switch Settings")]
        [SerializeField] bool randomlySwitchPositions;
        [SerializeField, Range(0f, 1f)] float switchChance = 0.5f;
        [SerializeField] float lessonValueSwitch = 1f;
        [SerializeField] float lessonValueRandomPos = 2f;

        float prog => EnvParamManager.Instance.prog;

        // Start is called before the first frame update
        void Start()
        {
            if (!curricularTraining) return;

            randomlySwitchPositions = false;
            changeX = false;
            changeZ = false;
        }

        // Update is called once per frame
        void Update()
        {
            CheckCurricular();

            if (!testReset) return;
            if (!Input.GetKeyDown(resetKey)) return;

            agentAI.GetComponent<TankController>()?.Reset();
            SetNewEpisode();
        }

        void CheckCurricular()
        {
            if (!curricularTraining || EnvParamManager.Instance == null || prog < 0) return;

            randomlySwitchPositions = prog >= lessonValueSwitch;
            changeX = prog >= lessonValueRandomPos;
            changeZ = prog >= lessonValueRandomPos;
        }

        public void SetNewEpisode()
        {
            trainerAI.Reset();

            trainerAI.transform.position = trainerAI.transform.position + 
                CalculateBoundaries(Random.Range(-xBounds, xBounds), Random.Range(-zBounds, zBounds));

            agentAI.transform.position = agentAI.transform.position + 
                CalculateBoundaries(Random.Range(-xBounds, xBounds), Random.Range(-zBounds, zBounds));

            if (!randomlySwitchPositions) return;
            if (Random.Range(0f, 1f) <= switchChance) return;

            Vector3 tempPos = trainerAI.transform.position;
            Quaternion tempRot = trainerAI.transform.rotation;

            trainerAI.transform.position = agentAI.transform.position;
            trainerAI.transform.rotation = agentAI.transform.rotation;
            
            agentAI.transform.position = tempPos;
            agentAI.transform.rotation = tempRot;
        }

        Vector3 CalculateBoundaries(float xPos, float zPos)
        {
            Vector3 output = Vector3.zero;
            if (changeX) output.x = xPos;
            if (changeZ) output.z = zPos;
            return output;
        }

        void OnDrawGizmosSelected()
        {
            if (agentAI == null) return;
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(agentAI.transform.position + CalculateBoundaries(xBounds, zBounds), 0.5f);
            Gizmos.DrawSphere(agentAI.transform.position + CalculateBoundaries(-xBounds, zBounds), 0.5f);
            Gizmos.DrawSphere(agentAI.transform.position + CalculateBoundaries(xBounds, -zBounds), 0.5f);
            Gizmos.DrawSphere(agentAI.transform.position + CalculateBoundaries(-xBounds, -zBounds), 0.5f);
        }
    }
}

