using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block
{
    public Mesh mesh;

    Chunk parentChunk;

    public Block(Vector3 offset, MeshUtils.BlockType blockType, Chunk chunk)
    {
        parentChunk = chunk;

        if (blockType == MeshUtils.BlockType.AIR) return;

        List<Quad> quads = new List<Quad>();

        if (!IsNeighbourSolid((int)offset.x, (int)offset.y - 1, (int)offset.z))
            quads.Add(new Quad(MeshUtils.BlockSide.BOTTOM, offset, blockType));
        if (!IsNeighbourSolid((int)offset.x, (int)offset.y + 1, (int)offset.z))
            quads.Add(new Quad(MeshUtils.BlockSide.TOP, offset, blockType));
        if (!IsNeighbourSolid((int)offset.x - 1, (int)offset.y, (int)offset.z))
            quads.Add(new Quad(MeshUtils.BlockSide.LEFT, offset, blockType));
        if (!IsNeighbourSolid((int)offset.x + 1, (int)offset.y, (int)offset.z))
            quads.Add(new Quad(MeshUtils.BlockSide.RIGHT, offset, blockType));
        if (!IsNeighbourSolid((int)offset.x, (int)offset.y, (int)offset.z + 1))
            quads.Add(new Quad(MeshUtils.BlockSide.FRONT, offset, blockType));
        if (!IsNeighbourSolid((int)offset.x, (int)offset.y, (int)offset.z - 1))
            quads.Add(new Quad(MeshUtils.BlockSide.BACK, offset, blockType));


        if (!quads.Any())
            return;

        Mesh[] sideMeshes = new Mesh[quads.Count];
        for (int i = 0; i < quads.Count; ++i)
        {
            sideMeshes[i] = quads[i].mesh;
        }

        mesh = MeshUtils.MergeMeshes(sideMeshes);
        mesh.name = $"Cube_{offset.x}_{offset.y}_{offset.z}";
    }

    /// <summary>
    /// Used to check if we should display side of the neighbouring block
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public bool IsNeighbourSolid(int x, int y, int z)
    {
        //check on edge
        if (x < 0 || x >= parentChunk.width ||
            y < 0 || y >= parentChunk.height ||
            z < 0 || z >= parentChunk.depth)
        {
            return false;
        }

        if (parentChunk.chunkData[x + parentChunk.width * (y + parentChunk.depth * z)] == MeshUtils.BlockType.AIR
            || parentChunk.chunkData[x + parentChunk.width * (y + parentChunk.depth * z)] == MeshUtils.BlockType.WATER)
        {
            return false;
        }

        return true;
    }
}
