using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WorldSpaceTransitions
{
    [ExecuteInEditMode]
    public class BuiltInRenderPipeline : MonoBehaviour
    {
        void OnEnable()
        {
            GraphicsSettings.renderPipelineAsset = null;
            QualitySettings.renderPipeline = null;
        }
    }
}