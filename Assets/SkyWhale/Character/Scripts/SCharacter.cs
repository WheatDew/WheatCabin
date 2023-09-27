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

    #region 外部引用

    public Material translucence;
    public CHitbox hitboxPrefab;

    #endregion

    #region 系统函数
    private void Start()
    {
        mapEditor.elementTypeInitEvent.AddListener(InitCharacter);
        FunctionMap.map.Add("SetHitbox", SetHitbox);
        FunctionMap.map.Add("DisplayHitbox", DisplayHitbox);
        Debug.Log("初始化角色系统完成");
    }
    #endregion


    public SMapEditor mapEditor;
    public void InitCharacter(INya data,GameObject obj)
    {
        Debug.Log(data.Type);
        if (data.Get(objectType,0).String == "Character")
        {

            var character = obj.AddComponent<CharacterEntity>();

            character.InitData(data);
            character.type = "Character";
            character.detailType = data.Get(detailType,0).String;
            if (character.detailType == "Player")
            {
                SPlayer.s.currentPlayer = obj;
                character.detailType = "Player";
            }
        }
    }

    public void SetHitbox(INya data)
    {
        for (int i = 0; i < 1; i++)
        {
            CharacterEntity character = PropertyMap.s.GetEntity<CharacterEntity>(data.List[0].Int);
            var hitBox = Instantiate(hitboxPrefab);
            hitBox.name = "HitBox";
            hitBox.transform.parent = character.transform;
            hitBox.transform.localPosition = data.GetVector3(1);
            hitBox.transform.localScale = data.GetVector3(4);
            character.hitBoxs.Add(hitBox);
            
        }
    }
    public void DisplayHitbox(INya data)
    {
        CharacterEntity character = PropertyMap.s.GetEntity<CharacterEntity>(data.List[0].Int);
        foreach(var item in character.hitBoxs)
        {
            item.meshRenderer.enabled = true;
        }
    }
}




