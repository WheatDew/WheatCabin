using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PropertyEditor : MonoBehaviour
{
    [HideInInspector] public PropertyData currentData;
    [HideInInspector] public PropertyEditorItem currentItem;
    public PropertyEditorItem itemPrefab;
    public Transform itemParent,pageParent;
    public AddItemPage addPagePrefab;
    private AddItemPage addPage;
    public EditItemPage editPagePrefab;
    private EditItemPage editPage;
    public UnityEvent<PropertyData> readEvent;
    public UnityEvent<PropertyData> SaveEvent;
    public void ReadData(PropertyData currentData)
    {
        this.currentData = currentData;        
    }

    public void SaveData(PropertyData currentData)
    {
        currentData = this.currentData;
    }

    public void CreateItem(string originName,string originData)
    {
        var obj = Instantiate(itemPrefab, itemParent);
        obj.editor = this;
        obj.originData = originData;
        obj.originName = originName;
        string[] nameSlices = obj.originName.Split(' ');
        if (nameSlices.Length == 2)
            obj.nameText.text = nameSlices[1];
        else if (nameSlices.Length == 1)
            obj.nameText.text = nameSlices[0];
    }

    public void SetItem(string originName,string originData)
    {
        var obj = currentItem;
        obj.editor = this;
        obj.originData = originData;
        obj.originName = originName;
        string[] nameSlices = obj.originName.Split(' ');
        if (nameSlices.Length == 2)
            obj.nameText.text = nameSlices[1];
        else if (nameSlices.Length == 1)
            obj.nameText.text = nameSlices[0];

    }

    public void AddItemButton()
    {
        if (addPage == null)
        {
            addPage = Instantiate(addPagePrefab, pageParent);
            addPage.editor = this;
        }

    }

    public void EditItemButton()
    {
        if(editPage==null)
        {
            editPage = Instantiate(editPagePrefab, pageParent);
            editPage.editor = this;
            string[] slices = currentItem.originName.Split(' ');
            if (slices.Length == 1)
                editPage.nameBox.text = slices[0];
            else if (slices.Length == 2)
            {
                editPage.nameBox.text = slices[1];
                editPage.typeBox.text = slices[0];
            }
            editPage.dataBox.text = currentItem.originData;

        }
    }



}

public class PropertyData
{
    public Dictionary<string, int> intData;
    public Dictionary<string, float> floatData;
    public Dictionary<string, string> stringData;

}
