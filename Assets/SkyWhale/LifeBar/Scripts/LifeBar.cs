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
    [HideInInspector] public string healthPointPositionOffset = "HealthPointPositionOffset";
    [HideInInspector] public string entityID = "EntityID";

    [HideInInspector] public PropertyData target;

    private Vector3 barSize=new Vector3(300,20);

    private Entity entity;
    private Vector3 positionOffset;

    private void Start()
    {
        entity = PropertyMap.s.GetEntity(target.GetIntData(entityID));
        if (target.GetVector3(healthPointPositionOffset) != null)
            positionOffset = target.GetVector3(healthPointPositionOffset);
        else
            positionOffset = Vector3.zero;

        Debug.Log(positionOffset);
    }

    private void Update()
    {
        barSize.x = target.GetFloat(healthPointKey) / target.GetFloat(maxHealthPointKey) * 300;
        topBar.sizeDelta = barSize;

        if (Camera.main != null)
        {
            if (!buttom.gameObject.activeSelf)
                buttom.gameObject.SetActive(true);
            self.position = Camera.main.WorldToScreenPoint(entity.transform.position+positionOffset);
        }
        else
        {
            if (buttom.gameObject.activeSelf)
                buttom.gameObject.SetActive(false);
        }

    }
}
