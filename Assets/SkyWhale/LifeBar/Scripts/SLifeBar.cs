using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SLifeBar : MonoBehaviour
{
    public static string CreateLifeBar = "CreateLifeBar";

    #region µ¥Àý

    private SLifeBar _s;
    public SLifeBar s { get { return this; } }

    private void Awake()
    {
        if (_s == null)
        {
            _s = this;
        }

        FunctionMap.Add("CreateLifeBar", CreateLifeBarFunction);
    }

    #endregion

    public LifeBar prefab;
    public Transform elementParent;

    public void CreateLifeBarFunction(PropertyData data)
    {
        var obj = Instantiate(prefab, elementParent);

        obj.target = data;
    }
}
