using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using static Microsoft.VisualBasic.Interaction;

public class ProcessSheet
{
    public static void processData()
    {

        DataTable dt = new DataTable();
        dt.Clear();
        dt.Columns.Add("FundName");
        dt.Columns.Add("JSECode");
        dt.Columns.Add("ISIN");
        dt.Columns.Add("OneYearPerf");
        dt.Columns.Add("ThreeYearsPerf");
        dt.Columns.Add("FiveYearsPerf");
        dt.Columns.Add("TenYearsPerf");
        dt.Columns.Add("YTDPerf");
        dt.Columns.Add("AsAtDate");
        dt.Columns.Add("RiskRating");

        dt.Columns[0].DataType = typeof(string);  //Set column datatypes
        dt.Columns[1].DataType = typeof(string);
        dt.Columns[2].DataType = typeof(string);
        dt.Columns[3].DataType = typeof(string);
        dt.Columns[4].DataType = typeof(string);
        dt.Columns[5].DataType = typeof(string);
        dt.Columns[6].DataType = typeof(string);
        dt.Columns[7].DataType = typeof(string);
        dt.Columns[8].DataType = typeof(string);
        dt.Columns[9].DataType = typeof(string);
        dt.Columns[10].DataType = typeof(string);

        int JSECol = 0;
        int valCol = 0;
        string idName = "";
        string valName = "";
        int numLines = 0;

        // Check fields, one at a time in each table

        for (int loop = 0; loop < Globals.numSheets; loop++)
        {
            (JSECol, valCol, idName, valName) = GetColumns(loop); // Get which columns are for required data from specific sheet

            DataTable currentSheet = Globals.sheets[loop];

            if (currentSheet != null)
            {

                int numRows = currentSheet.Rows.Count;


                for (int i = 0; i < numRows; i++)
                {
                    // select values from columns
                    // If field shows discrepancy, inform user

                    var currentJSECode = currentSheet.Rows[i][JSECol];
                    var currentVal = currentSheet.Rows[i][valCol];


                    DataRow anotherRow = dt.NewRow();

                    if (!String.IsNullOrWhiteSpace(currentJSECode.ToString())) //if there is a value
                    {
                        if (currentJSECode is string)
                        { anotherRow["JSECode"] = currentJSECode.ToString().Trim(); }
                        else
                        { Debug.WriteLine($"Table {Globals.sheetNames[loop]} has a non-string JSECode of type: {currentJSECode.GetType()} "); }

                    }

                    if (currentVal != null) // if there is a value
                    {
                        if (currentVal is decimal)
                        { anotherRow["YTDPerf"] = currentVal; } //Check data type
                        else
                        { Debug.WriteLine($"Table {Globals.sheetNames[loop]} has a non-decimal YTD performance value of type: {currentVal.GetType()}"); }
                    }

                    if (currentJSECode is string && !String.IsNullOrWhiteSpace(currentJSECode.ToString()) && currentVal is decimal) //middle condition is to prevent ID-less "totals" values from being added
                    {
                        dt.Rows.Add(anotherRow);
                        numLines += numRows;

                    }
                    else if (currentJSECode.ToString() == idName && currentVal.ToString() == valName) //Correct column headings detected
                    {
                        Debug.WriteLine($"Column Headings: \nJSECode - {idName} YTDPerf - {valName}");
                    }
                    else if (String.IsNullOrWhiteSpace(currentJSECode.ToString()) || String.IsNullOrWhiteSpace(currentVal.ToString())) // Blank fields due to totals column or titles
                    {
                        Debug.WriteLine($"Blank Field: \nJSECode - {currentJSECode} YTDPerf - {currentVal}");
                    }
                    else // Indicate that the column is likely incorrect (If NOT the correct heading or simply blank...)
                    {
                        MessageBox.Show($"Unexpected format, JSECode: {currentJSECode} and YTDPerf: {currentVal}");
                        return;
                    }
                   
                }
            }
        }

        (DataTable newDt, int newNumLines) = SaveToDataTable(dt);

    }

    // ============================================== FUNCTIONS ============================================================

    private static (int, int, string, string) GetColumns(int loop)
    {
        int JSECol = 0;
        string idName = "";
        int valCol = 0;
        string valName = "";

        string[,] currentSheet = Globals.sheetarr[loop];
        string currentSheetName = Globals.sheetNames[loop];
        int numRows = currentSheet.GetLength(0);
        int numCols = currentSheet.GetLength(1);


        // Allows program to check fields and headings in each table

        if (currentSheetName.Contains("Performance")) //Live funds sample sheet
        {
            JSECol = 2; 
            idName = "JSECode";
            valCol = 9; 
            valName = "YTDPerf";

        }
        else
            MessageBox.Show("Unknown file names!");

        return (JSECol, valCol, idName, valName);
    }

    public static int getUsedLength(string[] arr)
    {
        int count = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] != null)
            {
                count++;
            }
        }
        return count;
    }

    public static (DataTable, int) SaveToDataTable(DataTable dt)
    {
        // Sum values where the IDnumber is the same
        //var sumQry = from dt in dt where  
        var newDt = dt.AsEnumerable()
          .GroupBy(r => r.Field<string>("JSECode"))
          .Select(g =>
          {
              var row = dt.NewRow();
              row["JSECode"] = g.Key;
              row["YTDPerf"] = g.Sum(r => r.Field<decimal>("YTDPerf"));

              return row;
          }).CopyToDataTable();

        int newNumLines = newDt.Rows.Count;
        Debug.WriteLine($"Grouped Lines: {newNumLines}");

        return (newDt, newNumLines);
    }


    // *********************************************************** New Code **********************************************************************
    public static void CreateObjectList(int loop)
    {
        string[,] currentSheet = Globals.sheetarr[loop];
        string currentSheetName = Globals.sheetNames[loop];
        int numRows = currentSheet.GetLength(0);
        int numCols = currentSheet.GetLength(1);
        List<object> returnedList = new List<object>();


        // Allows program to check fields and headings in each table

        if (currentSheetName.Contains("Performance")) //Performance_YYYY_MM_DD.xlsx
        {
            returnedList = CreateFundsPerformanceList(currentSheet, numRows, numCols, loop).ToList();
            Globals.FundPerformanceList.Clear();
        }
        else
            MessageBox.Show("Unknown/ incorrect file names!");

        Debug.WriteLine("Returned row objects: " + returnedList.Count().ToString());

        //add to sheetlist
        Globals.listarr[loop] = returnedList;

        return;
    }

    private static List<object> CreateFundsPerformanceList(string[,] currentSheet, int numRows, int numCols, int loop)
    {
        int startRow = 0;
        string[] sheetTitles = GetRow(currentSheet, startRow, numCols);

        //Check column headings in first iteration
        bool titleError = CheckSheetTitles(sheetTitles, Globals.FundPerformanceFields, loop);

        if (titleError)
        {
            MessageBox.Show($"Unexpected Sheet Heading in {Globals.sheetNames[loop]}");
        }


        //Create the list of row objects
        for (int i = startRow + 1; i < numRows; i++)
        {
            //Create row object
            Globals.FundPerformanceList.Add(new FundsData
            {
                // Main mapping
                FundName = currentSheet[i, 0].Trim(),
                JSECode = currentSheet[i, 1].Trim(),
                ISIN = currentSheet[i, 2].Trim(),
                OneYearPerf = GetFormattedDecimal(currentSheet[i, 3].Trim()),
                ThreeYearsPerf = GetFormattedDecimal(currentSheet[i, 4].Trim()),
                FiveYearsPerf = GetFormattedDecimal(currentSheet[i, 5].Trim()), 
                TenYearsPerf = GetFormattedDecimal(currentSheet[i, 6].Trim()),
                YTDPerf = GetFormattedDecimal(currentSheet[i, 7].Trim()),
                AsAtDate = ParseDate(currentSheet[i, 8].Trim()),
                RiskRating = currentSheet[i, 9].Trim()
            });

        }

        return Globals.FundPerformanceList;
    }

    // Extract a complete row from the 2D sheet array
    public static string[] GetRow(string[,] matrix, int rowNumber, int numCols)
    {
        return Enumerable.Range(0, numCols).Select(x => matrix[rowNumber, x]).ToArray();
    }

    public static bool CheckSheetTitles(string[] sheetTitles, string[] knownTitles, int loop) // Compare title strings of each sheet based on the expected (global) ones
    {
        bool isDifferent = false;
        if (knownTitles.Length != sheetTitles.Length)
        {
            Debug.WriteLine($"{sheetTitles.Length} vs Expected - {knownTitles.Length}");
            isDifferent = true;

        }
        else
        {
            for (int y = 0; y < knownTitles.Length; y++)
            {
                if (sheetTitles[y] != knownTitles[y])
                {
                    Debug.WriteLine($"{sheetTitles[y]} vs Expected - {knownTitles[y]}");
                    isDifferent = true;
                    break;
                }
            }
        }
        return isDifferent;
    }

    private static string ParseDate(string dateString) // Re-formats Glacier dates from M/d/yyyy to yyyy/M/d
    {
        string formattedDate = "";

        dateString = dateString.Trim();

        try
        {
            formattedDate = DateTime.ParseExact(dateString, "dd-MMM-yyyy", null).ToString("yyyy-MM-dd");
            //Debug.WriteLine(formattedDate);
        }
        catch
        {
            formattedDate = null;
            Debug.WriteLine("formattedDate is null");
        }      

        return formattedDate;
    }

    private static decimal? GetFormattedDecimal(string currentValue) // Return formatted decimal or null value if "n/a"
    {
        decimal? currentVal = 0;
        currentValue = currentValue.Trim();

        if (currentValue.Contains("n/a"))
        {
            currentVal = null;
        }
        else
        {
           currentVal = Convert.ToDecimal(currentValue);
        }

        return currentVal;
    }
}

// By J. Koekemoer