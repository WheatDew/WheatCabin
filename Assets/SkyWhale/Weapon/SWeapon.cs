using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
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
        
    }

    #endregion

    #region 自定义函数

    public void Create(string name,Transform parent,Vector3 position,Vector3 rotation)
    {
        Property data = PropertyMap.s.map[name];
        Debug.Log(data.GetString(SMapEditor.packNameKey));
        Debug.Log(data.GetString(SMapEditor.packObjectNameKey));
        var obj = Instantiate(SMapEditor.GetAssetBundleElement(data.GetString(SMapEditor.packNameKey),data.GetString(SMapEditor.packObjectNameKey)),parent);

        obj.transform.localPosition = position;
        obj.transform.localRotation = Quaternion.Euler(rotation);
        var weapon = obj.AddComponent<Weapon>();
        weapon.InitData(data);
        GameObject start = new GameObject();
        start.transform.localPosition = data.GetVector3(rayStartKey);
        GameObject end = new GameObject();
        end.transform.localPosition = data.GetVector3(rayEndKey);
    }

    #endregion
}
