using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSectionInteraction_ : MonoBehaviour
{
    private Material mat;
    public Material hlmat;
    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().sharedMaterial;
        //GetComponent<Renderer>().material = mat;
        hlmat = new Material(mat);
        hlmat.SetColor("_EmissionColor", Color.gray);
        Debug.Log("start");
    }

    // Update is called once per frame
    void OnMouseEnter()
    {
        //mat.EnableKeyword("_EMISSION");
        //mat.SetColor("_EmissionColor", Color.white);
        Debug.Log(gameObject.name);
        GetComponent<Renderer>().material = hlmat;
        //mat.SetColor("_BaseColor", Color.gray);
    }
    void OnMouseOver()
    {
        //mat.EnableKeyword("_EMISSION");
        //mat.SetColor("_EmissionColor", Color.white);
    }
    void OnMouseExit()
    {
        //mat.DisableKeyword("_EMISSION");
        //mat.SetColor("_EmissionColor", Color.black);
        //mat.SetColor("_BaseColor", Color.black);
        GetComponent<Renderer>().material = mat;
    }
    void Update()
    {

    }
}
