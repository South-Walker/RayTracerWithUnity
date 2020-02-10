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

- (2020-x-x) v0.1
    - 修复法线错误
    - 修复会传递空缓冲区的错误
    - 支持纹理
## todo
> 法线插值（法线缓冲区） uv值纹理<br>
> 抖动采样 简单光源支持<br>
> 混合也使用compute shader，而非屏幕后处理<br>
> M矩阵更新<br>
> bvh<br>