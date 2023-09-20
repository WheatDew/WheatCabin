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
    /// �����ж��㣬����Ϊ�������ͣ�[0]Ϊ��ʼ�㣬[1]Ϊ�����㣬[2]Ϊ�˺���Դ,[3]Ϊ�˺�Ŀ��
    /// </summary>
    /// <param name="target"></param>
    public void SetDeterminationPoint(Property target)
    {

    }

    public void SetWeapon(Property origin)
    {
        var data = origin.GetData(CharacterEntity.WeaponKey);
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
                Debug.LogError("������������");
                break;
        }


        var weapon = SWeapon.s.Create(character, parent, positionOffset, rotationOffset);

        character.weapon = weapon;

        character.weaponPoint.gameObject.SetActive(false);
        //weapon.SetActive(false);
        //parent.gameObject.SetActive(false);

    }

    public void WeaponDispaly(Property data)
    {
        Debug.Log("����WeaponDisplay");
        Entity target = PropertyMap.s.entityMap[data.GetInt(0)];
        var character = (CharacterEntity)target;

        character.weaponPoint.gameObject.SetActive(true);
    }

    public void WeaponHidden(Property data)
    {
        Debug.Log("����WeaponHidden");
        Entity target = PropertyMap.s.entityMap[data.GetInt(0)];
        var character = (CharacterEntity)target;

        character.weaponPoint.gameObject.SetActive(false);
    }

    public void SetSecondary(Property data)
    {
        
    }

    //��������������
    public Transform FindChild(Transform parent, string name)
    {
        //�������������־���Ҫ�ҵ����֣�ֱ�ӷ��ظ�����
        if (parent.name == name)
        {
            return parent;
        }
        //���򣬱�������������������壬�ݹ���ñ�����
        foreach (Transform child in parent)
        {
            Transform result = FindChild(child, name);
            //����ҵ���ƥ��������壬���ؽ���������������
            if (result != null)
            {
                return result;
            }
        }
        //���û���ҵ�ƥ��������壬����null
        return null;
    }

}
