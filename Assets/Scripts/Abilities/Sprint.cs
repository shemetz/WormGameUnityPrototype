using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// While this is on, the person using this has increased run speed but also increased stamina usage.
/// <para>This ability is maintained, which means you can't activate most abilities while sprinting.</para>
/// </summary>
public class Sprint : Ability
{
    public static float speedMultiplier = 1.6f;
    public static float accelMultiplier = 1f;
    public override void Awake()
    {
        base.Awake();
        costType = CostType.STAMINA;
        rangeType = RangeType.NONE;
        maintainable = true;
        natural = true;
    }

    public override void initStats()
    {
        costPerSecond = 3 - 0.3f * level;
    }

    public override void disable()
    {
        disabled = true;
        if (on)
        {
            on = !on;
            user.maintaining = !user.maintaining;
        }
    }

    /// <summary>
    /// Is called when the user starts / stops sprinting.
    /// </summary>
    public override void activate()
    {
        on = !on;
        user.maintaining = !user.maintaining;
        if (on)
        {
            user.runAccel = accelMultiplier * user.runAccel;
            user.runSpeed = speedMultiplier * user.runSpeed;
        }
        else
        {
            user.runAccel = 1f / accelMultiplier * user.runAccel;
            user.runSpeed = 1f / speedMultiplier * user.runSpeed;
        }
    }

    public override void maintain()
    {
        float deltaTime = Time.deltaTime;
        //If user ran out of stamina
        if (user.stamina < deltaTime * costPerSecond)
        {
            activate();
            return;
        }

        user.stamina -= deltaTime * costPerSecond;
    }
}

