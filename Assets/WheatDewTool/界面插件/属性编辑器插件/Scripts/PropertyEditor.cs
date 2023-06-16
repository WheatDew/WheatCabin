using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropertyEditor : MonoBehaviour
{
    [HideInInspector] public PropertyData currentData;
    [HideInInspector] public PropertyEditorItem currentItem;
    public PropertyEditorItem itemPrefab;
    public Transform itemParent,pageParent;
    public AddItemPage addPagePrefab;
    private AddItemPage addPage;

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

    public void AddItemButton()
    {
        if (addPage == null)
        {
            addPage = Instantiate(addPagePrefab, pageParent);
            addPage.editor = this;
        }

    }



}

public class PropertyData
{
    public Dictionary<string, int> intData;
    public Dictionary<string, float> floatData;
    public Dictionary<string, string> stringData;

}
