using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 相机管理类。
/// </summary>
public partial class CameraRenderer
{
    private ScriptableRenderContext context;
    private Camera camera;
    private CullingResults cullingResults;

    private const string bufferName = "Render Camera";
    private CommandBuffer buffer = new CommandBuffer { name = bufferName };

    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");

    private Lighting lighting = new Lighting();

    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSettings)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();

        if (!Cull(shadowSettings.maxDistance)) return;

        buffer.BeginSample(SampleName);
        ExecuteBuffer();
        lighting.Setup(context, cullingResults, shadowSettings);
        buffer.EndSample(SampleName);
        ExecuteBuffer();

        Setup();
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        lighting.CleanUp();
        Submit();
    }

    /// <summary>
    /// 剔除
    /// </summary>
    private bool Cull(float maxShadowDistance)
    {
        ScriptableCullingParameters p;
        if (camera.TryGetCullingParameters(out p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);   //得到最大阴影距离，和相机远截面作比较，取最小的那个作为阴影距离
            cullingResults = context.Cull(ref p);
            return true; 
        }
        return false;
    }

    /// <summary>
    /// 设置相机的属性和矩阵
    /// </summary>
    private void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags; //得到相机的Clear Flags
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    /// <summary>
    /// 提交缓冲区渲染命令
    /// </summary>
    private void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    /// <summary>
    /// 执行CommandBuffer
    /// </summary>
    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    /// <summary>
    /// 绘制可见物
    /// </summary>
    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };  //设置绘制顺序和指定渲染相机
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);    //设置渲染的Shader Pass和排序模式
        drawingSettings.enableDynamicBatching = useDynamicBatching;     //设置渲染时使用的批处理状态
        drawingSettings.enableInstancing = useGPUInstancing;
        drawingSettings.SetShaderPassName(1, litShaderTagId);       //渲染CustomLit表示的Pass块
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);    //设置不透明的渲染队列可以被绘制
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);  //1.绘制不透明物体

        context.DrawSkybox(camera); //2.绘制天空盒

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);  //3.绘制透明物体
    }
}
