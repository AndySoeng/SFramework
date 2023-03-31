using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridClone : MonoBehaviour {
    public int n = 1;
    public float dist = 2;
    public bool rand = true;


	// Use this for initialization
	void Start () {

        if (transform.childCount == 1)
            Clone(transform.GetChild(0).gameObject);
	}
	
	// Update is called once per frame
	void Clone (GameObject source) {
        GameObject element;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    if (i == 0 && j == 0 && k == 0) { element = source; } else { element = Instantiate(source, transform); }
                    element.transform.localRotation = Quaternion.identity;
                    element.transform.localPosition = new Vector3((i - (n - 1) * 0.5f) * dist, ((k - (n - 1) * 0.5f)) * dist, ((j - (n - 1) * 0.5f)) * dist);
                    float sc = rand? Random.Range(0.5f, 1.5f):1;
                    element.transform.localScale = sc*Vector3.one;
                    Mesh mesh = element.GetComponent<MeshFilter>().mesh;
                    Vector2[] uvw = mesh.uv;
                    for (int i1 = 0; i1 < uvw.Length; i1++) 
                    {
                        uvw[i1] *= sc;
                    }
                    mesh.uv = uvw;

                }
            }
        }
		
	}
}
