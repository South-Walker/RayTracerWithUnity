using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct Sphere
{
    public Vector3 position;
    public float radius;
    public Vector3 albedo;
    public Vector3 specular;
    //放出光
    public Vector3 emission;
    //镜面粗糙程度,决定模型中alpha值,在cpu中计算完其实更好
    public float smoothness;
}
public class RayTracingMaster : MonoBehaviour
{
    public Vector2 SphereRadius = new Vector2(5.0f, 30.0f);
    public uint SpheresMax = 10000;
    public float SpherePlacementRadius = 200.0f;
    //与计算着色器交互
    private ComputeBuffer _sphereBuffer;

    public Light DirectionalLight;
    public Texture SkyboxTexture;
    public ComputeShader RayTracingShader;
    public Shader AddShader;
    public int SphereSeed;
    private RenderTexture _target;
    private RenderTexture _converged;
    private Camera _camera;
    private uint _currentSample = 0;
    private Material _addMaterial;
    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }
    private void OnEnable()
    {
        Random.InitState(SphereSeed);
        _currentSample = 0;
        SetUpScene();
    }
    private void OnDisable()
    {
        if (_sphereBuffer != null)
            _sphereBuffer.Release();
    }
    private void Update()
    {
        if (transform.hasChanged)
        {
            _currentSample = 0;
            transform.hasChanged = false;
        }
        if (Time.frameCount % 100 == 0)
        {
            Debug.Log(Time.frameCount);
        }
    }
    private void SetUpScene()
    {
        List<Sphere> spheres = new List<Sphere>();
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
                :Vector3.zero;
            sphere.smoothness = Random.Range(0.5f, 1.0f);
            spheres.Add(sphere);
        }
        //填充传递缓冲区,56是因为一个结构内部有4个vector3,2个float，
        //即14个float大小，每个float为4个字节
        _sphereBuffer = new ComputeBuffer(spheres.Count, 56);
        _sphereBuffer.SetData(spheres);
    }
    private void SetShaderParameters()
    {
        Vector3 l = DirectionalLight.transform.forward;
        Vector4 directionalLight = new Vector4(l.x, l.y, l.z, DirectionalLight.intensity);
        RayTracingShader.SetVector("_DirectionalLight", directionalLight);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
        RayTracingShader.SetFloat("_Seed", Random.value);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }
    private void Render(RenderTexture destination)
    {
        InitRenderTexture();

        RayTracingShader.SetTexture(0, "Result", _target);

        int threadGroupX = Mathf.CeilToInt(Screen.width / 8f);
        int threadGroupY = Mathf.CeilToInt(Screen.height / 8f);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.Dispatch(0, threadGroupX, threadGroupY, 1);
        
        if (_addMaterial == null)
        {
            _addMaterial = new Material(AddShader);
        }
        _addMaterial.SetFloat("_Sample", _currentSample);
        //默认的destination精度不够，先渲染到_converged上
        Graphics.Blit(_target, _converged, _addMaterial);
        Graphics.Blit(_converged, destination);
        _currentSample++;
    }
    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            if (_target != null)
                _target.Release();

            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
        if (_converged == null || _converged.width != Screen.width || _converged.height != Screen.height)
        {
            if (_converged != null)
                _converged.Release();
            _converged = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _converged.enableRandomWrite = true;
            _converged.Create();
        }
    }
}
