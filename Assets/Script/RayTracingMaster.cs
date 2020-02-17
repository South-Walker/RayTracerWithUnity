using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayTracingMaster : MonoBehaviour
{
    public Vector2 SphereRadius = new Vector2(5.0f, 30.0f);
    public uint SpheresMax = 1000;
    public float SpherePlacementRadius = 200.0f;
    public Text uiText;
    public Texture SkyboxTexture;
    public Texture SphereTexture;
    public ComputeShader RayTracingShader;
    public Shader AddShader;
    public int RandomSeed;
    private RenderTexture _target;
    private RenderTexture _converged;
    private Camera _camera;
    private static uint _currentSample = 0;
    private Material _addMaterial;
    private Sampler sampler;
    private void Start()
    {
        sampler = new MultiJitteredSampler();
        Screen.SetResolution(1280, 1024, false);
    }
    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }
    private void OnEnable()
    {
        Random.InitState(RandomSeed);
        _currentSample = 0;
        Sphere.SetRandomSpheres(SpheresMax, SphereRadius, SpherePlacementRadius);
        Sphere.SetObjectToBuffer();
    }
    private void OnDisable()
    {
        ComputeBufferHelper.Release();
    }
    private void Update()
    {
        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            Rerender();
        }
        if (Time.frameCount <= 5000)
            uiText.text = string.Format("{0} in {1} s\n{2} FPS now", Time.frameCount, (int)Time.time, (1.0 / Time.deltaTime).ToString("G3"));
    }
    private void SetShaderParameters()
    {
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetTexture(0, "_SphereTexture", SphereTexture);
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        ComputeBufferHelper.SetAllComputeBuffer(RayTracingShader);
        RayTracingShader.SetFloat("_Seed", Random.value);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Mesh.RebuildMeshObjectBuffers();
        Mesh.SetObjectToBuffer();
        SetShaderParameters();
        Render(destination);
    }
    private void Render(RenderTexture destination)
    {
        InitRenderTexture();

        RayTracingShader.SetTexture(0, "Result", _target);

        int threadGroupX = Mathf.CeilToInt(Screen.width / 8f);
        int threadGroupY = Mathf.CeilToInt(Screen.height / 8f);
        RayTracingShader.SetVector("_PixelOffset", sampler.NextOffset());
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
    public static void Rerender()
    {
        _currentSample = 0;
    }
}
