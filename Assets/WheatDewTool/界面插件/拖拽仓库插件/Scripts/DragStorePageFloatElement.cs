using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragStorePageFloatElement : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [HideInInspector] public DragStorePage dragStorePage;

    private void Start()
    {
        rectTransform.position = Input.mousePosition;
    }

    private void Update()
    {
        rectTransform.position = Input.mousePosition;

        if (dragStorePage.floatElementType==DragStorePage.FloatElementType.Border
            &&!dragStorePage.inBorder)
        {
            dragStorePage.DragEndEvent.Invoke();
            Destroy(gameObject);
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragStorePage.DragEndEvent.Invoke();
            Destroy(gameObject);
        }
    }

}
