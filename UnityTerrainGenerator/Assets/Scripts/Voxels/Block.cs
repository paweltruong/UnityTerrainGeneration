using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Mesh mesh;

    public Block(Vector3 offset, MeshUtils.BlockType blockType)
    {    
        Quad[] quads = new Quad[6];
        quads[0] = new Quad(MeshUtils.BlockSide.BOTTOM, offset, blockType);
        quads[1] = new Quad(MeshUtils.BlockSide.TOP, offset, blockType);
        quads[2] = new Quad(MeshUtils.BlockSide.LEFT, offset, blockType);
        quads[3] = new Quad(MeshUtils.BlockSide.RIGHT, offset, blockType);
        quads[4] = new Quad(MeshUtils.BlockSide.FRONT, offset, blockType);
        quads[5] = new Quad(MeshUtils.BlockSide.BACK, offset, blockType);

        Mesh[] sideMeshes = new Mesh[6];
        for (int i = 0; i < quads.Length; ++i)
        {
            sideMeshes[i] = quads[i].mesh;
        }

        mesh = MeshUtils.MergeMeshes(sideMeshes);
        mesh.name = $"Cube_{offset.x}_{offset.y}_{offset.z}";
    }
}
