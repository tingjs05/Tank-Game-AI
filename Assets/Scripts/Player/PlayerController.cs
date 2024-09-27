using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(TankController))]
    public class PlayerController : MonoBehaviour
    {
        public KeyCode forwardKey, backwardKey, leftKey, rightKey, shootKey;
        public float shootCooldown = 1f;

        private TankController controller;
        private Vector2 moveVector;
        private float shoot_cooldown;

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
            // increment shoot cooldown
            shoot_cooldown += Time.deltaTime;
            // check if shoot key has been pressed, if so shoot
            if (shoot_cooldown < shootCooldown || !Input.GetKeyDown(shootKey)) return;
            controller.Shoot();
            shoot_cooldown = 0f;
        }
    }
}
