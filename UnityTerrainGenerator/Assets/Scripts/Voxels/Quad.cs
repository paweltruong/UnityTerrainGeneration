using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad
{
    public Mesh mesh;

    public Quad(Block.BlockSide side, Vector3 offset, MeshUtils.BlockType blockType)
    {
        mesh = new Mesh();
        mesh.name = "ScriptedQuad";

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6];


        //Vector2 uv00 = new Vector2(0, 0);
        //Vector2 uv10 = new Vector2(1, 0);
        //Vector2 uv01 = new Vector2(0, 1);
        //Vector2 uv11 = new Vector2(1, 1);

        Vector2 uv00 = MeshUtils.blockUVs[(int)blockType, 0];
        Vector2 uv10 = MeshUtils.blockUVs[(int)blockType, 1];
        Vector2 uv01 = MeshUtils.blockUVs[(int)blockType, 2];
        Vector2 uv11 = MeshUtils.blockUVs[(int)blockType, 3];

        Vector3 p0 = new Vector3(-.5f, -.5f, .5f) + offset;
        Vector3 p1 = new Vector3(.5f, -.5f, .5f) + offset;
        Vector3 p2 = new Vector3(.5f, -.5f, -.5f) + offset;
        Vector3 p3 = new Vector3(-.5f, -.5f, -.5f) + offset;
        Vector3 p4 = new Vector3(-.5f, .5f, .5f) + offset;
        Vector3 p5 = new Vector3(.5f, .5f, .5f) + offset;
        Vector3 p6 = new Vector3(.5f, .5f, -.5f) + offset;
        Vector3 p7 = new Vector3(-.5f, .5f, -.5f) + offset;

        uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
        triangles = new int[] { 3, 1, 0, 3, 2, 1 };

        switch (side)
        {
            case Block.BlockSide.BOTTOM:
                {
                    vertices = new Vector3[] { p0, p1, p2, p3 };
                    normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
                    break;
                }
            case Block.BlockSide.TOP:
                {
                    vertices = new Vector3[] { p7, p6, p5, p4 };
                    normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                    break;
                }
            case Block.BlockSide.LEFT:
                {
                    vertices = new Vector3[] { p7, p4, p0, p3 };
                    normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                    break;
                }
            case Block.BlockSide.RIGHT:
                {
                    vertices = new Vector3[] { p5, p6, p2, p1 };
                    normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                    break;
                }
            case Block.BlockSide.FRONT:
                {
                    vertices = new Vector3[] { p4, p5, p1, p0 };
                    normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                    break;
                }
            case Block.BlockSide.BACK:
                {
                    vertices = new Vector3[] { p6, p7, p3, p2 };
                    normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                    break;
                }
            default:
                break;
        }


        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
    }
}
