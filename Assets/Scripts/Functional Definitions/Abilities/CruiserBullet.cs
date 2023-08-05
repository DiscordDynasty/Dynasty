using System.Net.Sockets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CruiserrBullet : WeaponAbility
{
    public GameObject bulletPrefab; // the prefabbed sprite for a bullet with a BulletScript
    protected float bulletSpeed; // the speed of the bullet
    protected float survivalTime; // the time the bullet takes to delete itself
    protected Vector3 prefabScale; // the scale of the bullet prefab, used to enlarge the siege turret bullet
    protected float pierceFactor = 0; // pierce factor; increase this to pierce more of the shell
    protected string bulletSound = "clip_cruiserbullet";
    public static readonly int bulletDamage = 400;

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        description = "Projectile that deals {damage} damage.";
        abilityName = "Bullet";
        bulletSpeed = 70;
        survivalTime = 1.7F;
        range = bulletSpeed * survivalTime;
        minRange = 30;
        ID = AbilityID.CruiserBullet;
        cooldownDuration = 60F;
        energyCost = 10;
        damage = bulletDamage;
        prefabScale = 0.4446325f * Vector3.one;
        category = Entity.EntityCategory.All;
    }

    protected override void Start()
    {
        bulletPrefab = ResourceManager.GetAsset<GameObject>("cruiserbullet_prefab");
        base.Start();
    }



    /// <summary>
    /// Fires the bullet using the helper method
    /// </summary>
    /// <param name="victimPos">The position to fire the bullet to</param>
    protected override bool Execute(Vector3 victimPos)
    {
        return FireBullet(victimPos); // fire if there is
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        AudioManager.PlayClipByID(bulletSound, transform.position);
    }

    /// <summary>
    /// Helper method for Execute() that creates a bullet and modifies it to be shot
    /// </summary>
    /// <param name="targetPos">The position to fire the bullet to</param>
    protected virtual bool FireBullet(Vector3 targetPos)
    {
        Vector3 originPos = part ? part.transform.position : Core.transform.position;
        Transform targeting = Core.GetExtendedTargetingSystem().ReturnHighestHealth(range, minRange, damage / 2, -1, true, this.transform);

        // Calculate future target position
        Vector2 targetVelocity = targeting ? targeting.GetComponentInParent<Rigidbody2D>().velocity : Vector2.zero;

        // Closed form solution to bullet lead problem involves finding t via a quadratic solved here.
        Vector2 relativeDistance = targetPos - originPos;
        var a = (bulletSpeed * bulletSpeed - Vector2.Dot(targetVelocity, targetVelocity));
        var b = -(2 * targetVelocity.x * relativeDistance.x + 2 * targetVelocity.y * relativeDistance.y);
        var c = -Vector2.Dot(relativeDistance, relativeDistance);

        if (a == 0 || b * b - 4 * a * c < 0)
        {
            return false;
        }

        var t1 = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
        var t2 = (-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);

        float t = t1 < 0 ? (t2 < 0 ? 0 : t2) : (t2 < 0 ? t1 : Mathf.Min(t1, t2));
        if (t < 0)
        {
            return false;
        }

        // Create the Bullet from the Bullet Prefab
        if (bulletPrefab == null)
        {
            bulletPrefab = ResourceManager.GetAsset<GameObject>("cruiserbullet_prefab");
        }
        ActivationCosmetic(targetPos);

        var bullet = Instantiate(bulletPrefab, originPos, Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(relativeDistance.y, relativeDistance.x) * Mathf.Rad2Deg - 90)));
        bullet.transform.localScale = prefabScale;

        // Update its damage to match main bullet
        var script = bullet.GetComponent<CruiserBulletScript>();
        script.owner = GetComponentInParent<Entity>();
        script.SetDamage(GetDamage());
        script.SetCategory(category);
        script.SetTerrain(terrain);
        script.SetShooterFaction(Core.faction);
        script.SetPierceFactor(pierceFactor);
        script.particleColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F, 1F, 1F, 0.9F);
        script.missParticles = true;

        // Add velocity to the bullet
        if (t != 0)
        {
            bullet.GetComponent<Rigidbody2D>().velocity = Vector3.Normalize(relativeDistance + targetVelocity * t) * bulletSpeed;
        }

        // Destroy the bullet after survival time
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off || !MasterNetworkAdapter.lettingServerDecide)
            script.StartSurvivalTimer(survivalTime);
        
        if (SceneManager.GetActiveScene().name != "SampleScene")
        {
            bullet.GetComponent<NetworkProjectileWrapper>().enabled = false;
            bullet.GetComponent<NetworkObject>().enabled = false;
        }

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && (!MasterNetworkAdapter.lettingServerDecide))
        {
            bullet.GetComponent<NetworkObject>().Spawn();
        }
        return true;
    }
}
