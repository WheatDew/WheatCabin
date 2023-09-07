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

    #region ϵͳ����
    private void Start()
    {
        mapEditor.elementTypeInitEvent.AddListener(InitCharacter);
        Debug.Log("��ʼ����ɫϵͳ");
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




