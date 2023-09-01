using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class SCharacter : MonoBehaviour
{
    #region 单例代码
    private static SCharacter _s;
    public static SCharacter s { get { return _s; } }

    private void Awake()
    {
        if (!_s) _s = this;
    }
    #endregion

    #region 属性转换

    public string displayName = "DisplayName";
    public string objectType = "Type";
    public string detailType = "DetialType";

    #endregion

    #region 系统函数
    private void Start()
    {
        mapEditor.elementTypeInitEvent.AddListener(InitCharacter);

    }
    #endregion


    public SMapEditor mapEditor;
    public void InitCharacter(PropertyData data,GameObject obj)
    {
        if (data.GetString(objectType) == "Character")
        {
            var cobj = obj.AddComponent<CharacterObject>();
            cobj.propertyData = data;
            cobj.type = "Character";
            cobj.detailType = data.GetString(detailType);
            if (detailType == "Player")
            {
                SPlayer.s.currentPlayer = obj;
                cobj.detailType = "Player";
            }
        }
    }

}




