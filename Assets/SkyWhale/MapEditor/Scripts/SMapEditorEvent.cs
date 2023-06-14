using SkyWhale;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMapEditorEvent : MonoBehaviour
{
    public void StartGame()
    {
        
        if (SPlayer.s.currentPlayer != null)
        {
            SCamera.s.SetThirdPersonCamera(SPlayer.s.currentPlayer.transform);
            SPlayer.s.currentPlayer.AddComponent<ThirdPersonController>();
        }

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
