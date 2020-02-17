using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Sampler
{
    protected bool hasInit;
    public abstract Vector2 NextOffset();
    public abstract void Init();
}
public class RandomSampler : Sampler
{

    public override void Init()
    {
        hasInit = true;
    }

    public override Vector2 NextOffset()
    {
        if (!hasInit)
        {
            Init();
        }
        return new Vector2(Random.value, Random.value);
    }
}
public class MultiJitteredSampler : Sampler
{
    private int n;
    private int samplenum;
    private float subcelllen;
    private Vector2[] samples;
    private int nowindex;
    private void SetSamples()
    {
        /* n=2的例子，先放置满足4-rooks采样
         * |(0,0)               |
         * |          (1,0)     |
         * |     (0,1)          |
         * |               (1,1)|
         * 随后用洗牌算法分别重新组合x,y               
         */
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                samples[i * n + j].x = (i * n + j) * subcelllen + Random.Range(0, subcelllen);
                samples[i * n + j].y = (j * n + i) * subcelllen + Random.Range(0, subcelllen);
            }
        }
        int k;
        float temp;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                k = Random.Range(j, n);
                temp = samples[i * n + j].x;
                samples[i * n + j].x = samples[i * n + k].x;
                samples[i * n + k].x = k;
            }
        }
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                k = Random.Range(j, n);
                temp = samples[i * n + j].y;
                samples[i * n + j].y = samples[i * n + k].y;
                samples[i * n + k].y = k;
            }
        }
        nowindex = 0;
    }
    public override void Init()
    {
        //在16x16网格上多重抖动采样
        n = 4;
        samplenum = 16;
        subcelllen = 1.0f / samplenum;
        samples = new Vector2[samplenum];
        SetSamples();

        hasInit = true;
    }

    public override Vector2 NextOffset()
    {
        if (!hasInit)
        {
            Init();
        }
        if (nowindex == samplenum)
        {
            SetSamples();
        }
        return samples[nowindex++];
    }
}