using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class JsonSystem : MonoBehaviour
{
    public static Dictionary<string,Property> ReadJsonProperty(string filePath)
    {


        Dictionary<string, Property> datas = new Dictionary<string, Property>();

        if (!File.Exists(filePath))
        {
            return datas;
        }

        JsonData originDatas = JsonMapper.ToObject(File.ReadAllText(filePath));

        return datas;
    }
}
