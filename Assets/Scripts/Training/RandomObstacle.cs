using System.Linq;
using UnityEngine;
using AI;

namespace Training
{
    public class RandomObstacle : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;
        [SerializeField] KeyCode resetKey = KeyCode.Alpha2;
        [SerializeField] bool testReset = false;
        [SerializeField] bool curricularTraining = false;

        [Header("Random Position Setting")]
        [SerializeField] Vector3[] obstaclePositions;
        [SerializeField] float[] positionLessonValues = new float[] { 2f, 3f };

        [Header("Obstacle Settings")]
        [SerializeField] Vector2 scaleBounds;
        [SerializeField] bool scaleX = true;
        [SerializeField] bool scaleZ = true;
        [SerializeField] bool rotateY = false;
        [SerializeField] bool changePos = false;
        [SerializeField] float obstacleLessonValue = 3f;

        float prog => EnvParamManager.Instance.prog;

        // Start is called before the first frame update
        void Start()
        {
            if (curricularTraining)
            {
                scaleX = false;
                scaleZ = false;
                rotateY = false;
                changePos = false;
            }

            if (agentAI == null) return;
            agentAI.OnNewEpisode += SetNewEpisode;
        }

        // Update is called once per frame
        void Update()
        {
            CheckCurricular();
            if (!testReset) return;
            if (!Input.GetKeyDown(resetKey)) return;
            SetNewEpisode();
        }

        void CheckCurricular()
        {
            if (!curricularTraining || EnvParamManager.Instance == null || prog < 0) return;

            scaleX = prog >= obstacleLessonValue;
            scaleZ = prog >= obstacleLessonValue;
            rotateY = prog >= obstacleLessonValue;
            changePos = positionLessonValues.Contains(prog);
        }

        void SetNewEpisode()
        {
            Vector3 scale = transform.localScale;

            if (scaleX)
                scale.x = Random.Range(scaleBounds.x, scaleBounds.y);
            if (scaleZ)
                scale.z = Random.Range(scaleBounds.x, scaleBounds.y);
            
            transform.localScale = scale;

            if (rotateY)
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, Random.Range(0f, 360f), transform.eulerAngles.z);
            
            if (obstaclePositions == null || obstaclePositions.Length <= 0 || !changePos) return;

            transform.localPosition = obstaclePositions[Random.Range(0, obstaclePositions.Length)];
        }
    }
}
