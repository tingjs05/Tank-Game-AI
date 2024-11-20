using System.Collections;
using UnityEngine;
using AI;

namespace Training
{
    public class RandomGroundTilt : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;
        [SerializeField] Vector2 tiltBoundaries = new Vector2(-0.5f, 0.5f);
        [SerializeField] KeyCode resetKey = KeyCode.Alpha4;
        [SerializeField] bool testReset = false;
        [SerializeField] bool curricularTraining = false;
        [SerializeField] bool tiltGround = false;
        [SerializeField] float lessonValue = 5f;
        [SerializeField] float resetDelay = 0.05f;
        
        Quaternion originalRotation;
        float prog => EnvParamManager.Instance.prog;

        // Start is called before the first frame update
        void Start()
        {
            originalRotation = transform.rotation;
            agentAI.OnNewEpisode += TiltGround;
        }

        // Update is called once per frame
        void Update()
        {
            // update lesson value based on progress
            if (curricularTraining) tiltGround = prog >= lessonValue;
            // check for test reset
            if (!testReset || !Input.GetKeyDown(resetKey)) return;
            TiltGround();
        }
        void TiltGround()
        {
            transform.rotation = originalRotation;
            StartCoroutine(DelayedTilt());
        }

        IEnumerator DelayedTilt()
        {
            yield return new WaitForSeconds(resetDelay);
            // check if need to tilt ground
            if (tiltGround)
                transform.rotation = Quaternion.Euler(
                    Random.Range(tiltBoundaries.x, tiltBoundaries.y), 
                    originalRotation.y, 
                    Random.Range(tiltBoundaries.x, tiltBoundaries.y));
        }
    }
}
