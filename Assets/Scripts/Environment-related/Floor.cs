using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : SpriteObject
{
    public enum Type { Asphalt }

    public Floor.Type type;

    protected override void Awake()
    {
        base.Awake();
        gameObject.AddComponent<BoxCollider2D>();
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(1, 1);
        gameObject.layer = LayerMask.NameToLayer("Floor");
        spriteRenderer.sortingLayerName = "Floor";
    }
    public void init()
    {
        setSprite();
    }
    /// <summary>
    /// Sets the friction of the floor according to a hardcoded switch-case code (yeah...). Also sets the sprite according to the name.
    /// <para>Sprite will be loaded from "Assets/Resources/Floors/floor_&lt;type&gt;"</para>
    /// </summary>
    public void setSprite()
    {
        string name = "***";
        switch (type)
        {
            case Floor.Type.Asphalt:
                name = "asphalt";
                break;
        }
        spriteRenderer.sprite = Resources.Load("Floors/floor_" + name, typeof(Sprite)) as Sprite;
    }
    public float getFriction()
    {
        switch (type)
        {
            case Floor.Type.Asphalt:
                return 0.95f;
            default:
                Debug.LogError("Missed friction parameter set for the folowing floor type: " + type);
                return 0;
        }
    }
}
