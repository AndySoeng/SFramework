using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAroundDetection_Player : MonoBehaviour
{
    public LookAroundDetection lookLAround;
    // Start is called before the first frame update
    void Start()
    {
        lookLAround.StartLookAround(Camera.main.transform);
        lookLAround.OnCompleteLookAroundLeft.AddListener(()=>{ Debug.Log( "已完成左环顾");});
        lookLAround.OnCompleteLookAroundRight.AddListener(()=>{ Debug.Log( "已完成右环顾");});
        lookLAround.OnCompleteLookAround.AddListener(()=>{ Debug.Log("已完成所有环顾");});
    }

}
