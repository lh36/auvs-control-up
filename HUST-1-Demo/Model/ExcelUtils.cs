using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.IO;

namespace HUST_1_Demo.Model
{
    class ExcelUtils
    {
        public Microsoft.Office.Interop.Excel.Application xlsApp = null;
        public Microsoft.Office.Interop.Excel.Workbook workbook = null;
        public Microsoft.Office.Interop.Excel.Worksheet worksheet = null;
        public string str_this_path = null;
        public string str_this_sheet = null;

        #region 打开某EXCEL文件的某个页
        /// <param name="str_path">EXCEL文件路径</param> 
        /// <param name="str_sheet">要操作的页</param>        
        public void Open(string str_path, string str_sheet)
        {
            str_this_path = str_path;
            str_this_sheet = str_sheet;
            //Excel Application 
            xlsApp = new Microsoft.Office.Interop.Excel.Application();
            //Excel Workbook           
            workbook = xlsApp.Workbooks.Open(str_path, 0, true, 5,
                                             System.Reflection.Missing.Value,
                                             System.Reflection.Missing.Value,
                                             false, System.Reflection.Missing.Value,
                                             System.Reflection.Missing.Value, true,
                                             false, System.Reflection.Missing.Value,
                                             false, false, false);
            //Excel Worksheet        
            worksheet = (Worksheet)workbook.Worksheets[str_sheet];
        }
        #endregion

        #region 将值写入某单元格
        /// <param name="row">行号</param>  
        /// <param name="col">列号</param>  
        /// <param name="str_value">待写入的值</param>  
        public void SetValue(int row, int col, string str_value)
        {
            if (row <= 0 || col <= 0 || str_value == null)
                throw new Exception("参数不合法");

            worksheet.Cells[row, col] = str_value;
        }
        #endregion

        #region 获取当前可用页中的已用的最大行号
        /// <returns>返回已用的最大行号</returns>
        public int GetCurSheetUsedRangeRowsCount()
        {
            if (xlsApp == null)
                throw new Exception("ExcelUtils对象尚未Open()");

            int used_rng_rows = worksheet.UsedRange.Rows.Count;
            return used_rng_rows;
        }
        #endregion

        #region 查找某字段名的列号(列号从1开始)
        /// <param name="colName">要查找的列名</param>     
        /// <returns>找到返回序号，找不到返回-1</returns>    
        public int GetColNoByName(string colName)
        {
            int col_used = worksheet.UsedRange.Columns.Count;
            for (int i = 1; i <= col_used; ++i)
            {
                if (GetValue(1, i).ToString().Trim() == colName)
                    return i;
            }
            return -1;
        }
        #endregion

        #region 得到某一单元格的值
        /// <param name="row">行号</param>      
        /// <param name="col">列号</param>       
        /// <returns>该单元格的string值</returns>   
        public string GetValue(int row, int col)
        {
            if (row <= 0 || col <= 0)
                throw new Exception("参数不合法");

          //  Range myRange = null;
          //  myRange = worksheet.get_Range(worksheet.Cells[row, col], worksheet.Cells[row, col]);
          //  string str = myRange.Text.ToString();
            string temp = ((Range)worksheet.Cells[row, col]).Text;
            return temp;
        }
        #endregion

        #region 将某excel当前页的某单元格的值写入到另一个excel当前页的某单元格
        /// <param name="getUtil">获取数据的ExcelUtils对象</param>     
        /// <param name="g_row">获取数据的ExcelUtils对象的某单元格行号</param>    
        /// <param name="g_col">获取数据的ExcelUtils对象的某单元格列号</param>    
        /// <param name="setUtil">待写入数据的ExcelUtils对象</param>     
        /// <param name="s_row">待写入数据的ExcelUtils对象的某单元格行号</param>   
        /// <param name="s_col">待写入数据的ExcelUtils对象的某单元格列号</param>    
        public static void BoxToBoxWrite(ExcelUtils getUtil, int g_row, int g_col,
                                         ExcelUtils setUtil, int s_row, int s_col)
        {
            if (getUtil == null || setUtil == null)
                throw new Exception("ExcelUtils对象尚未Open()");

            if (g_row <= 0 || g_col <= 0 || s_row <= 0 || s_col <= 0)
                throw new Exception("参数不合法");

            string str_to_write = getUtil.GetValue(g_row, g_col);
            setUtil.SetValue(s_row, s_col, str_to_write);
        }
        #endregion

        #region 将某excel页中某列从o_row_start到o_row_end的数据写入到另一个Excel页中，并从s_row_start行位置开始写入
        /// <param name="origUtil">源ExcelUtils对象</param>       
        /// <param name="origColName">源ExcelUtils对象中要操作的列名</param>     
        /// <param name="o_row_start">复制数据的起始行号</param>      
        /// <param name="o_row_end">复制数据的结束行号</param>     
        /// <param name="srcUtil">待写入的ExcelUtil对象</param>     
        /// <param name="srcColName">待写入的列名</param>       
        /// <param name="s_row_start">从s_row_start行开始写入</param>      
        public static void ColToColWrite(ExcelUtils origUtil, string origColName, int o_row_start, int o_row_end,
                                        ExcelUtils srcUtil, string srcColName, int s_row_start)
        {
            if (origUtil.worksheet == null || srcUtil.worksheet == null)
                throw new Exception("ExcelUtils对象尚未Open()");

            if (origColName == null || srcColName == null || o_row_start <= 0 ||
                o_row_end <= 0 || s_row_start <= 0 || o_row_start > o_row_end)
                throw new Exception("参数不合法");

            int o_col_index = origUtil.GetColNoByName(origColName);
            if (o_col_index < 0)
                throw new Exception("列名不存在");

            int s_col_index = srcUtil.GetColNoByName(srcColName);
            if (s_col_index < 0)
                throw new Exception("列名不存在");

            for (int i = o_row_start, j = s_row_start; i <= o_row_end; ++i, ++j)
            {
                BoxToBoxWrite(origUtil, i, o_col_index, srcUtil, j, s_col_index);
            }
        }
        #endregion

        #region 保存并关闭
        public void CloseAndSave()
        {
            xlsApp.DisplayAlerts = false;
            xlsApp.AlertBeforeOverwriting = false;

            if (File.Exists(str_this_path))
            {
                File.Delete(str_this_path);
            }

            xlsApp.ActiveWorkbook.SaveCopyAs(str_this_path);
            xlsApp.Quit();
            xlsApp = null;
            workbook = null;
            worksheet = null;
            str_this_path = null;
        }
        #endregion     
    }
}
