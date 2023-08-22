using System.Collections.Generic;
using DG.Tweening;
using Ex;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

public class FovLaserGen : SerializedMonoBehaviour
{
    public float fovNear = 3f;
    public float fovFar = 10f;
    public float fovVertical = 25.4f;
    public float fovHorizontal = 120f;
    public int fovSample = 1000;

    public Material mat_FovLine;
    public float fovLineWith = 0.1f;

    Vector3[] fovCutSurfaceLine = new Vector3[8];
    //分别对应近裁面的左上-右上、右上-右下、右下-左下、左下-左上，远裁面的左上-右上、右上-右下、右下-左下、左下-左上
    List<Vector3>[] fovPosList = new List<Vector3>[8];


    public GameObject prefab_Laser;
    public float laserLength = 225f;
    public int ColumnLaserCount = 12;

    // Start is called before the first frame update
    void Start()
    {
        ComputeFovLineData();
        DrawFovLine();

        GenLaserTween();
    }

    private void GenLaserTween()
    {
        Transform laserParent = new GameObject().transform;
        laserParent.SetParent(transform);
        laserParent.position = Vector3.zero;
        laserParent.rotation = transform.rotation* Quaternion.Euler(0,-fovHorizontal / 2f  ,  0);

        float columnLaserSampleAngle = fovVertical / ColumnLaserCount;

        for (int i = 0; i < ColumnLaserCount; i++)
        {
            GameObject laser = Instantiate(prefab_Laser, Vector3.zero, laserParent.rotation * Quaternion.Euler(-fovVertical / 2f + columnLaserSampleAngle * i, 0, 0), laserParent);
            laser.transform.localScale = new Vector3(1, 1, laserLength);
        }

        laserParent.DORotateQuaternion(transform.rotation * Quaternion.Euler(0, fovHorizontal / 2f, 0), 1).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    private void DrawFovLine()
    {
        for (int i = 0; i < fovPosList.Length; i++)
        {
            AddLineRenderer(fovPosList[i]);
        }

        AddLineRenderer(new List<Vector3>() { fovCutSurfaceLine[0], fovCutSurfaceLine[4] });
        AddLineRenderer(new List<Vector3>() { fovCutSurfaceLine[1], fovCutSurfaceLine[5] });
        AddLineRenderer(new List<Vector3>() { fovCutSurfaceLine[2], fovCutSurfaceLine[6] });
        AddLineRenderer(new List<Vector3>() { fovCutSurfaceLine[3], fovCutSurfaceLine[7] });
    }

    private void AddLineRenderer(List<Vector3> posList)
    {
        GameObject go = new GameObject();
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;
        go.transform.rotation = quaternion.identity;
        go.hideFlags = HideFlags.HideInHierarchy;

        LineRenderer fovLineRenderer = go.AddComponent<LineRenderer>();
        fovLineRenderer.material = mat_FovLine;
        fovLineRenderer.startWidth = fovLineWith;
        fovLineRenderer.endWidth = fovLineWith;
        fovLineRenderer.useWorldSpace = false;
        fovLineRenderer.positionCount = 0;
        fovLineRenderer.positionCount = posList.Count;
        fovLineRenderer.SetPositions(posList.ToArray());
    }


    public void ComputeFovLineData()
    {
        float halfFovHorizontal = fovHorizontal / 2;
        float halfFovVertical = fovVertical / 2;

        // 左上点
        fovCutSurfaceLine[0] = transform.GenFovPos(fovNear, halfFovHorizontal, -halfFovVertical);
        //右上点
        fovCutSurfaceLine[1] = transform.GenFovPos(fovNear, -halfFovHorizontal, -halfFovVertical);
        //右下点
        fovCutSurfaceLine[2] = transform.GenFovPos(fovNear, -halfFovHorizontal, halfFovVertical);
        // 左下点
        fovCutSurfaceLine[3] = transform.GenFovPos(fovNear, halfFovHorizontal, halfFovVertical);

        // 左上点
        fovCutSurfaceLine[4] = transform.GenFovPos(fovFar, halfFovHorizontal, -halfFovVertical);
        //右上点
        fovCutSurfaceLine[5] = transform.GenFovPos(fovFar, -halfFovHorizontal, -halfFovVertical);
        //右下点
        fovCutSurfaceLine[6] = transform.GenFovPos(fovFar, -halfFovHorizontal, halfFovVertical);
        // 左下点
        fovCutSurfaceLine[7] = transform.GenFovPos(fovFar, halfFovHorizontal, halfFovVertical);

        //近裁面的左上-右上
        fovPosList[0] = ComputeFovPosByHorizontal(fovNear, -halfFovHorizontal, halfFovHorizontal, -halfFovVertical);
        //近裁面的右上-右下
        fovPosList[1] = ComputeFovPosByVertical(fovNear, -halfFovVertical, halfFovVertical, -halfFovHorizontal);
        //近裁面的右下-左下
        fovPosList[2] = ComputeFovPosByHorizontal(fovNear, -halfFovHorizontal, halfFovHorizontal, halfFovVertical);
        //近裁面的左下-左上
        fovPosList[3] = ComputeFovPosByVertical(fovNear, -halfFovVertical, halfFovVertical, halfFovHorizontal);

        //远裁面的左上-右上
        fovPosList[4] = ComputeFovPosByHorizontal(fovFar, -halfFovHorizontal, halfFovHorizontal, -halfFovVertical);
        //远裁面的右上-右下
        fovPosList[5] = ComputeFovPosByVertical(fovFar, -halfFovVertical, halfFovVertical, -halfFovHorizontal);
        //远裁面的右下-左下
        fovPosList[6] = ComputeFovPosByHorizontal(fovFar, -halfFovHorizontal, halfFovHorizontal, halfFovVertical);
        //远裁面的左下-左上
        fovPosList[7] = ComputeFovPosByVertical(fovFar, -halfFovVertical, halfFovVertical, halfFovHorizontal);
        
        //把fovCutSurfaceLine插入到fovPosList中，防止点线不连接； fovPosList中的点位顺序是右->左、上->下
        fovPosList[0].Insert(0,fovCutSurfaceLine[1]);
        fovPosList[0].Add(fovCutSurfaceLine[0]);
        fovPosList[1].Insert(0,fovCutSurfaceLine[1]);
        fovPosList[1].Add(fovCutSurfaceLine[2]);
        fovPosList[2].Insert(0,fovCutSurfaceLine[2]);
        fovPosList[2].Add(fovCutSurfaceLine[3]);
        fovPosList[3].Insert(0,fovCutSurfaceLine[0]);
        fovPosList[3].Add(fovCutSurfaceLine[3]);
        
        
        fovPosList[4].Insert(0,fovCutSurfaceLine[5]);
        fovPosList[4].Add(fovCutSurfaceLine[4]);
        fovPosList[5].Insert(0,fovCutSurfaceLine[5]);
        fovPosList[5].Add(fovCutSurfaceLine[6]);
        fovPosList[6].Insert(0,fovCutSurfaceLine[6]);
        fovPosList[6].Add(fovCutSurfaceLine[7]);
        fovPosList[7].Insert(0,fovCutSurfaceLine[4]);
        fovPosList[7].Add(fovCutSurfaceLine[7]);
    }

    private List<Vector3> ComputeFovPosByVertical(float dis, float minFovVertical, float maxFovVertical, float fovHorizontal)
    {
        List<Vector3> fovPosList = new List<Vector3>();
        float sampleAngle = (maxFovVertical - minFovVertical) / fovSample;
        for (int i = 0; i < fovSample; i++)
        {
            fovPosList.Add(transform.GenFovPos(dis, fovHorizontal, minFovVertical + sampleAngle * i));
        }

        return fovPosList;
    }

    private List<Vector3> ComputeFovPosByHorizontal(float dis, float minFovHorizontal, float maxFovHorizontal, float fovVertical)
    {
        List<Vector3> fovPosList = new List<Vector3>();

        float sampleAngle = (maxFovHorizontal - minFovHorizontal) / fovSample;
        for (int i = 0; i < fovSample; i++)
        {
            fovPosList.Add(transform.GenFovPos(dis, minFovHorizontal + sampleAngle * i, fovVertical));
        }

        return fovPosList;
    }
}