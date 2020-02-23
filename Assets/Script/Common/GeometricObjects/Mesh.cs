using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
struct Mesh
{
    private static List<Mesh> _meshObjects = new List<Mesh>();
    private static List<Vector3> _vertices = new List<Vector3>();
    private static List<Vector3> _normals = new List<Vector3>();
    private static List<int> _indices = new List<int>();
    private static List<MeshObject> _rayTracingObjects = new List<MeshObject>();

    public Matrix4x4 localToWorldMatrix;
    //localToWorldMatrix的逆反
    public Matrix4x4 localNormalToWorldMatrix;
    public int indices_offset;
    public int indices_count;

    public static bool HasSetToBuffer { get; private set; }
    public static bool NeedRebuilding { get; private set; } 
    public static void AddMesh(MeshObject obj)
    {
        _rayTracingObjects.Add(obj);
        NeedRebuilding = true;
        HasSetToBuffer = false;
    }
    public static void RemoveMesh(MeshObject obj)
    {
        _rayTracingObjects.Remove(obj);
        NeedRebuilding = true;
        HasSetToBuffer = false;
    }
    public static void RebuildMeshObjectBuffers()
    {
        if (!NeedRebuilding)
            return;
        RayTracingMaster.Rerender();


        _meshObjects.Clear();
        _vertices.Clear();
        _normals.Clear();
        _indices.Clear();
        foreach (var obj in _rayTracingObjects)
        {
            int firstVertex = _vertices.Count;
            UnityEngine.Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
            _vertices.AddRange(mesh.vertices);
            _normals.AddRange(mesh.normals);
            //注意当前模型的第一个顶点并不是所有模型中的第一个顶点
            int firstIndex = _indices.Count;
            var indices = mesh.GetIndices(0);
            _indices.AddRange(indices.Select(index => index + firstVertex));
            _meshObjects.Add(new Mesh()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,
                localNormalToWorldMatrix = obj.transform.localToWorldMatrix.inverse.transpose,
                indices_offset = firstIndex,
                indices_count = indices.Length
            });
        }
        NeedRebuilding = false;
    }
    public static void SetObjectToBuffer()
    {
        if (HasSetToBuffer)
            return;
        ComputeBufferHelper.CreateComputeBuffer("_Meshes", _meshObjects);
        ComputeBufferHelper.CreateComputeBuffer("_Vertices", _vertices);
        ComputeBufferHelper.CreateComputeBuffer("_Normals", _normals);
        ComputeBufferHelper.CreateComputeBuffer("_Indices", _indices);
        HasSetToBuffer = true;
    }
}