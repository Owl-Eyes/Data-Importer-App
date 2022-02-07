using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
//using ECO.VeriFi.Domain.Core.DataContracts;
//using ECO.VeriFi.Domain.Core.Enums;
using OfficeOpenXml;
using OfficeOpenXml.Style;
//using XtremeCore.Extentions.String;
using System.Diagnostics;
using System.IO;
using Microsoft.Office.Interop.Excel;
using System.Windows.Forms;

namespace ECO.VeriFi.Importing.Excel
{
    public class Generic
    {
        public static int[] ImportRequestNumbers(string fileName)
        {
            //Application app = new Application();
            //Workbook wb = app.Workbooks.Open(fileName);
            //Worksheet ws = (Worksheet)wb.Worksheets[0];

            //save as .xlsx


            FileInfo file = new FileInfo(fileName);

            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                List<int> result = new List<int>();

                worksheet.GetValue(0, 0);
                int column = worksheet.Dimension.Start.Column;
                int startRow = worksheet.Dimension.Start.Row;
                int endRow = worksheet.Dimension.End.Row;

                for (int i = startRow; i <= endRow; i++)
                {
                    object obj = worksheet.GetValue(i, column);
                    int requestId = -1;
                    if (obj != null && int.TryParse(obj.ToString(), out requestId))
                        result.Add(requestId);
                }

                return result.ToArray();
            }
        }

        public static (string[,], string) Import(string fileName, Microsoft.Office.Interop.Excel.Application app)
        {
            FileInfo file = new FileInfo(fileName);

                try 
                {
                    Microsoft.Office.Interop.Excel.Workbook wb = app.Workbooks.Add(1);
                    wb = app.Workbooks.Open(fileName);

                    // Save as New xlsx file
                    var xlsxFile = file.FullName;

                    if (file.Extension == ".xls")
                    {
                        xlsxFile = file.FullName + "x";
                    }
                    else if (file.Extension == ".csv")
                    {
                        string justTheName = xlsxFile.Substring(0, xlsxFile.Length - 3);
                        xlsxFile = justTheName + "xlsx";
                    }

                //Microsoft.Office.Interop.Excel.DisplayAlerts = false;

                app.DisplayAlerts = false; // Overwrite files automatically

                wb.SaveAs(Filename: xlsxFile, FileFormat: Microsoft.Office.Interop.Excel.XlFileFormat.xlOpenXMLWorkbook, Type.Missing, Type.Missing, true, false, XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing);
                wb.Close(); // Close .xls file

                    // Then open the new xlsx file
                    FileInfo newfile = new FileInfo(xlsxFile);
                    using (ExcelPackage package = new ExcelPackage(newfile))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                        worksheet.GetValue(0, 0);
                        int startColumn = worksheet.Dimension.Start.Column;
                        int startRow = worksheet.Dimension.Start.Row;
                        int endRow = worksheet.Dimension.End.Row;
                        int endColumn = worksheet.Dimension.End.Column;

                        string[,] result = new string[endRow, endColumn];

                        result = ReadTheFile(worksheet, endRow, endColumn);

                        return (result,xlsxFile);
                    }

                }
                catch
                {
                    try
                    {
                        Microsoft.Office.Interop.Excel.Workbook wb = app.Workbooks.Add(1);

                        wb = app.Workbooks.Open(fileName);
                        Worksheet ws = wb.Worksheets.get_Item(1);
                        var cell = ws.Cells[1, 1];
                        Console.WriteLine($"{cell.Value}");

                        int startColumn = 1;
                        int startRow = 1;
                        // Find the last real row
                        int endRow = ws.Cells.Find("*", System.Reflection.Missing.Value,
                                                       System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                                                       Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows, Microsoft.Office.Interop.Excel.XlSearchDirection.xlPrevious,
                                                       false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;

                        // Find the last real column
                        int endColumn = ws.Cells.Find("*", System.Reflection.Missing.Value,
                                                       System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                                                       Microsoft.Office.Interop.Excel.XlSearchOrder.xlByColumns, Microsoft.Office.Interop.Excel.XlSearchDirection.xlPrevious,
                                                       false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Column;

                        Console.WriteLine($"Rows: {endRow}  Columns: {endColumn} ");

                        string[,] result = new string[endRow, endColumn];

                        for (int i = startRow; i <= endRow; i++)
                        {
                            for (int j = startColumn; j <= endColumn; j++)
                            {
                                object obj = ws.Cells[i, j].Value;
                                if (obj == null)
                                    result[i - 1, j - 1] = "";
                                else
                                    result[i - 1, j - 1] = obj.ToString().Trim();
                            }
                        }
                        wb.Close();
                        app.Quit();


                        return (result, fileName);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        string[,] result = new string[0, 0];
                        return (result, "");
                    }
                    finally
                    {
                        app.Quit();
                    }
                }            
        }

        private static string[,] ReadTheFile(ExcelWorksheet ws, int endRow, int endColumn)
        {
            string[,] result = new string[endRow, endColumn];

            for (int i = 1; i <= endRow; i++)
            {
                for (int j = 1; j <= endColumn; j++)
                {
                    object obj = ws.GetValue(i, j);
                    if (obj == null)
                        result[i - 1, j - 1] = "";
                    else
                        result[i - 1, j - 1] = obj.ToString();
                }
            }

            return result;
        }

    }
}
