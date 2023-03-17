using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragStoreScrollView : MonoBehaviour
{
    [SerializeField] private DragStorePage DragStorePage;
    [SerializeField] private RectTransform rectTransform;
    
    public bool CalculateInBorder(Vector3 mousePosition)
    {
        bool inBorder = true;
        if( mousePosition.x > rectTransform.position.x + rectTransform.sizeDelta.x * 0.5f
         || mousePosition.x < rectTransform.position.x - rectTransform.sizeDelta.x * 0.5f
         || mousePosition.y < rectTransform.position.y - rectTransform.sizeDelta.y * 0.5f
         || mousePosition.y > rectTransform.position.y + rectTransform.sizeDelta.y * 0.5f
         )
            inBorder = false;
        return inBorder;
    }

    private void Update()
    {
        DragStorePage.inBorder = CalculateInBorder(Input.mousePosition);
    }
}
