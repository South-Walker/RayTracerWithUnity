# RayTracerWithUnity
利用computeshader实现的实时光线追踪 <br><br>
![](/Img/normal.png)<br><br>左为函数构造的球，右为Unity自带的球模型

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

- (2020-x-xx) v0.2
    - 实现球体的简单纹理贴图
    - 支持点光源与平行光源
    
## todo
> 透明体
> 网格对象纹理
> 混合也使用compute shader，而非屏幕后处理<br>
> bvh<br>