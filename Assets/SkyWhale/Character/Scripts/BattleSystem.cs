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
        FunctionMap.Add("SetWeapon", SetWeapon);
    }


    /// <summary>
    /// 设置判定点，类型为数组类型，[0]为起始点，[1]为结束点，[2]为伤害来源,[3]为伤害目标
    /// </summary>
    /// <param name="target"></param>
    public void SetDeterminationPoint(INya target)
    {

    }

    public void SetWeapon(INya data)
    {
        WDEntity target = PropertyMap.s.entityMap[data.List[1].Int];
        var character = (CharacterEntity)target;
        Transform parent = FindChild(target.transform,data.List[2].String);
        character.weaponPoint = parent;
        Vector3 positionOffset,rotationOffset;

        positionOffset = Vector3.zero;
        rotationOffset = Vector3.zero;

        var weapon = SWeapon.s.Create(character, parent, positionOffset, rotationOffset, PropertyMap.s.map[data.List[3].String]);
        character.weapon = weapon;
        character.weaponPoint.gameObject.SetActive(false);
    }

    public void WeaponDispaly(INya data)
    {
        Debug.Log("调用WeaponDisplay");
        WDEntity target = PropertyMap.s.entityMap[data.List[0].Int];
        var character = (CharacterEntity)target;

        character.weaponPoint.gameObject.SetActive(true);
    }

    public void WeaponHidden(INya data)
    {
        Debug.Log("调用WeaponHidden");
        WDEntity target = PropertyMap.s.entityMap[data.List[0].Int];
        var character = (CharacterEntity)target;

        character.weaponPoint.gameObject.SetActive(false);
    }

    public void SetSecondary(INya data)
    {
        
    }

    //查找所有子物体
    public Transform FindChild(Transform parent, string name)
    {
        //如果父物体的名字就是要找的名字，直接返回父物体
        if (parent.name == name)
        {
            return parent;
        }
        //否则，遍历父物体的所有子物体，递归调用本函数
        foreach (Transform child in parent)
        {
            Transform result = FindChild(child, name);
            //如果找到了匹配的子物体，返回结果，否则继续查找
            if (result != null)
            {
                return result;
            }
        }
        //如果没有找到匹配的子物体，返回null
        return null;
    }

}
