using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DragStorePage : MonoBehaviour
{
    public enum FloatElementType { Border,Borderless };

    public DragStorePageElement elementPrefab;
    public DragStorePageFloatElement floatElementPrefab;
    public Transform elementParent;
    public Transform floatElementParent;
    public ScrollRect scrollRect;
    [HideInInspector] public FloatElementType floatElementType;

    public bool inBorder=false;

    public UnityEvent<INya> DragEndEvent;
    public UnityEvent InitEvent;
    public UnityEvent<INya> ItemInit;

    #region 属性对应列表

    public string storeElementKey = "StoreElement";

    #endregion


    private void Start()
    {
        InitEvent.Invoke();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
        }
    }

    public void CreateElement(string elementName)
    {
        var obj = Instantiate(elementPrefab, elementParent);
        obj.dragStorePage = this;
        obj.name = elementName;
        obj.elementName.text= elementName;
    }

    public void CreateElement(INya data,Sprite elementImage)
    {
        var obj = Instantiate(elementPrefab, elementParent);
        obj.dragStorePage = this;
        obj.name = data.GetString(storeElementKey,2);
        obj.elementName.text = data.GetString(storeElementKey, 2);
        obj.elementImage.sprite = elementImage;
        obj.data = data;
    }

    //public void CreateElement(string elementName,Texture2D texture)
    //{
    //    var obj = Instantiate(elementPrefab, elementParent);
    //    obj.dragStorePage = this;
    //    obj.name = elementName;
    //    obj.elementName.text = elementName;
    //    obj.elementImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f,0.5f));
    //}


    //加载资源
    public void LoadResource()
    {

    }

}
