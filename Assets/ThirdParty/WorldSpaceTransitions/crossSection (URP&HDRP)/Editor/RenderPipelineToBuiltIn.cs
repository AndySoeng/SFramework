using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WorldSpaceTransitions
{
    public class RenderPipelineToBuiltIn : MonoBehaviour
    {
        [MenuItem("Edit/Render Pipeline/To Built in")]
        static void DoSomething()
        {
            GraphicsSettings.renderPipelineAsset = null;
            QualitySettings.renderPipeline = null;
        }
    }
}