//////////////////////////////////////////////////////
// MK Glow Editor URP					      		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using MK.Glow.Editor;

namespace MK.Glow.URP.Editor
{
	using Tooltips = MK.Glow.Editor.EditorHelper.EditorUIContent.Tooltips;

	[CustomEditor(typeof(MK.Glow.URP.MKGlowRendererFeature))]
	internal class MKGlowRendererFeatureEditor : UnityEditor.Editor
	{
		//Behaviors
		private SerializedProperty _showEditorMainBehavior;
		private SerializedProperty _showEditorBloomBehavior;
		private SerializedProperty _showEditorLensSurfaceBehavior;
		private SerializedProperty _showEditorLensFlareBehavior;
		private SerializedProperty _showEditorGlareBehavior;

		private SerializedProperty _workMode;

		//Main
		private SerializedProperty _allowGeometryShaders;
		private SerializedProperty _allowComputeShaders;
		private SerializedProperty _renderPriority;
		private SerializedProperty _debugView;
		private SerializedProperty _quality;
		private SerializedProperty _antiFlickerMode;
		private SerializedProperty _workflow;
		private SerializedProperty _selectiveRenderLayerMask;
		private SerializedProperty _anamorphicRatio;
		private SerializedProperty _lumaScale;
		private SerializedProperty _blooming;

		//Bloom
		private SerializedProperty _bloomThreshold;
		private SerializedProperty _bloomScattering;
		private SerializedProperty _bloomIntensity;

		//Lens Surface
		private SerializedProperty _allowLensSurface;
		private SerializedProperty _lensSurfaceDirtTexture;
		private SerializedProperty _lensSurfaceDirtIntensity;
		private SerializedProperty _lensSurfaceDiffractionTexture;
		private SerializedProperty _lensSurfaceDiffractionIntensity;

		//Lens Flare
		private SerializedProperty _allowLensFlare;
		private SerializedProperty _lensFlareStyle;
		private SerializedProperty _lensFlareGhostFade;
		private SerializedProperty _lensFlareGhostIntensity;
		private SerializedProperty _lensFlareThreshold;
		private SerializedProperty _lensFlareScattering;
		private SerializedProperty _lensFlareColorRamp;
		private SerializedProperty _lensFlareChromaticAberration;
		private SerializedProperty _lensFlareGhostCount;
		private SerializedProperty _lensFlareGhostDispersal;
		private SerializedProperty _lensFlareHaloFade;
		private SerializedProperty _lensFlareHaloIntensity;
		private SerializedProperty _lensFlareHaloSize;

		//Glare
		private SerializedProperty _allowGlare;
		private SerializedProperty _glareBlend;
		private SerializedProperty _glareIntensity;
		private SerializedProperty _glareThreshold;
		private SerializedProperty _glareScattering;
		private SerializedProperty _glareAngle;
		private SerializedProperty _glareStyle;
		private SerializedProperty _glareStreaks;
		private SerializedProperty _glareSample0Scattering;
		private SerializedProperty _glareSample0Intensity;
		private SerializedProperty _glareSample0Angle;
		private SerializedProperty _glareSample0Offset;
		private SerializedProperty _glareSample1Scattering;
		private SerializedProperty _glareSample1Intensity;
		private SerializedProperty _glareSample1Angle;
		private SerializedProperty _glareSample1Offset;
		private SerializedProperty _glareSample2Scattering;
		private SerializedProperty _glareSample2Intensity;
		private SerializedProperty _glareSample2Angle;
		private SerializedProperty _glareSample2Offset;
		private SerializedProperty _glareSample3Scattering;
		private SerializedProperty _glareSample3Intensity;
		private SerializedProperty _glareSample3Angle;
		private SerializedProperty _glareSample3Offset;

		public void OnEnable()
		{
			//Editor
			_showEditorMainBehavior = serializedObject.FindProperty("showEditorMainBehavior");
			_showEditorBloomBehavior = serializedObject.FindProperty("showEditorBloomBehavior");
			_showEditorLensSurfaceBehavior = serializedObject.FindProperty("showEditorLensSurfaceBehavior");
			_showEditorLensFlareBehavior = serializedObject.FindProperty("showEditorLensFlareBehavior");
			_showEditorGlareBehavior = serializedObject.FindProperty("showEditorGlareBehavior");

			_workMode = serializedObject.FindProperty("workmode");

			//Main
			_allowGeometryShaders = serializedObject.FindProperty("allowGeometryShaders");
			_allowComputeShaders = serializedObject.FindProperty("allowComputeShaders");
			_renderPriority = serializedObject.FindProperty("renderPriority");
			_debugView = serializedObject.FindProperty("debugView");
			_quality = serializedObject.FindProperty("quality");
			_antiFlickerMode = serializedObject.FindProperty("antiFlickerMode");
			_workflow = serializedObject.FindProperty("workflow");
			_selectiveRenderLayerMask = serializedObject.FindProperty("selectiveRenderLayerMask");
			_anamorphicRatio = serializedObject.FindProperty("anamorphicRatio");
			_lumaScale = serializedObject.FindProperty("lumaScale");
			_blooming = serializedObject.FindProperty("blooming");

			//Bloom
			_bloomThreshold = serializedObject.FindProperty("bloomThreshold");
			_bloomScattering = serializedObject.FindProperty("bloomScattering");
			_bloomIntensity = serializedObject.FindProperty("bloomIntensity");

			_allowLensSurface = serializedObject.FindProperty("allowLensSurface");
			_lensSurfaceDirtTexture = serializedObject.FindProperty("lensSurfaceDirtTexture");
			_lensSurfaceDirtIntensity = serializedObject.FindProperty("lensSurfaceDirtIntensity");
			_lensSurfaceDiffractionTexture = serializedObject.FindProperty("lensSurfaceDiffractionTexture");
			_lensSurfaceDiffractionIntensity = serializedObject.FindProperty("lensSurfaceDiffractionIntensity");

			_allowLensFlare = serializedObject.FindProperty("allowLensFlare");
			_lensFlareStyle = serializedObject.FindProperty("lensFlareStyle");
			_lensFlareGhostFade = serializedObject.FindProperty("lensFlareGhostFade");
			_lensFlareGhostIntensity = serializedObject.FindProperty("lensFlareGhostIntensity");
			_lensFlareThreshold = serializedObject.FindProperty("lensFlareThreshold");
			_lensFlareScattering = serializedObject.FindProperty("lensFlareScattering");
			_lensFlareColorRamp = serializedObject.FindProperty("lensFlareColorRamp");
			_lensFlareChromaticAberration = serializedObject.FindProperty("lensFlareChromaticAberration");
			_lensFlareGhostCount = serializedObject.FindProperty("lensFlareGhostCount");
			_lensFlareGhostDispersal = serializedObject.FindProperty("lensFlareGhostDispersal");
			_lensFlareHaloFade = serializedObject.FindProperty("lensFlareHaloFade");
			_lensFlareHaloIntensity = serializedObject.FindProperty("lensFlareHaloIntensity");
			_lensFlareHaloSize = serializedObject.FindProperty("lensFlareHaloSize");

			_allowGlare = serializedObject.FindProperty("allowGlare");
			_glareBlend = serializedObject.FindProperty("glareBlend");
			_glareIntensity = serializedObject.FindProperty("glareIntensity");
			_glareThreshold = serializedObject.FindProperty("glareThreshold");
			_glareScattering = serializedObject.FindProperty("glareScattering");
			_glareStyle = serializedObject.FindProperty("glareStyle");
			_glareStreaks = serializedObject.FindProperty("glareStreaks");
			_glareAngle = serializedObject.FindProperty("glareAngle");
			_glareSample0Scattering = serializedObject.FindProperty("glareSample0Scattering");
			_glareSample0Intensity = serializedObject.FindProperty("glareSample0Intensity");
			_glareSample0Angle = serializedObject.FindProperty("glareSample0Angle");
			_glareSample0Offset = serializedObject.FindProperty("glareSample0Offset");
			_glareSample1Scattering = serializedObject.FindProperty("glareSample1Scattering");
			_glareSample1Intensity = serializedObject.FindProperty("glareSample1Intensity");
			_glareSample1Angle = serializedObject.FindProperty("glareSample1Angle");
			_glareSample1Offset = serializedObject.FindProperty("glareSample1Offset");
			_glareSample2Scattering = serializedObject.FindProperty("glareSample2Scattering");
			_glareSample2Intensity = serializedObject.FindProperty("glareSample2Intensity");
			_glareSample2Angle = serializedObject.FindProperty("glareSample2Angle");
			_glareSample2Offset = serializedObject.FindProperty("glareSample2Offset");
			_glareSample3Scattering = serializedObject.FindProperty("glareSample3Scattering");
			_glareSample3Intensity = serializedObject.FindProperty("glareSample3Intensity");
			_glareSample3Angle = serializedObject.FindProperty("glareSample3Angle");
			_glareSample3Offset = serializedObject.FindProperty("glareSample3Offset");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_workMode, Tooltips.workmode);
			if(_workMode.enumValueIndex == 0)
			{
				serializedObject.ApplyModifiedProperties();
				UnityEditor.EditorGUILayout.LabelField("Settings are controlled by your Post Processing Volumes.");
				return;
			}
			else
			{
				UnityEditor.EditorGUILayout.LabelField("Post Processing API is skipped entirely. Global setup is enabled.");
			}

			EditorHelper.VerticalSpace();

			EditorHelper.EditorUIContent.IsNotSupportedWarning();
			EditorHelper.EditorUIContent.XRUnityVersionWarning();
			if(_workflow.enumValueIndex == 1)
            {
				EditorHelper.EditorUIContent.SelectiveWorkflowDeprecated();
			}
			
			if(EditorHelper.HandleBehavior(_showEditorMainBehavior.serializedObject.targetObject, EditorHelper.EditorUIContent.mainTitle, "", _showEditorMainBehavior, null))
			{
				EditorGUILayout.PropertyField(_allowGeometryShaders, Tooltips.allowGeometryShaders);
				EditorGUILayout.PropertyField(_allowComputeShaders, Tooltips.allowComputeShaders);
				EditorGUILayout.PropertyField(_renderPriority, Tooltips.renderPriority);
				EditorGUILayout.PropertyField(_debugView, Tooltips.debugView);
				EditorGUILayout.PropertyField(_quality, Tooltips.quality);
				EditorGUILayout.PropertyField(_antiFlickerMode, Tooltips.antiFlickerMode);
				EditorGUILayout.PropertyField(_workflow, Tooltips.workflow);
				EditorHelper.EditorUIContent.SelectiveWorkflowVRWarning((Workflow)_workflow.enumValueIndex);
                if(_workflow.enumValueIndex == 1)
                {
                    EditorGUILayout.PropertyField(_selectiveRenderLayerMask, Tooltips.selectiveRenderLayerMask);
                }
				EditorGUILayout.PropertyField(_anamorphicRatio, Tooltips.anamorphicRatio);
				EditorGUILayout.PropertyField(_lumaScale, Tooltips.lumaScale);
				if(_renderPriority.enumValueIndex != 2 && _workflow.enumValueIndex != 2)
					EditorGUILayout.PropertyField(_blooming, Tooltips.blooming);
				EditorHelper.VerticalSpace();
			}
			
			if(EditorHelper.HandleBehavior(_showEditorBloomBehavior.serializedObject.targetObject, EditorHelper.EditorUIContent.bloomTitle, "", _showEditorBloomBehavior, null))
			{
				if(_workflow.enumValueIndex == 0)
					EditorGUILayout.PropertyField(_bloomThreshold, Tooltips.bloomThreshold);
				EditorGUILayout.PropertyField(_bloomScattering, Tooltips.bloomScattering);
				EditorGUILayout.PropertyField(_bloomIntensity, Tooltips.bloomIntensity);
				_bloomIntensity.floatValue = Mathf.Max(0, _bloomIntensity.floatValue);

				EditorHelper.VerticalSpace();
			}

			if(EditorHelper.HandleBehavior(_showEditorLensSurfaceBehavior.serializedObject.targetObject, EditorHelper.EditorUIContent.lensSurfaceTitle, "", _showEditorLensSurfaceBehavior, _allowLensSurface))
			{
				using (new EditorGUI.DisabledScope(!_allowLensSurface.boolValue))
                {
					EditorHelper.DrawHeader(EditorHelper.EditorUIContent.dirtTitle);
					EditorGUILayout.PropertyField(_lensSurfaceDirtTexture, Tooltips.lensSurfaceDirtTexture);
					EditorGUILayout.PropertyField(_lensSurfaceDirtIntensity, Tooltips.lensSurfaceDirtIntensity);
					_lensSurfaceDirtIntensity.floatValue = Mathf.Max(0, _lensSurfaceDirtIntensity.floatValue);
					EditorGUILayout.Space();
					EditorHelper.DrawHeader(EditorHelper.EditorUIContent.diffractionTitle);
					EditorGUILayout.PropertyField(_lensSurfaceDiffractionTexture, Tooltips.lensSurfaceDiffractionTexture);
					EditorGUILayout.PropertyField(_lensSurfaceDiffractionIntensity, Tooltips.lensSurfaceDiffractionIntensity);
					_lensSurfaceDiffractionIntensity.floatValue = Mathf.Max(0, _lensSurfaceDiffractionIntensity.floatValue);
				}
				EditorHelper.VerticalSpace();
			}

			if(Compatibility.CheckLensFlareFeatureSupport() && _quality.intValue <= 4)
			{
				if(EditorHelper.HandleBehavior(_showEditorLensFlareBehavior.serializedObject.targetObject, EditorHelper.EditorUIContent.lensFlareTitle, "", _showEditorLensFlareBehavior, _allowLensFlare))
				{
					using (new EditorGUI.DisabledScope(!_allowLensFlare.boolValue))
					{
						EditorGUILayout.PropertyField(_lensFlareStyle, Tooltips.lensFlareStyle);
						if(_workflow.enumValueIndex == 0)
							EditorGUILayout.PropertyField(_lensFlareThreshold, Tooltips.lensFlareThreshold);
						EditorGUILayout.PropertyField(_lensFlareScattering, Tooltips.lensFlareScattering);
						EditorGUILayout.PropertyField(_lensFlareColorRamp, Tooltips.lensFlareColorRamp);
						EditorGUILayout.PropertyField(_lensFlareChromaticAberration, Tooltips.lensFlareChromaticAberration);

						EditorGUILayout.Space();
						EditorHelper.DrawHeader(EditorHelper.EditorUIContent.ghostsTitle);
						if(_lensFlareStyle.enumValueIndex == 0)
						{
							EditorGUILayout.PropertyField(_lensFlareGhostFade, Tooltips.lensFlareGhostFade);
							EditorGUILayout.PropertyField(_lensFlareGhostCount, Tooltips.lensFlareGhostCount);
							EditorGUILayout.PropertyField(_lensFlareGhostDispersal, Tooltips.lensFlareGhostDispersal);
						}
						EditorGUILayout.PropertyField(_lensFlareGhostIntensity, Tooltips.lensFlareGhostIntensity);
						_lensFlareGhostIntensity.floatValue = Mathf.Max(0, _lensFlareGhostIntensity.floatValue);

						EditorGUILayout.Space();
						EditorHelper.DrawHeader(EditorHelper.EditorUIContent.haloTitle);
						if(_lensFlareStyle.enumValueIndex == 0)
						{
							EditorGUILayout.PropertyField(_lensFlareHaloFade, Tooltips.lensFlareHaloFade);
							EditorGUILayout.PropertyField(_lensFlareHaloSize, Tooltips.lensFlareHaloSize);
						}
						EditorGUILayout.PropertyField(_lensFlareHaloIntensity, Tooltips.lensFlareHaloIntensity);
						_lensFlareHaloIntensity.floatValue = Mathf.Max(0, _lensFlareHaloIntensity.floatValue);
					}
					EditorHelper.VerticalSpace();
				}
			}
			else
			{
				EditorHelper.DrawSplitter();
				EditorHelper.EditorUIContent.LensFlareFeatureNotSupportedWarning();
			}

			if(Compatibility.CheckGlareFeatureSupport() && _quality.intValue <= 4)
			{
				if(EditorHelper.HandleBehavior(_showEditorGlareBehavior.serializedObject.targetObject, EditorHelper.EditorUIContent.glareTitle, "", _showEditorGlareBehavior, _allowGlare))
				{
					using (new EditorGUI.DisabledScope(!_allowGlare.boolValue))
					{
						EditorGUILayout.PropertyField(_glareStyle, Tooltips.glareStyle);
						if(_workflow.enumValueIndex == 0)
							EditorGUILayout.PropertyField(_glareThreshold, Tooltips.glareThreshold);
						if(_glareStyle.enumValueIndex == 0)
							EditorGUILayout.PropertyField(_glareStreaks, Tooltips.glareStreaks);
						EditorGUILayout.PropertyField(_glareBlend, Tooltips.glareBlend);
						EditorGUILayout.PropertyField(_glareAngle, Tooltips.glareAngle);
						EditorGUILayout.PropertyField(_glareScattering, Tooltips.glareScattering);
						EditorGUILayout.PropertyField(_glareIntensity, Tooltips.glareIntensity);
						_glareScattering.floatValue = Mathf.Max(0, _glareScattering.floatValue);
						_glareIntensity.floatValue = Mathf.Max(0, _glareIntensity.floatValue);

						if(_glareStyle.enumValueIndex == 0)
						{
							EditorGUILayout.Space();
							EditorHelper.DrawHeader(EditorHelper.EditorUIContent.sample0Title);
							EditorGUILayout.PropertyField(_glareSample0Scattering, Tooltips.glareSample0Scattering);
							EditorGUILayout.PropertyField(_glareSample0Angle, Tooltips.glareSample0Angle);
							EditorGUILayout.PropertyField(_glareSample0Offset, Tooltips.glareSample0Offset);
							EditorGUILayout.PropertyField(_glareSample0Intensity, Tooltips.glareSample0Intensity);
							_glareSample0Intensity.floatValue = Mathf.Max(0, _glareSample0Intensity.floatValue);
							
							if(_glareStreaks.intValue >= 2)
							{
								EditorGUILayout.Space();
								EditorHelper.DrawHeader(EditorHelper.EditorUIContent.sample1Title);
								EditorGUILayout.PropertyField(_glareSample1Scattering, Tooltips.glareSample1Scattering);
								EditorGUILayout.PropertyField(_glareSample1Angle, Tooltips.glareSample1Angle);
								EditorGUILayout.PropertyField(_glareSample1Offset, Tooltips.glareSample1Offset);
								EditorGUILayout.PropertyField(_glareSample1Intensity, Tooltips.glareSample1Intensity);
								_glareSample1Intensity.floatValue = Mathf.Max(0, _glareSample1Intensity.floatValue);
							}

							if(_glareStreaks.intValue >= 3)
							{
								EditorGUILayout.Space();
								EditorHelper.DrawHeader(EditorHelper.EditorUIContent.sample2Title);
								EditorGUILayout.PropertyField(_glareSample2Scattering, Tooltips.glareSample2Scattering);
								EditorGUILayout.PropertyField(_glareSample2Angle, Tooltips.glareSample2Angle);
								EditorGUILayout.PropertyField(_glareSample2Offset, Tooltips.glareSample2Offset);
								EditorGUILayout.PropertyField(_glareSample2Intensity, Tooltips.glareSample2Intensity);
								_glareSample2Intensity.floatValue = Mathf.Max(0, _glareSample2Intensity.floatValue);
							}
							
							if(_glareStreaks.intValue >= 4)
							{
								EditorGUILayout.Space();
								EditorHelper.DrawHeader(EditorHelper.EditorUIContent.sample3Title);
								EditorGUILayout.PropertyField(_glareSample3Scattering, Tooltips.glareSample3Scattering);
								EditorGUILayout.PropertyField(_glareSample3Angle, Tooltips.glareSample3Angle);
								EditorGUILayout.PropertyField(_glareSample3Offset, Tooltips.glareSample3Offset);
								EditorGUILayout.PropertyField(_glareSample3Intensity, Tooltips.glareSample3Intensity);
								_glareSample3Intensity.floatValue = Mathf.Max(0, _glareSample3Intensity.floatValue);
							}
						}
					}
				}
				EditorHelper.VerticalSpace();
			}
			else
			{
				EditorHelper.DrawSplitter();
				EditorHelper.EditorUIContent.GlareFeatureNotSupportedWarning();
			}
			EditorHelper.DrawSplitter();

			serializedObject.ApplyModifiedProperties();
		}
    }
}
#endif