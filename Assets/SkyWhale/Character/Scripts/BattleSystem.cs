using System.Collections;
using System.Collections.Generic;
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
        var obj = Instantiate(SMapEditor.GetAssetBundleElement(data.GetString(2),data.GetString(3)), parent);
        parent.gameObject.SetActive(false);
        Debug.Log(obj.transform.parent);
    }

    public void WeaponDispaly(Property data)
    {
        Entity target = PropertyMap.s.entityMap[data.GetInt(0)];
        var character = (CharacterEntity)target;

        character.weaponPoint.gameObject.SetActive(true);
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
