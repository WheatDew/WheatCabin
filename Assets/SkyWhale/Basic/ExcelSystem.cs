using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ExcelSystem
{

    private static bool ReadExcelEnable_NPOI = true;
    private static bool ReadExcelEnable_ExcelDataReader = false;

    public static Dictionary<string,PropertyData> ReadExcel(string filePath)
    {
        if (ReadExcelEnable_NPOI == true)
        {

            HSSFWorkbook MyBook;

            using (FileStream MyAddress_Read = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                MyBook = new HSSFWorkbook(MyAddress_Read);
            }

            Dictionary<string, PropertyData> datas = new Dictionary<string, PropertyData>();

            for(int i = 0; i < MyBook.NumberOfSheets; i++)
            {
                ISheet Sheet_Read = MyBook.GetSheetAt(i);

                Dictionary<string, int> intDatas = new Dictionary<string, int>();
                Dictionary<string, float> floatDatas = new Dictionary<string, float>();
                Dictionary<string, string> stringDatas = new Dictionary<string, string>();
                Dictionary<string, bool> boolDatas = new Dictionary<string, bool>();

                for (int row = 0; row <= Sheet_Read.LastRowNum; row++)
                {

                    IRow Row_Read = Sheet_Read.GetRow(row);

                    string key = Row_Read.GetCell(0).ToSafeString();
                    Debug.Log(key);
                    if(Row_Read.GetCell(1).CellType == CellType.Numeric&& Row_Read.GetCell(1).ToSafeString().Contains('.'))
                    {
                        floatDatas.Add(key, (float)Row_Read.GetCell(1).NumericCellValue);
                    }
                    else if(Row_Read.GetCell(1).CellType == CellType.Numeric)
                    {
                        intDatas.Add(key, (int)Row_Read.GetCell(1).NumericCellValue);
                    }
                    else if(Row_Read.GetCell(1).CellType == CellType.Boolean)
                    {
                        boolDatas.Add(key, (bool)Row_Read.GetCell(1).BooleanCellValue);
                    }
                    else
                    {
                        stringDatas.Add(key, Row_Read.GetCell(1).ToSafeString());
                    }
                }

                datas.Add(Sheet_Read.SheetName, new PropertyData(intDatas, floatDatas, stringDatas,boolDatas));
            }

            return datas;
        }

        return null;
    }
}
