using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PropertyEditor : MonoBehaviour
{
    [HideInInspector] public PropertyData currentData;
    [HideInInspector] public PropertyData bufferData;
    [HideInInspector] public PropertyEditorItem currentItem;
    public PropertyEditorItem itemPrefab;
    public Transform itemParent,pageParent;
    public AddItemPage addPagePrefab;
    private AddItemPage addPage;
    public EditItemPage editPagePrefab;
    private EditItemPage editPage;

    
    public void SetCurrentData(PropertyData currentData)
    {
        this.currentData = currentData;
    }

    public void ReadData()
    {   
        foreach(var item in currentData.intData)
        {
            CreateItem(item.Key, item.Value.ToString());
        }
        //Ë¢ÐÂ»º´æ
        bufferData = new PropertyData(currentData);

    }

    public void SaveData()
    {
        //»º´æÐ´Èë¼ÇÂ¼
        currentData = new PropertyData(bufferData);
    }

    public void CreateItem(string originName,string originData)
    {
        var obj = Instantiate(itemPrefab, itemParent);
        obj.editor = this;
        obj.originData = originData;
        obj.originName = originName;
        string[] nameSlices = obj.originName.Split(' ');
        if (nameSlices.Length == 2)
        {
            obj.nameText.text = nameSlices[1];
        }
        else if (nameSlices.Length == 1)
        {
            obj.nameText.text = nameSlices[0];
        }

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

    public void TestData()
    {
        currentData = new PropertyData();
        
    }

}

public class PropertyData
{
    public Dictionary<string, int> intData;
    public Dictionary<string, float> floatData;
    public Dictionary<string, string> stringData;

    public PropertyData()
    {
        intData = new Dictionary<string, int>();
        floatData = new Dictionary<string, float>();
        stringData = new Dictionary<string, string>();
    }

    public PropertyData(PropertyData origin)
    {
        intData = new Dictionary<string, int>(origin.intData);
        floatData = new Dictionary<string, float>(origin.floatData);
        stringData = new Dictionary<string, string>(origin.stringData);
    }

    public void Print()
    {
        string s = "";
        foreach(var item in intData)
        {
            s += item.ToString() + " ";
        }
        s += '\n';
        foreach (var item in floatData)
        {
            s += item.ToString() + " ";
        }
        s += '\n';
        foreach (var item in stringData)
        {
            s += item.ToString() + " ";
        }

        Debug.Log(s);
    }
}
