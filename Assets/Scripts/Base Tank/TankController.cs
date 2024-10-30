using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviour, IDamagable
{
    [Header("Stats")]
    public float maxHealth = 10f;
    public float moveSpeed = 15f;
    public float rotationSpeed = 15f;

    [Header("Combat")]
    public float shootingRecoil = 0f;
    public float shootCooldown = 1f;

    [Header("Other Variables")]
    public GameObject projectilePrefab;
    public Slider healthBar;
    public string boundaryTag = "Boundary";

    [Header("Locks")]
    public bool lockMovement = false;
    public bool lockRotation = false;
    public bool lockShooting = false;
    
    public Rigidbody rb { get; private set; }
    public float Health { get; private set; } = 0f;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float shoot_cooldown;

    public event Action Damaged;
    public event Action Died;
    public event Action<bool> OnShoot;

    void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // reset health
        Health = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = Health;
    }

    void Update()
    {
        // increment shoot cooldown
        shoot_cooldown += Time.deltaTime;
    }

    void OnTriggerEnter(Collider other) 
    {
        // check if fallen off map
        if (!other.CompareTag(boundaryTag)) return;
        Died?.Invoke();
    }

    public void Move(Vector2 move)
    {
        // clamp values between -1 and 1
        move.x = Mathf.Clamp(move.x, -1f, 1f);
        move.y = Mathf.Clamp(move.y, -1f, 1f);

        // check for locks
        if (lockMovement) move.x = 0f;
        if (lockRotation) move.y = 0f;

        // apply movement
        rb.AddForce(transform.forward * move.x * Time.deltaTime * moveSpeed, ForceMode.VelocityChange);
        rb.angularVelocity = transform.up * move.y * rotationSpeed;
    }

    public void Shoot()
    {
        // check for shooting lock
        if (lockShooting) return;
        // check if can shoot (after waiting for cooldown)
        if (shoot_cooldown < shootCooldown) return;
        // reset cooldown after shooting
        shoot_cooldown = 0f;

        // handle shooting behaviour
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
        
        projectile.OnHidden += OnProjectileHidden;
        projectile.self = gameObject;
        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.identity;
        projectile.moveDir = transform.forward;
        projectile.Start();

        // apply recoil
        rb.AddForce(-transform.forward * shootingRecoil, ForceMode.Impulse);
    }

    public void Reset()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        Health = maxHealth;
        healthBar.value = Health;
    }

    public void SetResetPosition(Vector3 newPos)
    {
        originalPosition = newPos;
    }

    public void Damage(float damage)
    {
        Health -= damage;
        Health = Mathf.Clamp(Health, 0f, maxHealth);
        healthBar.value = Health;
        Damaged?.Invoke();
        if (Health > 0f) return;
        // TODO: handle death
        Died?.Invoke();
    }

    void OnProjectileHidden(Projectile ctx, bool hit)
    {
        ctx.OnHidden -= OnProjectileHidden;
        OnShoot?.Invoke(hit);
    }
}
