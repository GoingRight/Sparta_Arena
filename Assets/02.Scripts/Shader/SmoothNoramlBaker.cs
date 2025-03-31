using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class SmoothNoramlBaker : MonoBehaviour
{
    private void Awake()
    {
        SkinnedMeshRenderer[] skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer smr in skinnedMeshRenderer)
        {
            BakeSmoothNormals(smr);
            Debug.Log($"Find SMR: {smr.name}");
        }
    }

    //[ContextMenu("Bake Smooth Normal to UV1")]
    void BakeSmoothNormals(SkinnedMeshRenderer smr)
    {
        //SkinnedMeshRenderer  smr = GetComponent<SkinnedMeshRenderer>();
        if (smr == null || smr.sharedMesh == null)
            return;

        Mesh originMesh = smr.sharedMesh;
        Mesh meshInst = Instantiate(originMesh);
        meshInst.name = $"{originMesh.name}_SmoothInst";

        Vector3[] vertices = meshInst.vertices;
        Vector3[] normals = meshInst.normals;

        Dictionary<Vector3, List<int>> vertexGroup = new Dictionary<Vector3, List<int>>();

        // 버텍스 그룹화
        VertexGrp(vertices, ref vertexGroup);
        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    Vector3 key = RoundVec3(vertices[i], 0.0001f); // 정밀도 조정
        //    if (!vertexGroup.ContainsKey(key))
        //    {
        //        vertexGroup[key] = new List<int>();
        //    }
        //    vertexGroup[key].Add(i);
        //}

        // 노말 평균 찾기
        Vector3[] smoothNormals = FindNormalAVG(meshInst, vertexGroup);
        //Vector3[] smoothNormals = new Vector3[vertices.Length];
        //foreach (var grp in vertexGroup.Values)
        //{
        //    Vector3 avg = Vector3.zero;
        //    foreach (int i in grp)
        //    {
        //        avg += normals[i];
        //    }
        //    avg.Normalize();
        //    //Debug.Log($"{grp} : {avg}");

        //    foreach (int i in grp)
        //    {
        //        smoothNormals[i] = avg;
        //    }    
        //}

        meshInst.normals = smoothNormals;
        originMesh.normals = smoothNormals;
        meshInst.SetUVs(1, new List<Vector3>(smoothNormals));
        smr.sharedMesh = meshInst;

        //List<Vector3> origin_uv1 = new List<Vector3>(smoothNormals);
        //mesh.GetUVs(1, origin_uv1);

        //// UV1에 노말 평균 저장
        //List<Vector3> uv1 = new List<Vector3>(smoothNormals);
        //mesh.SetUVs(1, uv1);

        //if (!true) // Debug
        //{

        //    for (int i = 0; i < 5;  i++)
        //    {
        //        Vector3 oldUV = (i < origin_uv1.Count) ? origin_uv1[i] : Vector3.zero;
        //        Vector3 newUV = smoothNormals[i];

        //        Debug.Log($"[UV1 Debug] Vertex {i} - Original: {oldUV}, New: {newUV}");
        //    }
        //}
    }

    /// <summary>
    /// 정밀도 조정 메서드
    /// </summary>
    /// <param name="v">vertex</param>
    /// <param name="value">정밀도 조정 값</param>
    /// <returns></returns>
    Vector3 RoundVec3(Vector3 v, float value)
    {
        return new Vector3(
            Mathf.Round(v.x / value) * value,
            Mathf.Round(v.y / value) * value,
            Mathf.Round(v.z / value) * value);
    }

    void VertexGrp(Vector3[] vertices, ref Dictionary<Vector3, List<int>> vertexGroup)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 key = RoundVec3(vertices[i], 0.0001f); // 정밀도 조정
            if (!vertexGroup.ContainsKey(key))
            {
                vertexGroup[key] = new List<int>();
            }
            vertexGroup[key].Add(i);
        }
    }

    Vector3[] FindNormalAVG(Mesh mesh, Dictionary<Vector3, List<int>> vertexGroup)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector3[] smoothNormals = new Vector3[vertices.Length];
        foreach (var grp in vertexGroup.Values)
        {
            Vector3 avg = Vector3.zero;
            foreach (int i in grp)
            {
                avg += normals[i];
            }
            avg.Normalize();
            //Debug.Log($"{grp} : {avg}");

            foreach (int i in grp)
            {
                smoothNormals[i] = avg;
            }
        }

        return smoothNormals;
    }
}
