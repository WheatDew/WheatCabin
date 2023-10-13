
using Battlehub.UIControls;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class PropertyMap : MonoBehaviour
{
    #region 单例代码

    private static PropertyMap _s;
    public static PropertyMap s { get { return _s; } }

    private void Awake()
    {
        if (_s == null)
            _s = this;

        Init();
    }

    #endregion

    //数据模板
    public Dictionary<string, INya> map = new Dictionary<string, INya>();
    public Dictionary<int, Entity> entityMap = new Dictionary<int, Entity>();
    public static Dictionary<int, GameObject> gameObjectMap = new Dictionary<int, GameObject>();

    //载入数据模板
    private void Init()
    {
        //ReadExcel("PropertyData/MapEditor/Data/数据.xlsx");

        //获取Excel数据
        foreach (var item in ReadExcel("PropertyData/MapEditor/Data/数据.xls"))
        {
            map.Add(item.Key, item.Value);
        }
        ////获取Json数据
        //foreach (var item in JsonSystem.ReadJsonProperty("Core/MapEditor/Data/Property.json"))
        //{
        //    map.Add(item.Key, item.Value);
        //}

        
    }

    #region 读取Excel数据

    private bool ReadExcelEnable_NPOI = true;
    private bool ReadExcelEnable_ExcelDataReader = false;

    public Dictionary<string, INya> ReadExcel(string filePath)
    {
        if (ReadExcelEnable_NPOI == true)
        {

            HSSFWorkbook MyBook;

            using (FileStream MyAddress_Read = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                MyBook = new HSSFWorkbook(MyAddress_Read);
            }

            Dictionary<string, INya> datas = new Dictionary<string, INya>();

            //获取工作表
            for (int i = 0; i < MyBook.NumberOfSheets; i++)
            {
                ISheet Sheet_Read = MyBook.GetSheetAt(i);

                INya data = new NyaMap();


                //Debug.Log(Sheet_Read.SheetName);

                //获取条目
                for (int row = 0; row <= Sheet_Read.LastRowNum; row++)
                {
                    IRow Row_Read = Sheet_Read.GetRow(row);

                    if (Row_Read.GetCell(0)==null||Row_Read.GetCell(0).CellType == CellType.Blank || Row_Read.GetCell(0).ToString()[0] == '#')
                    {
                        continue;
                    }
                    //获取条目名
                    string key = Row_Read.GetCell(0).ToString();
                    //if(key=="")
                    //Debug.LogFormat("当前的键为:{0}，当前数组长度为:{1}", key,Row_Read.Cells.Count);

                    //获取条目内容
                    for (int column = 1; column < Row_Read.Cells.Count; column++)
                    {
                        if (Row_Read.GetCell(column)==null|| Row_Read.GetCell(column).CellType == CellType.Blank || Row_Read.GetCell(column).ToString()[0] == '#')
                        {
                            break;
                        }
                        //Debug.LogFormat("{0} {1} {2}",Row_Read.GetCell(column).CellType,Row_Read.GetCell(column),column);
                        

                        if (Row_Read.GetCell(column).CellType == CellType.Numeric && Row_Read.GetCell(column).ToString().Contains('.'))
                        {
                            data.Add(key,new NyaFloat((float)Row_Read.GetCell(column).NumericCellValue));
                        }
                        else if (Row_Read.GetCell(column).CellType == CellType.Numeric)
                        {
                            data.Add(key,new NyaInt((int)Row_Read.GetCell(column).NumericCellValue));
                        }
                        else if (Row_Read.GetCell(column).CellType == CellType.Boolean)
                        {
                            data.Add(key,new NyaBool((bool)Row_Read.GetCell(column).BooleanCellValue));
                        }
                        else
                        {
                            //if (key == "Weapon")
                            //{
                            //    Debug.Log(Row_Read.GetCell(column).ToSafeString());
                            //}
                            data.Add(key,new NyaString(Row_Read.GetCell(column).ToString()));
                        }
                    }
                }

                //data.SetMapReference();
                datas.Add(Sheet_Read.SheetName, data);
            }

            return datas;
        }

        return null;
    }

    #endregion

    #region 设置和获取实体
    public void SetEntity(int id,Entity entity)
    {

        if (entityMap.ContainsKey(id))
        {
            entityMap[id] = entity;
        }
        else
        {
            entityMap.Add(id, entity);
        }

        entity.data.Set("EntityID", 0,new NyaInt(id));
    }

    public Entity GetEntity(int id)
    {
        return entityMap[id];
    }

    public static void SetGameObject(int id, GameObject entity)
    {

        if (gameObjectMap.ContainsKey(id))
        {
            gameObjectMap[id] = entity;
        }
        else
        {
            gameObjectMap.Add(id, entity);
        }
    }

    public static GameObject GetGameObject(int id)
    {
        return gameObjectMap[id];
    }

    public T GetEntity<T>(int id) where T:Entity
    {
        if (PropertyMap.s.entityMap[id] is T obj)
        {
            return obj;
        }
        else
        {
            Debug.LogErrorFormat("类型转换错误");
            return null;
        }
    }



    #endregion


}




