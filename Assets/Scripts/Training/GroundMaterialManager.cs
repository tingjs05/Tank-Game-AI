using UnityEngine;
using AI;

namespace Training
{
    [RequireComponent(typeof(Renderer))]
    public class GroundMaterialManager : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;
        [SerializeField] TankController trainerAI;
        [SerializeField] Material succeedMat, failMat;
        Renderer rend;
        bool successfulRun = false;

        // Start is called before the first frame update
        void Start()
        {
            rend = GetComponent<Renderer>();
            agentAI.OnNewEpisode += NewEpisodeReset;
            trainerAI.Died += Succeed;
        }

        void NewEpisodeReset()
        {
            if (successfulRun)
            {
                successfulRun = false;
                return;
            }

            rend.material = failMat;
        }

        void Succeed()
        {
            successfulRun = true;
            rend.material = succeedMat;
        }
    }
}
