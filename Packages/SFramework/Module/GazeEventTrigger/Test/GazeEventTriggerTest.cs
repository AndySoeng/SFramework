using Ex;
using UnityEngine;
using UnityEngine.EventSystems;

public class GazeEventTriggerTest : MonoBehaviour
{
    public GameObject ui;

    public GameObject cube;

    // Start is called before the first frame update
    void Start()
    {
        ExEventTrigger.Add(ui, EventTriggerType.PointerEnter, OnEnter);
        ExEventTrigger.Add(ui, EventTriggerType.PointerClick, OnClick);
        ExEventTrigger.Add(ui, EventTriggerType.PointerExit, OnExit);
        ExEventTrigger.Add(cube, EventTriggerType.PointerEnter, OnEnter);
        ExEventTrigger.Add(cube, EventTriggerType.PointerClick, OnClick);
        ExEventTrigger.Add(cube, EventTriggerType.PointerExit, OnExit);
    }

    private void OnEnter(BaseEventData arg0)
    {
        PointerEventData a = arg0 as PointerEventData;

        Debug.Log("OnEnter");
    }

    private void OnClick(BaseEventData arg0)
    {
        PointerEventData a = arg0 as PointerEventData;

        Debug.Log("OnClick");
    }

    private void OnExit(BaseEventData arg0)
    {
        PointerEventData a = arg0 as PointerEventData;

        Debug.Log("OnExit");
    }
}