using UnityEngine;

/// <summary>
/// Immobilizes the nearest enemy
/// </summary>
public class PinDown : ActiveAbility
{
    Craft target;
    const float range = 15f;
    //float rangeSquared = range * range;
    private static float PINDOWN_ACTIVE_DURATION = 5f;

    public override float GetRange()
    {
        return range;
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = AbilityID.PinDown;
        energyCost = 10f;
        cooldownDuration = 0.1f;
        activeDuration = PINDOWN_ACTIVE_DURATION;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        if (target != null && target)
        {
            target.RemovePin();
        }
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        AudioManager.PlayClipByID("clip_activateability", targetPos);
        base.ActivationCosmetic(targetPos);
    }


    private static GameObject missileLinePrefab;
    public static void InflictionCosmetic(Entity entity, int coreFaction = 0)
    {
        if (!missileLinePrefab)
        {
            missileLinePrefab = new GameObject("Missile Line"); // create prefab and set to parent
            LineRenderer lineRenderer = missileLinePrefab.AddComponent<LineRenderer>(); // add line renderer
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material"); // get material
            MissileAnimationScript comp = missileLinePrefab.AddComponent<MissileAnimationScript>(); // add the animation script
        }

        var missileColor = new Color(0.8F, 1F, 1F, 0.9F);

        foreach (var part in entity.GetComponentsInChildren<ShellPart>())
        {
            var x = Instantiate(missileLinePrefab, part.transform); // instantiate
            x.GetComponent<MissileAnimationScript>().Initialize(); // initialize
            x.GetComponent<MissileAnimationScript>().lineColor = missileColor;
            Destroy(x, PINDOWN_ACTIVE_DURATION);
        }
    }
    /// <summary>
    /// Immobilizes a nearby enemy
    /// </summary>
    protected override void Execute()
    {
        ActivationCosmetic(transform.position);
        target = null;
        float minDist = Mathf.Pow(range, 2);
        var targetTransform = Core.GetTargetingSystem().GetTarget();

        if (targetTransform)
        {
            var targetTransformDist = (targetTransform.position - Core.transform.position).sqrMagnitude;
            if (targetTransformDist < minDist && ValidityCheck(targetTransform.GetComponent<Entity>()))
            {
                target = targetTransform.GetComponent<Entity>() as Craft;
            }
        }


        if (!target)
            for (int i = 0; i < AIData.entities.Count; i++)
            {
                if (ValidityCheck(AIData.entities[i]))
                {
                    float d = (Core.transform.position - AIData.entities[i].transform.position).sqrMagnitude;
                    if (d < minDist)
                    {
                        minDist = d;
                        target = AIData.entities[i] as Craft;
                    }
                }
            }

        if (target != null)
        {
            target.AddPin();
            InflictionCosmetic(target, Core.faction);
            if (target.networkAdapter) target.networkAdapter.InflictionCosmeticClientRpc((int)AbilityID.PinDown);
        }

        base.Execute();

    }


    /*private Transform ReturnPrioritizedTarget()
    {
        var highestHealth = 0f;
        Transform target = null;
        var rangePow = Mathf.Pow(range, 2);
        var core = Core.transform.position.sqrMagnitude;
        Transform coreTargeting = Core.GetTargetingSystem().GetTarget();
        if (coreTargeting != null && Core is PlayerCore)
        {
            return coreTargeting;
        }
        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            var scanHealth = scan.GetComponent<Entity>().GetMaxHealth()[0] + scan.GetComponent<Entity>().GetMaxHealth()[1];
            if (ValidityCheck(scan.GetComponent<Entity>()) && core - scan.position.sqrMagnitude < rangePow)
            {
                if (scanHealth > highestHealth)
                {
                    highestHealth = scanHealth;
                    target = scan;
                }
                if (scanHealth == highestHealth && core - scan.position.sqrMagnitude < core - target.position.sqrMagnitude)
                {
                    target = scan;
                }
            }
        }
        return target;
    }

    bool ValidityCheck(Entity ent)
    {
        return (ent is Craft && !ent.GetIsDead() && !FactionManager.IsAllied(ent.faction, Core.faction) && !ent.IsInvisible);
    }*/
    bool ValidityCheck(Entity ent)
    {
        return (!ent.GetIsDead() && !FactionManager.IsAllied(ent.faction, Core.faction) && !ent.IsInvisible);
    }
}
