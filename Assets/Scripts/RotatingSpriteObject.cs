using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingSpriteObject : SpriteObject
{
    private bool rendering;
    protected override void Awake()
    {
        base.Awake();
        //Set rotation to how I want it:
        rendering = true;
        OnRenderObject();
    }
    /// <summary>
    /// The rotation, during all physics calculations, assumes that an object's "up" is the positive Z axis, and its "forward" is where it is looking (if it were a person).
    /// <para>The Unity rotation for sprites in a 3D world requires that their "forward" direction is the positive Z axis, and their "up" is the opposite of where they would be looking, assuming the base image has them looking downwards.</para>
    /// <para>So, this method is called before rendering and restores the rotation to how Unity likes it. Later, it well be good again, with OnRenderObject().</para>
    /// </summary>
    private void OnWillRenderObject()
    {
        if (rendering == false)
        {
            rendering = true;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, -1 * transform.forward);
        }
    }

    /// <summary>
    /// Reverses what OnWillRenderObject() did. See that thing for more info.
    /// </summary>
    private void OnRenderObject()
    {
        if (rendering == true)
        {
            rendering = false;
            transform.rotation = Quaternion.LookRotation(-1 * transform.up, Vector3.forward);
        }
    }
}
