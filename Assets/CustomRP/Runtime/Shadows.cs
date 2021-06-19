using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    private const string bufferName = "Shadows";
    private CullingResults cullingResults;  //存储相机剔除后的结果
    private ShadowSettings shadowSettings;
    private ScriptableRenderContext context;
    private int shadowedDirectionalLightCount = 0;
    private const int maxShadowedDirectionalLightCount = 4;
    private ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];  //存储可投影阴影的可见光源的索引

    private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    private static int dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
    private static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount];     //存储阴影转换矩阵

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    private CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        this.shadowSettings = shadowSettings;
        this.context = context;
        shadowedDirectionalLightCount = 0;
    }

    /// <summary>
    /// 渲染阴影
    /// </summary>
    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
    }

    /// <summary>
    /// 渲染定向光阴影
    /// </summary>
    private void RenderDirectionalShadows()
    {
        int atlasSize = (int)shadowSettings.directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);  //创建RenderTexture，并指定该类型是阴影贴图
        buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);   //指定渲染数据存储到RT中
        buffer.ClearRenderTarget(true, false, Color.clear); //清理深度缓冲区

        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        int split = shadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;
        for (int i = 0; i < shadowedDirectionalLightCount; i++) //遍历所有方向光渲染阴影
        {
            RenderDirectionalShadows(i, split, tileSize);
        }
        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    /// <summary>
    /// 渲染单个光源阴影
    /// </summary>
    private void RenderDirectionalShadows(int index, int split, int tileSize)
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        //使用光源Index，获取viewMatrix、projectionMatrix和shadowSplitData。shadowSplitData包含了如何剔除投影对象的信息
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f, out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);
        shadowSettings.splitData = splitData;
        dirShadowMatrices[index] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewport(index, split, tileSize), split);   //投影矩阵乘以视图矩阵，得到从世界空间到灯光空间的转换矩阵
        buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix); //设置视图投影矩阵
        ExecuteBuffer();
        context.DrawShadows(ref shadowSettings);
    }

    /// <summary>
    /// 返回一个从世界空间转到阴影纹理图块空间的矩阵
    /// </summary>
    private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        if (SystemInfo.usesReversedZBuffer) //如果使用了反向ZBuffer
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }
        //设置矩阵坐标
        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        return m;
    }

    /// <summary>
    /// 调整渲染视口来渲染单个图块
    /// </summary>
    private Vector2 SetTileViewport(int index, int split, float tileSize)
    {
        //计算索引图块的偏移位置
        Vector2 offset = new Vector2(index % split, index / split);
        //设置渲染视口，拆分多个图块
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        return offset;
    }

    /// <summary>
    /// 存储定向光数据
    /// </summary>
    public Vector2 ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        //存储可见光源的索引，前提是光源开启了阴影投射并且阴影强度不能为0。还需要判断，是否在阴影最大投射距离内，有被该光源影响且需要投影的物体存在，如果没有就不需要渲染该光源的阴影贴图了
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount && light.shadows != LightShadows.None && light.shadowStrength > 0f && cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount] = new ShadowedDirectionalLight { visibleLightIndex = visibleLightIndex };
            return new Vector2(light.shadowStrength, shadowedDirectionalLightCount++);      //返回阴影强度和阴影图块的偏移
        }
        return Vector2.zero;
    }

    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public void CleanUp()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }
}
