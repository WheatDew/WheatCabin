using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    private static BattleSystem _s;
    public static BattleSystem s { get { return _s; } }

    private void Awake()
    {
        if (_s == null)
            _s = this;

        FunctionMap.Add("WeaponDisplay", WeaponDispaly);
        FunctionMap.Add("WeaponHidden", WeaponHidden);
    }

    /// <summary>
    /// 接收一个参数，参数的列表第一个为造成伤害的实体，第二个为接受伤害的实体
    /// </summary>
    /// <param name="target"></param>
    public void Damage(Property target)
    {
        var param = target.GetDatas();

        Entity passive = PropertyMap.s.entityMap[param[0].GetInt()];
        Entity active = PropertyMap.s.entityMap[param[1].GetInt()];
    }

    /// <summary>
    /// 判定
    /// </summary>
    public void Determination(Vector3 start,Vector3 end)
    {

    }

    /// <summary>
    /// 设置判定点，类型为数组类型，[0]为目标对象，[1]为起始点，[2]为结束点
    /// </summary>
    /// <param name="target"></param>
    public void SetDeterminationPoint(Property target)
    {

    }

    public void SetWeapon(Property data)
    {
        Entity target = PropertyMap.s.entityMap[data.GetInt(0)];
        var character = (CharacterEntity)target;
        Transform parent = FindChild(target.transform,data.GetString(1));
        character.weaponPoint = parent;
        Vector3 positionOffset,rotationOffset;


        positionOffset = Vector3.zero;
        rotationOffset = Vector3.zero;
        switch (data.GetDatas().Count)
        {
            case 3:
                break;
            case 4:
                positionOffset = data.GetVector3(3);
                rotationOffset = Vector3.zero;
                break;
            case 5:
                positionOffset = data.GetVector3(3);
                rotationOffset = data.GetVector3(4);
                break;
            default:
                Debug.LogError("参数数量错误");
                break;
        }


        SWeapon.s.Create(data.GetString(2), parent, positionOffset, rotationOffset);
        parent.gameObject.SetActive(false);

    }

    public void WeaponDispaly(Property data)
    {
        Entity target = PropertyMap.s.entityMap[data.GetInt(0)];
        var character = (CharacterEntity)target;

        character.weaponPoint.gameObject.SetActive(true);
    }

    public void WeaponHidden(Property data)
    {
        Entity target = PropertyMap.s.entityMap[data.GetInt(0)];
        var character = (CharacterEntity)target;

        character.weaponPoint.gameObject.SetActive(false);
    }

    public void SetSecondary(Property data)
    {
        
    }

    //查找所有子物体
    public Transform FindChild(Transform parent, string name)
    {
        Transform result = null;
        for(int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name)
            {
                return parent.GetChild(i);
            }
            result = FindChild(parent.GetChild(i), name);
            if (result != null)
            {
                return result;
            }


        }
        return result;
    }
}
