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

    #endregion

    #region 系统函数
    private void Start()
    {
        mapEditor.elementTypeInitEvent.AddListener(InitCharacter);
        //FunctionMap.map.Add("SetHitBox")
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

    public void SetHitBox(INya data)
    {
        for (int i = 0; i < 1; i++)
        {
            CharacterEntity character = (CharacterEntity)PropertyMap.s.entityMap[data.List[0].Int];
            GameObject hitBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hitBox.name = "HitBox";
            hitBox.transform.parent = character.transform;
            character.hitBoxs.Add(hitBox.GetComponent<BoxCollider>());
            character.hitBoxs[i].size = Vector3.one * 0.5f;
            character.hitBoxs[i].isTrigger = true;
            character.hitBoxs[i].enabled = false;
            var hitBoxMesh = hitBox.GetComponent<MeshRenderer>();
            hitBoxMesh.material = translucence;
        }
    }

}




