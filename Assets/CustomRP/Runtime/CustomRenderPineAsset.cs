using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//该标签会在Project下右键->Create菜单中添加一个新的子菜单（创建渲染管线Asset文件）
[CreateAssetMenu(menuName ="Rendering/CreateCustomRenderPipeline")]
public class CustomRenderPineAsset : RenderPipelineAsset
{
    [SerializeField]
    private bool useSRPBatcher = true;
    [SerializeField]
    private bool useDynamicBatching = true;
    [SerializeField]
    private bool useGPUInstancing = true;
    [SerializeField]
    ShadowSettings shadowSettings = default;

    //重写抽象方法，返回一个RenderPipeline实例对象
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher, shadowSettings);
    }
}
