using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshObject : MonoBehaviour
{
    private void OnEnable()
    {
        Mesh.AddMesh(this);
    }
    private void OnDisable()
    {
        Mesh.RemoveMesh(this);
    }
}
