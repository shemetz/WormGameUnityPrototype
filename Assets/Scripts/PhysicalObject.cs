using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalObject : RotatingSpriteObject
{
    public float timeEffect = 1f;
    public Rigidbody2D rig;
    protected override void Awake()
    {
        base.Awake();
        rig = GetComponent<Rigidbody2D>();
    }
}
