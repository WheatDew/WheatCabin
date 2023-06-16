using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropertyErrorPage : MonoBehaviour
{
    public Text errorText;

    public void DestroySelf()
    {
        gameObject.SetActive(false);
    }
}
