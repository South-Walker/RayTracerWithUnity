# RayTracerWithUnity
利用computeshader实现的实时光线追踪 <br><br>
![](/Img/RandomSphere.png)<br><br>

## Updates
- (2020-2-9) base版发布
    - 基于[GPU Ray Tracing in Unity](http://three-eyed-games.com/blog/)思路实现
    - 具有基本的路径追踪功能
    - 支持自定义球体与来自于模型的**`MeshObject`**
    - 实现了重要性采样
    - 材质方面暂只支持简单的镜面反射与lambertian
    
## todo
> 法线插值（法线缓冲区） uv值纹理<br>
> 抖动采样
> 混合也使用compute shader，而非屏幕后处理<br>
> M矩阵更新<br>
> bvh<br>