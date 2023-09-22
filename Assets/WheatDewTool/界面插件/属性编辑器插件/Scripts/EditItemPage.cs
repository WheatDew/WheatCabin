using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditItemPage : MonoBehaviour
{
    [HideInInspector] public PropertyEditor editor;
    public InputField nameBox;
    public InputField dataBox;
    public Dropdown typeDropdown;
    public PropertyErrorPage errorPage;

    public void SetTypeDropDown(string typeString)
    {
        int currentIndex = -1;
        for (int i = 0; i < typeDropdown.options.Count; i++)
        {
            if (typeDropdown.options[i].text == typeString)
            {
                currentIndex = i;
                typeDropdown.value = i;
                break;
            }
        }
    }

    public void ConfirmButton()
    {
        if (typeDropdown.captionText.text == "����")
        {
            editor.SetItem(string.Format("{0} {1}", "����", nameBox.text), dataBox.text);
            editor.bufferData.Add(nameBox.text,new NyaInt(int.Parse(dataBox.text)));
        }
        else if (typeDropdown.captionText.text == "������")
        {
            editor.SetItem(string.Format("{0} {1}", "������", nameBox.text), dataBox.text);
            editor.bufferData.Add(nameBox.text,new NyaFloat(float.Parse(dataBox.text)));
        }
        else if (typeDropdown.captionText.text == "�ַ���")
        {
            editor.SetItem(string.Format("{0} {1}", "�ַ���", nameBox.text), dataBox.text);
            editor.bufferData.Add( nameBox.text,new NyaString(dataBox.text));
        }

        Destroy(gameObject);
    }

    public void CancelButton()
    {
        Destroy(gameObject);
    }
}
