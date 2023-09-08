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
    }

    /// <summary>
    /// ����һ���������������б��һ��Ϊ����˺���ʵ�壬�ڶ���Ϊ�����˺���ʵ��
    /// </summary>
    /// <param name="target"></param>
    public void Damage(Property target)
    {
        var param = target.GetDatas();

        Entity passive = PropertyMap.s.entityMap[param[0].GetInt()];
        Entity active = PropertyMap.s.entityMap[param[1].GetInt()];
    }

    /// <summary>
    /// �ж�
    /// </summary>
    public void Determination(Vector3 start,Vector3 end)
    {

    }

    /// <summary>
    /// �����ж��㣬����Ϊ�������ͣ�[0]ΪĿ�����[1]Ϊ��ʼ�㣬[2]Ϊ������
    /// </summary>
    /// <param name="target"></param>
    public void SetDeterminationPoint(Property target)
    {

    }

    public void SetWeapon(Property data)
    {
        Entity target = PropertyMap.s.entityMap[data.GetInt(0)];
        Debug.Log(target.gameObject.name);
        Transform parent = target.transform.Find(data.GetString(1));
        var obj = Instantiate(SMapEditor.GetAssetBundleElement(data.GetString(2),data.GetString(3)), target.transform);
        Debug.Log(obj.transform.parent);
    }

    public void SetSecondary(Property data)
    {
        
    }
}
