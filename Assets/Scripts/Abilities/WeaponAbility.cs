﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponDiversityType {
    Strike,
    Gun,
    
    Torpedo,
    None
}


/// <summary>
/// Every ability that is used explicitly to attack other crafts is a weapon ability. 
/// These all have a respective range at which they are effective as well.
/// Their active status also does not depend on a duration and can be directly toggled on and off by the craft.
/// </summary>
public abstract class WeaponAbility : ActiveAbility {

    protected float range; // the range of the ability

    protected WeaponTargetingSystem targetingSystem;
    public Entity.TerrainType terrain = Entity.TerrainType.Unset;
    public Entity.EntityCategory category = Entity.EntityCategory.All;
    public WeaponDiversityType type = WeaponDiversityType.None;

    public bool CheckCategoryCompatibility(Entity entity)
    {
        if(type == WeaponDiversityType.Torpedo) return entity.Terrain == Entity.TerrainType.Ground;
        else return (category == Entity.EntityCategory.All || category == entity.category)
            && (terrain == Entity.TerrainType.All || terrain == entity.Terrain);
    }

    public Transform GetTarget()
    {
        return targetingSystem.target;
    }

    protected override void Awake()
    {
        switch(type) {
            case WeaponDiversityType.Strike:
                energyCost *= 0.6F;
                break;
            case WeaponDiversityType.Gun:
                cooldownDuration *= 0.6F;
                break;
            default:
                break;
        }
        isActive = true; // initialize abilities to be active
        targetingSystem = new WeaponTargetingSystem();
        targetingSystem.ability = this;
        if(abilityName == null) abilityName = "Weapon Ability";
    }

    /// <summary>
    /// Get the range of the weapon ability
    /// </summary>
    /// <returns>the range of the weapon ability</returns>
    public float GetRange() {
        return range; // get range
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    /// <summary>
    /// Override for active time remaining, just returns a value that is never greater or equal than zero if the ability is active
    /// and zero if it is not
    /// </summary>
    /// <returns>a float value that is directly based on isActive rather than a duration</returns>
    public override float GetActiveTimeRemaining()
    {
        if (isActive) return -1; // -1 is not zero so the ability is active
        else return 0; // inactive ability
    }

    /// <summary>
    /// Override for tick that integrates the targeting system of the core for players
    /// and adjusted for the new isActive behaviour
    /// </summary>
    /// <param name="key">the associated trigger key of the ability</param>
    public override void Tick(string key)
    {
        if(key != "")
        {
            if (key == "activate" || (Core as PlayerCore && Input.GetKeyDown(key)))
            { // toggle ability
                Core.MakeBusy(); // make core busy
                isActive = !isActive;
            }
        }
        if (isOnCD) // on cooldown
        {
            TickDown(cooldownDuration, ref CDRemaining, ref isOnCD); // tick the cooldown time
        }
        else if (isActive && Core.GetHealth()[2] >= energyCost && !Core.GetIsDead()) // if energy is sufficient, core isn't dead and key is pressed
        {
            Transform target = targetingSystem.GetTarget(true);
            if (target && target.GetComponent<Entity>()) { // check if there is a target
                Core.SetIntoCombat(); // now in combat
                Transform targetEntity = target;
                Entity tmp = targetEntity.GetComponent<Entity>();

                if (Vector2.Distance(Core.transform.position, targetEntity.position) <= GetRange()
                    && tmp.faction != Core.faction)
                    // check if in range
                {
                    bool success = Execute(targetEntity.position); // execute ability using the position to fire
                    if(success)
                        Core.TakeEnergy(energyCost); // take energy, if the ability was executed
                }
            }
        }
    }

    /// <summary>
    /// Unused override for weapon ability, use the position-overloaded Execute() override instead
    /// </summary>
    protected override void Execute()
    {
        Debug.Log("no argument execute is called on a weapon ability!");
        // not supposed to be called, log debug message
    }

    /// <summary>
    /// Virtual Execute() overload for weapon ability
    /// </summary>
    /// <param name="victimPos">The position to execute the ability to</param>
    /// <returns> whether or not the action was executed </returns>
    protected virtual bool Execute(Vector3 victimPos)
    {
        isOnCD = true; // set on cooldown
        return true;
    }
}
