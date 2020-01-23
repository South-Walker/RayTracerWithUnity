using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
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
    public uint SpheresMax = 1000;
    public float SpherePlacementRadius = 200.0f;
    //与计算着色器交互
    private ComputeBuffer _sphereBuffer;
    public Text uiText;
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
    private void Start()
    {
        Screen.SetResolution(1280, 1024, false);
    }
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
        ReleaseComputeBuffer(_sphereBuffer);
        ReleaseComputeBuffer(_meshObjectBuffer);
        ReleaseComputeBuffer(_vertexBuffer);
        ReleaseComputeBuffer(_indexBuffer);
    }
    private void Update()
    {
        if (transform.hasChanged)
        {
            _currentSample = 0;
            transform.hasChanged = false;
        }
        if (Time.frameCount <= 5000) 
            uiText.text = string.Format("{0} in {1} s", Time.frameCount, Time.time);
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
            sphere.smoothness = Random.Range(0.7f, 1.0f);
            sphere.smoothness = SmoothnessToPhongAlpha(sphere.smoothness);
            spheres.Add(sphere);
        }
        //填充传递缓冲区,56是因为一个结构内部有4个vector3,2个float，
        //即14个float大小，每个float为4个字节
        CreateComputeBuffer(ref _sphereBuffer, spheres, 56);
    }
    private void SetShaderParameters()
    {
        Vector3 l = DirectionalLight.transform.forward;
        Vector4 directionalLight = new Vector4(l.x, l.y, l.z, DirectionalLight.intensity);
        RayTracingShader.SetVector("_DirectionalLight", directionalLight);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);

        SetComputeBuffer("_Spheres", _sphereBuffer);
        SetComputeBuffer("_MeshObjects", _meshObjectBuffer);
        SetComputeBuffer("_Vertices", _vertexBuffer);
        SetComputeBuffer("_Indices", _indexBuffer);

        RayTracingShader.SetFloat("_Seed", Random.value);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RebuildMeshObjectBuffers();
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

    private float SmoothnessToPhongAlpha(float s)
    {
        return Mathf.Pow(1000.0f, s * s);
    }

    #region convert unity mesh to ray tracer mesh
    struct MeshObject
    {
        public Matrix4x4 localToWorldMatrix;
        public int indices_offset;
        public int indices_count;
    }
    private static bool _meshObjectsNeedRebuilding = false;
    private static List<RayTracingObject> _rayTracingObjects = new List<RayTracingObject>();
    public static void RegisterObject(RayTracingObject obj)
    {
        _rayTracingObjects.Add(obj);
        _meshObjectsNeedRebuilding = true;
    }
    public static void UnRegisterObject(RayTracingObject obj)
    {
        _rayTracingObjects.Remove(obj);
        _meshObjectsNeedRebuilding = true;
    }
    private static List<MeshObject> _meshObjects = new List<MeshObject>();
    private static List<Vector3> _vertices = new List<Vector3>();
    private static List<int> _indices = new List<int>();
    private ComputeBuffer _meshObjectBuffer;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _indexBuffer;
    //可优化的增删
    private void RebuildMeshObjectBuffers()
    {
        if (!_meshObjectsNeedRebuilding)
        {
            return;
        }
        _meshObjectsNeedRebuilding = false;
        _currentSample = 0;
        _meshObjects.Clear();
        _vertices.Clear();
        _indices.Clear();
        foreach (var obj in _rayTracingObjects)
        {
            int firstVertex = _vertices.Count;
            Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
            _vertices.AddRange(mesh.vertices);
            //注意当前模型的第一个顶点并不是所有模型中的第一个顶点
            int firstIndex = _indices.Count;
            var indices = mesh.GetIndices(0);
            _indices.AddRange(indices.Select(index => index + firstVertex));
            _meshObjects.Add(new MeshObject()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,
                indices_offset = firstIndex,
                indices_count = indices.Length
            });
        }
        CreateComputeBuffer(ref _meshObjectBuffer, _meshObjects, 72);
        CreateComputeBuffer(ref _vertexBuffer, _vertices, 12);
        CreateComputeBuffer(ref _indexBuffer, _indices, 4);
    }
    private static void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride)
        where T : struct
    {
        if (buffer != null)
        {
            if (data.Count == 0 || buffer.count != data.Count ||
                buffer.stride != stride)
            {
                buffer.Release();
                buffer = null;
            }
        }
        if (data.Count != 0)
        {
            if (buffer == null)
            {
                buffer = new ComputeBuffer(data.Count, stride);
            }
            buffer.SetData(data);
        }
    }
    #endregion
    private void ReleaseComputeBuffer(ComputeBuffer buffer)
    {
        if (buffer != null)
        {
            buffer.Release();
        }
    }
    private void SetComputeBuffer(string nameinshader, ComputeBuffer buffer)
    {
        if (buffer != null)
        {
            RayTracingShader.SetBuffer(0, nameinshader, buffer);
        }
    }
}
