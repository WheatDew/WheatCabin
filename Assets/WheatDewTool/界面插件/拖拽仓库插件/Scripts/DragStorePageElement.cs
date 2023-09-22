using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Data;

public class DragStorePageElement : MonoBehaviour, IPointerDownHandler,IPointerExitHandler
{
    [HideInInspector] public DragStorePage dragStorePage;
    public Dictionary<string, string> stringData = new Dictionary<string, string>();
    public INya data;
    public Text elementName;
    public Image elementImage;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(Input.GetMouseButton(0))
        {
            dragStorePage.scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var obj = Instantiate(dragStorePage.floatElementPrefab,dragStorePage.elementParent);
            obj.dragStorePage = dragStorePage;
            obj.name=transform.name;
            obj.image.sprite = elementImage.sprite;
            obj.data = data;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
