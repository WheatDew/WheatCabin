//using UnityEngine;
//using System.Collections;

//using System.Collections.Generic;


//using NPOI.HSSF.UserModel;
//using NPOI.SS.UserModel;
//using NPOI.HSSF.Util;
//using System.IO;
//using Excel;
//using System.Data;

//using ArabicSupport;
//public class MyExcel : MonoBehaviour
//{
//    private string MySheetName = "Sheet_Test";

//    public List<string> MyCellArray;
//    public List<string> MyCellArray02;

//    public List<string> MyString;

//    private bool ReadExcelEnable_NPOI = false;
//    private bool ReadExcelEnable_ExcelDataReader = false;

//    void OnGUI()
//    {
//        GUIStyle style = new GUIStyle();

//        GUI.Label(new Rect(10, 10, 100, 30), "Sheet Name:");
//        MySheetName = GUI.TextField(new Rect(90, 10, 200, 30), MySheetName, 25);

//        style.richText = true;

//        FileStream MyAddress = new FileStream(Application.dataPath + "/My First Excel.xls", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

//        if (GUI.Button(new Rect(300, 10, 200, 30), "Create Excel Files With " + "<color=red>NPOI</color>"))
//        {

//            HSSFWorkbook MyWorkbook = new HSSFWorkbook();

//            HSSFSheet Sheet01 = (HSSFSheet)MyWorkbook.CreateSheet(MySheetName);

//            for (int i = 0; i < 5; i++)
//            {

//                HSSFRow Row = (HSSFRow)Sheet01.CreateRow((short)i);

//                HSSFCell cell = (HSSFCell)Row.CreateCell((short)0);

//                cell.SetCellValue(MyCellArray[i]);

//                if (i < MyCellArray02.Count)
//                {

//                    HSSFCell cell02 = (HSSFCell)Row.CreateCell((short)1);

//                    cell02.SetCellValue(MyCellArray02[i]);
//                }
//                else
//                {

//                    HSSFCell cell02 = (HSSFCell)Row.CreateCell((short)1);

//                    cell02.SetCellValue("");
//                }

//                Row.RowStyle = MyWorkbook.CreateCellStyle();

//                Row.RowStyle.BorderBottom = BorderStyle.Double;

//                cell.CellStyle = MyWorkbook.CreateCellStyle();

//                cell.CellStyle.BorderRight = BorderStyle.Thin;
//                cell.CellStyle.BorderBottom = BorderStyle.Dashed;
//                cell.CellStyle.BottomBorderColor = HSSFColor.Red.Index;

//                HSSFFont MyFont = (HSSFFont)MyWorkbook.CreateFont();

//                MyFont.FontName = "Tahoma";
//                MyFont.FontHeightInPoints = 14;
//                MyFont.Color = HSSFColor.Gold.Index;
//                MyFont.Boldweight = (short)FontBoldWeight.Bold;

//                cell.CellStyle.SetFont(MyFont);
//            }

//            MyWorkbook.Write(MyAddress);

//            MyWorkbook.Close();

//        }
        
        
//        if (GUI.Button(new Rect(60, 120, 200, 30), "Reading Excel Files With " + "<color=yellow>NPOI</color>"))
//        {

//            ReadExcelEnable_NPOI = true;

//            ReadExcelEnable_ExcelDataReader = false;

//            MyString.Clear();

//        }

//        if (ReadExcelEnable_NPOI == true)
//        {

//            HSSFWorkbook MyBook;

//            using (FileStream MyAddress_Read = new FileStream(Application.dataPath + "/My First Excel.xls", FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
//            {
//                MyBook = new HSSFWorkbook(MyAddress_Read);
//            }

//            ISheet Sheet_Read = MyBook.GetSheet(MySheetName);

//            for (int row = 0; row <= Sheet_Read.LastRowNum; row++)
//            {

//                IRow Row_Read = Sheet_Read.GetRow(row);

//                for (int cells = 0; cells < Row_Read.LastCellNum; cells++)
//                {

//                    GUI.Label(new Rect(400 + cells * 200, 100 + row * 40, 200, 20),Row_Read.GetCell(cells).ToString());

//                    //Arabic And Persian Support
//                    //GUI.Label(new Rect(400 + cells * 200,100+ row * 40, 200, 20),ArabicFixer.Fix(Row_Read.GetCell(cells).ToString()));
//                }

//            }
//        }

//        if (GUI.Button(new Rect(10, 160, 300, 30), "Reading Excel Files With " + "<color=yellow>ExcelDataReader</color>"))
//        {

//            ReadExcelEnable_ExcelDataReader = true;

//            ReadExcelEnable_NPOI = false;

//            MyString.Clear();

//        }

//        if (ReadExcelEnable_ExcelDataReader == true)
//        {

//            IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(MyAddress);

            
//            DataSet Result = excelReader.AsDataSet();

//            excelReader.IsFirstRowAsColumnNames = true;

//            int NumberOfRows = Result.Tables[0].Rows.Count;

//            int NumberOfCols = Result.Tables[0].Columns.Count;

//            for (int i = 0; i < NumberOfRows; i++)
//            {

//                for (int j = 0; j < NumberOfCols; j++)
//                {

//                    if (i == NumberOfRows - 1 && j == NumberOfCols - 1)
//                    {

//                        ReadExcelEnable_ExcelDataReader = false;

//                    }

//                    MyString.Add(Result.Tables[0].Rows[i][j].ToString());

//                }
//            }
//        }

//        for (int b = 0; b < MyString.Count; b++)
//        {

//            GUI.Label(new Rect(400,100+ 20 * b, 200, 20),MyString[b]);

//            //Arabic And Persian Support
//            //GUI.Label(new Rect(400, 100 + 20 * b, 200, 20), ArabicFixer.Fix(MyString[b]));
//        }
//    }
//}
