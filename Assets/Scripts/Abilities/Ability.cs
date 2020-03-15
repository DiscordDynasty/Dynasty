﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for objects that can be executed by a handler; mainly used for abilities
/// </summary>
interface IPlayerExecutable {
    void Tick(string key); // the state of this object will change over time
}

/// <summary>
/// A trait that can be activated for a special effect; parts sometimes come with these
/// All weapons have abilities that deal their effect
/// </summary>
public abstract class Ability : MonoBehaviour, IPlayerExecutable {
    
    Entity core;  // craft that uses this ability
    public Entity Core
    {
        get
        {
            if (core == null)
                core = GetComponentInParent<Entity>();
            return core;
        }
        set
        {
            core = value;
        }
    }
    protected int ID; // Image ID, perhaps also ability ID if that were ever to be useful
    protected float cooldownDuration; // cooldown of the ability
    protected float energyCost; // energy cost of the ability
    protected float CDRemaining; // amount of time remaining on cooldown
    protected bool isOnCD = false; // check for cooldown
    protected bool isPassive = false; // if the ability is passive
    protected bool isEnabled = true; // if the ability is enabled
    protected bool isDestroyed = false; // has the part detached from the craft
    private Color originalIndicatorColor;
    protected int abilityTier;
    public ShellPart part;
    public string abilityName = "Ability";
    protected string description = "Does things";

    /// <summary>
    /// Setter method for isEnabled, will be used by parts
    /// </summary>
    /// <param name="input">boolean to set to</param>
    public void SetIsEnabled(bool input) {
        isEnabled = input; // set is enabled
    }

    public void SetTier(int abilityTier) {
        if(abilityTier > 3 || abilityTier < 0) Debug.LogError("An ability tier was set out of bounds!" + "number: " + abilityTier);
        this.abilityTier = abilityTier;
    }

    public int GetTier() {
        return abilityTier;
    }
    /// <summary>
    /// Getter method for isEnabled, will be used by the AbilityHandler
    /// </summary>
    /// <returns>true if ability is enabled, false otherwise</returns>
    public bool GetIsEnabled() {
        return isEnabled; // get is enabled
    }

    /// <summary>
    /// Getter method for ability description, will be used by the AbilityHandler
    /// </summary>
    /// <returns>ability description</returns>
    public string GetDescription()
    {
        return description;
    }

    /// <summary>
    /// Getter method for ability name, will be used by the AbilityHandler
    /// </summary>
    /// <returns>ability name</returns>
    public string GetName()
    {
        return abilityName;
    }

    /// <summary>
    /// Setter method for isDestroyed
    /// </summary>
    /// <param name="input">boolean to set to</param>
    virtual public void SetDestroyed(bool input)
    {
        isDestroyed = input; // set is destroyed
    }

    /// <summary>
    /// Getter method for isDestroyed
    /// </summary>
    /// <returns>true if ability is destroyed, false otherwise</returns>
    public bool IsDestroyed()
    {
        return isDestroyed; // get is destroyed
    }


    /// <summary>
    /// Initialization of every ability
    /// </summary>
    protected virtual void Awake() { }

    /// <summary>
    /// Get the isPassive of the ability
    /// </summary>
    /// <returns>the isPassive of the ability</returns>
    public bool GetIsPassive() {
        return isPassive; // is passive
    }

    /// <summary>
    /// Get the image ID of the ability
    /// </summary>
    /// <returns>Image ID of the ability</returns>
    public virtual int GetID() {
        return ID; // ID
    }

    /// <summary>
    /// Get the energy cost of the ability
    /// </summary>
    /// <returns>The energy cost of the ability</returns>
    public float GetEnergyCost() {
        return energyCost; // energy cost
    }

    /// <summary>
    /// Get the cooldown duration of the ability
    /// </summary>
    /// <returns>The cooldown of the ability</returns>
    public float GetCDDuration()
    {
        return cooldownDuration; // cooldown duration
    }

    /// <summary>
    /// Get the active time remaining on the ability; here defined as 0
    /// </summary>
    /// <returns>The active time remaining on the ability</returns>
    public virtual float GetActiveTimeRemaining() {
        return 0; // active time (unless overriden this is 0)
    }

    /// <summary>
    /// Get the cooldown remaining on the ability
    /// </summary>
    /// <returns>The cooldown duration remaining on the ability</returns>
    public float GetCDRemaining() {
        if (isOnCD) // on cooldown
        {
            return CDRemaining; // return the cooldown remaining, calculated prior to this call via TickDown
        }
        else return 0; // not on cooldown
    }

    /// <summary>
    /// Helper method used to update the ability's fields, usually called by Tick every update (not always)
    /// </summary>
    /// <param name="duration">Used to reset remaining; this is its default value</param>
    /// <param name="remaining">Duration to tick down</param>
    /// <param name="checkerVal">The boolean to flip once remaining ticks to 0</param>
    public void TickDown(float duration, ref float remaining, ref bool checkerVal) {
        if (remaining > Time.deltaTime) // tick down
        {
            remaining -= Time.deltaTime; // reduce by time passed since last frame
        }
        else
        {
            remaining = duration; // reset remaining
            checkerVal = false; // flip boolean
        }
    }

    /// <summary>
    /// Ability called to change the ability's state over time for players
    /// </summary>
    /// <param name="key">The associated button to press to activate</param>
    virtual public void Tick(string key) {
        if(isDestroyed)
        {
            return; // Part has been destroyed, ability can't be used
        }
        else if (isOnCD) // tick the cooldown down
        {
            TickDown(cooldownDuration, ref CDRemaining, ref isOnCD);  // tick down
        }
        else if ((!(Core as PlayerCore) || !(Core as PlayerCore).GetIsInteracting()) 
        && ((key == "activate") || (Core as PlayerCore && key != "" 
        && Input.GetKeyDown(key))) && Core.GetHealth()[2] >= energyCost) // enough energy and button pressed
        {
            Core.MakeBusy(); // make core busy
            Core.TakeEnergy(energyCost); // remove the energy
            Execute(); // execute the ability
        }
    }

    private bool blinking;
    virtual protected void ToggleIndicator(bool blink = false)
    {
        var indicator = transform.Find("Shooter");
        GameObject glowPrefab = ResourceManager.GetAsset<GameObject>("glow_prefab");
        glowPrefab = Instantiate(glowPrefab, transform, false);
        glowPrefab.transform.localScale = new Vector3(0.75F,0.75F,1);
        glowPrefab.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0.5F);
        Destroy(glowPrefab, GetActiveTimeRemaining());
        originalIndicatorColor = FactionColors.colors[Core.faction];
        if (indicator)
        {
            if(blink)
            {
                blinking = !blinking;
                if(blinking) StartCoroutine(Blinker(indicator, glowPrefab));
            }
            else indicator.GetComponent<SpriteRenderer>().color = indicator.GetComponent<SpriteRenderer>().color == Color.cyan ? originalIndicatorColor : Color.cyan;
        }
    }

    IEnumerator Blinker(Transform indicator, GameObject glowPrefab)
    {
        while(blinking)
        {
            indicator.GetComponent<SpriteRenderer>().color = indicator.GetComponent<SpriteRenderer>().color == Color.cyan ? originalIndicatorColor : Color.cyan;
            if(glowPrefab) glowPrefab.SetActive(!glowPrefab.activeSelf);
            yield return new WaitForSeconds(0.125F);
        }
    }

    /// <summary>
    /// Used to activate whatever effect the ability has, almost always overriden
    /// </summary>
    virtual protected void Execute() {
        isOnCD = true; // template to be overriden
    }
}
