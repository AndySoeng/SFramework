using UnitaskXNode.Base;
using UnityEngine;

public class StepGraphComponent : MonoBehaviour
{
    [SerializeField]
    //[HideInInspector]
    public StepGraph graph;

    private async void  Start()
    {
        await graph.StartGraph();
    }
}
