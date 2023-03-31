using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UVScaler : MonoBehaviour {
    private Mesh m;
    private Vector2[] muv;
	// Use this for initialization
	void Start () {
        m = GetComponent<MeshFilter>().mesh;
        muv = m.uv;
        SetUV();
	}
	
	// Update is called once per frame
	void Update () {

	}
    public void SetUV()
    {
        Vector2 offset = new Vector2(0, 0);
        offset = new Vector2(0.5f * (1 - transform.right.magnitude * transform.lossyScale.x), 0.5f*(1 - transform.up.magnitude * transform.lossyScale.y)); 

        muv[0] = new Vector2(0, 0) + offset;
        muv[2] = new Vector2(transform.right.magnitude * transform.lossyScale.x, 0) + offset;
        muv[3] = new Vector2(0, transform.up.magnitude * transform.lossyScale.y) + offset;
        muv[1] = new Vector2(transform.right.magnitude * transform.lossyScale.x, transform.up.magnitude * transform.lossyScale.y) + offset;
        m.uv = muv;
    }
}
