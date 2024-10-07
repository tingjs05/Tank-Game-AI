using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(TankController))]
    public class PlayerController : MonoBehaviour
    {
        public KeyCode forwardKey, backwardKey, leftKey, rightKey, shootKey;

        [Header("Testing")]
        [SerializeField] string boundaryTag = "Boundary";
        [SerializeField] bool resetOnDeath = false;

        private TankController controller;
        private Vector2 moveVector;

        void Start()
        {
            // get reference to controller
            controller = GetComponent<TankController>();
            // handle death reset
            if (resetOnDeath) controller.Died += controller.Reset;
        }

        void Update()
        {
            // handle movement
            moveVector = Vector2.zero;

            if (Input.GetKey(forwardKey))
                moveVector.x += 1f;
            if (Input.GetKey(backwardKey))
                moveVector.x -= 1f;
            if (Input.GetKey(rightKey))
                moveVector.y += 1f;
            if (Input.GetKey(leftKey))
                moveVector.y -= 1f;
            
            // handle shooting
            if (!Input.GetKey(shootKey)) return;
            controller.Shoot();
        }

        void FixedUpdate()
        {
            // apply movement in fixed update
            controller.Move(moveVector);
        }

        void OnTriggerEnter(Collider other) 
        {
            // check for falling out of boundary
            if (!resetOnDeath || !other.CompareTag(boundaryTag)) return;
            controller.Reset();
        }
    }
}
