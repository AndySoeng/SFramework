using UnityEngine;
using System.Collections;

public class EnableShadersKeyword : MonoBehaviour {
    public string kwd = "CLIP_PLANE";
	// Use this for initialization
	void Start () {
        Material[] mats = GetComponent<Renderer>().materials;
        foreach (Material m in mats) m.EnableKeyword(kwd);
	}

}
