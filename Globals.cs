using System;
using System.Collections.Generic;
using System.Data;
using static FundsData;

public class Globals
{
    public static int numSheets = 1;

    // Maximum amount of property fields in the investment data objects
    public static int maxFields = 10;

    // Save the "order" files were selected:
    public static string[] sheetNames = new string[numSheets];

    // Make all tables globally available:        
    public static DataTable[] sheets = new DataTable[numSheets];
    public static string[][,] sheetarr = new string[numSheets][,];
    public static List<object>[] listarr = new List<object>[numSheets];

    // Temporary List 
    public static List<object> FundPerformanceList = new List<object>();

    // Known Table Headings for comparison
    public static string[] FundPerformanceFields =
        {"FundName"," JSECode"," ISIN"," 1YearPerf"," 3YearsPerf"," 5YearsPerf"," 10YearsPerf"," YTDPerf"," AsAtDate","Risk Rating"};
    
    public static string[] RegularlyUpdatedFields =
        {"AsAtDate","OneYearPerf","ThreeYearsPerf","FiveYearsPerf","tenYearsPerf","YTDPerf"};

    // DB Connection string
    public static string connString = @"server = 127.0.0.1; user id = root; Password = 5l@mshut; persist security info = True; database = poliprice";
};

// By J. Koekemoer