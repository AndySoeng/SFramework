
using DG.Tweening;
using UnityEngine;

public class LookAroundDetection_Target : MonoBehaviour
{
    public Transform target;
    public LookAroundDetection lookLAround;
    // Start is called before the first frame update
    void Start()
    {
        lookLAround.StartLookAround(target);
        lookLAround.OnCompleteLookAroundLeft.AddListener(()=>{ Debug.Log( "已完成左环顾");});
        lookLAround.OnCompleteLookAroundRight.AddListener(()=>{ Debug.Log( "已完成右环顾");});
        lookLAround.OnCompleteLookAround.AddListener(()=>{ Debug.Log("已完成所有环顾");});
        target.DOLocalRotate(Vector3.up* 360f, 4f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetDelay(2f);
    }

}
