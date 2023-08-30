using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class JsonSystem : MonoBehaviour
{
    public static Dictionary<string,PropertyData> ReadJsonProperty(string filePath)
    {


        Dictionary<string, PropertyData> datas = new Dictionary<string, PropertyData>();

        if (!File.Exists(filePath))
        {
            return datas;
        }

        JsonData originDatas = JsonMapper.ToObject(File.ReadAllText(filePath));

        return datas;
    }
}
