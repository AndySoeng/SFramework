using System;
using UnityEngine;

namespace UnityEditor.CrossSection.URP.ShaderGUI
{
    internal class SimpleLitShader : BaseShaderGUI
    {
        // Properties
        private UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.SimpleLitProperties shadingModelProperties;

        MaterialProperty sectionColor = null;
        MaterialProperty inverse = null;
        MaterialProperty retractBackfaces = null;
        MaterialProperty stencilMask = null;

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            shadingModelProperties = new UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.SimpleLitProperties(properties);

            sectionColor = FindProperty("_SectionColor", properties);
            inverse = FindProperty("_inverse", properties, false);
            retractBackfaces = FindProperty("_retractBackfaces", properties, false);
            stencilMask = FindProperty("_stencilMask", properties, false);
        }

        // material changed check
        public override void ValidateMaterial(Material material)
        {
            SetMaterialKeywords(material, UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.SetMaterialKeywords);
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            base.DrawSurfaceOptions(material);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.Inputs(shadingModelProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
            //CrossSection
            materialEditor.ColorProperty(sectionColor, "_SectionColor");
            if (inverse != null)
                materialEditor.ShaderProperty(inverse, "_inverse");
            if (retractBackfaces != null)
                materialEditor.ShaderProperty(retractBackfaces, "_retractBackfaces");
            if (stencilMask != null) materialEditor.RangeProperty(stencilMask, "_StencilMask");
        }

        public override void DrawAdvancedOptions(Material material)
        {
            UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.Advanced(shadingModelProperties);
            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Surface", (float)surfaceType);
            material.SetFloat("_Blend", (float)blendMode);
        }
    }
}
