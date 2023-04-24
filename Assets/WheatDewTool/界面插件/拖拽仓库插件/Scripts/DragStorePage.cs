using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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

    public UnityEvent<string> DragEndEvent;
    public UnityEvent InitEvent;
    public UnityEvent<string> ItemInit;


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

    public void CreateElement(string elementName,Sprite elementImage)
    {
        var obj = Instantiate(elementPrefab, elementParent);
        obj.dragStorePage = this;
        obj.name = elementName;
        obj.elementName.text = elementName;
        obj.elementImage.sprite = elementImage;
    }

    public void CreateElement(string elementName,Texture2D texture)
    {
        var obj = Instantiate(elementPrefab, elementParent);
        obj.dragStorePage = this;
        obj.name = elementName;
        obj.elementName.text = elementName;
        obj.elementImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f,0.5f));
    }


    //º”‘ÿ◊ ‘¥
    public void LoadResource()
    {

    }

}
