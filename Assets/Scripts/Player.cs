using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Person
{
    /// <summary>
    /// Mid-Click, Shift, Q, E, R, F, V, C, X, Z
    /// </summary>
    [HideInInspector]
    public int[] hotkeys;
    [HideInInspector]
    public int[] oldHotkeys = null;

    [HideInInspector]
    public enum AimType
    {
        CREATE_FF, PORTALS, CREATE_IN_GRID, TARGET_IN_RANGE, EXPLOSION, TELEPORT, LOOP, WILD_POWER, CLONE, AIMLESS, NONE
    };

    [HideInInspector]
    public AimType aimType = AimType.NONE;

    [HideInInspector]
    public bool successfulTarget;

    private void Start()
    {
    }

    protected override void Awake()
    {
        base.Awake();
        hotkeys = new int[10];
        for (int i = 0; i < 10; i++)
            hotkeys[i] = -1;
        defaultHotkeys();

        rig.constraints = RigidbodyConstraints2D.FreezeRotation;

        //Testing
        spriteRenderer.sprite = Resources.Load("temp/temp person stand", typeof(Sprite)) as Sprite;
        animator.runtimeAnimatorController = Resources.Load("temp/temp person", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, vertical, 0);

        float strengthOfMovement = (direction == Vector3.zero ? 0 : 1);
        direction.Normalize();
        moveInDirectionOrApplyFriction(direction, strengthOfMovement);
    }
    public void defaultHotkeys()
    {
        hotkeys = new int[10];
        Ability[] abilities = getAbilities();
        int k = 0;
        for (int i = 0; i < abilities.Length && k < hotkeys.Length; i++, k++)
            if (abilities[i].hasTag("passive"))
                k--;
            else
                hotkeys[k] = i;
        if (k < hotkeys.Length)
            for (; k < hotkeys.Length; k++)
                hotkeys[k] = -1;
    }
}
