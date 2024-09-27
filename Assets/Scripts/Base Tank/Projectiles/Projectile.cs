using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float damage = 1f;
    public float speed = 20f;
    public float maxLifetime = 5f;
    private float currentLifetime;
    private Rigidbody rb;

    [HideInInspector] public GameObject self;
    [HideInInspector] public Vector3 moveDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Start()
    {
        currentLifetime = 0f;
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
        }

        // end lifetime upon collision
        HandleEndLifetime();
    }

    void HandleEndLifetime()
    {
        if (ProjectileManager.Instance == null)
            Destroy(gameObject);
        else 
            gameObject.SetActive(false);
    }
}
