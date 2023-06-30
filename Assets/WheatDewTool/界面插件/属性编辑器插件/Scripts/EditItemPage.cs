using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditItemPage : MonoBehaviour
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
            editor.SetItem(string.Format("{0} {1}", typeBox.text, nameBox.text), dataBox.text);
            editor.bufferData.intData[string.Format("{0} {1}", typeBox.text, nameBox.text)]= int.Parse(dataBox.text);
        }
        else if (typeBox.text == "浮点数")
        {
            editor.SetItem(string.Format("{0} {1}", typeBox.text, nameBox.text), dataBox.text);
            editor.bufferData.floatData[string.Format("{0} {1}", typeBox.text, nameBox.text)] = float.Parse(dataBox.text);
        }
        else if (typeBox.text == "字符串"||typeBox.text=="")
        {
            editor.SetItem(string.Format("{0} {1}", typeBox.text, nameBox.text), dataBox.text);
            editor.bufferData.stringData[string.Format("{0} {1}", typeBox.text, nameBox.text)] = dataBox.text;
        }

        Destroy(gameObject);
    }

    public void CancelButton()
    {
        Destroy(gameObject);
    }
}
