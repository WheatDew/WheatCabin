using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragStorePageFloatElement : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [HideInInspector] public DragStorePage dragStorePage;
    public Property data;
    public Image image;

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
            dragStorePage.DragEndEvent.Invoke(data);
            Destroy(gameObject);
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragStorePage.DragEndEvent.Invoke(data);
            Destroy(gameObject);
        }
    }

}
