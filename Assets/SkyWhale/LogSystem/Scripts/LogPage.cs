using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogPage : MonoBehaviour
{
    public Text content;

    private void Update()
    {
        content.text = LogSystem.s.logContent;
    }

    public void ClosePage()
    {
        Destroy(gameObject);
    }
}
