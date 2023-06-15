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
    }
    #endregion

    public void InitCharacter(string name,string type,string detailType,Dictionary<string,int> intStatus,Dictionary<string,float> floatStatus,Dictionary<string,string> stringStatus,GameObject obj)
    {
        if (type == "Character")
        {
            var cobj = obj.AddComponent<NormalObject>();
            cobj.intStatus = intStatus;
            cobj.floatStatus = floatStatus;
            cobj.stringStatus = stringStatus;
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
        InitCharacter(data.name, data.type, data.detailType, data.intStatus, data.floatStatus, data.stringStatus, obj);
    }

    public void InitCharacter(StoreItem data,GameObject obj)
    {
        InitCharacter(data.name, data.type, data.detailType, new Dictionary<string, int>(), new Dictionary<string, float>(), new Dictionary<string, string>(), obj);
    }
}

