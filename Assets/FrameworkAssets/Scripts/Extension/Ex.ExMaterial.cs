using UnityEngine;


namespace Ex
{
    public static class ExMaterial
    {

        private static Material TransparentMat;

        public static Material[] TransparentToOpaque(this Material[] mat, bool needProperties = false)
        {
            if (TransparentMat == null)
            {
                TransparentMat = Resources.Load<Material>("ExMaterial/TransparentMat");
            }

            Material[] newMats = new Material[mat.Length];
            if (needProperties)
            {
                for (int i = 0; i < mat.Length; i++)
                {
                    newMats[i] = SetMaterialProperties(mat[i], new Material(TransparentMat));
                }
            }
            else
            {
                for (int i = 0; i < mat.Length; i++)
                {
                    newMats[i] = new Material(TransparentMat);
                }
            }

            return newMats;
        }

        public static Material TransparentToOpaque(this Material mat, bool needProperties = false)
        {
            if (TransparentMat == null)
            {
                TransparentMat = Resources.Load<Material>("ExMaterial/TransparentMat");
            }

            Material tempMat = new Material(TransparentMat);
            if (needProperties)
            {
                tempMat = SetMaterialProperties(mat, tempMat);
            }

            return tempMat;
        }

        private static Material SetMaterialProperties(Material oldMat, Material newMat)
        {
            newMat.SetFloat("_WorkflowMode", oldMat.GetFloat("_WorkflowMode"));
            newMat.SetColor("_BaseColor", oldMat.GetColor("_BaseColor"));
            newMat.SetTexture("_BaseMap", oldMat.GetTexture("_BaseMap"));
            newMat.SetFloat("_Cutoff", oldMat.GetFloat("_Cutoff"));
            newMat.SetFloat("_Smoothness", oldMat.GetFloat("_Smoothness"));
            newMat.SetFloat("_GlossMapScale", oldMat.GetFloat("_GlossMapScale"));
            newMat.SetFloat("_SmoothnessTextureChannel", oldMat.GetFloat("_SmoothnessTextureChannel"));
            newMat.SetFloat("_Metallic", oldMat.GetFloat("_Metallic"));
            newMat.SetTexture("_MetallicGlossMap", oldMat.GetTexture("_MetallicGlossMap"));
            newMat.SetColor("_SpecColor", oldMat.GetColor("_SpecColor"));
            newMat.SetTexture("_SpecGlossMap", oldMat.GetTexture("_SpecGlossMap"));
            newMat.SetFloat("_SpecularHighlights", oldMat.GetFloat("_SpecularHighlights"));
            newMat.SetFloat("_EnvironmentReflections", oldMat.GetFloat("_EnvironmentReflections"));

            newMat.SetFloat("_BumpScale", oldMat.GetFloat("_BumpScale"));
            newMat.SetTexture("_BumpMap", oldMat.GetTexture("_BumpMap"));

            newMat.SetFloat("_OcclusionStrength", oldMat.GetFloat("_OcclusionStrength"));
            newMat.SetTexture("_OcclusionMap", oldMat.GetTexture("_OcclusionMap"));

            newMat.SetColor("_EmissionColor", oldMat.GetColor("_EmissionColor"));
            newMat.SetTexture("_EmissionMap", oldMat.GetTexture("_EmissionMap"));

            //HideInInspector
            // newMat.SetFloat("_Surface",oldMat.GetFloat("_Surface"));
            // newMat.SetFloat("_Blend",oldMat.GetFloat("_Blend"));
            // newMat.SetFloat("_SrcBlend",oldMat.GetFloat("_SrcBlend"));
            // newMat.SetFloat("_DstBlend",oldMat.GetFloat("_DstBlend"));
            // newMat.SetFloat("_ZWrite",oldMat.GetFloat("_ZWrite"));
            // newMat.SetFloat("_Cull",oldMat.GetFloat("_Cull"));

            newMat.SetFloat("_ReceiveShadows", oldMat.GetFloat("_ReceiveShadows"));


            newMat.SetFloat("_ReceiveShadows", oldMat.GetFloat("_ReceiveShadows"));
            newMat.SetFloat("_QueueOffset", oldMat.GetFloat("_QueueOffset"));
            newMat.SetTexture("_MainTex", oldMat.GetTexture("_MainTex"));
            newMat.SetColor("_Color", oldMat.GetColor("_Color"));

            newMat.SetFloat("_GlossMapScale", oldMat.GetFloat("_GlossMapScale"));
            newMat.SetFloat("_Glossiness", oldMat.GetFloat("_Glossiness"));
            newMat.SetFloat("_GlossyReflections", oldMat.GetFloat("_GlossyReflections"));
            return newMat;
        }


    }
}
