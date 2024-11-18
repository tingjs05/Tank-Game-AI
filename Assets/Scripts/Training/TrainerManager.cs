using UnityEngine;
using AI.FSM;

namespace Training
{
    public class TrainerManager : MonoBehaviour
    {
        [SerializeField] TankFSM trainerAI;
        [SerializeField] bool curricularTraining = false;
        [SerializeField] float lessonValue = 4f;
        float prog => EnvParamManager.Instance.prog;

        // Update is called once per frame
        void Update()
        {
            if (!curricularTraining || EnvParamManager.Instance == null || prog < 0) return;

            trainerAI.enabled = prog >= lessonValue;
        }
    }
}
