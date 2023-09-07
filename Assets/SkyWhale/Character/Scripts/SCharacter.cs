using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

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

    private string displayName = "DisplayName";
    private string objectType = "Type";
    private string detailType = "DetailType";

    #endregion

    #region 系统函数
    private void Start()
    {
        mapEditor.elementTypeInitEvent.AddListener(InitCharacter);
        Debug.Log("初始化角色系统");
    }
    #endregion


    public SMapEditor mapEditor;
    public void InitCharacter(Property data,GameObject obj)
    {
        if (data.GetString(objectType) == "Character")
        {
            var cobj = obj.AddComponent<CharacterEntity>();
            cobj.propertyData = data;
            cobj.type = "Character";
            cobj.detailType = data.GetString(detailType);
            

            if (cobj.detailType == "Player")
            {
                SPlayer.s.currentPlayer = obj;
                cobj.detailType = "Player";
            }
        }
    }

}




