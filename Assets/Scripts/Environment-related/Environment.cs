using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class Environment : MonoBehaviour
{
    /// <summary>
    /// Dictionary that maps a Point key (x and y coordinates in chunkspace) to the relevant Chunk, if one has been created.
    /// </summary>
    public Dictionary<Point, Chunk> chunks;
    private int chunkSize; //width and height per chunk, if constant.

    public void init()
    {
        chunks = new Dictionary<Point, Chunk>();
        chunkSize = 10;

        //temp - 3x3 asphalt chunks
        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                Chunk newChunk = makeRandomBlocksChunk(x, y, 0.3f);
                newChunk.transform.SetParent(transform);
            }
        updateCornersStandard(-10, -10, 19, 19);

    }

    private Chunk makeDefaultChunk(int chunkXcoord, int chunkYcoord)
    {
        Floor.Type typeOfFloor = Floor.Type.Asphalt;
        Block.Type typeOfEdgeBlock = Block.Type.wCement;

        string typeOfFloorName = typeOfFloor.ToString();
        string typeOfBlockName = Block.toStrings[typeOfEdgeBlock];
        Chunk chunk = new GameObject().AddComponent<Chunk>();
        chunk.name = "Default chunk " + chunkXcoord + ", " + chunkYcoord;
        chunk.chunkX = chunkXcoord;
        chunk.chunkY = chunkYcoord;
        chunk.width = chunkSize;
        chunk.height = chunkSize;
        chunk.init();
        chunks.Add(new global::Point(chunkXcoord, chunkYcoord), chunk);

        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
            {
                chunk.setFloor(x, y, typeOfFloor, typeOfFloorName);
                if (x == 0 || y == 0 || x == chunkSize - 1 || y == chunkSize - 1)
                    setBlock(chunkXcoord, chunkYcoord, x, y, typeOfEdgeBlock, false, typeOfBlockName);
            }
        return chunk;
    }

    private Chunk makeRandomBlocksChunk(int chunkXcoord, int chunkYcoord, float densityOfBlocks)
    {
        Floor.Type typeOfFloor = Floor.Type.Asphalt;

        string typeOfFloorName = typeOfFloor.ToString();
        Chunk chunk = new GameObject().AddComponent<Chunk>();
        chunk.name = "Random chunk " + chunkXcoord + ", " + chunkYcoord;
        chunk.chunkX = chunkXcoord;
        chunk.chunkY = chunkYcoord;
        chunk.width = chunkSize;
        chunk.height = chunkSize;
        chunk.init();
        chunks.Add(new global::Point(chunkXcoord, chunkYcoord), chunk);

        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
            {
                chunk.setFloor(x, y, typeOfFloor, typeOfFloorName);
                if (Random.Range(0f, 1f) < densityOfBlocks)
                    if (Random.Range(0f, 1f) < 0.5)
                        setBlock(chunkXcoord, chunkYcoord, x, y, Block.Type.wIce, false, "ice wall");
                    else
                        setBlock(chunkXcoord, chunkYcoord, x, y, Block.Type.wFlesh, false, "flesh wall");
            }
        return chunk;
    }

    /// <summary>
    /// Sets the specified block (by chunk coordinates and local coordinates) to the specified type, updating corners if needed.
    /// </summary>
    public void setBlock(int chunkX, int chunkY, int localX, int localY, Block.Type blockType, bool updateCorners, string blockName = "???")
    {
        Chunk chunk;
        //If chunk is valid
        if (chunks.TryGetValue(new global::Point(chunkX, chunkY), out chunk))
        {
            chunk.setBlock(localX, localY, blockType, blockName);
            if (updateCorners)
                updateCornersStandard((int)(chunkX * chunkSize + localX), (int)(chunkY * chunkSize + localY), (int)(chunkX * chunkSize + localX), (int)(chunkY * chunkSize + localY));
        }
        else //attempt to set a block in a nonexistent chunk
        {
            Debug.LogError("You just made an attempt to set a block in a nonexistent chunk!");
            return;
        }
    }

    /// <summary>
    /// Updates all corners that are inside or surrounding the specified rectangle of tiles, including ends. Coordinates are GLOBAL.
    /// Assumes the environment is made of chunks with equal widths and heights!
    /// <para>This function is wasteful to use if no corners actually changed. If you just need to update transparency/cracks, call updateCornerCracks or something!</para>
    /// </summary>
    public void updateCornersStandard(int startX, int startY, int endX, int endY)
    {
        Dictionary<int, int> globalToChunk = new Dictionary<int, int>();
        Dictionary<int, int> globalToLocal = new Dictionary<int, int>();
        float inverseChunkSize = 1f / chunkSize;

        for (int x = startX - 1; x <= endX + 1; x++)
        {
            globalToChunk.Add(x, div(x, inverseChunkSize));
            globalToLocal.Add(x, modulo(x, chunkSize));
        }
        for (int y = startY - 1; y <= endY + 1; y++)
        {
            if (!globalToChunk.ContainsKey(y))
                globalToChunk.Add(y, div(y, inverseChunkSize));
            if (!globalToLocal.ContainsKey(y))
                globalToLocal.Add(y, modulo(y, chunkSize));
        }


        for (int x = startX - 1; x <= endX; x++)
            for (int y = startY - 1; y <= endY; y++)
            {
                int chunkXcoord = globalToChunk[x + 1];
                int chunkYcoord = globalToChunk[y + 1];
                int localX = globalToLocal[x + 1];
                int localY = globalToLocal[y + 1];
                Chunk chunk;
                //If there is no chunk where the new junction is supposed to exist:
                if (!chunks.TryGetValue(new Point(chunkXcoord, chunkYcoord), out chunk))
                {
                    //We will create a new chunk, with nothing at all in it
                    chunk = new GameObject().AddComponent<Chunk>();
                    chunk.name = "Empty chunk " + chunkXcoord + ", " + chunkYcoord;
                    chunk.chunkX = chunkXcoord;
                    chunk.chunkY = chunkYcoord;
                    chunk.width = chunkSize;
                    chunk.height = chunkSize;
                    chunk.init();
                    chunk.transform.SetParent(transform);
                    chunks.Add(new Point(chunkXcoord, chunkYcoord), chunk);
                }

                Chunk topLeft, topRight, bottomLeft, bottomRight;
                Block topLeftBlock = null, topRightBlock = null, bottomLeftBlock = null, bottomRightBlock = null;
                if (chunks.TryGetValue(new Point(globalToChunk[x], globalToChunk[y]), out topLeft))
                    topLeftBlock = topLeft.blocks[globalToLocal[x], globalToLocal[y]];
                if (chunks.TryGetValue(new Point(globalToChunk[x + 1], globalToChunk[y]), out topRight))
                    topRightBlock = topRight.blocks[globalToLocal[x + 1], globalToLocal[y]];
                if (chunks.TryGetValue(new Point(globalToChunk[x], globalToChunk[y + 1]), out bottomLeft))
                    bottomLeftBlock = bottomLeft.blocks[globalToLocal[x], globalToLocal[y + 1]];
                if (chunks.TryGetValue(new Point(globalToChunk[x + 1], globalToChunk[y + 1]), out bottomRight))
                    bottomRightBlock = bottomRight.blocks[globalToLocal[x + 1], globalToLocal[y + 1]];
                Junction junction = Junction.createJunction(topLeftBlock, topRightBlock, bottomLeftBlock, bottomRightBlock);

                chunk.setCorner(localX, localY, junction);
            }


    }

    /// <summary>
    /// Updates corners for a single block. TODO make sure it's optimized enough.
    /// </summary>
    /// <param name="b"></param>
    public void updateCorners(Block b)
    {
        Chunk blockChunk = b.GetComponentInParent<Chunk>();
        int globalX = blockChunk.chunkX * chunkSize + (int)(b.transform.localPosition.x);
        int globalY = blockChunk.chunkY * chunkSize + (int)(b.transform.localPosition.y);
        updateCornersStandard(globalX, globalY, globalX, globalY);
    }

    /// <summary>
    /// Updates corner cracks. Assumes all corners already exists - will not create/destroy corners!
    /// </summary>
    public void updateCornerCracks(int startX, int startY, int endX, int endY)
    {
        Dictionary<int, int> globalToChunk = new Dictionary<int, int>();
        Dictionary<int, int> globalToLocal = new Dictionary<int, int>();
        float inverseChunkSize = 1f / chunkSize;

        for (int x = startX - 1; x <= endX + 1; x++)
        {
            globalToChunk.Add(x, div(x, inverseChunkSize));
            globalToLocal.Add(x, modulo(x, chunkSize));
        }
        for (int y = startY - 1; y <= endY + 1; y++)
        {
            if (!globalToChunk.ContainsKey(y))
                globalToChunk.Add(y, div(y, inverseChunkSize));
            if (!globalToLocal.ContainsKey(y))
                globalToLocal.Add(y, modulo(y, chunkSize));
        }


        for (int x = startX - 1; x <= endX; x++)
            for (int y = startY - 1; y <= endY; y++)
            {
                int chunkXcoord = globalToChunk[x + 1];
                int chunkYcoord = globalToChunk[y + 1];
                int localX = globalToLocal[x + 1];
                int localY = globalToLocal[y + 1];
                Chunk chunk = chunks[new Point(chunkXcoord, chunkYcoord)]; //assumes chunk exists
                Junction junction = chunk.corners[localX, localY];
                if (junction == null)
                    continue;
                Block topLeftBlock = chunks[new Point(globalToChunk[x], globalToChunk[y])].blocks[globalToLocal[x], globalToLocal[y]];
                Block topRightBlock = chunks[new Point(globalToChunk[x + 1], globalToChunk[y])].blocks[globalToLocal[x + 1], globalToLocal[y]];
                Block bottomLeftBlock = chunks[new Point(globalToChunk[x], globalToChunk[y + 1])].blocks[globalToLocal[x], globalToLocal[y + 1]];
                Block bottomRightBlock = chunks[new Point(globalToChunk[x + 1], globalToChunk[y + 1])].blocks[globalToLocal[x + 1], globalToLocal[y + 1]];
                if (junction.weirdJunction)
                {
                    updateCornersForStupidEdgeCaseGoddammit(chunk.corners[localX, localY], topLeftBlock, topRightBlock, bottomLeftBlock, bottomRightBlock);
                    return;
                }

                float sum = 0;
                int num = 0;
                if (Junction.sameType(junction, topLeftBlock))
                {
                    num++;
                    sum += topLeftBlock.life;
                }
                if (Junction.sameType(junction, topRightBlock))
                {
                    num++;
                    sum += topRightBlock.life;
                }
                if (Junction.sameType(junction, bottomLeftBlock))
                {
                    num++;
                    sum += bottomLeftBlock.life;
                }
                if (Junction.sameType(junction, bottomRightBlock))
                {
                    num++;
                    sum += bottomRightBlock.life;
                }

                junction.life = sum / num;
                chunk.corners[localX, localY].updateLife();
            }

    }

    /// <summary>
    /// For that dumb double junction case.
    /// </summary>
    private void updateCornersForStupidEdgeCaseGoddammit(Junction junkMom, Block topLeftBlock, Block topRightBlock, Block bottomLeftBlock, Block bottomRightBlock)
    {
        Junction[] children = junkMom.GetComponentsInChildren<Junction>();

        int firstChildIndex = Junction.sameType(children[0], topLeftBlock) ? 0 : 1;
        Junction firstChild = children[firstChildIndex], secondChild = children[1 - firstChildIndex];
        if (Junction.sameType(children[0], topRightBlock))
        {
            //AABB
            firstChild.life = (topLeftBlock.life + topRightBlock.life) / 2;
            secondChild.life = (bottomLeftBlock.life + bottomRightBlock.life) / 2;
        }
        else if (Junction.sameType(children[0], bottomLeftBlock))
        {
            //ABAB
            firstChild.life = (topLeftBlock.life + bottomLeftBlock.life) / 2;
            secondChild.life = (topRightBlock.life + bottomRightBlock.life) / 2;
        }
        else
        {
            //ABBA
            firstChild.life = (topLeftBlock.life + bottomRightBlock.life) / 2;
            secondChild.life = (topRightBlock.life + bottomLeftBlock.life) / 2;
        }
        firstChild.updateLife();
        secondChild.updateLife();
    }

    /// <summary>
    /// Fixes modulo for negatives
    /// </summary>
    private int modulo(int a, int b)
    {
        return (a % b + b) % b;
    }

    /// <summary>
    /// Fixes integer division for negatives. I'm pretty sure this works. 
    /// </summary>
    private int div(int a, float invB)
    {
        /* //If you're confused about this and mod, try printing the following:
          int number = 4; //in the code, this would be similar to chunkSize
            float inv = 1f/number;
            for (int x = -9; x < 9; x++) //9 because it's big enough to show several "chunks" of 4 (assuming number=4)
            {
                for (int y = -9; y < 9; y++)
                    Console.Write(div(x,inv)+","+div(y,inv)+";"+modulo(x,number)+","+modulo(y,number)+"    ");
                Console.WriteLine();
            }
            */
        if (a >= 0)
            return (int)(a * invB);
        else
            return (int)((a + 1) * invB) - 1;
    }

    /// <summary>
    /// Self explanatory.
    /// </summary>
    public bool chunkExists(int x, int y)
    {
        return chunks.ContainsKey(new Point(x, y));
    }
}
