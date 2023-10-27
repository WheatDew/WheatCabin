using Battlehub.RTCommon;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

public class SCamera : MonoBehaviour
{
    #region ЕЅР§ДњТы
    private static SCamera _s;
    public static SCamera s { get { return _s; } }

    private void Awake()
    {
        if (_s == null)
            _s = this;
    }

    #endregion

    public ThirdPersonCameraGroup thirdPersonCameraGroup;
    public QuarterView quarterView;
    [HideInInspector] public GameObject EditorCameraGroup;

    [HideInInspector] public Camera currentCamera;

    private void Start()
    {
        EditorCameraGroup = GameObject.Find("RTE SceneView Camera");
    }

    public void SetThirdPersonCamera(Transform target)
    {
        EditorCameraGroup.SetActive(false);
        thirdPersonCameraGroup.gameObject.SetActive(true);
        thirdPersonCameraGroup.virtualCamera.Follow = target;
        thirdPersonCameraGroup.virtualCamera.LookAt = target;
        currentCamera = thirdPersonCameraGroup.targetCamera;
    }

    public void SetEditorPersonCamera()
    {
        EditorCameraGroup.SetActive(true);
        thirdPersonCameraGroup.gameObject.SetActive(false);
        currentCamera = null;
    }

    public void SetQuarterView(Transform target)
    {
        EditorCameraGroup.SetActive(false);
        thirdPersonCameraGroup.gameObject.SetActive(false);
        quarterView.gameObject.SetActive(true);
        quarterView.target = target;
        currentCamera = quarterView.targetCamera;
    }
}
