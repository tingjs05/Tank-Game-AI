using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;

namespace Training
{
    public class RandomPosition : MonoBehaviour
    {
        [SerializeField] TankAgent agentAI;
        [SerializeField] TankController trainerAI;
        [SerializeField] KeyCode resetKey = KeyCode.Alpha1;
        [SerializeField] bool testReset = false;

        [Header("Position Settings")]
        [SerializeField] bool changeX;
        [SerializeField] bool changeZ;
        [SerializeField] float xBounds;
        [SerializeField] float zBounds;

        // Start is called before the first frame update
        void Start()
        {
            if (agentAI == null || trainerAI == null) return;
            agentAI.OnNewEpisode += SetNewEpisode;
        }

        // Update is called once per frame
        void Update()
        {
            if (!testReset) return;
            if (!Input.GetKeyDown(resetKey)) return;

            agentAI.GetComponent<TankController>()?.Reset();
            SetNewEpisode();
        }

        void SetNewEpisode()
        {
            trainerAI.Reset();

            trainerAI.transform.position = trainerAI.transform.position + 
                CalculateBoundaries(Random.Range(-xBounds, xBounds), Random.Range(-zBounds, zBounds));

            agentAI.transform.position = agentAI.transform.position + 
                CalculateBoundaries(Random.Range(-xBounds, xBounds), Random.Range(-zBounds, zBounds));
        }

        Vector3 CalculateBoundaries(float xPos, float zPos)
        {
            Vector3 output = Vector3.zero;
            if (changeX) output.x = xPos;
            if (changeZ) output.z = zPos;
            return output;
        }

        void OnDrawGizmosSelected()
        {
            if (agentAI == null) return;
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(agentAI.transform.position + CalculateBoundaries(xBounds, zBounds), 0.5f);
            Gizmos.DrawSphere(agentAI.transform.position + CalculateBoundaries(-xBounds, zBounds), 0.5f);
            Gizmos.DrawSphere(agentAI.transform.position + CalculateBoundaries(xBounds, -zBounds), 0.5f);
            Gizmos.DrawSphere(agentAI.transform.position + CalculateBoundaries(-xBounds, -zBounds), 0.5f);
        }
    }
}

