using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeBar : MonoBehaviour
{
    public RectTransform topBar;
    public RectTransform buttom;
    public RectTransform self;

    [HideInInspector] public string healthPointKey = "HealthPoint";
    [HideInInspector] public string maxHealthPointKey = "MaxHealthPoint";

    [HideInInspector] public INya target;

    private Vector3 barSize=new Vector3(300,20);

    private Entity entity;
    private Vector3 positionOffset;

    private void Start()
    {

        entity = PropertyMap.s.GetEntity(target.Map["EntityID"].List[0].Int);
        Debug.Log(entity.gameObject.name);
        positionOffset.y = target.Map[DataKey.LifeBar].List[1].Float;

    }


    private void Update()
    {
        barSize.x = target.Map[healthPointKey].List[0].Float / target.Map[maxHealthPointKey].List[0].Float * 300;
        topBar.sizeDelta = barSize;

        if (Camera.main != null)
        {
            if (!buttom.gameObject.activeSelf)
                buttom.gameObject.SetActive(true);

            var endPosition = Camera.main.WorldToScreenPoint(entity.transform.position + positionOffset);

            if (Vector3.Distance(self.position, endPosition) > 5)
                self.position = Vector3.Lerp(self.position, endPosition, 0.1f);
        }
        else
        {
            if (buttom.gameObject.activeSelf)
                buttom.gameObject.SetActive(false);
        }

    }
}
