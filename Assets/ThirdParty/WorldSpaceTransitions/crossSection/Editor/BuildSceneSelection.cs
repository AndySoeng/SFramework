using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CreateAssetMenu(fileName = "BuildScenesSelection", menuName = "ScriptableObjects/BuildScenesSelection", order = 1)]
public class BuildSceneSelection : ScriptableObject
{
    public List<SceneAsset> scenes; 
}