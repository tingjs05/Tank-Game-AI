using UnityEngine;
using AI;

namespace Training
{
    public class RandomObstacle : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;
        [SerializeField] KeyCode resetKey = KeyCode.Alpha2;
        [SerializeField] bool testReset = false;

        [Header("Obstacle")]
        [SerializeField] Vector2 scaleBounds;

        [Header("Settings")]
        [SerializeField] bool scaleX = true;
        [SerializeField] bool scaleZ = true;
        [SerializeField] bool rotateY = false;

        // Start is called before the first frame update
        void Start()
        {
            if (agentAI == null) return;
            agentAI.OnNewEpisode += SetNewEpisode;
        }

        // Update is called once per frame
        void Update()
        {
            if (!testReset) return;
            if (!Input.GetKeyDown(resetKey)) return;
            SetNewEpisode();
        }

        void SetNewEpisode()
        {
            Vector3 scale = transform.localScale;

            if (scaleX)
                scale.x = Random.Range(scaleBounds.x, scaleBounds.y);
            if (scaleZ)
                scale.z = Random.Range(scaleBounds.x, scaleBounds.y);
            
            transform.localScale = scale;

            if (!rotateY) return;

            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, Random.Range(0f, 360f), transform.eulerAngles.z);
        }
    }
}
