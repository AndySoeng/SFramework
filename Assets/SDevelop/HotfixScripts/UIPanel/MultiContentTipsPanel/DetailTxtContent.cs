using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DetailTxtContent : MonoBehaviour
{
    public TMP_Text txt_Content;

    public void SetContent(string content)
    {
        txt_Content.text = content;    
    }
}
