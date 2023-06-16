using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropertyEditorItem : MonoBehaviour
{
    [HideInInspector]public PropertyEditor editor;
    public Text nameText;
    public string originName,originData,displayName;

    private void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(delegate
        {
            for(int i = 0; i < editor.itemParent.childCount; i++)
            {
                editor.itemParent.GetChild(i).GetComponent<Image>().color = Color.white;
            }
            GetComponent<Image>().color = Color.green;
            editor.currentItem = this;
        });
    }

}
