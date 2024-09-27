using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviour, IDamagable
{
    [Header("Stats")]
    public float maxHealth = 10f;
    public float moveSpeed = 15f;
    public float rotationSpeed = 15f;
    public float shootingRecoil = 0f;
    public GameObject projectilePrefab;
    public Slider healthBar;
    
    public float Health { get; private set; } = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // reset health
        Health = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = Health;
    }

    public void Move(Vector2 move)
    {
        // clamp values between -1 and 1
        move.x = Mathf.Clamp(move.x, -1f, 1f);
        move.y = Mathf.Clamp(move.y, -1f, 1f);

        // apply movement
        rb.AddForce(transform.forward * move.x * Time.deltaTime * moveSpeed, ForceMode.VelocityChange);
        rb.angularVelocity = transform.up * move.y * rotationSpeed;
    }

    public void Shoot()
    {
        Projectile projectile;

        // if there is no projectile namager, manually instantiate projectile
        if (ProjectileManager.Instance == null)
        {
            Instantiate(projectilePrefab);
            projectile = projectilePrefab.GetComponent<Projectile>();
        }
        else
        {
            projectile = ProjectileManager.Instance.InstantiateProjectile(projectilePrefab);
        }
        
        projectile.self = gameObject;
        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.identity;
        projectile.moveDir = transform.forward;
        projectile.Start();

        // apply recoil
        rb.AddForce(-transform.forward * shootingRecoil, ForceMode.Impulse);
    }

    public void Damage(float damage)
    {
        Health -= damage;
        Health = Mathf.Clamp(Health, 0f, maxHealth);
        healthBar.value = Health;
        if (Health > 0f) return;
        // TODO: handle death
    }
}
