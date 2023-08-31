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
        if (typeDropdown.captionText.text == "����")
        {
            editor.CreateItem(string.Format("{0} {1}", "����", nameBox.text), dataBox.text);
            editor.bufferData.Add(string.Format("{0} {1}", "����", nameBox.text), int.Parse(dataBox.text));
        }
        else if (typeDropdown.captionText.text == "������")
        {
            editor.CreateItem(string.Format("{0} {1}", "������", nameBox.text), dataBox.text);
            editor.bufferData.Add(string.Format("{0} {1}", "������", nameBox.text), float.Parse(dataBox.text));
        }
        else if (typeDropdown.captionText.text == "�ַ���")
        {
            editor.CreateItem(string.Format("{0} {1}", "�ַ���", nameBox.text), dataBox.text);
            editor.bufferData.Add(string.Format("{0} {1}", "�ַ���", nameBox.text), dataBox.text);
        }


        Destroy(gameObject);
    }

    public void CancelButton()
    {
        Destroy(gameObject);
    }
}
