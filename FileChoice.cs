using System;
using static Microsoft.VisualBasic.Interaction;
using System.Windows.Forms;
using ECO.VeriFi.Importing.Excel;
using System.Data;
using System.Diagnostics;

public class FileChoice
{
    public static void chooseNumSheets()
    {
        bool flag = false;
        string userInputNumSheets = "0";

        while (!flag)
        {
            userInputNumSheets = InputBox("Number of Files", "Please enter the number of Excel sheets that need to be included in the AUM summary", "0");

            if (Int32.TryParse(userInputNumSheets, out Globals.numSheets))
            {
                flag = true;
                Debug.WriteLine(Globals.numSheets.ToString());
            }
            else
            {
                MessageBox.Show("Enter a Valid Integer.");
            }
        }
    }

    public static void openDialog()
    {
        int numSelected = 0;
        int numRows = 0;
        string[,] requestList; //2D arrays which store the table data
        Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
        //string[] xlsxNameArray;
        string xlsxName = "";

        // Make progress bar accessible
        ProgressBar p = Application.OpenForms["FundsDataExporterForm"].Controls["prgBarFile"] as ProgressBar;

        while (numSelected < Globals.numSheets)
        {
            //File dialog  
            try
            {
                // Create an instance of the open file dialog box.
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.Title = "Select the required Excel file";
                // Set filter options and filter index.
                openFileDialog1.Filter = "Excel File (.xls, .xlsx, .csv) | *.xls;*.xlsx;*.csv";
                openFileDialog1.FilterIndex = 1;

                openFileDialog1.Multiselect = false;

                // Call the ShowDialog method to show the dialog box.
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    (requestList, xlsxName) = Generic.Import(openFileDialog1.FileName, app);
                    Debug.WriteLine(requestList[1,1] + " Imported");
                    //p.Maximum = requestList.GetUpperBound(0) + 1;     // Set progress bar properties
                    //p.Minimum = 0;
                    //p.Value = 0;

                    int row = 0;

                    try
                    {
                        while (row <= requestList.GetUpperBound(0))
                        {
                            //p.Value++;    // Fill up progress bar as import completes
                            //p.Refresh();
                            Application.DoEvents();

                            row++;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    { app.Quit(); }

                    if (!Array.Exists(Globals.sheetNames, element => element == openFileDialog1.FileName)
                        && !Array.Exists(Globals.sheetNames, element => element == xlsxName.Substring(0, xlsxName.Length - 1))) // If the user hasn't chosen this file yet, behave "normally"
                    {
                        Globals.sheetNames[numSelected] = openFileDialog1.FileName;

                        string dtName = $"Table {numSelected + 1}";
                        Debug.WriteLine(dtName + " " + openFileDialog1.FileName);
                        //Globals.sheets[numSelected] = new DataTable(dtName);
                        //Globals.sheets[numSelected] = ArraytoDatatable(requestList);

                        // Keep as 2D string array
                        Globals.sheetarr[numSelected] = requestList;
                        Globals.sheets[numSelected] = ArraytoDatatable(requestList);

                        numSelected++;
                        MessageBox.Show("Import Complete");
                        numRows += row;
                    }
                    else { MessageBox.Show($"You've chosen this file before! - {openFileDialog1.FileName}"); }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        Debug.WriteLine($"{numRows}");
    }

    public static DataTable ArraytoDatatable(string[,] entries) // 2D array experiment
    {
        DataTable dt = new DataTable();
        for (int i = 0; i < entries.GetLength(1); i++)
        {
            dt.Columns.Add("Column" + (i + 1));
        }

        for (var i = 0; i < entries.GetLength(0); ++i)
        {
            DataRow row = dt.NewRow();
            for (var j = 0; j < entries.GetLength(1); ++j)
            {
                row[j] = entries[i, j];
            }
            dt.Rows.Add(row);
        }
        return dt;
    }
}

// By J. Koekemoer