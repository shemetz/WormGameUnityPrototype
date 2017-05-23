using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elastic : Ability //should be PassiveAbility
{
    public float minimumVelocityPow2 = Mathf.Pow(500, 2);
    public float frictionMultiplier = 0.1f;

    private int bonusStrength;

    public override void initStats()
    {
        damage = level * 1;
        pushback = level * 2;
        bonusStrength = level * 1;
    }

    public override void activate()
    {
        on = !on;
        user.STRENGTH += bonusStrength * (on ? 1 : -1);
    }

    public override void disable()
    {
        activate();
    }

}
