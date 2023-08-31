using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddItemPage : MonoBehaviour
{
    [HideInInspector] public PropertyEditor editor;
    public InputField nameBox;
    public InputField dataBox;
    public PropertyErrorPage errorPage;

    public Dropdown typeDropdown;


    public void ConfirmButton()
    {
        if (typeDropdown.captionText.text == "整型")
        {
            editor.CreateItem(string.Format("{0} {1}", "整型", nameBox.text), dataBox.text);
            editor.bufferData.Add(string.Format("{0} {1}", "整型", nameBox.text), int.Parse(dataBox.text));
        }
        else if (typeDropdown.captionText.text == "浮点型")
        {
            editor.CreateItem(string.Format("{0} {1}", "浮点型", nameBox.text), dataBox.text);
            editor.bufferData.Add(string.Format("{0} {1}", "浮点型", nameBox.text), float.Parse(dataBox.text));
        }
        else if (typeDropdown.captionText.text == "字符串")
        {
            editor.CreateItem(string.Format("{0} {1}", "字符串", nameBox.text), dataBox.text);
            editor.bufferData.Add(string.Format("{0} {1}", "字符串", nameBox.text), dataBox.text);
        }


        Destroy(gameObject);
    }

    public void CancelButton()
    {
        Destroy(gameObject);
    }
}
