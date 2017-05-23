using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A junction is the area in the intersection of the grid lines, defined by the four surrounding diagonal walls/pools. It exists to have a sprite that matches the walls/pools, plus have cracks/transparency depending on their health.
/// <para>The cracks are a child object.</para>
/// </summary>
public class Junction : SpriteObject
{
    /// <summary>
    /// The following notation is used:     A junction's shape is defined by four booleans, describing the four corners, so
    /// here, a junction defined by the top left, top right, bottom left, bottom right corners, in that order,
    /// will have an X for every "true" (non-empty) boolean/position and an O for every "false" one.
    /// <para>For example: a junction with two pools above it is of shape XXOO. With three walls to all but the top-left corner of it (x-1, y-1) is of shape OXXX.</para>
    /// <para>An empty junction theoretically has shape Null. But actually this object would just be null.</para>
    /// <para>Same for XOOO, OXOO, etc.  They don't have a junction for themselves.</para>
    /// </summary>
    public enum Shape
    {
        Fat,
        //OOOO,
        //XOOO, OXOO, OOXO, OOOX,
        XXOO, XOXO, OXOX, OOXX, //"up" rotated
        XOOX, OXXO, //"bridge" rotated
        XXXO, XXOX, XOXX, OXXX, //"bite" rotated
        XXXX //"full"
    };

    public enum Type
    {
        Cracks, Sad,
        //No fire, wind, electricity, energy, acid, lava walls;
        //No fire, wind, energy, metal pools;
        Fire, Water, Wind, Electricity, Metal, Ice, Energy, Acid, Lava, Flesh, Earth, Plant,
        Cement
    }

    public static int maxLife = 100;
    /// <summary>
    /// Wall = true, Pool = false
    /// </summary>
    public bool WallOrPool;
    public Type type;
    public Shape shape;
    public float life;
    /// <summary>
    /// A junction is weird if it's got two child junctions (of different types), which happens if two pairs of walls/pools are around it.
    /// </summary>
    public bool weirdJunction = false;

    /// <summary>
    /// Only call this function when the thingie is created. Otherwise, you maybe want UpdateLife()?
    /// </summary>
    public void updateSprite()
    {
        if (weirdJunction)
        {
            transform.GetChild(0).gameObject.GetComponent<Junction>().updateSprite();
            transform.GetChild(1).gameObject.GetComponent<Junction>().updateSprite(); //This one is the favorite child
            return;
        }
        string spriteString = "";
        spriteString += WallOrPool ? "w" : "p";
        spriteString += "Corner_";
        spriteString += type.ToString() + "_";
        float angle = 0;
        switch (shape)
        {
            case Shape.Fat:
                Debug.LogError("No no no no no, don't put it there!");
                return;
            case Shape.OOXX: //down
                angle += 180;
                goto case Shape.XXOO;
            case Shape.OXOX: //right
                angle += 270;
                goto case Shape.XXOO;
            case Shape.XOXO: //left
                angle += 90;
                goto case Shape.XXOO;
            case Shape.XXOO: //up
                spriteString += "up";
                break;

            case Shape.OXXO: //top-right to bottom-left
                angle += 90;
                goto case Shape.XOOX;
            case Shape.XOOX: //top-left to bottom-right
                spriteString += "bridge";
                break;

            case Shape.OXXX: //top left hole
                angle += 180;
                goto case Shape.XXXO;
            case Shape.XXOX: //bottom left hole
                angle += 270;
                goto case Shape.XXXO;
            case Shape.XOXX: //top right hole
                angle += 90;
                goto case Shape.XXXO;
            case Shape.XXXO: //bottom right hole
                spriteString += "bite";
                break;

            case Shape.XXXX:
                spriteString += "full";
                break;
        }
        spriteRenderer.sortingOrder = 2;

        spriteRenderer.sprite = Resources.Load<Sprite>("Blocks/" + spriteString);//e.g. "Blocks/wCorner_Cement_bridge"
        transform.Rotate(Vector3.forward, angle);

        gameObject.layer = LayerMask.NameToLayer("Corners"); //TODO make everything efficient, but especially this
        if (WallOrPool) //If Wall
        {
            spriteRenderer.sortingLayerName = "Walls";

            //Add child object for cracks:
            GameObject buttCrack = new GameObject();
            buttCrack.transform.SetParent(transform);
            Junction junk = buttCrack.AddComponent<Junction>();
            junk.name = "Cracks";
            junk.type = Type.Cracks;
            junk.shape = shape;
            junk.life = life;
            junk.updateCracks();
        }
        else //if pool
        {
            spriteRenderer.sortingLayerName = "Floor";
            spriteRenderer.color = new Color(1f, 1f, 1f, ((float)life) / ((float)maxLife));
        }
    }

    public void updateLife()
    {
        if (WallOrPool)
        {
            //update cracks
            Junction cracks = GetComponentInChildren<Junction>();
            cracks.life = life;
            cracks.updateCracks();
        }
    }

    /// <summary>
    /// Like updateSprite(), but for cracks! Note that crack-junctions should always have their life be 75% or less of maxLife.
    /// </summary>
    public void updateCracks()
    {
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
        spriteString += number + "_";
        spriteString += shape.ToString();
        spriteRenderer.sprite = Resources.Load<Sprite>("Blocks/Cracks/" + spriteString); //e.g. "Blocks/Cracks/cracks_2_XXOX"
        gameObject.layer = LayerMask.NameToLayer("Walls"); //TODO make everything efficient, but especially this
        spriteRenderer.sortingLayerName = "Walls";
        spriteRenderer.sortingOrder = 3;
    }

    /// <summary>
    /// Returns true if the junction fits for the blocks. False if not. Also, false if the block is null, or the junction, or both.
    /// <para>"Fits" = type matches and wallness/poolness matches</para>
    /// </summary>
    public static bool sameType(Junction j, Block b)
    {
        if (b == null || j == null)
            return false;
        string blockTypeString = b.type.ToString(); //TODO optimize if I even care
        if (blockTypeString[0] == 'w')
        {
            if (j.WallOrPool == false)
                return false;
        }
        else if (j.WallOrPool == true)
            return false;
        Junction.Type type = (Junction.Type)System.Enum.Parse(typeof(Junction.Type), blockTypeString.Substring(1));
        if (type != j.type)
            return false;
        return true;
    }

    /// <summary>
    /// Creates and returns the junction that fits a junction who's neighboring on the four input blocks, some of which can be null.
    /// <para>Will return null if there should be none there - which happens if none of them appear more than once.</para>
    /// <para>In case of ties (two Cement Walls and two Water Pools, for example), will return a junction with two children</para>
    /// </summary>
    public static Junction createJunction(Block topLeft, Block topRight, Block bottomLeft, Block bottomRight)
    {
        HashSet<Block.Type> types = new HashSet<Block.Type>();
        Block.Type firstMajority = Block.Type.None;
        Block.Type secondMajority = Block.Type.None;
        Block[] array = new Block[] { topLeft, topRight, bottomLeft, bottomRight };
        int numOfNeighbors = 0;
        foreach (Block b in array)
            if (b != null)
            {
                numOfNeighbors++;
                if (types.Contains(b.type))
                //type appears at least twice
                {
                    if (firstMajority == Block.Type.None)
                        firstMajority = b.type;
                    else if (b.type != firstMajority)
                    {
                        //exactly 2 of two different types :(
                        secondMajority = b.type;
                        break;
                    }
                }
                else
                    types.Add(b.type);
            }
        if (firstMajority == Block.Type.None) //0 or 1 junctions per type. Trivial case that will happen 70% of the time!
            return null;
        if (secondMajority == Block.Type.None) //one majority. That's the easy case that will happen 29% of the time!
        {
            string shapeString = "";
            //NOTE: I programmed this with "top" being the lower Y value, but Unity has it with a positive Y, so that's why the following 4 lines are bottom-to-top.
            shapeString += (bottomLeft == null || bottomLeft.type != firstMajority) ? "O" : "X";
            shapeString += (bottomRight == null || bottomRight.type != firstMajority) ? "O" : "X";
            shapeString += (topLeft == null || topLeft.type != firstMajority) ? "O" : "X";
            shapeString += (topRight == null || topRight.type != firstMajority) ? "O" : "X";
            Junction.Shape shape = (Junction.Shape)System.Enum.Parse(typeof(Junction.Shape), shapeString);
            Junction j = new GameObject().AddComponent<Junction>();
            j.shape = shape;
            //This area of the code probably takes the most time, with all the string conversions... TODO visit this if stuff is slow
            string blockType = firstMajority.ToString();
            if (blockType[0] == 'w') //wall
                j.WallOrPool = true;
            else if (blockType[0] == 'p') //pool
                j.WallOrPool = false;
            else
                Debug.LogError("Good evening, dear sir or madam. I seem to have found a bug: This Block thing exists, but its enum type does not start with a \"p\" or a \"w\". How peculiar! Here, see for yourself: " + blockType);
            j.type = (Junction.Type)System.Enum.Parse(typeof(Junction.Type), blockType.Substring(1));
            j.life = (
                (topLeft == null ? 0 : topLeft.life)
                + (topRight == null ? 0 : topRight.life)
                + (bottomLeft == null ? 0 : bottomLeft.life)
                + (bottomRight == null ? 0 : bottomRight.life)
                ) / numOfNeighbors;
            j.updateSprite();
            return j;
        }
        if (secondMajority != Block.Type.None) //This check is unnecessary. Like this stupid edge case.
        {
            //(Explanation for this case: This is what happens when two junctions need to be in the same place)

            string shapeString1 = "";
            shapeString1 += (bottomLeft.type != firstMajority) ? "O" : "X";
            shapeString1 += (bottomRight.type != firstMajority) ? "O" : "X";
            shapeString1 += (topLeft.type != firstMajority) ? "O" : "X";
            shapeString1 += (topRight.type != firstMajority) ? "O" : "X";
            string shapeString2 = "";
            foreach (char c in shapeString1)
                shapeString2 += c == 'O' ? 'X' : 'O';
            Junction.Shape shape1 = (Junction.Shape)System.Enum.Parse(typeof(Junction.Shape), shapeString1);
            Junction.Shape shape2 = (Junction.Shape)System.Enum.Parse(typeof(Junction.Shape), shapeString2);

            Junction favoriteChild = new GameObject().AddComponent<Junction>();
            string blockType1 = firstMajority.ToString();
            if (blockType1[0] == 'w')
                favoriteChild.WallOrPool = true;
            else if (blockType1[0] == 'p')
                favoriteChild.WallOrPool = false;
            favoriteChild.type = (Junction.Type)System.Enum.Parse(typeof(Junction.Type), blockType1.Substring(1));
            favoriteChild.shape = shape1;

            Junction theOtherFuckingChild = new GameObject().AddComponent<Junction>();
            string blockType2 = secondMajority.ToString();
            if (blockType2[0] == 'w')
                theOtherFuckingChild.WallOrPool = true;
            else if (blockType2[0] == 'p')
                theOtherFuckingChild.WallOrPool = false;
            theOtherFuckingChild.type = (Junction.Type)System.Enum.Parse(typeof(Junction.Type), blockType2.Substring(1));
            theOtherFuckingChild.shape = shape2;

            //make their lifes correct
            favoriteChild.life = 0;
            theOtherFuckingChild.life = 0;
            if (bottomLeft.type == firstMajority)
                favoriteChild.life += bottomLeft.life;
            else
                theOtherFuckingChild.life += bottomLeft.life;
            if (bottomRight.type == firstMajority)
                favoriteChild.life += bottomRight.life;
            else
                theOtherFuckingChild.life += bottomRight.life;
            if (topLeft.type == firstMajority)
                favoriteChild.life += topLeft.life;
            else
                theOtherFuckingChild.life += topLeft.life;
            if (topRight.type == firstMajority)
                favoriteChild.life += topRight.life;
            else
                theOtherFuckingChild.life += topRight.life;
            favoriteChild.life /= 2; //average
            theOtherFuckingChild.life /= 2; //
            favoriteChild.updateSprite();
            theOtherFuckingChild.updateSprite();

            Junction sadMom = new GameObject().AddComponent<Junction>();
            sadMom.shape = Junction.Shape.Fat; // :(
            sadMom.WallOrPool = false; //Irrelevant
            sadMom.type = Junction.Type.Sad; // :(
            favoriteChild.transform.SetParent(sadMom.transform);
            theOtherFuckingChild.transform.SetParent(sadMom.transform);
            theOtherFuckingChild.transform.SetAsLastSibling(); //Hmph!
            favoriteChild.name = "favorite child";
            theOtherFuckingChild.name = "that other fucking child";

            return sadMom;
        }
        //WON'T HAPPEN
        return null;
    }

    private void OnDestroy()
    {
        //Kill the children too, for good measure
        foreach (Transform childTransform in transform)
            Destroy(childTransform.gameObject);
    }
}
