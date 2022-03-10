using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [System.Serializable]
    public enum BlockSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };

    public Material atlas;

    void Start()
    {
        MeshFilter mf = this.gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();
        mr.material = atlas;

        var offset = Vector3.zero;

        Quad[] quads = new Quad[6];
        quads[0] = new Quad(BlockSide.BOTTOM, offset);
        quads[1] = new Quad(BlockSide.TOP, offset);
        quads[2] = new Quad(BlockSide.LEFT, offset);
        quads[3] = new Quad(BlockSide.RIGHT, offset);
        quads[4] = new Quad(BlockSide.FRONT, offset);
        quads[5] = new Quad(BlockSide.BACK, offset);

        Mesh[] sideMeshes = new Mesh[6];
        for (int i = 0; i < quads.Length; ++i)
        {
            sideMeshes[i] = quads[i].mesh;
        }

        mf.mesh = MeshUtils.MergeMeshes(sideMeshes);
        mf.mesh.name = $"Cube_{offset.x}_{offset.y}_{offset.z}";
    }


    void Update()
    {
        
    }
}