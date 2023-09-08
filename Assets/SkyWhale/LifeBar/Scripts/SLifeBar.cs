using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SLifeBar : MonoBehaviour
{
    public static string CreateLifeBarKey = "CreateLifeBar";

    #region µ¥Àý

    private static SLifeBar _s;
    public static SLifeBar s { get { return _s; } }

    private void Awake()
    {
        if (_s == null)
        {
            _s = this;
        }

        FunctionMap.Add("CreateLifeBar", CreateLifeBar);
    }

    #endregion

    public LifeBar prefab;
    public Transform elementParent;

    public void CreateLifeBar(Property data)
    {
        var obj = Instantiate(prefab, elementParent);

        obj.target = data;
    }
}
