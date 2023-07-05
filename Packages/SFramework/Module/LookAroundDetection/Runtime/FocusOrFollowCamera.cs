using Ex;
using Sirenix.OdinInspector;
using UnityEngine;

public class FocusOrFollowCamera : MonoBehaviour
{
    [LabelText("当前物体是否需要反转")] public bool reverFace = false;

    [ToggleGroup("followCamera")]
    [LabelText("当前物体是否需要跟随主相机")] public bool followCamera = false;
    [ToggleGroup("followCamera")]
    [LabelText("当前物体是否需要跟随主相机")]  public float cameraForwardDistance =2f;

    // Update is called once per frame
    void Update()
    {
        CanvasLookAtCamera();
    }

    private void CanvasLookAtCamera()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera)
        {
            Vector3 targetPos = transform.position + mainCamera.transform.rotation * (reverFace ? Vector3.back : Vector3.forward); //确定位置方向
            transform.LookAt(targetPos); //朝向

            if (followCamera)
                transform.position = Vector3.Lerp(transform.position, Camera.main.transform.Forward(2f), 10 * Time.deltaTime);
        }
    }
}