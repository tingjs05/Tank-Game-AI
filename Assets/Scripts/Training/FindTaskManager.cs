using UnityEngine;
using AI;

namespace Training
{
    public class FindTaskManager : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;
        [SerializeField] GroundMaterialManager groundManager;
        [SerializeField] bool curricularTraining = false;
        [SerializeField] bool changeTask = false;
        [SerializeField] float lessonValue = 2f;

        float prog => EnvParamManager.Instance.prog;
        bool targetFound = false;

        void FixedUpdate()
        {
            if (curricularTraining) 
                changeTask = prog < lessonValue;

            if (!changeTask)
            {
                groundManager.overrideCondition = null;
                return;
            }

            targetFound = agentAI.TargetInRange();
            groundManager.overrideCondition = () => targetFound;
            if (!targetFound) return;
            groundManager.Succeed();
            agentAI.EndEpisode();
        }
    }
}
