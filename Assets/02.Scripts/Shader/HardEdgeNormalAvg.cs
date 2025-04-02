using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardEdgeNormalAvg : MonoBehaviour
{
    public static void MeshNormalAverage(Mesh mesh)
    {
        Dictionary<Vector3, List<int>> map = new Dictionary<Vector3, List<int>>();

        #region build the map of vertex and triangles' relation
        for (int v = 0; v < mesh.vertexCount; ++v)
        {
            if (!map.ContainsKey(mesh.vertices[v]))
            {
                map.Add(mesh.vertices[v], new List<int>());
            }
            map[mesh.vertices[v]].Add(v);
        }
        #endregion

        Vector3[] normals = mesh.normals;
        Vector3 normal;

        #region the same vertex use the same normal(average)
        foreach (var p in map)
        {
            normal = Vector3.zero;

            foreach (var n in p.Value)
            {
                normal += mesh.normals[n];
            }

            normal /= p.Value.Count;

            foreach (var n in p.Value)
            {
                normals[n] = normal;
            }
        }
        #endregion

        mesh.normals = normals;
    }

}
