using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldSpaceTransitions
{
    public class EdgeFeature : ScriptableRendererFeature
    {
        class EdgePass : ScriptableRenderPass
        {
            const string ProfilerTag = "Edge Pass";
            private RenderTargetIdentifier source { get; set; }
            RenderTargetIdentifier temporaryBuffer;
            int temporaryBufferID = Shader.PropertyToID("_EdgeTexture");
            public Material edgeMaterial = null;

            public EdgePass(Material edgeMaterial)
            {
                this.edgeMaterial = edgeMaterial;
            }



            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in an performance manner.
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

                // Set the number of depth bits we need for our temporary render texture.
                descriptor.depthBufferBits = 0;

                // Enable these if your pass requires access to the CameraDepthTexture or the CameraNormalsTexture.
                // ConfigureInput(ScriptableRenderPassInput.Depth);
                // ConfigureInput(ScriptableRenderPassInput.Normal);

                // Grab the color buffer from the renderer camera color target.
                source = renderingData.cameraData.renderer.cameraColorTarget;

                // Create a temporary render texture using the descriptor from above.
                cmd.GetTemporaryRT(temporaryBufferID, descriptor, FilterMode.Bilinear);
                temporaryBuffer = new RenderTargetIdentifier(temporaryBufferID);

            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get();

                RenderTextureDescriptor opaqueDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDescriptor.depthBufferBits = 0;

                using (new ProfilingScope(cmd, new ProfilingSampler(ProfilerTag)))
                {
                    Blit(cmd, source, temporaryBuffer, edgeMaterial, 0);
                    Blit(cmd, temporaryBuffer, source);
                    //cmd.SetRenderTarget(temporaryBuffer);
                    //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, edgeMaterial);
                }
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(temporaryBufferID);
            }
        }

        public Material edgeMaterial = null;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        EdgePass edgePass;
        //RenderTargetHandle edgeTexture;

        public override void Create()
        {
            if (edgeMaterial == null)
            {
                Debug.LogWarningFormat("Missing Edge Material");
                return;
            }
            edgePass = new EdgePass(edgeMaterial);
            edgePass.renderPassEvent = renderPassEvent;//RenderPassEvent.AfterRenderingTransparents;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {

            renderer.EnqueuePass(edgePass);
        }
    }
}


