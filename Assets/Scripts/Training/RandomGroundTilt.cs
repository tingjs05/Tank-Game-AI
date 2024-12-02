using UnityEngine;

namespace Training
{
    public class RandomGroundTilt : MonoBehaviour
    {
        [SerializeField] Vector2 tiltBoundaries = new Vector2(-0.5f, 0.5f);
        [SerializeField] KeyCode resetKey = KeyCode.Alpha4;
        [SerializeField] bool testReset = false;
        [SerializeField] bool curricularTraining = false;
        [SerializeField] bool tiltGround = false;
        [SerializeField] float lessonValue = 5f;
        
        Quaternion originalRotation;
        float prog => EnvParamManager.Instance.prog;

        // Start is called before the first frame update
        void Start()
        {
            originalRotation = transform.rotation;
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

        public void ResetTilt()
        {
            transform.rotation = originalRotation;
        }

        public void TiltGround()
        {
            // check if need to tilt ground
            if (!tiltGround) return;
            // randomly tilt in x and z axis
            transform.rotation = Quaternion.Euler(
                Random.Range(tiltBoundaries.x, tiltBoundaries.y), 
                originalRotation.y, 
                Random.Range(tiltBoundaries.x, tiltBoundaries.y));
        }
    }
}
