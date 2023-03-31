/*using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class SwitchSettings : MonoBehaviour {
    public SectionSetter M1;
    public HorizontalClippingSection M2;


    public void Switch(bool val)
    {
        if (val)
        {
            M2.enabled = !val;
            M2.GetComponent<BoxCollider>().enabled = val;
            M1.enabled = val;
        }else{
            M2.GetComponent<BoxCollider>().enabled = val;
            M1.enabled = val;
            M2.enabled = !val;
        }
    }


	void Start () {
        Switch(true);
	}
	

}*/
