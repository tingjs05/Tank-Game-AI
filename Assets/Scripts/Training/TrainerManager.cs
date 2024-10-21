using UnityEngine;
using Unity.MLAgents;
using AI.FSM;

namespace Training
{
    public class TrainerManager : MonoBehaviour
    {
        [SerializeField] TankFSM trainerAI;
        [SerializeField] bool curricularTraining = false;
        float prog => Academy.Instance.EnvironmentParameters.GetWithDefault("env_params", -1);

        // Update is called once per frame
        void Update()
        {
            if (!curricularTraining || prog < 0) return;

            trainerAI.enabled = prog >= 4f;
        }
    }
}
