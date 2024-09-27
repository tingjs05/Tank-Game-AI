using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public float rotationSpeed = 15f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float shootingRecoil = 0f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector2 move)
    {
        // clamp values between -1 and 1
        move.x = Mathf.Clamp(move.x, -1f, 1f);
        move.y = Mathf.Clamp(move.y, -1f, 1f);

        // apply movement
        rb.velocity = transform.forward * move.x * moveSpeed;
        rb.angularVelocity = transform.up * move.y * rotationSpeed;
    }

    public void Shoot()
    {

    }
}
