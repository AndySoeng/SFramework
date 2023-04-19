//////////////////////////////////////////////////////
// MK Glow URP Volume Component     	            //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MK.Glow.URP
{
    [ExecuteInEditMode, VolumeComponentMenu("Post-processing/MK/MKGlow")]
    public class MKGlow : VolumeComponent, IPostProcessComponent, MK.Glow.ISettings
    {
        [System.Serializable]
        public sealed class RenderPriorityParameter : VolumeParameter<RenderPriority>
        {
            public override void Interp(RenderPriority from, RenderPriority to, float t)
            {
                value = t > 0 ? to : from;
            }
        }

        [System.Serializable]
        public sealed class Texture2DParameter : VolumeParameter<Texture2D>
        {
            public override void Interp(Texture2D from, Texture2D to, float t)
            {
                value = t > 0 ? to : from;
            }
        }

        [System.Serializable]
        public sealed class DebugViewParameter : VolumeParameter<MK.Glow.DebugView>
        {
            public override void Interp(MK.Glow.DebugView from, MK.Glow.DebugView to, float t)
            {
                value = t > 0 ? to : from;
            }
        }

        [System.Serializable]
        public sealed class QualityParameter : VolumeParameter<MK.Glow.Quality>
        {
            public override void Interp(MK.Glow.Quality from, MK.Glow.Quality to, float t)
            {
                value = t > 0 ? to : from;
            }
        }

        [System.Serializable]
        public sealed class AntiFlickerModeParameter : VolumeParameter<MK.Glow.AntiFlickerMode>
        {
            public override void Interp(MK.Glow.AntiFlickerMode from, MK.Glow.AntiFlickerMode to, float t)
            {
                value = t > 0 ? to : from;
            }
        }

        [System.Serializable]
        public sealed class WorkflowParameter : VolumeParameter<MK.Glow.Workflow>
        {
            public override void Interp(MK.Glow.Workflow from, MK.Glow.Workflow to, float t)
            {
                value = t > 0 ? to : from;
            }
        }

        [System.Serializable]
        public sealed class LayerMaskParameter : VolumeParameter<LayerMask>
        {
            public override void Interp(LayerMask from, LayerMask to, float t)
            {
                value = t > 0 ? to : from;
            }
        }

        [System.Serializable]
        public sealed class MinMaxRangeParameter : VolumeParameter<MK.Glow.MinMaxRange>
        {
            public override void Interp(MK.Glow.MinMaxRange from, MK.Glow.MinMaxRange to, float t)
            {
                m_Value.minValue = Mathf.Lerp(from.minValue, to.minValue, t);
                m_Value.maxValue = Mathf.Lerp(from.maxValue, to.maxValue, t);
            }
        }

        [System.Serializable]
        public sealed class GlareStyleParameter : VolumeParameter<GlareStyle>
        {
            public override void Interp(GlareStyle from, GlareStyle to, float t)
            {
                value = t > 0 ? to : from;
            }
        }

        [System.Serializable]
        public sealed class LensFlareStyleParameter : VolumeParameter<LensFlareStyle>
        {
            public override void Interp(LensFlareStyle from, LensFlareStyle to, float t)
            {
                value = t > 0 ? to : from;
            }
        }

        #if UNITY_EDITOR
        public BoolParameter showEditorMainBehavior = new BoolParameter(true);
		public BoolParameter showEditorBloomBehavior = new BoolParameter(false);
		public BoolParameter showEditorLensSurfaceBehavior = new BoolParameter(false);
		public BoolParameter showEditorLensFlareBehavior = new BoolParameter(false);
		public BoolParameter showEditorGlareBehavior = new BoolParameter(false);
        /// <summary>
        /// Keep this value always untouched, editor internal only
        /// </summary>
        public BoolParameter isInitialized = new BoolParameter(false, true);
        #endif
        
        //Main
        public BoolParameter allowGeometryShaders = new BoolParameter(true);
        public BoolParameter allowComputeShaders = new BoolParameter(true);
        public RenderPriorityParameter renderPriority = new RenderPriorityParameter() { value = RenderPriority.Balanced };
        public DebugViewParameter debugView = new DebugViewParameter() { value = MK.Glow.DebugView.None };
        public QualityParameter quality = new QualityParameter() { value = MK.Glow.Quality.High };
        public AntiFlickerModeParameter antiFlickerMode = new AntiFlickerModeParameter() { value = MK.Glow.AntiFlickerMode.Balanced };
        public WorkflowParameter workflow = new WorkflowParameter() { value = MK.Glow.Workflow.Threshold };
        public LayerMaskParameter selectiveRenderLayerMask = new LayerMaskParameter() { value = -1 };
        [Range(-1f, 1f)]
        public ClampedFloatParameter anamorphicRatio = new ClampedFloatParameter(0, -1, 1);
        [Range(0f, 1f)]
		public ClampedFloatParameter lumaScale = new ClampedFloatParameter(0.5f, 0, 1);
        [Range(0f, 1f)]
		public ClampedFloatParameter blooming = new ClampedFloatParameter(0, 0, 1);

        //Bloom
        [MK.Glow.MinMaxRange(0, 10)]
        public MinMaxRangeParameter bloomThreshold = new MinMaxRangeParameter() { value = new MinMaxRange(1.25f, 10f) };
        [Range(1f, 10f)]
		public ClampedFloatParameter bloomScattering = new ClampedFloatParameter(7, 1, 10);
		public FloatParameter bloomIntensity = new FloatParameter(0);

        //LensSurface
        public BoolParameter allowLensSurface = new BoolParameter(false, true);
		public Texture2DParameter lensSurfaceDirtTexture = new Texture2DParameter();
		public FloatParameter lensSurfaceDirtIntensity = new FloatParameter(0);
		public Texture2DParameter lensSurfaceDiffractionTexture = new Texture2DParameter();
		public FloatParameter lensSurfaceDiffractionIntensity = new FloatParameter(0);

        //LensFlare
        public BoolParameter allowLensFlare = new BoolParameter(false, true);
        public LensFlareStyleParameter lensFlareStyle = new LensFlareStyleParameter() { value = LensFlareStyle.Average };
        [Range(0f, 25f)]
		public ClampedFloatParameter lensFlareGhostFade = new ClampedFloatParameter(10, 0, 25);
		public FloatParameter lensFlareGhostIntensity = new FloatParameter(0);
        [MK.Glow.MinMaxRange(0, 10)]
		public MinMaxRangeParameter lensFlareThreshold = new MinMaxRangeParameter() { value = new MinMaxRange(1.3f, 10f) };
        [Range(0f, 8f)]
		public ClampedFloatParameter lensFlareScattering = new ClampedFloatParameter(5, 0, 8);
		public Texture2DParameter lensFlareColorRamp = new Texture2DParameter();
        [Range(-100f, 100f)]
		public ClampedFloatParameter lensFlareChromaticAberration = new ClampedFloatParameter(53, -100, 100);
        [Range(0, 5)]
		public ClampedIntParameter lensFlareGhostCount = new ClampedIntParameter(3, 0, 5);
        [Range(-1f, 1f)]
		public ClampedFloatParameter lensFlareGhostDispersal = new ClampedFloatParameter(0.6f, -1, 1);
        [Range(0f, 25f)]
		public ClampedFloatParameter lensFlareHaloFade = new ClampedFloatParameter(2, 0, 25);
		public FloatParameter lensFlareHaloIntensity = new FloatParameter(2);
        [Range(0f, 1f)]
		public ClampedFloatParameter lensFlareHaloSize = new ClampedFloatParameter(0.4f, 0, 1);

        //Glare
        public BoolParameter allowGlare = new BoolParameter(false, true);
        [Range(0.0f, 1.0f)]
        public ClampedFloatParameter glareBlend = new ClampedFloatParameter(0.33f, 0, 1);
        public FloatParameter glareIntensity = new FloatParameter(1);
        [Range(0.0f, 360.0f)]
        public ClampedFloatParameter glareAngle = new ClampedFloatParameter(0, 0, 360);
        [MK.Glow.MinMaxRange(0, 10)]
        public MinMaxRangeParameter glareThreshold = new MinMaxRangeParameter() { value = new MinMaxRange(1.25f, 10f)};
        [Range(1, 4)]
		public ClampedIntParameter glareStreaks = new ClampedIntParameter(4, 1, 4);
        [Range(0.0f, 4.0f)]
        public ClampedFloatParameter glareScattering = new ClampedFloatParameter(2, 0, 4);
        public GlareStyleParameter glareStyle = new GlareStyleParameter() { value = GlareStyle.DistortedCross };
        //Sample0
        [Range(0f, 10f)]
        public ClampedFloatParameter glareSample0Scattering = new ClampedFloatParameter(5, 0, 10);
        [Range(0f, 360f)]
        public ClampedFloatParameter glareSample0Angle = new ClampedFloatParameter(0, 0, 360);
        public FloatParameter glareSample0Intensity = new FloatParameter(0);
        [Range(0f, 10f)]
        public ClampedFloatParameter glareSample0Offset = new ClampedFloatParameter(0, 0, 10);
        //Sample1
        [Range(0f, 10f)]
        public ClampedFloatParameter glareSample1Scattering = new ClampedFloatParameter(5, 0, 10);
        [Range(0f, 360f)]
        public ClampedFloatParameter glareSample1Angle = new ClampedFloatParameter(45, 0, 360);
        public FloatParameter glareSample1Intensity = new FloatParameter(0);
        [Range(0f, 10f)]
        public ClampedFloatParameter glareSample1Offset = new ClampedFloatParameter(0, 0, 10);
        //Sample0
        [Range(0f, 10f)]
        public ClampedFloatParameter glareSample2Scattering = new ClampedFloatParameter(5, 0, 10);
        [Range(0f, 360f)]
        public ClampedFloatParameter glareSample2Angle = new ClampedFloatParameter(90, 0, 360);
        public FloatParameter glareSample2Intensity = new FloatParameter(0);
        [Range(0f, 10f)]
        public ClampedFloatParameter glareSample2Offset = new ClampedFloatParameter(0, 0, 10);
        //Sample0
        [Range(0f, 10f)]
        public ClampedFloatParameter glareSample3Scattering = new ClampedFloatParameter(5, 0, 10);
        [Range(0f, 360f)]
        public ClampedFloatParameter glareSample3Angle = new ClampedFloatParameter(135, 0, 360);
        public FloatParameter glareSample3Intensity = new FloatParameter(0);
        [Range(0f, 10f)]
        public ClampedFloatParameter glareSample3Offset = new ClampedFloatParameter(0, 0, 10);
        
        public bool IsActive() => Compatibility.IsSupported && (bloomIntensity.value > 0 || allowLensFlare.value && (lensFlareGhostIntensity.value > 0 || lensFlareHaloIntensity.value > 0) || allowGlare.value && glareIntensity.value > 0 && (glareSample0Intensity.value > 0 || glareSample1Intensity.value > 0 || glareSample2Intensity.value > 0 || glareSample3Intensity.value > 0));

        public bool IsTileCompatible() => true;

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Settings
        /////////////////////////////////////////////////////////////////////////////////////////////
        public bool GetAllowGeometryShaders()
        { 
            return false;
        }
        public bool GetAllowComputeShaders()
        { 
            return false;
        }
        public RenderPriority GetRenderPriority()
        { 
            return renderPriority.value;
        }
        public MK.Glow.DebugView GetDebugView()
        { 
			return debugView.value;
		}
        public MK.Glow.Quality GetQuality()
        { 
			return quality.value;
		}
        public MK.Glow.AntiFlickerMode GetAntiFlickerMode()
        { 
			return antiFlickerMode.value;
		}
        public MK.Glow.Workflow GetWorkflow()
        { 
			return workflow.value;
		}
        public LayerMask GetSelectiveRenderLayerMask()
        { 
			return selectiveRenderLayerMask.value;
		}
        public float GetAnamorphicRatio()
        { 
			return anamorphicRatio.value;
		}
        public float GetLumaScale()
        { 
			return lumaScale.value;
		}
		public float GetBlooming()
		{ 
			return blooming.value;
		}

        //Bloom
		public MK.Glow.MinMaxRange GetBloomThreshold()
		{ 
			return bloomThreshold.value;
		}
		public float GetBloomScattering()
		{ 
			return bloomScattering.value;
		}
		public float GetBloomIntensity()
		{ 
			return bloomIntensity.value;
		}

        //LensSurface
		public bool GetAllowLensSurface()
		{ 
			return allowLensSurface.value;
		}
		public Texture2D GetLensSurfaceDirtTexture()
		{ 
			return lensSurfaceDirtTexture.value;
		}
		public float GetLensSurfaceDirtIntensity()
		{ 
			return lensSurfaceDirtIntensity.value;
		}
		public Texture2D GetLensSurfaceDiffractionTexture()
		{ 
			return lensSurfaceDiffractionTexture.value;
		}
		public float GetLensSurfaceDiffractionIntensity()
		{ 
			return lensSurfaceDiffractionIntensity.value;
		}

        //LensFlare
		public bool GetAllowLensFlare()
		{ 
			return allowLensFlare.value;
		}
        public LensFlareStyle GetLensFlareStyle()
		{ 
			return lensFlareStyle.value;
		}
		public float GetLensFlareGhostFade()
		{ 
			return lensFlareGhostFade.value;
		}
		public float GetLensFlareGhostIntensity()
		{ 
			return lensFlareGhostIntensity.value;
		}
		public MK.Glow.MinMaxRange GetLensFlareThreshold()
		{ 
			return lensFlareThreshold.value;
		}
		public float GetLensFlareScattering()
		{ 
			return lensFlareScattering.value;
		}
		public Texture2D GetLensFlareColorRamp()
		{ 
			return lensFlareColorRamp.value;
		}
		public float GetLensFlareChromaticAberration()
		{ 
			return lensFlareChromaticAberration.value;
		}
		public int GetLensFlareGhostCount()
		{ 
			return lensFlareGhostCount.value;
		}
		public float GetLensFlareGhostDispersal()
		{ 
			return lensFlareGhostDispersal.value;
		}
		public float GetLensFlareHaloFade()
		{
			return lensFlareHaloFade.value;
		}
		public float GetLensFlareHaloIntensity()
		{ 
			return lensFlareHaloIntensity.value;
		}
		public float GetLensFlareHaloSize()
		{ 
			return lensFlareHaloSize.value;
		}

        public void SetLensFlareGhostFade(float fade)
        {
            lensFlareGhostFade.value = fade;
        }
        public void SetLensFlareGhostCount(int count)
        {
            lensFlareGhostCount.value = count;
        }
        public void SetLensFlareGhostDispersal(float dispersal)
        {
            lensFlareGhostDispersal.value = dispersal;
        }
        public void SetLensFlareHaloFade(float fade)
        {
            lensFlareHaloFade.value = fade;
        }
        public void SetLensFlareHaloSize(float size)
        {
            lensFlareHaloSize.value = size;
        }

        //Glare
		public bool GetAllowGlare()
		{ 
			return allowGlare.value;
		}
        public float GetGlareBlend()
        { 
			return glareBlend.value;
		}
        public float GetGlareIntensity()
        {
            return glareIntensity.value;
        }
        public float GetGlareAngle()
        {
            return glareAngle.value;
        }
		public MK.Glow.MinMaxRange GetGlareThreshold()
		{ 
			return glareThreshold.value;
		}
		public int GetGlareStreaks()
		{ 
			return glareStreaks.value;
		}
        public void SetGlareStreaks(int count)
        {
            glareStreaks.value = count;
        }
        public float GetGlareScattering()
        {
            return glareScattering.value;
        }
        public GlareStyle GetGlareStyle()
        {
            return glareStyle.value;
        }

        //Sample0
        public float GetGlareSample0Scattering()
        {
            return glareSample0Scattering.value;
        }
        public float GetGlareSample0Angle()
        {
            return glareSample0Angle.value;
        }
        public float GetGlareSample0Intensity()
        {
            return glareSample0Intensity.value;
        }
        public float GetGlareSample0Offset()
        {
            return glareSample0Offset.value;
        }

        public void SetGlareSample0Scattering(float scattering)
        {
            glareSample0Scattering.value = scattering;
        }
        public void SetGlareSample0Angle(float angle)
        {
            glareSample0Angle.value = angle;
        }
        public void SetGlareSample0Intensity(float intensity)
        {
            glareSample0Intensity.value = intensity;
        }
        public void SetGlareSample0Offset(float offset)
        {
            glareSample0Offset.value = offset;
        }

        //Sample1
        public float GetGlareSample1Scattering()
        {
            return glareSample1Scattering.value;
        }
        public float GetGlareSample1Angle()
        {
            return glareSample1Angle.value;
        }
        public float GetGlareSample1Intensity()
        {
            return glareSample1Intensity.value;
        }
        public float GetGlareSample1Offset()
        {
            return glareSample1Offset.value;
        }

        public void SetGlareSample1Scattering(float scattering)
        {
            glareSample1Scattering.value = scattering;
        }
        public void SetGlareSample1Angle(float angle)
        {
            glareSample1Angle.value = angle;
        }
        public void SetGlareSample1Intensity(float intensity)
        {
            glareSample1Intensity.value = intensity;
        }
        public void SetGlareSample1Offset(float offset)
        {
            glareSample1Offset.value = offset;
        }

        //Sample2
        public float GetGlareSample2Scattering()
        {
            return glareSample2Scattering.value;
        }
        public float GetGlareSample2Angle()
        {
            return glareSample2Angle.value;
        }
        public float GetGlareSample2Intensity()
        {
            return glareSample2Intensity.value;
        }
        public float GetGlareSample2Offset()
        {
            return glareSample2Offset.value;
        }

        public void SetGlareSample2Scattering(float scattering)
        {
            glareSample2Scattering.value = scattering;
        }
        public void SetGlareSample2Angle(float angle)
        {
            glareSample2Angle.value = angle;
        }
        public void SetGlareSample2Intensity(float intensity)
        {
            glareSample2Intensity.value = intensity;
        }
        public void SetGlareSample2Offset(float offset)
        {
            glareSample2Offset.value = offset;
        }

        //Sample3
        public float GetGlareSample3Scattering()
        {
            return glareSample3Scattering.value;
        }
        public float GetGlareSample3Angle()
        {
            return glareSample3Angle.value;
        }
        public float GetGlareSample3Intensity()
        {
            return glareSample3Intensity.value;
        }
        public float GetGlareSample3Offset()
        {
            return glareSample3Offset.value;
        }

        public void SetGlareSample3Scattering(float scattering)
        {
            glareSample3Scattering.value = scattering;
        }
        public void SetGlareSample3Angle(float angle)
        {
            glareSample3Angle.value = angle;
        }
        public void SetGlareSample3Intensity(float intensity)
        {
            glareSample3Intensity.value = intensity;
        }
        public void SetGlareSample3Offset(float offset)
        {
            glareSample3Offset.value = offset;
        }
    }
}
