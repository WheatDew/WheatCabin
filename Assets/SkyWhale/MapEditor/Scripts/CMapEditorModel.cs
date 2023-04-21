using Battlehub.RTCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMapEditorModel : MonoBehaviour
{
    private Camera m_Camera;

    void Start()
    {
        m_Camera = GameObject.Find("RTE SceneView Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit result;
            Physics.Raycast(ray, out result, 100, LayerMask.GetMask("Ground"));
            transform.localPosition = result.point;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Destroy(this);
        }
    }
}
