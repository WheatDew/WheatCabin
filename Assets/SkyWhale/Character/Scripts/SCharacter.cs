using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCharacter : MonoBehaviour
{
    #region ЕЅР§ДњТы
    private static SCharacter _s;
    public static SCharacter s { get { return _s; } }

    private void Awake()
    {
        if (!_s) _s = this;

        mapEditor.mapEditorModelEvent.AddListener(InitCharacter);
    }
    #endregion


    public SMapEditor mapEditor;
    public void InitCharacter(string name,string type,string detailType,PropertyData propertyData,GameObject obj)
    {
        if (type == "Character")
        {
            var cobj = obj.AddComponent<NormalObject>();
            cobj.propertyData = propertyData;
            cobj.type = "Character";
            cobj.detailType = detailType;
            if (detailType == "Player")
            {
                SPlayer.s.currentPlayer = obj;
                cobj.detailType = "Player";
            }
        }
    }
    public void InitCharacter(SceneObjData data,GameObject obj)
    {
        InitCharacter(data.name, data.type, data.detailType, new PropertyData(data.intStatus, data.floatStatus, data.stringStatus), obj);
    }

    public void InitCharacter(StoreItem data,GameObject obj)
    {
        InitCharacter(data.name, data.type, data.detailType, new PropertyData(), obj);
    }
}

