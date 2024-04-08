using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateLog : MonoBehaviour
{
    public void SetUpdateLogPage(GameObject target)
    {
        target.SetActive(target.activeSelf);
    }
}
