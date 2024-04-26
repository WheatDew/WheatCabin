using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PropertyEditor : MonoBehaviour
{
    [HideInInspector] public WDEntity currentTarget;
    [HideInInspector] public NyaMap bufferData;
    [HideInInspector] public PropertyEditorItem currentItem;
    public PropertyEditorItem itemPrefab;
    public Transform itemParent,pageParent;
    public AddItemPage addPagePrefab;
    private AddItemPage addPage;
    public EditItemPage editPagePrefab;
    private EditItemPage editPage;
    public Text title;


    public void SetCurrentTarget(WDEntity target,string currentName)
    {
        title.text = currentName;
        this.currentTarget = target;

        ReadData();
    }

    public void ReadData()
    {

        //for (int i = 0; i < itemParent.childCount; i++)
        //{
        //    Destroy(itemParent.GetChild(i).gameObject);
        //}
        //foreach (var item in currentTarget.propertyData.i)
        //{
        //    CreateItem(item.Key+" ÕûÐÍ", item.Value.ToString());
        //}
        //foreach (var item in currentTarget.propertyData.f)
        //{
        //    CreateItem(item.Key+" ¸¡µãÐÍ", item.Value.ToString());
        //}
        //foreach (var item in currentTarget.propertyData.s)
        //{
        //    CreateItem(item.Key+" ×Ö·û´®", item.Value.ToString());
        //}
        ////Ë¢ÐÂ»º´æ
        //bufferData = new PropertyData(currentTarget.propertyData);

    }

    public void SaveData()
    {
        //»º´æÐ´Èë¼ÇÂ¼
        currentTarget.data = new NyaMap(bufferData);
    }

    public void CreateItem(string originName,string originData)
    {
        //Debug.LogFormat("{0}-{1}",originName, originData);
        var obj = Instantiate(itemPrefab, itemParent);
        obj.editor = this;
        obj.originData = originData;
        obj.originName = originName;
        string[] nameSlices = obj.originName.Split(' ');
        obj.nameText.text = nameSlices[0];

    }

    public void SetItem(string originName,string originData)
    {
        var obj = currentItem;
        obj.editor = this;
        obj.originData = originData;
        obj.originName = originName;
        string[] nameSlices = obj.originName.Split(' ');
        obj.nameText.text = nameSlices[0];

    }

    public void AddItemButton()
    {
        if (addPage == null)
        {
            addPage = Instantiate(addPagePrefab, pageParent);
            addPage.editor = this;
            addPage.typeDropdown.value = 0;
        }
    }

    public void EditItemButton()
    {
        if(editPage==null)
        {
            editPage = Instantiate(editPagePrefab, pageParent);
            editPage.editor = this;
            string[] slices = currentItem.originName.Split(' ');
            editPage.nameBox.text = slices[0];
            editPage.dataBox.text = currentItem.originData;
            if (slices.Length == 2)
                editPage.SetTypeDropDown(slices[1]);
        }
    }


}


