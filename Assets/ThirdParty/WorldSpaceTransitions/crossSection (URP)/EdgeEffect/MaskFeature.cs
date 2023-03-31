using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldSpaceTransitions
{
    public class MaskFeature : ScriptableRendererFeature
    {
        class MaskPass : ScriptableRenderPass
        {
            Material _maskMaterial = null;
            SortingCriteria _sortingCriteria;
            readonly int _renderTargetId;
            readonly ProfilingSampler _profilingSampler;
            readonly List<ShaderTagId> _shaderTagIds = new List<ShaderTagId>();

            RenderTargetIdentifier _renderTargetIdentifier;
            FilteringSettings m_FilteringSettings;
            RenderStateBlock _renderStateBlock;
            public MaskPass(string profilerTag, int renderTargetId, RenderQueueRange renderQueueRange, SortingCriteria sortingCriteria, LayerMask layerMask, Material material)
            {
                _profilingSampler = new ProfilingSampler(profilerTag);
                _renderTargetId = renderTargetId;

                m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
                this._maskMaterial = material;

                _shaderTagIds.Add(new ShaderTagId("UniversalForward"));
                _shaderTagIds.Add(new ShaderTagId("UniversalForwardOnly"));

                _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            }

            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in a performant manner.
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 32;
                descriptor.colorFormat = RenderTextureFormat.ARGB32;

                cmd.GetTemporaryRT(_renderTargetId, descriptor, FilterMode.Point);
                //ConfigureInput(ScriptableRenderPassInput.Normal);//render the Unity URP DepthNormals texture
                _renderTargetIdentifier = new RenderTargetIdentifier(_renderTargetId);

                ConfigureTarget(_renderTargetIdentifier, renderingData.cameraData.renderer.cameraDepthTarget);
                ConfigureClear(ClearFlag.All, Color.black);
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagIds, ref renderingData, _sortingCriteria);
                drawingSettings.overrideMaterial = _maskMaterial;
                CommandBuffer cmd = CommandBufferPool.Get();

                using (new ProfilingScope(cmd, _profilingSampler))
                {
                    context.DrawRenderers(renderingData.cullResults, ref drawingSettings,
                        ref m_FilteringSettings);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            // Cleanup any allocated resources that were created during the execution of this render pass.
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(_renderTargetId);
            }
        }

        MaskPass maskPass;
        const string PassTag = "Mask Prepass";
        [SerializeField] string _renderTargetId = "_EdgeMap";
        [SerializeField] LayerMask _layerMask = -1;
        // Configures where the render pass should be injected.
        [SerializeField] RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
        //[SerializeField]
        RenderQueueRange _renderQueueRange = RenderQueueRange.all;
        [SerializeField] SortingCriteria _sortingCriteria = SortingCriteria.None;
        [SerializeField] Shader maskShader;// this is a reference to ensure to get the shader into the build
        Material maskMaterial;

        public override void Create()
        {
            if (maskShader != null)
            {
                maskMaterial = CoreUtils.CreateEngineMaterial(maskShader);
            }
            else
            {
                maskMaterial = CoreUtils.CreateEngineMaterial("Hidden/CrossSectionURP/FaceSideMask");
            }
            int renderTargetId = Shader.PropertyToID(_renderTargetId);
            maskPass = new MaskPass(PassTag, renderTargetId, _renderQueueRange, _sortingCriteria, _layerMask, maskMaterial);
            // Configures where the render pass should be injected.
            maskPass.renderPassEvent = _renderPassEvent;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(maskPass);
        }
    }
}


