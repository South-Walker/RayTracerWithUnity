using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class ComputeBufferHelper
{
    //static List<ComputeBuffer> ComputeBuffers = new List<ComputeBuffer>();
    //static List<string> ComputeBufferNames = new List<string>();
    static Dictionary<string, ComputeBuffer> ComputeBuffers = new Dictionary<string, ComputeBuffer>();
    public static void Init()
    {

    }
    public static void Release()
    {
        foreach (var kvpair in ComputeBuffers)
        {
            if (kvpair.Value != null)
            {
                kvpair.Value.Release();
            }
        }
    }
    public static void Release(string name)
    {
        if (ComputeBuffers.ContainsKey(name))
        {
            if (ComputeBuffers[name] != null)
            {
                ComputeBuffers[name].Release();
                ComputeBuffers[name] = null;
            }
        }
    }
    static public void SetComputeBuffer(ComputeShader targetshader, string name)
    {
        if (ComputeBuffers[name] != null)
        {
            targetshader.SetBuffer(0, name, ComputeBuffers[name]);
        }
    }
    static public void SetAllComputeBuffer(ComputeShader targetshader)
    {
        foreach (var kvpair in ComputeBuffers)
        {
            if (kvpair.Value != null)
            {
                targetshader.SetBuffer(0, kvpair.Key, kvpair.Value);
            }
        }
    }
    public static void CreateComputeBuffer<T>(string name, List<T> data)
        where T : unmanaged
    {
        Release(name);
        //防止传入空数据
        T tail = new T();
        data.Add(tail);
        int size;
        unsafe
        {
            size = sizeof(T);
        }

        ComputeBuffers[name] = new ComputeBuffer(data.Count, size);
        ComputeBuffers[name].SetData(data);
        data.RemoveAt(data.Count - 1);
    }
}
