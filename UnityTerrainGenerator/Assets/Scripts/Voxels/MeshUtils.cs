using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VertexData = System.Tuple<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector2>;


public static class MeshUtils
{
    [System.Serializable]
    public enum BlockType
    {
        GRASSTOP, GRASSSIDE, DIRT, WATER, STONE, SAND
    }

    public static Vector2[,] blockUVs = {
        /*GRASSTOP*/
        { new Vector2(0.125f, .375f), new Vector2(.1875f,.375f), new Vector2(.125f, .4375f), new Vector2(.1875f, .475f) },
        /*GRASSSIDE*/
        { new Vector2(0.1875f, .9375f), new Vector2(.25f,.9375f), new Vector2(.1875f, 1), new Vector2(.25f,1) },
        /*DIRT*/
        { new Vector2(0.125f, .9375f), new Vector2(.1875f,.9375f), new Vector2(.125f, 1), new Vector2(.1875f, 1) },
        /*WATER*/
        { new Vector2(0.875f, .125f), new Vector2(.9375f,.125f), new Vector2(.875f, .1875f), new Vector2(.9375f, .1875f) },
        /*STONE*/
        { new Vector2(0, .875f), new Vector2(.0625f,.875f), new Vector2(0, .9375f), new Vector2(.0625f, .9375f) },
        /*SAND*/
        { new Vector2(0.125f, .875f), new Vector2(.1875f,.875f), new Vector2(.125f, .9375f), new Vector2(.1875f, .9375f) }
    };

    public static Mesh MergeMeshes(Mesh[] meshes)
    {
        Mesh mesh = new Mesh();

        Dictionary<VertexData, int> pointsOrder = new Dictionary<VertexData, int>();
        HashSet<VertexData> pointsHash = new HashSet<VertexData>();
        List<int> tris = new List<int>();

        int pIndex = 0;
        for (int i = 0; i < meshes.Length; i++) //loop through each mesh
        {
            if (meshes[i] == null) continue;
            for (int j = 0; j < meshes[i].vertices.Length; j++) //loop through each vertex of the current mesh
            {
                Vector3 v = meshes[i].vertices[j];
                Vector3 n = meshes[i].normals[j];
                Vector2 u = meshes[i].uv[j];
                VertexData p = new VertexData(v, n, u);

                //To check if we already have this point
                if (!pointsHash.Contains(p))
                {
                    pointsOrder.Add(p, pIndex);
                    pointsHash.Add(p);

                    pIndex++;
                }
            }

            for (int t = 0; t < meshes[i].triangles.Length; t++)
            {
                //Get current mesh triable point local index
                int triPointIndex = meshes[i].triangles[t];
                Vector3 v = meshes[i].vertices[triPointIndex];
                Vector3 n = meshes[i].normals[triPointIndex];
                Vector2 u = meshes[i].uv[triPointIndex];
                VertexData p = new VertexData(v, n, u);

                int index;
                //add to global triangle vertex index list
                if (pointsOrder.TryGetValue(p, out index))
                    tris.Add(index);
            }
            meshes[i] = null;
        }
        //set mesh data
        ExtractArrays(pointsOrder, mesh);
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static void ExtractArrays(Dictionary<VertexData, int> list, Mesh mesh)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();


        foreach (var vData in list.Keys)
        {
            verts.Add(vData.Item1);
            norms.Add(vData.Item2);
            uvs.Add(vData.Item3);
        }
        mesh.vertices = verts.ToArray();
        mesh.normals = norms.ToArray();
        mesh.uv = uvs.ToArray();
    }
}
