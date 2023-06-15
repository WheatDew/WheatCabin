using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPlayer : MonoBehaviour
{
    #region ЕЅР§ДњТы
    private static SPlayer _s;
    public static SPlayer s { get { return _s; } }

    private void Awake()
    {
        if (_s == null)
            _s = this;
    }

    #endregion

    [HideInInspector] public GameObject currentPlayer;
}
