using Battlehub.RTCommon;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

using static UnityEngine.Rendering.DebugUI;

public class SMapEditorBrowser : MonoBehaviour
{
    private static SMapEditorBrowser _s;
    public static SMapEditorBrowser s { get { return _s; } }

    private void Awake()
    {
        if (!_s) _s = this;
    }

    public SMapEditor mapEditor;

    public void SaveSceneFile()
    {
        SaveFileDlg pth = new SaveFileDlg();
        pth.structSize = Marshal.SizeOf(pth);
        //pth.filter = "All files (*.png)|*.*";
        pth.filter = "All Files\0 *.dzx\0\0";

        pth.file = new string(new char[256]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[64]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = Application.dataPath; //默认路径
        pth.title = "保存场景";
        pth.defExt = "dzx";
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (SaveFileDialog.GetSaveFileName(pth))
        {
            string filepath = pth.file; //选择的文件路径;  
            Debug.Log(filepath);
            SceneDataToJson(filepath);
        }
    }

    public void OpenSceneFile()
    {
        OpenFileDlg pth = new OpenFileDlg();
        pth.structSize = Marshal.SizeOf(pth);
        //pth.filter = "All files (*.*)|*.*";
        pth.filter = "All Files\0 *.dzx\0\0";
        pth.file = new string(new char[256]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[64]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = Application.dataPath.Replace("/", "\\") + "\\Resources"; //默认路径
        pth.title = "打开场景";
        pth.defExt = "dzx";
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (OpenFileDialog.GetOpenFileName(pth))
        {
            string filepath = pth.file; //选择的文件路径;  
            Debug.Log(filepath);
            JsonToSceneData(filepath);
        }
    }

    public void SceneDataToJson(string path)
    {
        //var buildingList = JsonMapper.ToObject<BuildingPrefabDataList>(File.ReadAllText(path));
        var sceneObjects = FindObjectsOfType<NormalObject>();
        SceneDataFile sceneDataFile = new SceneDataFile();
        sceneDataFile.sceneObjDataList = new SceneObjData[sceneObjects.Length];


        for(int i = 0; i < sceneObjects.Length; i++)
        {
            var item = sceneObjects[i];
            sceneDataFile.sceneObjDataList[i] = new SceneObjData(item.name, item.type, item.detailType, item.transform.position,item.transform.rotation,item.propertyData.i,item.propertyData.f,item.propertyData.s);
        }
        string s = JsonMapper.ToJson(sceneDataFile);

        s = Regex.Replace(s, @"\\u(?<Value>[a-zA-Z0-9]{4})", m => {
            return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
        });

        Debug.Log(s);
        File.WriteAllText(path, s,System.Text.Encoding.UTF8);
        Debug.LogFormat("写入文件{0}成功", path);
    }

    public void JsonToSceneData(string path)
    {
        //var readData = JsonMapper.ToObject<SceneDataFile>(File.ReadAllText(path));
        //for(int i = 0; i < readData.sceneObjDataList.Length; i++)
        //{
        //    var item = readData.sceneObjDataList[i];


        //    var obj = Instantiate(mapEditor.storeItemMap[item.name].gameObject,new Vector3(item.positionX,item.positionY,item.positionZ),new Quaternion(item.rotationX,item.rotationY,item.rotationZ,item.rotationW));
        //    Regex regex = new Regex(@"\([C|c]lone\)$");
        //    if (regex.IsMatch(obj.name))
        //    {
        //        obj.name = obj.name[..^7];
        //    }
        //    obj.AddComponent<ExposeToEditor>();
        //    SCharacter.s.InitCharacter(item, obj);
        //    SBuilding.s.InitBuilding(item, obj);
        //}
    }

}

public class SceneObjData
{
    public string name;
    public string type;
    public string detailType;
    public float positionX,positionY,positionZ;
    public float rotationX,rotationY,rotationZ,rotationW;
    public Dictionary<string, int> intStatus;
    public Dictionary<string, float> floatStatus;
    public Dictionary<string, string> stringStatus;

    public SceneObjData()
    {
        this.name = "";
        this.type = "";
        this.detailType = "";
        this.positionX = 0;
        this.positionY = 0;
        this.positionZ = 0;
        this.rotationX = Quaternion.identity.x;
        this.rotationY = Quaternion.identity.y;
        this.rotationZ = Quaternion.identity.z;
        this.rotationW = Quaternion.identity.w;
        this.intStatus = new Dictionary<string, int>();
        this.floatStatus = new Dictionary<string, float>();
        this.stringStatus = new Dictionary<string, string>();
    }

    public SceneObjData(string name,string type,string detailType,Vector3 position,Quaternion rotation,Dictionary<string,int> intStatus,Dictionary<string,float> floatStatus,Dictionary<string,string> stringStatus)
    {
        this.name = name;
        this.type = type;
        this.detailType = detailType;
        this.positionX = position.x;
        this.positionY = position.y;
        this.positionZ = position.z;
        this.rotationX = rotation.x;
        this.rotationY = rotation.y;
        this.rotationZ = rotation.z;
        this.rotationW = rotation.w;
        this.intStatus = intStatus;
        this.floatStatus = floatStatus;
        this.stringStatus = stringStatus;
    }
}

public class SceneDataFile
{
    public SceneObjData[] sceneObjDataList;
}
