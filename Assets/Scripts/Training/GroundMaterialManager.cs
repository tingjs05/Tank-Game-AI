using UnityEngine;

namespace Training
{
    [RequireComponent(typeof(Renderer))]
    public class GroundMaterialManager : MonoBehaviour
    {
        [SerializeField] TankController trainerAI;
        [SerializeField] Material succeedMat, failMat;
        Renderer rend;
        bool successfulRun = false;

        public delegate bool Condition();
        public Condition overrideCondition = null;

        // Start is called before the first frame update
        void Start()
        {
            rend = GetComponent<Renderer>();
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

        public void NewEpisodeReset()
        {
            // reset material on reset
            if (!successfulRun) rend.material = failMat;
            successfulRun = false;
        }
    }
}
