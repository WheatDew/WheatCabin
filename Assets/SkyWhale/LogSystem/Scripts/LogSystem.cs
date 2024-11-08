using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogSystem : MonoBehaviour
{
    private static LogSystem _s;
    public static LogSystem s { get { return _s; } }

    private void Awake()
    {
        if(_s==null)
        {
            _s = this;
        }

        FunctionMap.Add("WriteLog", WriteLog);
    }


    [HideInInspector] public string logContent;

    public Transform canvas;
    [SerializeField] private LogPage logPagePrefab;
    [HideInInspector] public LogPage logPage;

    private void Start()
    {
        
    }

    public void OpenOrCloseLogPage()
    {
        if (logPage == null)
        {
            logPage = Instantiate(logPagePrefab,canvas);
        }
        else
        {
            Destroy(logPage.gameObject);
        }
    }

    public void WriteLog(string content)
    {
        logContent+=content+"\n";
    }

    public void WriteLog(INya data)
    {
        WriteLog(data.String);
    }
}
