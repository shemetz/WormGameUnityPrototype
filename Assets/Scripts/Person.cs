using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : PhysicalObject
{
    #region VARIABLES
    /// <summary>
    /// Higher = rotate faster.
    /// </summary>
    public static float rotationDamping = 10f;

    /// <summary>
    /// Value that multiplies the person's friction with the floor if they are in ghost mode
    /// </summary>
    public static float ghostFrictionMultiplier = 0.7f;

    /// <summary>
    /// Number of seconds that have to pass without the person engaging in combat (attacking / being attacked) to cause inCombat to go false.
    /// </summary>
    public static float cooldownToOutOfCombat = 20;

    /// <summary>
    /// Number of seconds for something. Really doesn't matter, as long as it's higher than ~1 or so?
    /// </summary>
    public static float maxTimeBetweenDamageTexts = 60;

    /// <summary>
    /// Time between frames during which the targetInaccuracyAngle changes to a new random value within the possible inaccuracy angle "arc".
    /// </summary>
    public static float timeBetweenInaccuracyAngleChanges = 0.5f;

    /// <summary>
    /// While out of combat, the player's life/mana/stamina will regenerate in a different rate. This is by how much.
    /// </summary>
    public static float regenOutOfCombatMultiplier = 2f;

    /// <summary>
    /// ID of the person. Unique to every person, clones have the same ID. TODO make sure
    /// </summary>
    public int id;

    /// <summary>
    /// List of the EPs that gave the person their powers.
    /// </summary>
    public List<EP> DNA;

    // MAIN STATS - these should only get number bonuses, not multipliers.
    public int STRENGTH;
    public int FITNESS;
    public int DEXTERITY;
    public int WITS;
    public int KNOWLEDGE;
    public int SOCIAL;

    // SUB-STATS - these should only get number bonuses, not multipliers.
    /// <summary>
    /// Maximum value that the person's life can be.
    /// </summary>
    public float maxLife;
    /// <summary>
    /// Maximum value that the person's mana can be.
    /// </summary>
    public float maxMana;
    /// <summary>
    /// Maximum value that the person's stamina can be.
    /// </summary>
    public float maxStamina;
    // maxCharge is 100. Always.
    /// <summary>
    /// Amount of life regained per second during combat
    /// </summary>
    public float lifeRegen;
    /// <summary>
    /// Amount of mana regained per second during combat
    /// </summary>
    public float manaRegen;
    /// <summary>
    /// Amount of stamina regained per second during combat
    /// </summary>
    public float staminaRegen;
    /// <summary>
    /// Maximum speed in meters per second in a single direction, while running. If you go beyond this speed you cannot accelerate in *any* direction.
    /// </summary>
    public float runSpeed;
    /// <summary>
    /// Amount of acceleration in meters per second per second, while attempting to move on your own.
    /// </summary>
    public float runAccel;
    /// <summary>
    /// Amount of armor that is constantly part of your body - is very low.
    /// </summary>
    public float naturalArmor;
    /// <summary>
    /// Distance you see to every side, while flying, per meter of your height above ground.
    /// </summary>
    public float flightVisionDistance;
    /// <summary>
    /// Cost per second of stamina, while running.
    /// </summary>
    public float runningStaminaCost;
    /// <summary>
    /// Amount of pushback resistance/immunity you have. When dealt pushback, the amount is decreased by this stat, and possibly becomes 0 (no pushback).
    /// </summary>
    public float pushbackResistance;
    /// <summary>
    /// Multiplier of your punch speed.
    /// </summary>
    public float punchSpeedMultiplier;

    //these should only get multiplier bonuses, lower = better
    /// <summary>
    /// 1 minus the chance of an attack to miss you.
    /// </summary>
    public float oneMinusEvasionMultiplier;
    /// <summary>
    /// 1 minus the chance of critically hitting with your attack
    /// </summary>
    public float oneMinusCriticalChanceMultiplier;
    /// <summary>
    /// One minus your accuracy. An OMA of 1 means your miss angle is 90 degrees (maximum); an OMA of 0 means your miss angle is 0 degrees (minimum); and OMA of 0.5 means your miss angle is 45 degrees.
    /// </summary>
    public float oneMinusAccuracy; // from 0 to 1. 1 = 90 degree miss, 0 = 0 degree miss, 0.5 = 45 degree miss.

    // Rest of the fucking variables
    /// <summary>
    /// If you run out of life, you die.
    /// </summary>
    public float life;
    /// <summary>
    /// Mana is a resource used to activate abilities.
    /// </summary>
    public float mana;
    /// <summary>
    /// Stamina is a resource used to activate abilities, mostly punching and running/sprinting.
    /// </summary>
    public float stamina;
    /// <summary>
    /// Charge is a resource that only increases under certain condition, and usually used for specific purposes.
    /// </summary>
    public float charge;
    /// <summary>
    /// Dead men tell no tales.
    /// </summary>
    public bool dead;
    /// <summary>
    /// TODO find out!
    /// </summary>
    public float timeBetweenDamageTexts;
    /// <summary>
    /// Your voice clips depend on this. can only be "M" and "F" at the moment.
    /// </summary>
    public string voiceType;
    /// <summary>
    /// ID of this person's group leader. If there is no group, this will be the person's own ID.
    /// </summary>
    public int commanderID;
    /// <summary>
    /// Current speed of flight. Is -1 while on the ground.
    /// </summary>
    public float flySpeed;
    /// <summary>
    /// Amount of seconds that passed since the last time this person was hit/damaged.
    /// </summary>
    public float timeSinceLastHit;
    /// <summary>
    /// Time left until this person is not slipping anymore. 0 = not slipping.
    /// </summary>
    public float slippedTimeLeft;

    /// <summary>
    /// Currently targeted position in the world. Used by most abilities as the target of the power.
    /// </summary>
    [HideInInspector]
    public Vector2 target;
    /// <summary>
    /// True if this person is currently maintaining an ability.
    /// </summary>
    [HideInInspector]
    public bool maintaining; // whether or not the person is using a maintained ability like Shield or Escalating Scream
    /// <summary>
    /// True if this person is currently in combat.
    /// </summary>
    [HideInInspector]
    public bool inCombat;
    /// <summary>
    /// True if this person is inside a wall.
    /// </summary>
    [HideInInspector]
    public bool insideWall;
    /// <summary>
    /// True if this person is in ghost mode (phasing through objects).
    /// </summary>
    [HideInInspector]
    public bool ghostMode;
    /// <summary>
    /// True if this person is panicking (happens while they're on fire)
    /// </summary>
    [HideInInspector]
    public bool panic;
    /// <summary>
    /// True if this person is slipping, lying on the ground.
    /// </summary>
    [HideInInspector]
    public bool prone;
    /// <summary>
    /// True if the person is under "portal confusion" - a tilted inner direction that affects movement directions.
    /// </summary>
    [HideInInspector]
    public float timeUntilPortalConfusionIsOver = 0;
    /// <summary>
    /// Used for some calculations? TODO
    /// </summary>
    [HideInInspector]
    public float lastSpeed = 0;
    /// <summary>
    /// True if this person is currently usign a Plant Beam (Vine) and grabbling an enemy.
    /// </summary>
    [HideInInspector]
    public bool holdingVine = false;
    /// <summary>
    /// True if this person is an avatar (controlled by its creator)
    /// </summary>
    [HideInInspector]
    public bool isAvatar = false;
    /// <summary>
    /// True if this person has a Charge-based ability.
    /// </summary>
    [HideInInspector]
    public bool hasChargeAbility = false;
    /// <summary>
    /// True if this person is charging their Charge ability. TODO see if necessary
    /// </summary>
    [HideInInspector]
    public bool isChargingChargeAbility = false;
    /// <summary>
    /// True if this person is under "slow rotation" mode - for example, while using a beam.
    /// </summary>
    [HideInInspector]
    public bool slowRotation = false;
    /// <summary>
    /// TODO
    /// </summary>
    [HideInInspector]
    public int portalToOtherEnvironment = -1;
    /// <summary>
    /// TODO
    /// </summary>
    [HideInInspector]
    public float portalVariableX = 0;
    /// <summary>
    /// TODO
    /// </summary>
    [HideInInspector]
    public float portalVariableY = 0;
    /// <summary>
    /// TODO
    /// </summary>
    [HideInInspector]
    public bool startStopPossession;
    /// <summary>
    /// TODO
    /// </summary>
    [HideInInspector]
    public float possessedTimeLeft;
    /// <summary>
    /// TODO
    /// </summary>
    [HideInInspector]
    public int possessingControllerID = -1;
    /// <summary>
    /// TODO
    /// </summary>
    [HideInInspector]
    public int possessionTargetID = -1;
    /// <summary>
    /// TODO
    /// </summary>
    [HideInInspector]
    public bool possessionVessel = false;
    /// <summary>
    /// True if this person has no special abilities.
    /// </summary>
    [HideInInspector]
    public bool onlyNaturalAbilities = false;
    /// <summary>
    /// Amount of "waiting damage" (??? TODO ???)
    /// </summary>
    [HideInInspector]
    public float waitingDamage;
    /// <summary>
    /// TODO ???
    /// </summary>
    [HideInInspector]
    public float notMovingTimer = 0;
    /// <summary>
    /// TODO ???
    /// </summary>
    [HideInInspector]
    public bool notAnimating = false;
    /// <summary>
    /// 1 = up, -1 = down, 0 = neither. Could be an int? TODO
    /// </summary>
    [HideInInspector]
    public float flyDirection = 0; // 1 = up, -1 = down.
    /// <summary>
    /// True if this person is currently "Twitching" (TODO)
    /// </summary>
    [HideInInspector]
    public bool twitching;
    /// <summary>
    /// Friction of the current floor the person is standing on.
    /// </summary>
    [HideInInspector]
    private float currentFloorFriction = 0;

    // for continuous inaccuracy stuff like beams
    /// <summary>
    /// Current angle of inaccuracy. Value smoothly moves towards inaccuracyAngleTarget.
    /// </summary>
    [HideInInspector]
    public float inaccuracyAngle = 0;
    /// <summary>
    /// Current target of inaccuracy angle. Changes every time timeUntilNextInaccuracyAngle becomes 0.
    /// </summary>
    [HideInInspector]
    public float inaccuracyAngleTarget = 0;
    /// <summary>
    /// Time until the next inaccuracy angle switch. TODO where's the constant cooldown?
    /// </summary>
    [HideInInspector]
    public float timeUntilNextInaccuracyAngleChange = 0;

    //Unity stuff
    /// <summary>
    /// The animator component of this person.
    /// </summary>
    public Animator animator;

    #endregion

    protected override void Awake()
    {
        base.Awake();
        gameObject.AddComponent<AbilityActivation>();
        gameObject.layer = LayerMask.NameToLayer("People");
        rig = gameObject.AddComponent<Rigidbody2D>();
        rig.mass = 70;
        animator = gameObject.AddComponent<Animator>();
        spriteRenderer.sortingLayerName = "People";
        CircleCollider2D circColl = gameObject.AddComponent<CircleCollider2D>();
        circColl.radius = 0.24f;

        id = Person.giveID();
        commanderID = id;
        transform.position = new Vector3(transform.position.x, transform.position.y, 0); //Z = 0
        prone = false;
        dead = false;
        flySpeed = -1;
        slippedTimeLeft = 0;
        maintaining = false;
        DNA = new List<EP>();
        ghostMode = false;
        insideWall = false;
        timeSinceLastHit = 999;
        timeBetweenDamageTexts = 0;
        waitingDamage = 0;
        panic = false;
        target = new Vector2(-1, -1);

        // randomize gender
        switch (Random.Range(0, 2))
        {
            case 0:
                voiceType = "M";
                break;
            case 1:
                voiceType = "F";
                break;
            default:
                Debug.LogError("Got my A machines on the table got my B machines in the drawer");
                break;
        }

        initStats();
    }

    /// <summary>
    /// Gives 3 to STR, DEX, ... SOC, 100 life, 10 mana, sets life, mana, stamina, charge, and updates sub stats.
    /// <para>Also gives the person the Punch and Sprint abilities.</para>
    /// </summary>
    protected virtual void initStats()
    {
        // all of this is TEMP
        STRENGTH = 3;
        DEXTERITY = 3;
        FITNESS = 3;
        WITS = 3;
        KNOWLEDGE = 3;
        SOCIAL = 3;

        maxLife = 100;
        maxMana = 10;

        initSubStats();

        life = (float)maxLife;
        mana = (float)maxMana;
        stamina = (float)maxStamina;
        charge = 100;

        // Natural abilities
        Ability punch = gameObject.AddComponent<Punch>();
        punch.level = 0;
        Ability sprint = gameObject.AddComponent<Sprint>();
        sprint.level = 0;
    }

    private void initSubStats()
    {
        maxStamina = 4 + 2 * FITNESS;
        naturalArmor = STRENGTH * 0.7f + 0.3f * FITNESS;
        lifeRegen = 1;
        manaRegen = 0.5f;
        staminaRegen = 0.2f + 0.1f * FITNESS;
        runAccel = 2 * FITNESS;
        runSpeed = 1 * FITNESS;
        flightVisionDistance = 1 * WITS;
        oneMinusAccuracy = 0.6f / (DEXTERITY + 1);
        runningStaminaCost = 0.45f;
        pushbackResistance = 0;
        oneMinusEvasionMultiplier = 1;
        oneMinusCriticalChanceMultiplier = 1;
        punchSpeedMultiplier = 1;
    }

    public float getEvasion()
    {
        return 1 - oneMinusEvasionMultiplier * Mathf.Pow(0.95f, (float)(DEXTERITY * WITS) / 9f);
    }

    public float getCriticalChance()
    {
        return 1 - oneMinusCriticalChanceMultiplier * Mathf.Pow(0.95f, (float)(DEXTERITY * WITS) / 9f);
    }

    public float getPunchSpeed()
    {
        return punchSpeedMultiplier * (0.55f - Mathf.Min(0.15f, (float)(0.02f * FITNESS)));
    }

    public float getMissAngle()
    {
        return oneMinusAccuracy * Mathf.PI / 3; // DEX 0 = 60 degree miss, DEX 3 = 15.
    }

    protected virtual void Update()
    {
        float deltaTime = timeEffect * Time.deltaTime;
        if (timeSinceLastHit < Person.cooldownToOutOfCombat) // 20 seconds of no damage = combat stopped
            timeSinceLastHit += deltaTime;
        else
            inCombat = false;
        if (timeBetweenDamageTexts < Person.maxTimeBetweenDamageTexts)
            timeBetweenDamageTexts += deltaTime;
        if (timeUntilPortalConfusionIsOver > 0)
            timeUntilPortalConfusionIsOver -= deltaTime;
        if (notMovingTimer > 0)
        {
            notMovingTimer -= deltaTime;
            animator.SetFloat("Punch Time Left", notMovingTimer);
        }

        if (prone)
        {
            if (slippedTimeLeft > 0)
                slippedTimeLeft -= deltaTime;
            if (slippedTimeLeft > 1)
                animator.SetTrigger("Slipping");
            else if (slippedTimeLeft > 0 && slippedTimeLeft <= 1)
                animator.SetTrigger("Getting Up From Slipping");
            if (slippedTimeLeft <= 0)
            {
                stopSlipping();
            }
        }

        if (timeUntilNextInaccuracyAngleChange > 0)
        {
            timeUntilNextInaccuracyAngleChange -= deltaTime;
            inaccuracyAngle = Mathf.LerpAngle(inaccuracyAngle, inaccuracyAngleTarget, 2.15f * deltaTime); // 2.15 because I felt like it
        }
        else
        {
            timeUntilNextInaccuracyAngleChange = Player.timeBetweenInaccuracyAngleChanges;
            inaccuracyAngleTarget = (Random.Range(-1f, 1f)) * getMissAngle();
        }

        // TODO balance this if needed?
        if (inCombat)
        {
            life += lifeRegen * deltaTime;
            mana += manaRegen * Time.deltaTime; // mana regen is unaffected by time shenanigans
            stamina += staminaRegen * deltaTime;
        }
        else
        {
            life += lifeRegen * Player.regenOutOfCombatMultiplier * deltaTime;
            mana += manaRegen * Player.regenOutOfCombatMultiplier * Time.deltaTime; // mana regen is unaffected by time shenanigans
            stamina += staminaRegen * Player.regenOutOfCombatMultiplier * deltaTime;
        }

        // boundary checking:
        if (life > maxLife)
            life = maxLife;
        if (mana > maxMana)
            mana = maxMana;
        if (stamina > maxStamina)
            stamina = maxStamina;
        if (charge > 100)
            charge = 100;
        if (life < 0)
            life = 0;
        if (mana < 0)
            mana = 0;
        if (stamina < 0)
            stamina = 0;
        if (charge < 0)
            charge = 0;

        // DIE
        if (life == 0)
        {
            die();
        }

        Ability[] abilities = GetComponents<Ability>();
        for (int i = 0; i < abilities.Length; i++) //Some abilities will be added here!
        {
            Ability a = abilities[i];
            if (!a.hasTag("passive")) // check if ability isn't passive
            {
                if (a is Punch)
                {
                    if (a.cooldownLeft > 0)
                        a.cooldownLeft -= deltaTime;// affected by time stretching
                    if (a.cooldownLeft < 0)
                        a.cooldownLeft = 0;
                }
                else
                {
                    if (a.cooldownLeft > 0)
                        a.cooldownLeft -= Time.deltaTime;// unaffected by time stretching
                    if (a.cooldownLeft < 0)
                        a.cooldownLeft = 0;
                }
            }
            else if (!a.on) // check if this passive ability is unactivated
                if (!a.disabled)
                    a.activate();
        }

    }

    protected virtual void FixedUpdate()
    {
        //Friction with ground
        calculateFriction();
    }

    private void calculateFriction()
    {
        int floorLayer = 1 << LayerMask.NameToLayer("Floor") | 1 << LayerMask.NameToLayer("Pools");
        Collider2D[] colliders = Physics2D.OverlapPointAll(transform.position, floorLayer, 0, 1);
        float friction = 0f;
        foreach (Collider2D coll in colliders)
        {
            if (coll.gameObject.layer == LayerMask.NameToLayer("Floor"))
            {
                friction = coll.gameObject.GetComponent<Floor>().getFriction();
            }
            else if (coll.gameObject.layer == LayerMask.NameToLayer("Pools"))
            {
                //Pool friction takes priority over floor friction
                friction = coll.gameObject.GetComponent<Block>().getFriction();
                break;
            }
        }
        //NOTE: if there is no floor, i.e. the person is flying or something, then the friction is set to 0.
        if (friction > 0)
        {
            if (ghostMode)
                friction *= Person.ghostFrictionMultiplier;
            foreach (Ability a in getAbilities())
                if (a is Elastic)
                    friction *= ((Elastic)a).frictionMultiplier;
        }


        currentFloorFriction = friction;
    }

    public void startSlipping()
    {
        Debug.LogError("no code in startSlipping....... pls fix");
    }

    public void stopSlipping()
    {
        Debug.LogError("no code in stopSlipping....... pls fix");
    }

    public void die()
    {
        Debug.LogError("no code in die....... pls fix");
    }

    /// <summary>
    /// NOTICE: you must call this function every FixedUpdate, because it applies friction too!
    /// </summary>
    protected void moveInDirectionOrApplyFriction(Vector3 direction, float strengthOfMovement)
    {
        float friction = currentFloorFriction;
        if (notMovingTimer > 0)
            strengthOfMovement = 0;
        if (strengthOfMovement == 0)
        {
            animator.SetTrigger("Stop Moving");

            //Apply friction to velocity
            rig.velocity = Vector2.Lerp(rig.velocity, Vector2.zero, Time.fixedDeltaTime * runAccel * friction);

            return;
        }

        animator.SetTrigger("Start Moving");


        //Lerp velocity towards required velocity
        rig.velocity = Vector2.Lerp(rig.velocity, direction * strengthOfMovement * runSpeed, Time.fixedDeltaTime * runAccel * friction);

        //Rotate towards move direction
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.forward);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * Person.rotationDamping);
    }

    public Ability[] getAbilities()
    {
        return GetComponents<Ability>();
    }

    private static int globalID = 0;
    public static int giveID()
    {
        if (globalID == int.MaxValue)
            Debug.LogError("huh! I think we ran out of numbers.");
        globalID += 1;
        return globalID;
    }
}
