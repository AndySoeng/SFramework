
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DetailWindowButton : MonoBehaviour
{
    public TMP_Text txt_NormalTxt;
    public TMP_Text txt_PressTxt;

    public void SetName(string name)
    {
        txt_NormalTxt.text=name;
        txt_PressTxt.text=name;
    }
}
