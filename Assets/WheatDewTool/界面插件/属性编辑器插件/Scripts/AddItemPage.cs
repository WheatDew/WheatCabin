using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddItemPage : MonoBehaviour
{
    [HideInInspector] public PropertyEditor editor;
    public InputField nameBox;
    public InputField typeBox;
    public InputField dataBox;
    public PropertyErrorPage errorPage;

    public void ConfirmButton()
    {
        if (typeBox.text == "整型")
        {
            editor.CreateItem(string.Format("{0} {1}", typeBox.text, nameBox.text), dataBox.text);
        }
        else if (typeBox.text == "浮点数")
        {
            editor.CreateItem(string.Format("{0} {1}", typeBox.text, nameBox.text), dataBox.text);
        }
        else if (typeBox.text == "字符串")
        {
            editor.CreateItem(string.Format("{0} {1}", typeBox.text, nameBox.text), dataBox.text);
        }

        Destroy(gameObject);
    }

    public void CancelButton()
    {
        Destroy(gameObject);
    }
}
