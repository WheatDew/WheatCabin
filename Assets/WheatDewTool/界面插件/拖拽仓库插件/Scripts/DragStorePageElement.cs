using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DragStorePageElement : MonoBehaviour, IPointerDownHandler,IPointerExitHandler
{
    [HideInInspector] public DragStorePage dragStorePage;
    public Dictionary<string, string> stringData = new Dictionary<string, string>();


    public void OnPointerDown(PointerEventData eventData)
    {
        if(Input.GetMouseButton(0))
        {
            dragStorePage.scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var obj = Instantiate(dragStorePage.floatElementPrefab,dragStorePage.elementParent);
            obj.dragStorePage = dragStorePage;

            Debug.Log("×ó¼ü´¥·¢");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
