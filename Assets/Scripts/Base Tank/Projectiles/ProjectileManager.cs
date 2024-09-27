using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }
    private List<Projectile> projectilePool = new List<Projectile>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    public Projectile InstantiateProjectile(GameObject prefab)
    {
        // attempt to get an obejct from the pool
        for (int i = 0; i < projectilePool.Count; i++)
        {
            if (projectilePool[i].gameObject.activeSelf) continue;
            return projectilePool[i];
        }

        // if none are found, create a new object
        GameObject newObj = Instantiate(prefab);
        Projectile newProj = newObj.GetComponent<Projectile>();
        projectilePool.Add(newProj);
        return newProj;
    }
}
