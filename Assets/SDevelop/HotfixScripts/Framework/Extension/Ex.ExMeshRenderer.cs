using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


namespace Ex
{
    public static class ExMeshRenderer
    {
        #region MeshRenderer

        private static Dictionary<MeshRenderer, Material[]>
            OriginMaterials = new Dictionary<MeshRenderer, Material[]>();

        private static Material[] SaveMaterial(this MeshRenderer meshRenderer)
        {
            if (OriginMaterials.ContainsKey(meshRenderer))
                return OriginMaterials[meshRenderer];
            else
                return OriginMaterials[meshRenderer] = meshRenderer.sharedMaterials;
        }

        public static void SetAlpha(this MeshRenderer meshRenderer, float alpha = 0.5f, float duration = 0.5f,
            bool needProperties = false)
        {
            Material[] tempMats =
                meshRenderer.materials = SaveMaterial(meshRenderer).TransparentToOpaque(needProperties);
            for (int i = 0; i < tempMats.Length; i++)
            {
                Color cr = tempMats[i].color;
                tempMats[i].DOColor(new Color(cr.r, cr.g, cr.b, alpha), duration);
            }
        }

        public static void ResetMaterials(this MeshRenderer meshRenderer)
        {
            meshRenderer.materials = OriginMaterials[meshRenderer];
        }

        public static void SetAlphas(this GameObject go, float alpha = 0.5f, float duration = 0.5f,
            bool needProperties = false)
        {
            MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].SetAlpha(alpha, duration, needProperties);
            }
        }

        public static void ResetMaterials(this GameObject go)
        {
            MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].ResetMaterials();
            }
        }

        #endregion
    }
}
