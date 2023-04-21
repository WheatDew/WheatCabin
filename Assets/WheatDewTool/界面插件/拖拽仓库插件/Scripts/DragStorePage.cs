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


    //º”‘ÿ◊ ‘¥
    public void LoadResource()
    {

    }

}
