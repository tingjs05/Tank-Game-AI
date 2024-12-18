using UnityEngine;
using AI;

namespace Training
{
    public class RandomObstacle : MonoBehaviour
    {
        [SerializeField] KeyCode resetKey = KeyCode.Alpha2;
        [SerializeField] bool testReset = false;
        [SerializeField] bool curricularTraining = false;

        [Header("Random Position Setting")]
        [SerializeField] Transform randomObstacleTarget;
        [SerializeField] Vector2 positionLessonValues = new Vector2(1f, 3f);
        [SerializeField] Vector3[] obstaclePositions;

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
            if (!curricularTraining) return;
            
            scaleX = false;
            scaleZ = false;
            rotateY = false;
            changePos = false;
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
            changePos = prog >= positionLessonValues.x && prog <= positionLessonValues.y;
        }

        public void SetNewEpisode()
        {
            // handle random scale
            Vector3 scale = transform.localScale;

            if (scaleX)
                scale.x = Random.Range(scaleBounds.x, scaleBounds.y);
            if (scaleZ)
                scale.z = Random.Range(scaleBounds.x, scaleBounds.y);
            
            transform.localScale = scale;

            // handle random rotation
            if (rotateY)
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, Random.Range(0f, 360f), transform.eulerAngles.z);
            
            // handle random position
            randomObstacleTarget.gameObject.SetActive(changePos);

            if (obstaclePositions == null || obstaclePositions.Length <= 0 || !changePos) return;

            randomObstacleTarget.localPosition = obstaclePositions[Random.Range(0, obstaclePositions.Length)];
        }
    }
}
