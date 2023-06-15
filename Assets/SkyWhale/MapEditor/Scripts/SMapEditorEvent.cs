using SkyWhale;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMapEditorEvent : MonoBehaviour
{
    public GameObject mapEditorPage;
    public GameObject stopRunningButton;

    public void StartGame()
    {
        
        if (SPlayer.s.currentPlayer != null)
        {
            SCamera.s.SetThirdPersonCamera(SPlayer.s.currentPlayer.transform);
            SPlayer.s.currentPlayer.AddComponent<ThirdPersonController>();
            mapEditorPage.SetActive(false);
            stopRunningButton.SetActive(true);
        }

    }

    public void StopGame()
    {
        stopRunningButton.SetActive(false);
        mapEditorPage.SetActive(true);
        foreach(var item in FindObjectsOfType<ThirdPersonController>())
        {
            Destroy(item);
        }
        SCamera.s.SetEditorPersonCamera();
    }

    public void SaveData()
    {

    }

    public void ReadData()
    {

    }

    public void NewScene()
    {
        var sceneObjects = FindObjectsOfType<NormalObject>();
        foreach(var item in sceneObjects)
        {
            Destroy(item.gameObject);
        }
    }
}
