using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A chunk is a square piece of the world, that is loaded if the player is close enough to it. When it is unloaded, it will remember where everything inside of it was,
/// by saving into a file or something, and then free up the memory for other chunks to be created without an FPS drop.
/// <para>A chunk holds three big arrays - floors, walls, pools - because each of those things is constrained to a grid. </para>
/// <para>It also holds two big arrays, wall connections and pool connections, for the overlay graphics that make those things blend nicely.</para>
/// <para>This makes it much faster to test collisions and add/remove walls.</para>
/// <para>Also, note that every grid square is a 1x1 unit in Unity.</para>
/// </summary>
public class Chunk : MonoBehaviour
{
    private static float chunkOffsetX = 0.5f, chunkOffsetY = 0.5f; //to make all blocks/floors align with the grid.

    public int chunkX, chunkY; //every chunk distance is a single increment in this form
    public int width, height; //in units

    public Floor[,] floors; //x, y
    public Block[,] blocks; //x, y
    public Junction[,] corners; // x, y; corners on the *UP-LEFT* corner of the corresponding square.
    private GameObject floorsObject, blocksObject, cornersObject;

    //NOTE - every non-grid object that is in the grid should be a child transform.
    //NOTE - the chunk's transform is its center in the world map 

    /// <summary>
    /// Call this after setting width, height, chunkX, chunkY
    /// </summary>
    public void init()
    {
        blocks = new Block[width, height];
        floors = new Floor[width, height];
        corners = new Junction[width, height];
        transform.position = new Vector3(chunkX * width + Chunk.chunkOffsetX, chunkY * height + Chunk.chunkOffsetY, 0);
        floorsObject = new GameObject();
        floorsObject.name = "Floors";
        floorsObject.transform.SetParent(transform);
        floorsObject.transform.localPosition = Vector3.zero;
        blocksObject = new GameObject();
        blocksObject.name = "Blocks";
        blocksObject.transform.SetParent(transform);
        blocksObject.transform.localPosition = Vector3.zero;
        cornersObject = new GameObject();
        cornersObject.name = "Corners";
        cornersObject.transform.SetParent(transform);
        cornersObject.transform.localPosition = Vector3.zero;
    }

    public void setFloor(int x, int y, Floor.Type floorType, string floorName = "???")
    {
        GameObject newFloor = new GameObject();
        newFloor.name = floorName + " floor " + x + "," + y;
        newFloor.transform.SetParent(floorsObject.transform); //FYI, always setParent before changing localPosition!
        newFloor.transform.localPosition = new Vector3(x, y);
        Floor floorscript = newFloor.AddComponent<Floor>();
        floorscript.type = floorType;
        floorscript.init();
        if (floors[x, y] != null)
            Destroy(floors[x, y].gameObject);
        floors[x, y] = floorscript;
    }

    /// <summary>
    /// Sets a block in the specified position, of the specified type. Does NOT update any corners.
    /// <para>blockName is here for optimization - didn't want to use enum.toString() every time. The GameObject will be called "blockName x,y" with the given values.</para>
    /// </summary>
    public void setBlock(int x, int y, Block.Type blockType, string blockName = "???")
    {
        if (blockType == Block.Type.None)
        {
            if (blocks[x, y] != null)
            {
                Destroy(blocks[x, y].gameObject);
                blocks[x, y] = null;
            }
            return;
        }
        GameObject newBlock = new GameObject();
        newBlock.name = blockName + " " + x + "," + y;
        newBlock.transform.SetParent(blocksObject.transform);
        newBlock.transform.localPosition = new Vector3(x, y);
        Block blockscript = newBlock.AddComponent<Block>();
        blockscript.type = blockType;
        blockscript.init();
        if (blocks[x, y] != null)
            Destroy(blocks[x, y].gameObject);
        blocks[x, y] = blockscript;
    }

    /// <summary>
    /// Weird method. yeah
    /// </summary>
    public void setCorner(int x, int y, Junction j)
    {
        if (j == null)
        {
            if (corners[x, y] != null)
            {
                Destroy(corners[x, y].gameObject);
            }
            corners[x, y] = j;
            return;
        }
        j.gameObject.name = "Corner " + x + "," + y;
        j.gameObject.transform.SetParent(cornersObject.transform);
        j.transform.localPosition = new Vector3(x - 0.5f, y - 0.5f);
        if (corners[x, y] != null)
            Destroy(corners[x, y].gameObject);
        corners[x, y] = j;
    }
}
