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

    public Dropdown typeDropdown;

    public void ConfirmButton()
    {
        if (typeBox.text == "����")
        {
            editor.CreateItem(string.Format("{0} {1}", typeBox.text, nameBox.text), dataBox.text);
            editor.bufferData.intData.Add(string.Format("{0} {1}", typeBox.text, nameBox.text), int.Parse(dataBox.text));
        }
        else if (typeBox.text == "������")
        {
            editor.CreateItem(string.Format("{0} {1}", typeBox.text, nameBox.text), dataBox.text);
            editor.bufferData.floatData.Add(string.Format("{0} {1}", typeBox.text, nameBox.text), float.Parse(dataBox.text));
        }
        else if (typeBox.text == "�ַ���"||typeBox.text=="")
        {
            editor.CreateItem(string.Format("{0} {1}", typeBox.text, nameBox.text), dataBox.text);
            editor.bufferData.stringData.Add(string.Format("{0} {1}", typeBox.text, nameBox.text), dataBox.text);
        }


        Destroy(gameObject);
    }

    public void CancelButton()
    {
        Destroy(gameObject);
    }
}
