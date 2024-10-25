using UnityEngine;
using AI;

namespace Training
{
    public class FindTaskManager : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;
        [SerializeField] Transform trainerAI;
        [SerializeField] GroundMaterialManager groundManager;
        [SerializeField, Range(0f, 1f)] float aimDirThreshold = 0.99f;
        [SerializeField] bool curricularTraining = false;
        [SerializeField] bool changeTask = false;
        [SerializeField] float lessonValue = 2f;

        float prog => EnvParamManager.Instance.prog;
        float dot;
        bool targetFound, success = false;

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
            dot = Vector3.Dot(agentAI.transform.forward, (trainerAI.position - agentAI.transform.position).normalized);
            success = dot >= aimDirThreshold;
            groundManager.overrideCondition = () => success;
            if (!success) return;
            groundManager.Succeed();
            agentAI.EndEpisode();
        }
    }
}
