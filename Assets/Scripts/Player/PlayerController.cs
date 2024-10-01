using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(TankController))]
    public class PlayerController : MonoBehaviour
    {
        public KeyCode forwardKey, backwardKey, leftKey, rightKey, shootKey;

        private TankController controller;
        private Vector2 moveVector;

        void Start()
        {
            controller = GetComponent<TankController>();
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
            
            controller.Move(moveVector);

            // handle shooting
            if (!Input.GetKey(shootKey)) return;
            controller.Shoot();
        }
    }
}
