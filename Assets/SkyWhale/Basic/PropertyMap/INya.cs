using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static UnityEngine.UI.Image;


public enum NyaType { Empty, Data, Int, String, Float, Bool, List, Map,Symbol,Placeholder }

public interface INya
{
    //value
    int Int { get => throw new NotImplementedException(string.Format("当前类型为{0}", Type)); set => throw new NotImplementedException(); }
    string String { get => throw new NotImplementedException(string.Format("当前类型为{0}", Type)); set => throw new NotImplementedException(); }
    float Float { get => throw new NotImplementedException(string.Format("当前类型为{0}", Type)); set => throw new NotImplementedException(); }
    bool Bool { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    string Placeholder { get => throw new NotImplementedException(string.Format("读取失败,当前类型为{0}", Type)); set => throw new NotImplementedException(string.Format("写入失败,当前类型为{0}", Type)); }
    INya Data { get => throw new NotImplementedException(string.Format("当前类型为{0}", Type)); set => throw new NotImplementedException(); }

    //list
    List<INya> List { get => throw new NotImplementedException(string.Format("当前类型为{0}", Type)); set => throw new NotImplementedException(); }
    void Set(int index, INya data) { throw new NotImplementedException(); }
    INya Get(int index) { throw new NotImplementedException(string.Format("当前类型为{0}", Type)); }
    void Add(INya data) { throw new NotImplementedException(); }
    Vector3 Vector3 { get => throw new NotImplementedException(); }
    Vector3 GetVector3(int index) { throw new NotImplementedException(string.Format("当前类型为{0}", Type)); }
    Quaternion Quaternion { get => throw new NotImplementedException(); }

    //map
    Dictionary<string, INya> Map { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    void Set(string key, int index, INya data) { throw new NotImplementedException(string.Format("当前类型为{0}", Type)); }
    INya Get(string key, int index) { throw new NotImplementedException(string.Format("当前类型为{0}", Type)); }
    void Add(string key, INya data) { throw new NotImplementedException(); }
    void SetMapReference() { Debug.LogError("错误"); }

    //public
    NyaType Type { get => throw new NotImplementedException(); }
    INya Clone { get => throw new NotImplementedException(); }
    int Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    float Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action OnValueChange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

}


public class NyaInt : INya
{
    public int Int { get; set; }
    public NyaType Type { get; } = NyaType.Int;
    public INya Clone { get { return new NyaInt(Int); } }

    public NyaInt(int data)
    {
        Int = data;
    }

}

public class NyaFloat : INya
{
    private float value;
    public float Float { get => value; set { if (this.value != value) { this.value = value; OnValueChange?.Invoke(); } } }
    public string String { get => value.ToString(); set => this.value = float.Parse(value); }
    public Action OnValueChange { get; set; }

    public NyaType Type { get; } = NyaType.Float;
    public INya Clone { get { return new NyaFloat(Float); } }
    public NyaFloat(float data)
    {
        Float = data;
    }
}

public class NyaBool : INya
{
    public bool Bool { get; set; }
    public NyaType Type { get; } = NyaType.Bool;
    public INya Clone { get { return new NyaBool(Bool); } }
    public NyaBool(bool data)
    {
        Bool = data;
    }
}

public class NyaString : INya
{
    public string String { get; set; }
    public NyaType Type { get; } = NyaType.String;
    public INya Clone { get { return new NyaString(String); } }
    public NyaString(string data)
    {
        String = data;
    }
}


public class NyaPlaceholder : INya
{
    public string Placeholder { get; set; }
    public float Value { get; set; }
    public NyaType Type { get; } = NyaType.Placeholder;
    public INya Clone { get { return new NyaPlaceholder(Placeholder); } }
    public NyaPlaceholder(string data)
    {
        Placeholder = data;
    }
}

public class NyaData : INya
{
    public INya Data { get; set; }
    public int Int { get => Data.Int; set => Data.Int = value; }
    public string String { get => Data.String; set => Data.String = value; }
    public float Float { get => Data.Float; set => Data.Float = value; }
    public bool Bool { get => Data.Bool; set => Data.Bool = value; }
    public List<INya> List { get => Data.List; set => Data.List = value; }
    public Dictionary<string, INya> Map { get => Data.Map; set => Data.Map = value; }

    public NyaType Type { get; } = NyaType.Data;
    public INya Clone { get { return new NyaData(Data); } }
    public NyaData(INya data)
    {
        Data = data;
    }
    public Vector3 GetVector3(int index) { return Data.GetVector3(index); }
    public void Set(string key, int index, INya data) { Data.Map[key].Set(index, data); }
    public void Set(int index, INya data) { Data.List[index] = data; }
    public INya Get(string key, int index) { return Data.Map[key].List[index]; }
}

public class NyaList : INya
{
    public List<INya> List { get; set; }
    public int Int { get => List[0].Int; set => List[0].Int = value; }
    public string String { get => List[0].String; set => List[0].String = value; }
    public float Float { get => List[0].Float; set => List[0].Float = value; }
    public bool Bool { get => List[0].Bool; set => List[0].Bool = value; }
    public NyaType Type { get; } = NyaType.List;
    public Vector3 Vector3 { get => new Vector3(List[0].Float, List[1].Float, List[2].Float); }
    public Quaternion Quaternion { get => new Quaternion(List[0].Float, List[1].Float, List[2].Float, List[3].Float); }
    public INya Clone
    {
        get
        {
            NyaList result = new NyaList();
            foreach (var item in List)
            {
                result.List.Add(item.Clone);
            }
            return result;
        }
    }
    public NyaList()
    {
        List = new List<INya>();
    }

    public NyaList(NyaList origin)
    {
        List = origin.Clone.List;
    }
    public void Set(int index, INya data)
    {
        List[index] = data;
    }
    public void Add(INya data)
    {
        List.Add(data);
    }
    public Vector3 GetVector3(int index)
    {
        return new Vector3(List[index].Float, List[index + 1].Float, List[index + 2].Float);
    }

}

public class NyaMap : INya
{
    public Dictionary<string, INya> Map { get; set; }
    public NyaType Type { get; } = NyaType.Map;
    public INya Clone
    {
        get
        {
            NyaMap result = new NyaMap();
            foreach (var item in Map)
            {
                result.Map.Add(item.Key, item.Value.Clone);
            }
            return result;
        }
    }
    public NyaMap()
    {
        Map = new Dictionary<string, INya>();
    }
    public NyaMap(NyaMap origin)
    {
        Map = origin.Clone.Map;
    }
    public void Set(string key, int index, INya data)
    {
        Map[key].Set(index, data);
    }
    public INya Get(string key, int index)
    {
        return Map[key].List[index];
    }
    public void Add(string key, INya data)
    {
        if (Map.ContainsKey(key))
        {
            Map[key].Add(data);
        }
        else
        {
            Map.Add(key, new NyaList());
            Map[key].Add(data);
        }

    }

    public void SetMapReference()
    {
        //Debug.LogFormat("{0} {1}",Map.Count,Map["Name"].String);
        foreach (var item in Map)
        {
            //Debug.LogFormat("{0} {1}", item.Key, item.Value.List.Count);
            if (item.Value != null && item.Value.Type == NyaType.List && item.Value.List.Count > 0)
            {
                for (int i = 0; i < item.Value.List.Count; i++)
                {
                    var target = item.Value.List[i];
                    if (target.Type == NyaType.String && target.String != null && target.String[0] == '&')
                    {
                        item.Value.List[i] = new NyaData(Map[target.String[1..]]);
                    }
                    else if (target.Type == NyaType.String && target.String != null && target.String[0] == '$' && target.String[1..].Contains('$'))
                    {
                        string origin;
                        float upperLimit = float.MaxValue, lowerLimit = float.MinValue;
                        NyaFloat current = new NyaFloat(0);

                        origin = target.String[1..];

                        string[] slices = origin.Split('$');
                        if (slices.Length == 2)
                        {
                            current.Float = Regex.IsMatch(slices[0], @"\d+") ? float.Parse(slices[0]) : Map[slices[0]].Float;
                            origin = slices[1];
                        }
                        else if(slices.Length==3)
                        {
                            current.Float = Regex.IsMatch(slices[0], @"\d+") ? float.Parse(slices[0]) : Map[slices[0]].Float;
                            upperLimit= Regex.IsMatch(slices[1], @"\d+") ? float.Parse(slices[1]) : Map[slices[1]].Float;
                            origin = slices[2];
                        }
                        else if (slices.Length == 4)
                        {
                            current.Float = Regex.IsMatch(slices[0], @"\d+") ? float.Parse(slices[0]) : Map[slices[0]].Float;
                            upperLimit = Regex.IsMatch(slices[1], @"\d+") ? float.Parse(slices[1]) : Map[slices[1]].Float;
                            lowerLimit = Regex.IsMatch(slices[2], @"\d+") ? float.Parse(slices[2]) : Map[slices[2]].Float;
                            origin = slices[3];
                        }



                        INya paramers = new NyaList();

                        string pattern = @"\{[^}]+\}|\d+";
                        string patternNumber = @"^\d+$";
                        MatchCollection matches = Regex.Matches(origin, pattern);

                        char currentLetter = 'a';
                        foreach (Match match in matches)
                        {
                            string matchedText = match.Value;
                            if (Regex.IsMatch(matchedText, @"\d+"))
                            {
                                // 如果匹配到数字，替换为字母
                                origin = origin.Replace(matchedText, currentLetter.ToString());
                            }
                            else
                            {
                                // 如果匹配到大括号内的文本，也替换为字母
                                origin = origin.Replace(matchedText, currentLetter.ToString());
                            }
                            currentLetter++;
                        }

                        NyaExpression expression = new NyaExpression(origin);

                        foreach (Match match in matches)
                        {
                            string content;
                            if (match.Value[0] == '{')
                            {
                                content = match.Value[1..^1];
                            }
                            else
                            {
                                content = match.Value;
                            }
                            if (Regex.IsMatch(content, patternNumber))
                            {
                                paramers.Add(new NyaFloat(float.Parse(content)));
                            }
                            else if (content == "self")
                            {
                                paramers.Add(current);
                            }
                            else
                            {

                                NyaFloat floatValue = new NyaFloat(Map[content].Float);
                                paramers.Add(floatValue);
                            }
                        }
                        Global.circle += delegate {
                            float value = expression.Calculate(paramers, 0);
                            if (value > upperLimit)
                                value = upperLimit;
                            else if (value < lowerLimit)
                                value = lowerLimit;
                            current.Float = value;
                            //Debug.Log("当前值为" + current.Float.ToString());
                        };


                        item.Value.List[i] = current;

                    }
                    else if (target.Type == NyaType.String && target.String != null && target.String[0] == '$')
                    {
                        string origin;
                        NyaFloat current = new NyaFloat(0);
                        origin = target.String[1..];

                        INya paramers = new NyaList();

                        string pattern = @"\{[^}]+\}|\d+";
                        string patternNumber = @"^\d+$";
                        MatchCollection matches = Regex.Matches(origin, pattern);

                        char currentLetter = 'a';
                        foreach (Match match in matches)
                        {
                            string matchedText = match.Value;
                            if (Regex.IsMatch(matchedText, @"\d+"))
                            {
                                // 如果匹配到数字，替换为字母
                                origin = origin.Replace(matchedText, currentLetter.ToString());
                            }
                            else
                            {
                                // 如果匹配到大括号内的文本，也替换为字母
                                origin = origin.Replace(matchedText, currentLetter.ToString());
                            }
                            currentLetter++;
                        }

                        NyaExpression expression = new NyaExpression(origin);
                        Action calculate = delegate
                        {
                            current.Float = expression.Calculate(paramers, 0);
                        };

                        foreach (Match match in matches)
                        {
                            string content = match.Value[1..^1];
                            if (Regex.IsMatch(content, patternNumber))
                            {
                                paramers.Add(new NyaFloat(float.Parse(content)));
                            }
                            else
                            {
                                NyaFloat floatValue = new NyaFloat(Map[content].Float);
                                floatValue.OnValueChange += calculate;
                                paramers.Add(floatValue);
                            }
                        }

                        current.Float = expression.Calculate(paramers, 0);
                        item.Value.List[i] = current;

                    }
                }
            }
        }

    }

    private INya GetMapDataByKey(string referenceKey)
    {
        if (referenceKey[0] == '&')
        {
            return new NyaData(Map[referenceKey[1..]]);
        }
        return new NyaString(referenceKey);
    }


}

