using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

public class SWeapon : MonoBehaviour
{
    #region 单例代码
    private static SWeapon _s;
    public static SWeapon s { get { return _s; } }

    private void Awake()
    {
        if (!_s) _s = this;
    }
    #endregion

    #region 属性转换

    private string rayStartKey = "RayStart";
    private string rayEndKey = "RayEnd";

    #endregion

    #region 系统函数

    private void Start()
    {
        FunctionMap.Add("Damage", Damage);
        FunctionMap.Add("DisplayWeaponRange", DisplayWeaponRange);
    }

    #endregion

    #region 自定义函数

    public Weapon Create(CharacterEntity origin,Transform parent,Vector3 position,Vector3 rotation)
    {

        Property data = PropertyMap.s.map[origin.data.GetString(CharacterEntity.WeaponKey,2)];
        var obj = Instantiate(SMapEditor.GetAssetBundleElement(data.GetString(SMapEditor.packNameKey),data.GetString(SMapEditor.packObjectNameKey)),parent);

        obj.transform.localPosition = position;
        obj.transform.localRotation = Quaternion.Euler(rotation);
        var weapon = obj.AddComponent<Weapon>();
        weapon.InitData(data);


        GameObject start = new GameObject();
        start.transform.parent = parent;
        start.name = "start";
        start.transform.localPosition = data.GetVector3(rayStartKey);
        start.transform.localRotation = Quaternion.identity;
        GameObject end = new GameObject();
        end.transform.parent = parent;
        end.name = "end";
        end.transform.localPosition = data.GetVector3(rayEndKey);
        end.transform.localRotation = Quaternion.identity;
        weapon.start = start;
        weapon.end = end;

        origin.weapon = weapon;

        weapon.StartEvent();

        return weapon;
    }

    /// <summary>
    /// 接收一个参数，参数的列表第一个为造成伤害的实体，第二个为接受伤害的实体
    /// </summary>
    /// <param name="target"></param>
    public void Damage(Property data)
    {
        Debug.Log(data.GetData(0));
        //Entity target = PropertyMap.s.entityMap[data.GetInt(0)];
        //var character = (CharacterEntity)target;

        //character.weapon.DamageStart();

    }


    public void DisplayWeaponRange(Property data)
    {
        Entity target = PropertyMap.s.entityMap[data.GetInt(0)];
        var weapon = (Weapon)target;

        weapon.DisplayWeaponRange();
    }

    #endregion
}
