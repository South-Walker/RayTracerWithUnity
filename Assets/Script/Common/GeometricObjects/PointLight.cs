using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PointLight
{
    private static List<PointLight> pointlights = new List<PointLight>();
    public Vector3 position;
    public Vector3 color;
    public float intensity;
    public static void BuildPointLightList(IEnumerable<Light> lights)
    {
        foreach (var light in lights)
        {
            PointLight newlight = new PointLight
            {
                position = light.transform.position,
                color = new Vector3(light.color.r, light.color.g, light.color.b),
                intensity = light.intensity
            };
            pointlights.Add(newlight);
        }
    }
    public static void SetObjectToBuffer()
    {
        ComputeBufferHelper.CreateComputeBuffer("_PointLights", pointlights);
    }
}
