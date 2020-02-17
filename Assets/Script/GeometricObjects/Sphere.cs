using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct Sphere
{
    private static List<Sphere> spheres = new List<Sphere>();

    public Vector3 position;
    public float radius;
    public Vector3 albedo;
    public Vector3 specular;
    //放出光
    public Vector3 emission;
    //镜面粗糙程度,决定模型中alpha值,在cpu中计算完
    public float smoothness;
    public static float SmoothnessToPhongAlpha(float s)
    {
        return Mathf.Pow(1000.0f, s * s);
    }
    public static void SetRandomSpheres(uint SpheresMax, Vector3 SphereRadius, float SpherePlacementRadius)
    {
        RayTracingMaster.Rerender();
        for (int i = 0; i < SpheresMax; i++)
        {
            Sphere sphere = new Sphere();
            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            //由单位圆内随机映射为所给半径圆内随机
            Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
            //与y=0平面相切
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);
            //判断是否与已生成球重合
            bool isInOther = false;
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                {
                    isInOther = true;
                }
            }
            if (isInOther)
            {
                continue;
            }
            //一半几率生成漫反射，一半几率生成镜面反射
            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.5f;
            sphere.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;
            Color emissioncolor = Random.ColorHSV();

            sphere.emission = (Random.value > 0.85) ?
                new Vector3(emissioncolor.r, emissioncolor.g, emissioncolor.b) * Random.Range(3, 40)
                : Vector3.zero;
            sphere.smoothness = Random.Range(0.7f, 1.0f);
            sphere.smoothness = SmoothnessToPhongAlpha(sphere.smoothness);
            spheres.Add(sphere);
        }
    }
    public static void SetObjectToBuffer()
    {
        ComputeBufferHelper.CreateComputeBuffer("_Spheres", spheres);
    }
}