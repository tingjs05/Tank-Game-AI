using UnityEngine;
using Unity.MLAgents;
using AI;

namespace Training
{
    public class EnvParamManager : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;

        public static EnvParamManager Instance { get; private set; }
        public float prog { get; private set; } = -1f;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            agentAI.OnNewEpisode += SetNewEpisode;
        }

        void SetNewEpisode()
        {
            prog = Academy.Instance.EnvironmentParameters.GetWithDefault("env_params", -1);
        }
    }
}
