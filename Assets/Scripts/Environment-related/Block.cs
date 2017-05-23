using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A wall or a pool.
/// </summary>
public class Block : SpriteObject
{
    public enum Type
    {
        None,
        wMetal, wIce, wFlesh, wEarth, wPlant,
        pWater, pIce, pAcid, pLava, pFlesh, pEarth, pPlant,
        wCement,
    };

    public static Dictionary<Block.Type, string> toStrings;

    public Type type;

    public float life;
    public float armor;
    public static float maxLife = 100f;
    public static float wallArmor = 10f; //temp?

    protected override void Awake()
    {
        base.Awake();
        gameObject.AddComponent<BoxCollider2D>();
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(1, 1);
        life = Block.maxLife;
    }

    /// <summary>
    /// Call this after setting the value of "type".
    /// </summary>
    public void init()
    {
        switch (type)
        {
            case Type.wMetal:
            case Type.wIce:
            case Type.wFlesh:
            case Type.wEarth:
            case Type.wPlant:
            case Type.wCement:
                gameObject.layer = LayerMask.NameToLayer("Walls"); //TODO optimize
                spriteRenderer.sortingLayerName = "Walls";
                spriteRenderer.sortingOrder = 0; //corners are 2
                break;
            case Type.pWater:
            case Type.pIce:
            case Type.pAcid:
            case Type.pLava:
            case Type.pFlesh:
            case Type.pEarth:
            case Type.pPlant:
                gameObject.layer = LayerMask.NameToLayer("Pools");
                spriteRenderer.sortingLayerName = "Floor";
                spriteRenderer.sortingOrder = 1; //floors are 0, corners are 2
                break;
            case Type.None:
                Debug.LogError("None type for this wall/pool!");
                break;
            default:
                Debug.LogError("Incorrect/nonexisting type of wall/pool: " + type.ToString());
                break;
        }
        setSprite();
        if (gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            GameObject cracks = new GameObject();
            cracks.transform.SetParent(this.transform);
            cracks.name = "Cracks";
            SpriteRenderer spree = cracks.AddComponent<SpriteRenderer>();
            spree.sortingLayerName = "Walls";
            spree.sortingOrder = 1;
        }
    }

    /// <summary>
    /// Sets the sprite according to the type.
    /// <para>Sprite will be loaded from "Assets/Resources/Floors/floor_&lt;type&gt;"</para>
    /// </summary>
    public void setSprite()
    {
        string typeName = toStrings[type];
        spriteRenderer.sprite = Resources.Load("Blocks/" + typeName, typeof(Sprite)) as Sprite;
    }

    /// <summary>
    /// Deals damage to this block (will only be called for walls). the controller should be a Person's object, not a projectile/ability. Will update corners (and might damage connected walls).
    /// </summary>
    public void dealDamage(float damage, float pushback, GameObject damageSourceController, Elements.DamageType damageType)
    {
        //No damage is dealt if the block's damage type is equal to the source's damage type, as long as they aren't the normal types (blunt/sharp)
        float finalDamage = damage + pushback - armor;
        if (finalDamage < 1)
            return;
        if (damageType != Elements.DamageType.Blunt && damageType != Elements.DamageType.Sharp && damageType == getDamageType())
            return;
        finalDamage *= elementEffectiveness(damageType);
        life -= finalDamage;

        if (life <= 0)
            destroy();
        else
            updateChildCracks();
        if (finalDamage >= 30 - armor)
            damageConnectedWalls(finalDamage + armor, damageSourceController, damageType);
        updateCorners();
    }

    /// <summary>
    /// Destroys this wall. Will NOT update nearby corners. Will create debris.
    /// </summary>
    public void destroy()
    {
        Environment env = GetComponentInParent<Environment>(); //neat function!
        Chunk chunk = GetComponentInParent<Chunk>();
        env.setBlock(chunk.chunkX, chunk.chunkY, (int)(transform.localPosition.x), (int)(transform.localPosition.y), Block.Type.None, true);
        //TODO debris
    }

    /// <summary>
    /// Is called when the wall is heavily damaged (30+ damage). Will damage nearby walls. Will update corners.
    /// </summary>
    public void damageConnectedWalls(float damage, GameObject damageSourceController, Elements.DamageType damageType)
    {
        float radius = damage / 11; //why 11? I dunno
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.NameToLayer("Walls"));
        float minX = transform.position.x, minY = transform.position.y, maxX = transform.position.x, maxY = transform.position.y;
        foreach (Collider2D c in colliders)
        {
            Block wall = c.gameObject.GetComponent<Block>();
            if (wall == null)
                Debug.LogError("Pastramamia! " + c.gameObject.name);
            if (wall.transform.position == this.transform.position)
                continue;
            //damage = damage divided by the distance
            float itsDamage = damage / (c.gameObject.transform.position - transform.position).magnitude;
            wall.nonRecursiveDealDamage(itsDamage, damageType);

            minX = Mathf.Min(minX, c.transform.position.x);
            minY = Mathf.Min(minY, c.transform.position.y);
            maxX = Mathf.Max(maxX, c.transform.position.x);
            maxY = Mathf.Max(maxY, c.transform.position.y);
        }
        Environment env = GetComponentInParent<Environment>(); //neat function!
        env.updateCornersStandard((int)minX, (int)minY, (int)maxX, (int)maxY); //TODO make sure I don't need to add 0.5 or something, because of chunk offset
    }

    /// <summary>
    /// Is called when walls next to the main dmaged walls are damaged by proxy. Doesn't call damageConnectedWalls, hence the "non recursive". Will NOT update corners.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="damageType"></param>
    public void nonRecursiveDealDamage(float damage, Elements.DamageType damageType)
    {
        //No damage is dealt if the block's damage type is equal to the source's damage type, as long as they aren't the normal types (blunt/sharp)
        float finalDamage = damage - armor;
        if (finalDamage < 1)
            return;
        if (damageType != Elements.DamageType.Blunt && damageType != Elements.DamageType.Sharp && damageType == getDamageType())
            return;
        finalDamage *= elementEffectiveness(damageType);
        life -= finalDamage;

        if (life <= 0)
            destroy();
        else
            updateChildCracks();
    }

    /// <summary>
    /// Updates cracks in child object, if this is a wall. TODO optimize this so that the sprite is only changed when life goes from one "percentage group" to the next.
    /// </summary>
    private void updateChildCracks()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            SpriteRenderer spree = GetComponentInChildren<SpriteRenderer>();
            string spriteString = "cracks_";
            int number = -1;
            if (life <= 0.25 * maxLife)
                number = 2;
            else if (life <= 0.50 * maxLife)
                number = 1;
            else if (life <= 0.75 * maxLife)
                number = 0;
            if (number == -1)
                return;
            spriteString += number + "_wall";
            spree.sprite = Resources.Load<Sprite>("Blocks/Cracks/" + spriteString); //e.g. "Blocks/Cracks/cracks_2_wall
        }
    }

    /// <summary>
    /// Updates corners around this block, in the surrounding environment.
    /// </summary>
    public void updateCorners()
    {
        Environment env = GetComponentInParent<Environment>(); //neat function!
        env.updateCorners(this);
    }

    /// <summary>
    /// Returns the effectiveness multiplier. Default is 1. Is 2 if the attack is super-effective (e.g. Fire VS Plant) and 0.5 if it's not efective (e.g. Acid VS Cement). Hard-coded!
    /// </summary>
    /// <param name="damageType"></param>
    /// <returns></returns>
    public float elementEffectiveness(Elements.DamageType damageType)
    {
        switch (damageType)
        {
            case Elements.DamageType.Blunt:
                return 1;
            case Elements.DamageType.Sharp:
                return 1;
            case Elements.DamageType.Burn:
                if (type == Type.wPlant)
                    return 2;
                if (type == Type.wIce)
                    return 2;
                if (type == Type.wFlesh)
                    return 2f;
                if (type == Type.wMetal)
                    return 0.5f;
                if (type == Type.wCement)
                    return 0.5f;
                if (type == Type.wEarth)
                    return 0.5f;
                return 1;
            case Elements.DamageType.Acid:
                if (type == Type.wFlesh)
                    return 2;
                if (type == Type.wMetal)
                    return 2f;
                if (type == Type.wIce)
                    return 0.5f;
                if (type == Type.wCement)
                    return 0.5f;
                return 1;
            case Elements.DamageType.Shock:
                if (type == Type.wFlesh)
                    return 2;
                if (type == Type.wMetal)
                    return 0f;
                return 0.5f;
            default:
                Debug.LogError("Weird damage type for the damage of the damag source that dealt damage to this damaged block pls halp");
                return 10;
        }
    }

    /// <summary>
    /// Returns the damage type of this block. For example - a metal wall is Blunt. A pool of lava is Burn. If this block can't deal damage (e.g. pool of water), this shold not be called.
    /// </summary>
    /// <returns></returns>
    public Elements.DamageType getDamageType()
    {
        switch (type)
        {
            case Type.wMetal:
            case Type.wEarth:
            case Type.wPlant:
            case Type.wCement:
                return Elements.DamageType.Blunt;
            case Type.wIce:
            case Type.wFlesh:
            case Type.pPlant: //stabby vines
            case Type.pEarth: //stabby spikes
                return Elements.DamageType.Sharp;
            case Type.pLava:
                return Elements.DamageType.Burn; // fire
            case Type.pAcid:
                return Elements.DamageType.Acid; // acid
            default:
                Debug.LogError("This element doesn't have a block damage type! " + type);
                return Elements.DamageType.Error;
        }
    }

    /// <summary>
    /// If this is a pool, returns the friction for it. Normal floor friction is ~0.9, slippery surfaces like water and ice are 0.10.
    /// </summary>
    /// <returns></returns>
    public float getFriction()
    {
        switch (type)
        {
            case Type.pWater:
                return 0.10f;
            case Type.pIce:
                return 0.05f;
            case Type.pAcid:
                return 0.15f;
            case Type.pLava:
                return 1.0f;
            case Type.pFlesh: //pool of blood
                return 0.10f;
            case Type.pEarth: //...spikes
                return 1.0f;
            case Type.pPlant: //......plant spikes?
                return 0.95f;
            case Type.wMetal:
            case Type.wIce:
            case Type.wFlesh:
            case Type.wEarth:
            case Type.wPlant:
            case Type.wCement:
                Debug.LogError("...No. walls don't have friction. stop this.");
                return 0;
            case Type.None:
                Debug.LogError("This bug might actually happen. Codeword: giraffe alabama savant.");
                return 0;
            default:
                Debug.LogError("This error will never come up! Haha I shouldn't be worried here. But if it does - it means you forgot to add friction to a new pool thingie.");
                return 0;
        }
    }

    /// <summary>
    /// Should optimize toString() calls
    /// </summary>
    public static void initializeStrings()
    {
        toStrings = new Dictionary<Type, string>();
        foreach (Type t in System.Enum.GetValues(typeof(Type)))
        {
            string fileName = t.ToString();
            if (fileName[0] == 'w')
                fileName = "wall_" + fileName.Substring(1);
            else if (fileName[0] == 'p')
                fileName = "pool_" + fileName.Substring(1);
            else if (fileName == "None")
                continue;
            else
                Debug.LogError("Weird type of block discovered! " + fileName);
            toStrings.Add(t, fileName);
        }
    }
}
