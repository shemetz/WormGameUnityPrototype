using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteObject : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    protected virtual void Awake()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }
}
