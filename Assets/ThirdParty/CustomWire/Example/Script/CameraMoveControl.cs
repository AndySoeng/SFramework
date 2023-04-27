/************************************************************
  Copyright (C), 2007-2016,BJ Rainier Tech. Co., Ltd.
  FileName: CameraMoveControl.cs
  Author: 裴超琦       Version :1.0          Date: 2016年03月23日
  Description:  相机移动控制类
************************************************************/

using UnityEngine;
using System.Collections;

public class CameraMoveControl : MonoBehaviour
{
    private static CameraMoveControl instace;

    public bool canRotate = true;
    public bool canMove = true;
    public bool needCheckCollider = false;

    public Transform leapCameraParent;
    /// <summary>
    /// 移动时自身位置保存
    /// </summary>
    private Vector3 selfPosition;
    /// <summary>
    /// 移动时自身角度保存
    /// </summary>
    private Vector3 selfeuler;
    //视角关注物体
    public GameObject viewObj;
    private Vector3 CenterPoint;
    private GameObject last_viewpoint;
    //最小距离
    public float minDistance = 0.5f;
    //最大距离
    public float maxDistance = 10;

    //旋转速度
    public float rotateSpeed=2;
    //平移速度
    public float translatespeed=2;

    //x方向旋转
    private float roatatex;
    //y方向旋转
    private float roatatey;

    //拉近标识
    private bool Lerp = false;
    public float distance = 3f;
    private bool StopMouse = false;

    //限制最小角度
    public float limitMineuler = 0f;
    //限制最大角度
    public float limitMaxeuler = 80f;
    //射线
    private Ray ray;
    //射线信息
    private RaycastHit hitInfo;
    private Vector3 moveTargetPos;
    private Vector3 moveTargetRot;

    [HideInInspector]
    //临时位置存储的数组
    public Transform[] tempTrans;

    [HideInInspector]
    //临时播放的序列的动画名称
    public string[] tempAnimationName;

    [HideInInspector]
    //临时位置存储的数组
    public GameObject[] tempObj;

    /// <summary>
    /// 执行
    /// </summary>
    public string funName = "";

    void Awake()
    {
        instace = this;
        CenterPoint = viewObj.transform.position;
    }
    public static CameraMoveControl GetInstance()
    {
        return instace;
    }
    public void Update()
    {
        if (canRotate)
        {
            // 滑动鼠标滚轮 平滑移动
            ViewPointLerp();

            // 视角控制旋转  拖动平滑移动
            ViewPointrotate();
                        
            // 鼠标滚轮按住平移
            if (canMove)
                ViewPointMoveTranslate();

            //碰撞检测
            if (needCheckCollider)
                CheckCollider();
        }
    }

    /// <summary>
    /// 滑动鼠标滚轮 平滑移动
    /// </summary>
    private void ViewPointLerp()
    {
        if (!Lerp)
        {
            distance -= Input.GetAxis("Mouse ScrollWheel") * translatespeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            CameraMove();
        }
    }
      
    /// <summary>
    /// 视角控制旋转  拖动平滑移动
    /// </summary>
    private void ViewPointrotate()
    {
        if (Input.GetMouseButton(1) && !StopMouse)
        {
            roatatex = Input.GetAxis("Mouse X") * Time.deltaTime * rotateSpeed;
            transform.RotateAround( CenterPoint , Vector3.up , roatatex );
            roatatey = -Input.GetAxis("Mouse Y") * Time.deltaTime * rotateSpeed;
            roatatey = Mathf.Clamp( transform.eulerAngles.x + roatatey , limitMineuler , limitMaxeuler ) - transform.eulerAngles.x;
            transform.RotateAround( CenterPoint , transform.right , roatatey );
        }
        else if (Input.GetMouseButtonUp(1) && !StopMouse)
        {
            StopAllCoroutines();
            StartCoroutine(DampRotate(roatatex, roatatey));
            roatatex = 0;
            roatatey = 0;
        }
    }

    /// <summary>
    /// 鼠标滚轮按住平移
    /// </summary>
    private void ViewPointMoveTranslate()
    {
        if (Input.GetMouseButton(2))
        {
            float fxValue = Input.GetAxis("Mouse X") * -1;
            float fyValue = Input.GetAxis("Mouse Y") * -1;
            //相机平移
            transform.transform.Translate(transform.right * translatespeed * Time.deltaTime * fxValue, Space.World);
            transform.transform.Translate(transform.up * translatespeed * Time.deltaTime * fyValue, Space.World);
            //目标点平移
            //目标点平移
            float transX = translatespeed * Time.deltaTime * fxValue;
            float transY = translatespeed * Time.deltaTime * fyValue;
            CenterPoint = CenterPoint +
                          transform.right * translatespeed * Time.deltaTime * fxValue +
                          transform.up * translatespeed * Time.deltaTime * fyValue;

        }
    }


    public void Mouse_Stop()
    {
        StopMouse = true;
    }
    public void Mouse_Begin()
    {
        StopMouse = false;
    }   

    /// <summary>
    /// 射线检测
    /// </summary>
    private void RayCheck()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo,1000f, 1 << LayerMask.NameToLayer("CenterObj")))
        {
            if (Input.GetMouseButtonUp(1))
            {
                if (last_viewpoint)
                {
                    Destroy(last_viewpoint);
                }
                last_viewpoint=new GameObject("ViewPoints");
                last_viewpoint.transform.position=hitInfo.point;
                viewObj = last_viewpoint;
           }
        }
    }

    /// <summary>
    /// 平滑旋转减速
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private IEnumerator DampRotate(float x, float y)
    {
        int xi = 1;
        int yi = 1;
        if (x >= 0)
        {
            xi = 1;
        }
        else
        {
            xi = -1;
        }
        if (y >= 0)
        {
            yi = -1;
        }
        else
        {
            yi = 1;
        }
        while ((x > -20f && x < 20) && (y > -20 && y < 20f) && !Input.GetMouseButtonDown(0))
        {
            x = Mathf.Abs(x);
            y = Mathf.Abs(y);
            x -= Time.deltaTime * 10f;
            y -= Time.deltaTime * 10f;
            if (Mathf.Abs(x) < 1f && Mathf.Abs(y) < 1f)
            {
                break;
            }
            transform.RotateAround( CenterPoint , Vector3.up , x * xi );
            float temprotatey = -y * yi;
            temprotatey = Mathf.Clamp( transform.eulerAngles.x + temprotatey , limitMineuler , limitMaxeuler ) - transform.eulerAngles.x;
            transform.RotateAround( CenterPoint , transform.right , temprotatey );
            yield return 0;
        }
        yield return 0;
    }

    /// <summary>
    /// 将函数放置到Update函数中
    /// 默认的距离为X：0f  Y：0f  Z：0.2f
    /// </summary>
    /// <param name="camera">相机对象</param>
    /// <param name="target">要移动到的位置</param>
    public void CameraMove()
    {
        //求目标物体 与挂脚本物体的向量
        Vector3 direction = CenterPoint - transform.position;
        //想要到达的位置
        Vector3 wantedPosition = CenterPoint - direction.normalized * distance;

        //使用Quaternion.LookRotation  获取一帧中  目标和相机的四元数的值
        Vector3 directionpos = CenterPoint - transform.position;

        if (transform.position.y < CenterPoint.y)
        {
            directionpos = new Vector3(directionpos.x, 0, directionpos.z);
            wantedPosition = new Vector3(wantedPosition.x, CenterPoint.y, wantedPosition.z);
            transform.position = Vector3.Lerp(transform.position, wantedPosition, 2.0f * Time.deltaTime);
        }
        else
        {
            //通过vector3的 Lerp 进行平滑移动    
            //transform.position = Vector3.Lerp(transform.position, wantedPosition, 2.0f * Time.deltaTime);
            transform.position = wantedPosition;
        }

        Quaternion rotate = Quaternion.LookRotation(directionpos);
        //将相机的四元数的值进行平滑设置    
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotate, Time.deltaTime * 3.0f);
        transform.rotation = rotate;
    }


    private void CheckCollider()
    {
        RaycastHit hit;
        Vector3 direction = transform.position - viewObj.transform.position;
        if (Physics.Linecast(viewObj.transform.position, transform.position, out hit, 1 << LayerMask.NameToLayer("Barrier")))
        {
            transform.position = hit.point;
        }
    }
}
