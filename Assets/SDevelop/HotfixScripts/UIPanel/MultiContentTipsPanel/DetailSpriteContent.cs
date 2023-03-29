using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailSpriteContent : MonoBehaviour
{
    public Image ig_Content;

    public void SetContent(Sprite content)
    {
        ig_Content.sprite = content;    
    }
}
