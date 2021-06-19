using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShadowSettings
{
    [Min(0f)]
    public float maxDistance = 100f;    //阴影最大距离
    public enum TextureSize     //阴影贴图大小
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }

    [System.Serializable]
    public struct Directional   //方向光的阴影配置
    {
        public TextureSize atlasSize;
    }

    public Directional directional = new Directional() { atlasSize = TextureSize._1024 };   //方向光默认尺寸1024
}
