using System;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedTargetingSystem
{
    //Note - Functions may update with similar health range comparisons I.E. - Two shellcores within 200 points of health of eachother treated as equal.

    public Transform parent;
    public float range;
    private Entity parentEntity;
    private Transform target = null;
    private float parentPositionSqrd;
    private Transform reticleTargeting;

    public ExtendedTargetingSystem(Transform parent)
    {
        this.parent = parent;
        parentEntity = parent.GetComponentInParent<Entity>();
        float parentPositionSqrd = parent.position.sqrMagnitude;
        reticleTargeting = parentEntity.GetTargetingSystem().GetTarget();
    }

    public Transform ReturnHighestHealth(float range, float minRange, float minHealth, float maxHealth, bool furthest, Transform partTransform) //Furthest functions on returns select furthest or nearest enemy if of equal health
    {
        var rangeSqrd = Mathf.Pow(range, 2);
        var minRangeSqrd = Mathf.Pow(minRange, 2);
        var highestHealth = 0f;
        var partPositionSqrd = partTransform.position.sqrMagnitude;

        if (reticleTargeting != null && parentEntity is PlayerCore) //Allows player to select an entity to target, otherwise use the targeting system
        {
            return reticleTargeting;
        }

        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            Debug.Log(scan);
            var scanHealth = scan.GetComponent<Entity>().GetMaxHealth()[0] + scan.GetComponent<Entity>().GetMaxHealth()[1]; //Grab max core and shell
            var scanSqrMagnitude = scan.position.sqrMagnitude;
            var currentTargetDistanceCheck = partPositionSqrd - scanSqrMagnitude;
            var oldTargetDistanceCheck = 0f;
            if (target != null)
            {
                oldTargetDistanceCheck = partPositionSqrd - target.position.sqrMagnitude;
            }
            bool withinLowerHealthBounds = minHealth != -1 && minHealth < scanHealth;
            bool withinUpperHealthBounds = maxHealth != -1  && maxHealth > scanHealth;

            if (ValidityCheck(scan.GetComponent<Entity>()) && currentTargetDistanceCheck < rangeSqrd && currentTargetDistanceCheck > minRangeSqrd && withinLowerHealthBounds && withinUpperHealthBounds)
            {
                if (scanHealth > highestHealth)
                {
                    highestHealth = scanHealth;
                    target = scan;
                }

                if (scanHealth == highestHealth && currentTargetDistanceCheck < oldTargetDistanceCheck && !furthest) //Grab nearest comparison
                {
                    target = scan;
                }
                else if (scanHealth == highestHealth && currentTargetDistanceCheck > oldTargetDistanceCheck) //Grab furthest comparison
                {
                    target = scan;
                }
            }
        }
        return target;
    }

    public Transform ReturnHighestCurrentHealth(float range, float minRange, float minHealth, float maxHealth, bool furthest, Transform partTransform)
    {
        var rangeSqrd = Mathf.Pow(range, 2);
        var highestHealth = 0f;
        var partPositionSqrd = partTransform.position.sqrMagnitude;

        if (reticleTargeting != null && parentEntity is PlayerCore)
        {
            return reticleTargeting;
        }

        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            var scanHealth = scan.GetComponent<Entity>().GetHealth()[0] + scan.GetComponent<Entity>().GetHealth()[1]; //Grab current core and shell
            var scanSqrMagnitude = scan.position.sqrMagnitude;
            var currentTargetDistanceCheck = partPositionSqrd - scanSqrMagnitude;
            var oldTargetDistanceCheck = 0f;
            if (target != null)
            {
                oldTargetDistanceCheck = partPositionSqrd - target.position.sqrMagnitude;
            }
            bool withinHealthBounds = minHealth != -1 && maxHealth != -1 && minHealth <= scanHealth && maxHealth >= scanHealth;

            if (ValidityCheck(scan.GetComponent<Entity>()) && currentTargetDistanceCheck < rangeSqrd && currentTargetDistanceCheck > minRange && withinHealthBounds)
            {
                if (scanHealth > highestHealth)
                {
                    highestHealth = scanHealth;
                    target = scan;
                }

                if (scanHealth == highestHealth && currentTargetDistanceCheck < oldTargetDistanceCheck && !furthest)
                {
                    target = scan;
                }
                else if (scanHealth == highestHealth && currentTargetDistanceCheck > oldTargetDistanceCheck)
                {
                    target = scan;
                }
            }
        }
        return target;
    }

    public Transform ReturnLowestHealth(float range, float minRange, float minHealth, float maxHealth, bool furthest, Transform partTransform)
    {
        var rangeSqrd = Mathf.Pow(range, 2);
        var lowestHealth = 0f;
        var partPositionSqrd = partTransform.position.sqrMagnitude;

        if (reticleTargeting != null && parentEntity is PlayerCore)
        {
            return reticleTargeting;
        }

        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            var scanHealth = scan.GetComponent<Entity>().GetMaxHealth()[0] + scan.GetComponent<Entity>().GetMaxHealth()[1];
            var scanSqrMagnitude = scan.position.sqrMagnitude;
            var currentTargetDistanceCheck = partPositionSqrd - scanSqrMagnitude;
            var oldTargetDistanceCheck = 0f;
            if (target != null)
            {
                oldTargetDistanceCheck = partPositionSqrd - target.position.sqrMagnitude;
            }
            bool withinHealthBounds = minHealth != -1 && maxHealth != -1 && minHealth <= scanHealth && maxHealth >= scanHealth;

            if (ValidityCheck(scan.GetComponent<Entity>()) && currentTargetDistanceCheck < rangeSqrd && currentTargetDistanceCheck > Mathf.Pow(minRange, 2) && withinHealthBounds)
            {
                if (scanHealth < lowestHealth)
                {
                    lowestHealth = scanHealth;
                    target = scan;
                }

                if (scanHealth == lowestHealth && currentTargetDistanceCheck < oldTargetDistanceCheck && !furthest)
                {
                    target = scan;
                }
                else if (scanHealth == lowestHealth && currentTargetDistanceCheck > oldTargetDistanceCheck)
                {
                    target = scan;
                }
            }
        }
        return target;
    }

    public Transform ReturnLowestCurrentHealth(float range, float minRange, float minHealth, float maxHealth, bool furthest, Transform partTransform)
    {
        var rangeSqrd = Mathf.Pow(range, 2);
        var lowestHealth = 0f;
        var partPositionSqrd = partTransform.position.sqrMagnitude;

        if (reticleTargeting != null && parentEntity is PlayerCore)
        {
            return reticleTargeting;
        }

        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            var scanHealth = scan.GetComponent<Entity>().GetHealth()[0] + scan.GetComponent<Entity>().GetHealth()[1];
            var scanSqrMagnitude = scan.position.sqrMagnitude;
            var currentTargetDistanceCheck = partPositionSqrd - scanSqrMagnitude;
            var oldTargetDistanceCheck = 0f;
            if (target != null)
            {
                oldTargetDistanceCheck = partPositionSqrd - target.position.sqrMagnitude;
            }
            bool withinHealthBounds = minHealth != -1 && maxHealth != -1 && minHealth <= scanHealth && maxHealth >= scanHealth;

            if (ValidityCheck(scan.GetComponent<Entity>()) && currentTargetDistanceCheck < rangeSqrd && currentTargetDistanceCheck > Mathf.Pow(minRange, 2) && withinHealthBounds)
            {
                if (scanHealth < lowestHealth)
                {
                    lowestHealth = scanHealth;
                    target = scan;
                }

                if (scanHealth == lowestHealth && currentTargetDistanceCheck < oldTargetDistanceCheck && !furthest)
                {
                    target = scan;
                }
                else if (scanHealth == lowestHealth && currentTargetDistanceCheck > oldTargetDistanceCheck)
                {
                    target = scan;
                }
            }
        }
        return target;
    }

    public Transform ReturnFarthest(float range, float minRange, float minHealth, float maxHealth, Transform partTransform)
    {
        var partPositionSqrd = partTransform.position.sqrMagnitude;
        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            var scanSqrMagnitude = scan.position.sqrMagnitude;
            var distanceCheck = partPositionSqrd - scanSqrMagnitude;
            var oldTargetDistanceCheck = 0f;
            if (target != null)
            {
                oldTargetDistanceCheck = partPositionSqrd - target.position.sqrMagnitude;
            }

            var scanHealth = scan.GetComponent<Entity>().GetHealth()[0] + scan.GetComponent<Entity>().GetHealth()[1];
            bool withinHealthBounds = minHealth != -1 && maxHealth != -1 && minHealth <= scanHealth && maxHealth >= scanHealth;

            if (distanceCheck > oldTargetDistanceCheck && distanceCheck < range && distanceCheck > Mathf.Pow(minRange, 2) && withinHealthBounds)
            {
                target = scan;
            }
        }
        return target;
    }


    bool ValidityCheck(Entity ent)
    {
        return (!ent.GetIsDead() && !FactionManager.IsAllied(ent.faction, parentEntity.faction) && !ent.IsInvisible);
    }

}