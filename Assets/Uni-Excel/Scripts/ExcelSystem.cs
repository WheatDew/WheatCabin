using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ExcelSystem : MonoBehaviour
{
    #region 单例

    private static ExcelSystem _s;
    public static ExcelSystem s { get { return _s; } }

    private void Awake()
    {
        if (!_s) _s = this;
    }

    #endregion

    private string MySheetName = "测试";

    private bool ReadExcelEnable_NPOI = true;
    private bool ReadExcelEnable_ExcelDataReader = false;

    [HideInInspector] public List<string> QuestionList1 = new List<string>();
    [HideInInspector] public List<string[]> questionDatas = new List<string[]>();

    private void Start()
    {
        ReadExcel("角色", QuestionList1);
    }

    public void CreateExcel(List<List<string>> datas)
    {
        FileStream MyAddress = new FileStream(Application.streamingAssetsPath + "/数据.xls", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

        HSSFWorkbook MyWorkbook = new HSSFWorkbook();

        HSSFSheet Sheet01 = (HSSFSheet)MyWorkbook.CreateSheet(MySheetName);

        HSSFRow Row0 = (HSSFRow)Sheet01.CreateRow((short)0);
        HSSFCell cell0 = (HSSFCell)Row0.CreateCell((short)0);
        cell0.SetCellValue("ID");
        HSSFCell cell1 = (HSSFCell)Row0.CreateCell((short)1);
        cell1.SetCellValue("列表");
        HSSFCell cell2 = (HSSFCell)Row0.CreateCell((short)2);
        cell2.SetCellValue("接口");
        HSSFCell cell3 = (HSSFCell)Row0.CreateCell((short)3);
        cell3.SetCellValue("地址");


        for (int i = 0; i < datas.Count; i++)
        {
            HSSFRow Row = (HSSFRow)Sheet01.CreateRow((short)i + 1);
            for (int j = 0; j < datas[i].Count; j++)
            {
                HSSFCell cell = (HSSFCell)Row.CreateCell((short)j);
                cell.SetCellValue(datas[i][j]);
            }
        }


        MyWorkbook.Write(MyAddress);

        MyWorkbook.Close();
    }

    public void ReadExcel(string sheetName, List<string> questionList)
    {
        if (ReadExcelEnable_NPOI == true)
        {

            HSSFWorkbook MyBook;

            using (FileStream MyAddress_Read = new FileStream(Application.streamingAssetsPath + "/数据.xls", FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                MyBook = new HSSFWorkbook(MyAddress_Read);
            }

            ISheet Sheet_Read = MyBook.GetSheet(sheetName);

            for (int row = 0; row <= Sheet_Read.LastRowNum; row++)
            {

                IRow Row_Read = Sheet_Read.GetRow(row);

                Debug.Log(Row_Read.Cells.Count);

                //questionList.Add(string.Format("{0}&{1}&{2}&{3}&{4}&{5}&{6}&{7}",
                //    Row_Read.GetCell(0), Row_Read.GetCell(1), Row_Read.GetCell(2), Row_Read.GetCell(3), Row_Read.GetCell(4), Row_Read.GetCell(5), Row_Read.GetCell(6), Row_Read.GetCell(7)));


            }
        }
    }
}
