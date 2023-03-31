using UnityEngine;
using UnityEngine.Rendering;

namespace WorldSpaceTransitions
{
    [ExecuteAlways]
    public class AutoLoadPipelineAsset : MonoBehaviour
    {
        public RenderPipelineAsset pipelineAsset;

        private void OnEnable()
        {
            UpdatePipeline();
        }

        void UpdatePipeline()
        {
            //if (pipelineAsset)
            //{
            GraphicsSettings.renderPipelineAsset = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;
            //}
        }
    }
}
