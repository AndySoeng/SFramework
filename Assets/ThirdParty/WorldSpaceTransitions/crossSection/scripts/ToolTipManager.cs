using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToolTipManager : MonoBehaviour {

    private static GameObject currentObj;
    public GameObject tooltipPanel;
    private static GameObject ttPanel;

	void Start () {
        ttPanel = tooltipPanel;
        ttPanel.SetActive(false);
        foreach (Transform t in gameObject.transform) t.gameObject.AddComponent<TooltipObject>();
	}
	
	// Update is called once per frame
	void Update () {
        if (currentObj) {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(currentObj.transform.position);
            tooltipPanel.GetComponent<Image>().rectTransform.position = screenPos + new Vector3(0f,20f,0);
        }

	}

    public static void SetCurrent(GameObject g)
    {
        currentObj = g;
        if (g)
        {
            ttPanel.SetActive(true);
            ttPanel.GetComponentInChildren<Text>().text = g.GetComponent<Renderer>().materials[0].shader.name;
        }
        else {
            ttPanel.SetActive(false);
        }
    }
}
