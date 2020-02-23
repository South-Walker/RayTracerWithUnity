using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DirectionalLight
{
    private static List<DirectionalLight> directionallights = new List<DirectionalLight>();
    public Vector3 direction;
    public Vector3 color;
    public float intensity;
    public static void BuildDirectionalLightList(IEnumerable<Light> lights)
    {
        Vector4 z = new Vector4(0, 0, 1, 0);
        foreach (var light in lights)
        {
            DirectionalLight newlight = new DirectionalLight
            {
                direction = light.transform.localToWorldMatrix * z,
                color = new Vector3(light.color.r, light.color.g, light.color.b),
                intensity = light.intensity
            };
            directionallights.Add(newlight);
        }
    }
    public static void SetObjectToBuffer()
    {
        ComputeBufferHelper.CreateComputeBuffer("_DirectionalLights", directionallights);
    }
}
