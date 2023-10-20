using Battlehub.RTCommon;
using JetBrains.Annotations;
using SkyWhale;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMapEditorEvent : MonoBehaviour
{
    public GameObject mapEditorPage;
    public GameObject stopRunningButton;

    public PropertyEditor propertyEditor;

    public Dictionary<GameObject, EditorTimeTransformData> editorTimeTransformData = new();

    public void StartGame()
    {
        editorTimeTransformData.Clear();
        if (SPlayer.s.currentPlayer != null)
        {
            //SCamera.s.SetThirdPersonCamera(SPlayer.s.currentPlayer.transform);
            SCamera.s.SetQuarterView(SPlayer.s.currentPlayer.transform);
            //SPlayer.s.currentPlayer.AddComponent<ThirdPersonController>();
            SPlayer.s.currentPlayer.AddComponent<QuarterViewController>();
            mapEditorPage.SetActive(false);
            stopRunningButton.SetActive(true);
            FindObjectOfType<RTEBase>().gameObject.SetActive(false);
            foreach(var item in FindObjectsOfType<Entity>())
            {
                editorTimeTransformData.Add(item.gameObject,new EditorTimeTransformData(item.transform.position,item.transform.rotation));
            }
        }

    }

    public void StopGame()
    {
        FindObjectOfType<RTEBase>(true).gameObject.SetActive(true);
        stopRunningButton.SetActive(false);
        mapEditorPage.SetActive(true);
        foreach(var item in FindObjectsOfType<ThirdPersonController>())
        {
            Destroy(item);
        }
        foreach (var item in FindObjectsOfType<QuarterViewController>())
        {
            Destroy(item);
        }
        SCamera.s.SetEditorPersonCamera();
        foreach (var item in editorTimeTransformData)
        {
            item.Key.transform.position = item.Value.position;
            item.Key.transform.rotation = item.Value.rotation;
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
        var sceneObjects = FindObjectsOfType<Entity>();
        foreach(var item in sceneObjects)
        {
            Destroy(item.gameObject);
        }
    }

    public void DisplayOrHiddenPage(GameObject propertyPage)
    {
        propertyPage.SetActive(!propertyPage.activeSelf);
    }
}

public class EditorTimeTransformData
{
    public Vector3 position;
    public Quaternion rotation;

    public EditorTimeTransformData(Vector3 position,Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}
