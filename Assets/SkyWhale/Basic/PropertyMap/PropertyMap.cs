
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
    #region ��������

    private static PropertyMap _s;
    public static PropertyMap s { get { return _s; } }

    private void Awake()
    {
        if (_s == null)
            _s = this;

        Init();
    }

    #endregion

    //����ģ��
    public Dictionary<string, INya> map = new Dictionary<string, INya>();
    public Dictionary<int, Entity> entityMap = new Dictionary<int, Entity>();
    public static Dictionary<int, GameObject> gameObjectMap = new Dictionary<int, GameObject>();

    //��������ģ��
    private void Init()
    {
        //ReadExcel("PropertyData/MapEditor/Data/����.xlsx");

        //��ȡExcel����
        foreach (var item in ReadExcel("PropertyData/MapEditor/Data/����.xls"))
        {
            map.Add(item.Key, item.Value);
        }
        ////��ȡJson����
        //foreach (var item in JsonSystem.ReadJsonProperty("Core/MapEditor/Data/Property.json"))
        //{
        //    map.Add(item.Key, item.Value);
        //}

        
    }

    #region ��ȡExcel����

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

            //��ȡ������
            for (int i = 0; i < MyBook.NumberOfSheets; i++)
            {
                ISheet Sheet_Read = MyBook.GetSheetAt(i);

                INya data = new NyaMap();


                //Debug.Log(Sheet_Read.SheetName);

                //��ȡ��Ŀ
                for (int row = 0; row <= Sheet_Read.LastRowNum; row++)
                {
                    IRow Row_Read = Sheet_Read.GetRow(row);

                    if (Row_Read.GetCell(0)==null||Row_Read.GetCell(0).CellType == CellType.Blank || Row_Read.GetCell(0).ToString()[0] == '#')
                    {
                        continue;
                    }
                    //��ȡ��Ŀ��
                    string key = Row_Read.GetCell(0).ToString();
                    //if(key=="")
                    //Debug.LogFormat("��ǰ�ļ�Ϊ:{0}����ǰ���鳤��Ϊ:{1}", key,Row_Read.Cells.Count);

                    //��ȡ��Ŀ����
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

    #region ���úͻ�ȡʵ��
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
            Debug.LogErrorFormat("����ת������");
            return null;
        }
    }



    #endregion


}




