using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const string bufferName = "Lighting";
    private CullingResults cullingResults;  //存储相机剔除后的结果
    private const int maxDirLightCount = 4; //限制最大可见平行光数量为4

    private Shadows shadows = new Shadows();

    private CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    private static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    private static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
    private static int dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

    private static Vector4[] dirLightShadowData = new Vector4[maxDirLightCount];    //存储阴影数据
    private static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    private static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        shadows.Setup(context, cullingResults, shadowSettings); //传递阴影数据
        SetupLights();
        shadows.Render();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    /// <summary>
    /// 发送多个光源数据
    /// </summary>
    private void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights; //得到所有可见光

        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)    //如果是方向光，我们才进行数据存储
            {
                SetupDirectionalLight(dirLightCount++, ref visibleLight);   //VisibleLight结构很大，我们改为传递引用，不传递值，这样不会生成副本
                if (dirLightCount >= maxDirLightCount) break;
            }
        }

        buffer.SetGlobalInt(dirLightCountId, dirLightCount);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
    }

    /// <summary>
    /// 将可见光的光照颜色和方向存储到数组
    /// </summary>
    private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        dirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, index);   //存储阴影数据
    }

    public void CleanUp()
    {
        shadows.CleanUp();
    }
}
