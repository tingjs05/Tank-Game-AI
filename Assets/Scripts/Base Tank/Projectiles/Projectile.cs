using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float damage = 1f;
    public float speed = 20f;
    public float maxLifetime = 5f;
    private float currentLifetime;
    private bool hit;
    private Rigidbody rb;

    [HideInInspector] public GameObject self;
    [HideInInspector] public Vector3 moveDir;

    public event Action<Projectile, bool> OnHidden;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Start()
    {
        currentLifetime = 0f;
        hit = false;
        gameObject.SetActive(true);
    }

    void Update()
    {
        rb.velocity = moveDir * speed;
        currentLifetime += Time.deltaTime;
        if (currentLifetime < maxLifetime) return;
        HandleEndLifetime();
    }

    void OnTriggerEnter(Collider other) 
    {
        // do not detect collision with self
        if (self != null && other.gameObject == self) return;

        if (other.TryGetComponent<IDamagable>(out IDamagable damagable))
        {
            damagable.Damage(damage);
            hit = true;
        }

        // end lifetime upon collision
        HandleEndLifetime();
    }

    void HandleEndLifetime()
    {
        OnHidden?.Invoke(this, hit);

        if (ProjectileManager.Instance == null)
            Destroy(gameObject);
        else 
            gameObject.SetActive(false);
    }
}
