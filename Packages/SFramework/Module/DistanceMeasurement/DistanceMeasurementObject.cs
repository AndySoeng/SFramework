using TMPro;
using UnityEngine;

public class DistanceMeasurementObject : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Canvas canvas_Dis;
    [SerializeField] private TMP_Text txt_Dis;

    public Vector3 StartSpherePos
    {
        get { return startPoint.position; }
        set
        {
            startPoint.position = value;
            UpdateComponent();
        }
    }

    public Vector3 startRealePos;


    public Vector3 EndSpherePos
    {
        get { return endPoint.position; }
        set
        {
            endPoint.position = value;
            UpdateComponent();
        }
    }
    public Vector3 endRealePos;

    public void UpdateComponent()
    {
        lineRenderer.SetPosition(0, StartSpherePos);
        lineRenderer.SetPosition(1, EndSpherePos);

        if (canvas_Dis != null)
        {
            canvas_Dis.transform.position = (StartSpherePos + EndSpherePos) / 2;
            canvas_Dis.transform.localPosition += new Vector3(0, 0.075f, 0); //加一个球的半径

            txt_Dis.text = Vector3.Distance(startRealePos, endRealePos).ToString("F");
        }
    }
}