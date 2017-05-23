using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO allow punching several people. Will need to add a list of punched-already targets, probably.
/// <para>This ability, when activated, will make the user punch in a direction, hitting if there is anything in the appropriate layers that stops the short-range Raycast. Behaves differently for flying punches.</para>
/// </summary>
public class Punch : Ability
{

    public float punchRotationSpeed = 9.0f;
    public float timeNotMovingAfterPunch = 0.3f;
    public float punchHitboxTime = 0.2f;
    public int punchableLayers;

    private bool punchedSomething;
    private bool lastHandUsedIsRight;
    private Elements.DamageType damageType;

    public override void initStats()
    {
        damageType = Elements.DamageType.Blunt;
        cooldown = 0.55f - Mathf.Min(0.02f * user.FITNESS, 0.15f);
        cost = 1.5f;
        range = 0.6f;
        damage = 3 + user.STRENGTH * 0.7f; // it's fine
        pushback = 3 + user.STRENGTH * 1.5f;
    }

    public override void Awake()
    {
        base.Awake();
        costType = CostType.STAMINA;
        rangeType = RangeType.EXACT_RANGE;
        stopsMovement = true;
        instant = true;
        natural = true;

        punchedSomething = false;
        punchableLayers = 1 << LayerMask.NameToLayer("People") | 1 << LayerMask.NameToLayer("Walls");
        lastHandUsedIsRight = Random.Range(0, 2) == 1 ? true : false; //starts random
    }

    public override void disable()
    {
        disabled = true;
    }

    /// <summary>
    /// Will rotate the user slowly to face the target angle, and then laucnh a punch towards it. This function should be called every frame the button is down.
    /// </summary>
    public override void activate()
    {
        //Calculates difference between the user's rotation and the required rotation to aim towards the target point
        //Remember: the "forward" direction of every object is the positive Z axis, and the "down" direction is where they're looking at.
        Quaternion toTarget = Quaternion.LookRotation(new Vector3(user.target.x - user.transform.position.x, user.target.y - user.transform.position.y, 0), new Vector3(0, 0, 1));
        float angleDifference = Quaternion.Angle(toTarget, user.transform.rotation);

        //If standing on ground (more or less)
        if (user.flySpeed == -1 && user.transform.position.z < 1)
        {
            //If not prone, and not maintaining (or maintaining a Sprint, because Sprint-punching is allowed)
            if (!user.prone && (!user.maintaining || user.GetComponent<Sprint>().on))
                //If user has enough stamina to punch
                if (cost <= user.stamina)
                {
                    user.notMovingTimer = timeNotMovingAfterPunch;
                    user.animator.SetFloat("Punch Time Left", timeNotMovingAfterPunch);
                    user.animator.SetTrigger("Prepare To Punch");
                    if (cooldownLeft <= 0)
                    {
                        if (angleDifference < 0.2)
                        {
                            punchedSomething = false;
                            float addedAngle = user.getMissAngle() * Random.Range(-1f, 1f); // possibility to miss with a punch, of course.
                                                                                            //QUATERNION MATH WARNING! I might have made a mistake here.
                            user.transform.rotation = toTarget * Quaternion.Euler(0, 0, addedAngle); //should be equal to the toTarget rotation, rotated by a small angle to the clockwise/anticlockwise around the Z axis
                            usePunch();

                            user.animator.SetTrigger("Launch Punch");
                            lastHandUsedIsRight = !lastHandUsedIsRight;
                            user.animator.SetBool("Punch Direction Is Right", lastHandUsedIsRight);
                            user.stamina -= cost;
                            cooldownLeft = cooldown;
                        }
                        // Rotate towards target anyways
                        else
                        {
                            user.transform.rotation = Quaternion.Lerp(user.transform.rotation, toTarget, punchRotationSpeed * Time.deltaTime);
                        }
                    }
                    else if (cooldownLeft <= cooldown && cooldownLeft > cooldown - punchHitboxTime) // during a punch that still hasn't hit anything
                    {
                        if (punchedSomething == false)
                            //Keep trying to punch, since nothing was hit
                            usePunch();
                    }
                }
        }
        else if (user.transform.position.z > 1 && user.flySpeed > 0) // Fly, flight punch
        {
            if (!user.prone && !user.maintaining && cost <= user.stamina) // maybe the previous if can be inserted after the identical if of the other one? dunno
            {
                /*
				 * Flight-punches have no cooldown, and instead of activating immediately when pressing they cause the user to glide downwards while moving (like what happens when you don't press any key while flying), except they stop going lower
				 * when they're at z = 1.1, and then they keep staying at the same level. When they are close enough to an enemy (and aiming at it) they will fist it (ha) and have their stamina reduced for the cost, as always.
				 */

                if (cooldownLeft == 0)
                    punchedSomething = false;

                if (!punchedSomething)
                    if (usePunch())
                    {
                        user.stamina -= cost;
                        user.animator.SetTrigger("Launch Punch");
                        cooldownLeft = cooldown;
                    }
                    else
                        user.animator.SetTrigger("Prepare To Punch");

            }
        }
    }

    /// <summary>
    /// Attempts to punch in a direction. If there is an object within range and direction of the user, it will be punched, and the method will return true.
    /// </summary>
    private bool usePunch()
    {
        RaycastHit2D hit = Physics2D.Raycast(user.transform.position, user.transform.forward, range, punchableLayers, 0, 1);
        //NOTE: Since I disabled the "Queries start in colliders" setting in the Project's Physics2D Settings, the raycast should never hit the user's collider.
        //However, if somehow it doesn't do what I wanted it to do, you can replace hit with a RaycastHit2D[] array called "hits", use Physics2D.RaycastAll(...), and then remove hits[0].
        if (hit.collider != null)
        {
            punchedSomething = true;
            GameObject obj = hit.collider.gameObject;

            if (obj.GetComponent<Block>() != null) //alternatively, I could check what layer it is
            {
                //hit wall (not pool)
                Block block = obj.GetComponent<Block>();
                block.dealDamage(damage, pushback, user.gameObject, damageType);
            }




            return true;
        }
        return false;
    }

    public override void updatePlayerTargeting(Player player)
    {
        player.aimType = Player.AimType.NONE;
    }


}
