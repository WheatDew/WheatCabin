using Battlehub.RTCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SRunTimeEditor : MonoBehaviour
{
    public SMapEditor mapEditor;
    public PropertyEditor propertyEditor;

    private void Awake()
    {
        mapEditor.rteComponentEvent.AddListener(InitRunTimeEditor);
    }

    public void InitRunTimeEditor(ExposeToEditor crte)
    {
        if (crte.Selected == null)
            crte.Selected = new ExposeToEditorUnityEvent();

        crte.Selected.AddListener(delegate
        {
            Entity obj = crte.gameObject.GetComponent<Entity>();

            propertyEditor.SetCurrentTarget(obj, crte.name);
            Debug.Log(crte.name);
        });
    }

    
}
