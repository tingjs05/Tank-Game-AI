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

        [Header("Obstacle Settings")]
        [SerializeField] Vector2 scaleBounds;
        [SerializeField] bool scaleX = true;
        [SerializeField] bool scaleZ = true;
        [SerializeField] bool rotateY = false;

        float prog => EnvParamManager.Instance.prog;

        // Start is called before the first frame update
        void Start()
        {
            if (curricularTraining)
            {
                scaleX = false;
                scaleZ = false;
                rotateY = false;
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

            scaleX = prog >= 3f;
            scaleZ = prog >= 3f;
            rotateY = prog >= 3f;
        }

        void SetNewEpisode()
        {
            Vector3 scale = transform.localScale;

            if (scaleX)
                scale.x = Random.Range(scaleBounds.x, scaleBounds.y);
            if (scaleZ)
                scale.z = Random.Range(scaleBounds.x, scaleBounds.y);
            
            transform.localScale = scale;

            if (!rotateY) return;

            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, Random.Range(0f, 360f), transform.eulerAngles.z);
        }
    }
}
