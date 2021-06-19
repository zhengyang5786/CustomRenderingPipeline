using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer renderer = new CameraRenderer();
    private bool useDynamicBatching = false;
    private bool useGPUInstancing = false;
    private ShadowSettings shadowSettings;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadowSettings)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.shadowSettings = shadowSettings;
    }

    /// <summary>
    /// 每帧调用。
    /// </summary>
    /// <param name="context">SRP用于渲染的最底层接口之一，通过调用其封装的各种方法来实现基本的渲染绘制</param>
    /// <param name="cameras">相机对象的数组，存储了参与这一帧渲染的所有相机对象</param>
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera, useDynamicBatching, useGPUInstancing, shadowSettings);
        }
    }
}
