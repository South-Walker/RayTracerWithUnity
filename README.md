# RayTracerWithUnity
利用computeshader实现的实时光线追踪 <br><br>
![](/Img/messEarth.png)<br><br>包含一个平行光、一个有贴图函数球、一个平面、一个121个顶点的三角形网格的场景
![](/Img/Earth.png)<br><br>不包含三角形网格的场景，瓶颈可能在网格信息传递上


## Updates
- (2020-2-9) base版发布
    - 基于[GPU Ray Tracing in Unity](http://three-eyed-games.com/blog/)思路实现
    - 具有基本的路径追踪功能
    - 支持自定义球体与来自于模型的**`MeshObject`**
    - 实现了重要性采样
    - 材质方面暂只支持简单的镜面反射与lambertian

- (2020-2-11) v0.1
    - 修复法线错误
    - 修复会传递空缓冲区的错误
    - 支持多重抖动采样

- (2020-2-23) v0.2
    - 实现球体的简单纹理贴图
    - 支持点光源与平行光源
    
## todo
> 透明体<br>
> 网格对象纹理<br>
> hitable position + 西格玛值<br>
> 混合也使用compute shader，而非屏幕后处理<br>
> bvh<br>