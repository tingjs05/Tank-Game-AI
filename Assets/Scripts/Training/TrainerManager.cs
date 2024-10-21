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
            if (!curricularTraining || EnvParamManager.Instance == null) return;

            float prog = EnvParamManager.Instance.prog;

            if (prog < 0) return;

            trainerAI.enabled = prog >= 4f;
        }
    }
}
