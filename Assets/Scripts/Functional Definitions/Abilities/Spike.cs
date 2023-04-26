using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : ActiveAbility
{
    float gcdTemp = 0f;
    protected override void Awake()
    {
        base.Awake();
        ID = AbilityID.Spike;
        energyCost = 200f;
        cooldownDuration = 30;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        Core.WeaponGCD = gcdTemp;
    }

    protected override void Execute()
    {
        gcdTemp = Core.WeaponGCD;
        base.Execute();
        Core.WeaponGCD = 0;

    }



}
