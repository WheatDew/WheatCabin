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
    #region ��������
    private static SCharacter _s;
    public static SCharacter s { get { return _s; } }

    private void Awake()
    {
        if (!_s) _s = this;
    }
    #endregion

    #region ����ת��

    private string displayName = "DisplayName";
    private string objectType = "Type";
    private string detailType = "DetailType";

    #endregion

    #region �ⲿ����

    public Material translucence;
    public CHitbox hitboxPrefab;

    #endregion

    #region ϵͳ����
    private void Start()
    {
        mapEditor.elementTypeInitEvent.AddListener(InitCharacter);
        FunctionMap.map.Add("SetHitbox", SetHitbox);
        FunctionMap.map.Add("DisplayHitbox", DisplayHitbox);
        FunctionMap.map.Add("HiddenHitbox", HiddenHitbox);
        FunctionMap.map.Add("EnableHitbox", EnableHitbox);
        FunctionMap.map.Add("DisableHitbox", DisableHitbox);
        Debug.Log("��ʼ����ɫϵͳ���");
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
        CharacterEntity character = PropertyMap.s.GetEntity<CharacterEntity>(data.List[1].Int);
        Vector3 position = data.GetVector3(2);
        Vector3 scale = data.GetVector3(5);
        character.SetHitbox(hitboxPrefab,position,scale);
    }
    public void DisplayHitbox(INya data)
    {
        CharacterEntity character = PropertyMap.s.GetEntity<CharacterEntity>(data.List[1].Int);
        character.DisplayHitbox();
    }

    public void HiddenHitbox(INya data)
    {
        CharacterEntity character = PropertyMap.s.GetEntity<CharacterEntity>(data.List[1].Int);
        character.HiddenHitbox();
    }

    public void EnableHitbox(INya data)
    {
        CharacterEntity character = PropertyMap.s.GetEntity<CharacterEntity>(data.List[0].Int);
        Vector3 position = data.GetVector3(1);
        Vector3 scale = data.GetVector3(4);
        character.EnableHitbox(position,scale);
    }

    public void DisableHitbox(INya data)
    {
        CharacterEntity character = PropertyMap.s.GetEntity<CharacterEntity>(data.Int);
        character.DisableHitbox(20f);
    }

}




