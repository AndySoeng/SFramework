using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

public class CommonSceneReference : SerializedMonoBehaviour
{
    public static CommonSceneReference Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
    }
}
