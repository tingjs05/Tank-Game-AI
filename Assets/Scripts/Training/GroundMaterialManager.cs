using System.Collections;
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
        [SerializeField] float resetDelay = 0.01f;
        Renderer rend;
        bool successfulRun = false;

        public delegate bool Condition();
        public Condition overrideCondition = null;

        // Start is called before the first frame update
        void Start()
        {
            rend = GetComponent<Renderer>();
            agentAI.OnNewEpisode += NewEpisodeReset;
            trainerAI.Died += Succeed;
        }

        public void Succeed()
        {
            // check if there is an override condition
            if (overrideCondition != null && !overrideCondition.Invoke()) return;
            // set colour to green
            successfulRun = true;
            rend.material = succeedMat;
        }

        void NewEpisodeReset()
        {
            StartCoroutine(DelayedReset());
        }

        IEnumerator DelayedReset()
        {
            yield return new WaitForSeconds(resetDelay);
            if (!successfulRun) rend.material = failMat;
            successfulRun = false;
        }
    }
}
