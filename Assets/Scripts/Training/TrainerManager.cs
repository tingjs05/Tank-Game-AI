using UnityEngine;
using Unity.MLAgents;
using AI.FSM;

namespace Training
{
    public class TrainerManager : MonoBehaviour
    {
        [SerializeField] TankFSM trainerAI;
        [SerializeField] bool curricularTraining = false;

        // Update is called once per frame
        void Update()
        {
            if (!curricularTraining) return;

            float prog = Academy.Instance.EnvironmentParameters.GetWithDefault("env_params", -1);

            if (prog < 0) return;

            trainerAI.enabled = prog >= 4f;
        }
    }
}
