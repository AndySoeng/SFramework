public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	// DOTween.dll
	// SFramework.dll
	// Unity.ResourceManager.dll
	// UnityEngine.CoreModule.dll
	// mscorlib.dll
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// SFramework.MonoSingleton<object>
	// System.Action<object>
	// System.Action<object,int,int>
	// System.Collections.Generic.Dictionary<ExpMoudle,int>
	// System.Collections.Generic.Dictionary<ExpMoudle,byte>
	// System.Collections.Generic.Dictionary<SFramework.UI.LabQuestSprite,object>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary.Enumerator<ExpMoudle,byte>
	// System.Collections.Generic.Dictionary.Enumerator<ExpMoudle,int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.KeyValuePair<ExpMoudle,byte>
	// System.Collections.Generic.KeyValuePair<ExpMoudle,int>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.List<SFramework.UI.ChoiceIndexStr>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityAction<int>
	// UnityEngine.Events.UnityAction<byte>
	// UnityEngine.Events.UnityEvent<byte>
	// UnityEngine.Events.UnityEvent<object>
	// UnityEngine.Events.UnityEvent<int>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// }}

	public void RefMethods()
	{
		// object DG.Tweening.TweenSettingsExtensions.OnUpdate<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.SetEase<object>(object,DG.Tweening.Ease)
		// bool SFramework.SUIManager.CloseUI<object>()
		// object SFramework.SUIManager.GetUI<object>()
		// System.Void SFramework.SUIManager.OpenUI<object>(SFramework.UIOpenScreenParameterBase,System.Action<SFramework.UIScreenBase>)
		// object UnityEngine.Component.GetComponent<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>()
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.Resources.Load<object>(string)
	}
}