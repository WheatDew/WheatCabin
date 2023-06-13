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
}
