// New

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.Data;

public static class DataToDB
{
    public static void PopulateTblByType(List<object> currentList, int loop, MySqlConnection conn)
    {
        string currentSheetName = Globals.sheetNames[loop];

        if (currentSheetName.Contains("Performance")) // Springpoint_ClientList.csv ***OR*** InvestmentList_###_AllClients_YYYY-MM-DD (old) / Glacier
        {
            PopulatePerformanceData(currentList, loop, conn);
        }
        else
            MessageBox.Show("Unknown/ incorrect sheet type!");

    }


    // ================================================= Populating the DB ============================================================

    private static void PopulatePerformanceData(List<object> currentList, int loop, MySqlConnection conn)
    {
        int i = 0;
        int numMatches = 0;
        string popTable = "tbllivefundperformance"; // Default table
        string histTable = "tbllivefundperformancehistory";
        string currentPrimary = "JSECode";
        string currentPrimaryKey = "";
        string truePrimary = "pkiFundId";
        int truePrimaryKey = 0;
        bool addedToday = false;
        FundsData oldFund = new FundsData();
        MySqlTransaction trans;

        foreach (FundsData row in currentList)
        {
            // Get row's pk
            currentPrimaryKey = row.GetType().GetProperty(currentPrimary).GetValue(row, null).ToString();
            Debug.WriteLine(currentPrimaryKey);

            // Check if JSE Code exists, if so, trigger updates, else just append
            string findOld = $"SELECT * FROM `{popTable}` WHERE `JSECode` = @currentPrimaryKey";
            MySqlCommand cmd = new MySqlCommand(findOld, conn);

            cmd.Connection = conn;
            cmd.CommandText = findOld;
            cmd.Prepare();
            cmd.Parameters.Add(new MySqlParameter("@currentPrimaryKey", MySqlDbType.String) { Value = (currentPrimaryKey.Trim()) });
            MySqlDataReader dr = cmd.ExecuteReader();

            i = 0;
            while (dr.Read() && dr != null)
            {
                truePrimaryKey = dr.GetInt32(truePrimary); // pkiFundId

                oldFund.FundName = dr.GetString("FundName").Trim();
                oldFund.JSECode = dr.GetString("JSECode").Trim();
                oldFund.ISIN = dr.GetString("ISIN").Trim();
                oldFund.OneYearPerf = ParseDBDecimal(dr["OneYearPerf"] == DBNull.Value ? null : (decimal?)dr["OneYearPerf"]);
                oldFund.ThreeYearsPerf = ParseDBDecimal(dr["ThreeYearsPerf"] == DBNull.Value ? null : (decimal?)dr["ThreeYearsPerf"]);
                oldFund.FiveYearsPerf = ParseDBDecimal(dr["FiveYearsPerf"] == DBNull.Value ? null : (decimal?)dr["FiveYearsPerf"]);
                oldFund.TenYearsPerf = ParseDBDecimal(dr["TenYearsPerf"] == DBNull.Value ? null : (decimal?)dr["TenYearsPerf"]);
                oldFund.AsAtDate = ParseDBDate(dr.GetString("AsAtDate"));
                oldFund.RiskRating = dr.GetString("RiskRating").Trim();

                i++;
            }

            dr.Close();

            //Debug.WriteLine(i.ToString());
            numMatches = i;

            if (numMatches > 0)
            {
                using (trans = conn.BeginTransaction())
                {
                    try
                    {
                        // update row
                        UpdateInvestmentRow(oldFund, row, currentPrimary, currentPrimaryKey,
                                        truePrimary, truePrimaryKey, addedToday, conn, trans);
                        trans.Commit();     // Row successful
                    }
                    catch
                    {
                        trans.Rollback();   // Row unsuccessful
                        throw;
                    }
                }

            }
            else
            {
                using (trans = conn.BeginTransaction())
                {
                    try
                    {
                        AppendInvestmentRow(row, popTable, histTable, currentPrimary, currentPrimaryKey, truePrimary, truePrimaryKey, conn, trans);
                        trans.Commit(); // Row successful!
                        Debug.WriteLine("No match found");
                    }
                    catch
                    {
                        trans.Rollback(); // Row unsuccessful
                        throw;
                    }
                }
            }

            // Populate derived fields
        }

        Debug.WriteLine(" ################### Data Transfer Complete ###################");
    }

    private static void UpdateInvestmentRow(object oldRow, object row, string currentPrimary, string currentPrimaryKey, string truePrimary,
                                            int truePrimaryKey, bool addedToday, MySqlConnection conn, MySqlTransaction trans)
    {
        int tableId = 31;
        int rowsMod = 0;
        string popTable = "tbllivefundperformance";
        string histTable = "tbllivefundperformancehistory";
        string auditTable = "tbllivefundperformanceaudit";
        string currentFieldName = "";
        DateTime timeNow = DateTime.Now;

        string stringifiedNewValue = "";
        string stringifiedOldValue = "";

        int fieldLoop = 0;
        int fieldLoopHist = 0;
        int fieldLoopAudit = 0;
        string updateList = "";
        string fieldListHist = "";
        string fieldValuesHist = "";

        string[] changedFields = new string[Globals.maxFields];
        string[] newValues = new string[Globals.maxFields];
        string[] fieldArrHist = new string[Globals.maxFields];
        string[] fieldValArrHist = new string[Globals.maxFields];        
        string[] fieldArrAudit = new string[Globals.maxFields];
        string[] oldValues = new string[Globals.maxFields];
        string[] newAuditValues = new string[Globals.maxFields];

        // Loop through properties/fields if pk exists to find changes
        foreach (PropertyInfo currentField in row.GetType().GetProperties())
        {
            currentFieldName = currentField.Name;

            if (row.GetType().GetProperty(currentFieldName) != null)
            {
                if (row.GetType().GetProperty(currentFieldName).GetValue(row, null) != null)
                {
                    var currentFieldValue = row.GetType().GetProperty(currentFieldName).GetValue(row, null);
                    var oldFieldValue = oldRow.GetType().GetProperty(currentFieldName).GetValue(oldRow, null);

                    // detect and temporarily store changes...
                    if (currentFieldValue != oldFieldValue) // if not null match
                    {
                        if (currentFieldValue != null && oldFieldValue != null)
                        {
                            stringifiedNewValue = currentFieldValue.ToString();
                            stringifiedOldValue = oldFieldValue.ToString();

                            if (!String.Equals(stringifiedNewValue, stringifiedOldValue)) // Compare converted strings
                            {
                                Debug.WriteLine($"{stringifiedNewValue} != {stringifiedOldValue}");

                                changedFields[fieldLoop] = $"`{currentFieldName}`";
                                newValues[fieldLoop] = $"'{stringifiedNewValue}'";
                                fieldLoop++;

                                if (!Globals.RegularlyUpdatedFields.Contains(currentFieldName)) // audit entry
                                {
                                    fieldArrAudit[fieldLoopAudit] = $"`{currentFieldName}`";
                                    newAuditValues[fieldLoopAudit] = $"'{stringifiedNewValue}'";
                                    oldValues[fieldLoopAudit] = $"'{stringifiedOldValue}'";
                                    fieldLoopAudit++;
                                }
                            }
                        }
                    }

                    if (Globals.RegularlyUpdatedFields.Contains(currentFieldName) && currentFieldValue != null) // History values -> independent of changes
                    {
                        fieldArrHist[fieldLoopHist] = $"`{currentFieldName}`";
                        fieldValArrHist[fieldLoopHist] = $"'{currentFieldValue.ToString()}'";

                        fieldLoopHist++;
                    }
                }
            }
            
        }

        if (fieldLoopHist > 0) // History entry needed
        {
            string[] fieldArrHistTemp = new string[fieldLoopHist];
            string[] fieldValArrHistTemp = new string[fieldLoopHist];

            // Trim the empty array elements
            for (int loop = 0; loop < fieldLoopHist; loop++)
            {
                fieldArrHistTemp[loop] = fieldArrHist[loop];
                fieldValArrHistTemp[loop] = fieldValArrHist[loop];
            }
            fieldArrHist = fieldArrHist.Where(x => !string.IsNullOrEmpty(x) && Array.IndexOf(fieldArrHist, null) >= fieldLoopHist).ToArray();
            //fieldValArrHist = fieldValArrHist.Where(x => !string.IsNullOrEmpty(x) && Array.IndexOf(fieldValArrHist, null) >= fieldLoopHist).ToArray();

            fieldListHist = String.Join(", ", fieldArrHist);
            fieldValuesHist = String.Join(", ", fieldValArrHistTemp);

            // Update tbl*history (NEWFundID, update date, Perfs)
            string sqlhistory = $"INSERT INTO `{histTable}` ({fieldListHist}, `fkiFundId`, `UpdatedDate`) VALUES({fieldValuesHist}, @truePrimary, @updatedTime)";

            MySqlCommand cmd2 = new MySqlCommand(sqlhistory, conn, trans);
            cmd2.Connection = conn;
            cmd2.Transaction = trans;
            cmd2.CommandText = sqlhistory;
            cmd2.Prepare();
            cmd2.Parameters.Add(new MySqlParameter("@truePrimary", MySqlDbType.Int32) { Value = truePrimaryKey });
            cmd2.Parameters.Add(new MySqlParameter("@updatedTime", MySqlDbType.DateTime) { Value = timeNow });

            rowsMod = cmd2.ExecuteNonQuery();

            if (rowsMod > 0)
            {
                Debug.WriteLine($"APPENDED ROW: Table: {histTable} at Key: fkiFundId - {truePrimaryKey}");
            }
        }

        if (fieldLoop > 0) // if there are changes in the main table
        {
            // Trim the empty array elements
            changedFields = changedFields.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            newValues = newValues.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var fieldsandvalues = changedFields.Zip(newValues, (f, v) => new { Field = f, Value = v });
            foreach (var fv in fieldsandvalues)
            {
                updateList += fv.Field + " = " + fv.Value + ", ";
            }

            updateList = updateList.Substring(0, updateList.Length - 2);
            Debug.WriteLine(updateList);

            if (fieldLoopAudit > 0) // if there are unexpected changes
            {
                fieldArrAudit = fieldArrAudit.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                oldValues = oldValues.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                newAuditValues = newAuditValues.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            }

            //// Update row and then create history and check for any audit entries
            //// Populate relevant table only on value change
            string updateChangeOnly = $"UPDATE `{popTable}` SET {updateList} WHERE `{truePrimary}` = @truePrimaryKey";
            MySqlCommand cmd1 = new MySqlCommand(updateChangeOnly, conn, trans);
            cmd1.Connection = conn;
            cmd1.Transaction = trans;
            cmd1.CommandText = updateChangeOnly;
            cmd1.Prepare();
            cmd1.Parameters.Add(new MySqlParameter("@truePrimaryKey", MySqlDbType.String) { Value = truePrimaryKey });

            rowsMod = cmd1.ExecuteNonQuery();

            Debug.WriteLine($"UPDATED ROW: Table: {popTable} at Key: {truePrimary} - {truePrimaryKey.ToString()}");

            if (rowsMod > 0 && fieldLoopAudit > 0) // Audit entries needed
            {
                // Update tbl*audit (fundID, unexpected change, misc.)
                for (int j = 0; j < fieldLoopAudit; j++) // One entry per updated field
                {
                    string sqlaudit = $"INSERT INTO `{auditTable}` " +
                        $"(`fkiFundId`,`fkiUserId`,`fkiAuditTableId`,`fkiAuditTypeId`,`ChangedField`,`OldValue`,`NewValue`,`TimeStamp`) " +
                        $"VALUES (@truePrimary, @user, @auditID, @audit, @updatedField, @oldEntry, @newEntry, @updatedTime)";

                    MySqlCommand cmd3 = new MySqlCommand(sqlaudit, conn, trans);
                    cmd3.Connection = conn;
                    cmd3.Transaction = trans;
                    cmd3.CommandText = sqlaudit;
                    cmd3.Prepare();
                    cmd3.Parameters.Add(new MySqlParameter("@truePrimary", MySqlDbType.Int32) { Value = truePrimaryKey });
                    cmd3.Parameters.Add(new MySqlParameter("@user", MySqlDbType.Int32) { Value = -1 });
                    cmd3.Parameters.Add(new MySqlParameter("@auditId", MySqlDbType.Int32) { Value = tableId });
                    cmd3.Parameters.Add(new MySqlParameter("@audit", MySqlDbType.Int32) { Value = 15 });
                    cmd3.Parameters.Add(new MySqlParameter("@updatedField", MySqlDbType.String) { Value = fieldArrAudit[j] });
                    cmd3.Parameters.Add(new MySqlParameter("@oldEntry", MySqlDbType.String) { Value = oldValues[j] });
                    cmd3.Parameters.Add(new MySqlParameter("@newEntry", MySqlDbType.String) { Value = newAuditValues[j] });
                    cmd3.Parameters.Add(new MySqlParameter("@updatedTime", MySqlDbType.DateTime) { Value = timeNow });

                    cmd3.ExecuteNonQuery();
                }
            }
        }

        return;
    }

    private static void AppendInvestmentRow(object row, string popTable, string histTable, string currentPrimary, string currentPrimaryKey,
                                            string truePrimary, int truePrimaryKey, MySqlConnection conn, MySqlTransaction trans)
    {
        int rowsMod = 0;

        string currentFieldName = "";
        string currentFieldValue = "";

        // Main table entry
        int fieldLoop = 0;
        string fieldList = "";
        string fieldValues = "";
        string[] fieldArrTemp = new string[Globals.maxFields];
        string[] fieldValArrTemp = new string[Globals.maxFields];

        // History table entry
        int fieldLoopHist = 0;
        string fieldListHist = "";
        string fieldValuesHist = "";
        string[] fieldArrTempHist = new string[Globals.maxFields];
        string[] fieldValArrTempHist = new string[Globals.maxFields];

        DateTime timeNow = DateTime.Now;

        //Get properties
        foreach (PropertyInfo currentField in row.GetType().GetProperties())
        {
            currentFieldName = currentField.Name;

            if (row.GetType().GetProperty(currentFieldName).GetValue(row, null) != null)
            {
                currentFieldValue = row.GetType().GetProperty(currentFieldName).GetValue(row, null).ToString();

                fieldArrTemp[fieldLoop] = $"`{currentFieldName}`";
                fieldValArrTemp[fieldLoop] = $"'{currentFieldValue.Trim()}'";
                fieldLoop++;

                if (Globals.RegularlyUpdatedFields.Contains(currentFieldName))
                {
                    fieldArrTempHist[fieldLoopHist] = $"`{currentFieldName}`";
                    fieldValArrTempHist[fieldLoopHist] = $"'{currentFieldValue.Trim()}'";
                    fieldLoopHist++;
                }
            }
            else
            {
                continue; // check what would be most appropriate here
            }
        }

        // Trim the empty array elements
        string[] fieldArr = fieldArrTemp.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        string[] fieldValArr = fieldValArrTemp.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        string[] fieldArrHist = fieldArrTempHist.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        string[] fieldValArrHist = fieldValArrTempHist.Where(x => !string.IsNullOrEmpty(x)).ToArray();

        fieldList = String.Join(", ", fieldArr);
        fieldValues = String.Join(", ", fieldValArr);
        fieldListHist = String.Join(", ", fieldArrHist);
        fieldValuesHist = String.Join(", ", fieldValArrHist);
        //Debug.WriteLine(fieldListHist);
        //Debug.WriteLine(fieldListHist);

        // Populate relevant table (ALL normal account fields)
        string insertOnly = $"INSERT INTO `{popTable}` ({fieldList}, `LastUpdatedDate`) VALUES({fieldValues}, @updateTime)";

        Debug.WriteLine(insertOnly);

        MySqlCommand cmd1 = new MySqlCommand(insertOnly, conn, trans);

        cmd1.Connection = conn;
        cmd1.Transaction = trans;
        cmd1.CommandText = insertOnly;
        cmd1.Prepare();
        cmd1.Parameters.Add(new MySqlParameter("@updateTime", MySqlDbType.DateTime) { Value = timeNow });

        rowsMod = cmd1.ExecuteNonQuery();

        if (rowsMod > 0)
        {
            Debug.WriteLine($"APPENDED ROW: Table: {popTable} at JSE Code {currentPrimaryKey}");

            truePrimaryKey = (Int32)cmd1.LastInsertedId;
            Debug.WriteLine($"Inserted Row at Id: {truePrimaryKey}");

            // Update tbl*history (NEWFundID, update date, Perfs)
            string sqlhistory = $"INSERT INTO `{histTable}` ({fieldListHist}, `fkiFundId`, `UpdatedDate`) VALUES({fieldValuesHist}, @truePrimary, @updatedTime)";

            MySqlCommand cmd3 = new MySqlCommand(sqlhistory, conn, trans);
            cmd3.Connection = conn;
            cmd3.Transaction = trans;
            cmd3.CommandText = sqlhistory;
            cmd3.Prepare();
            cmd3.Parameters.Add(new MySqlParameter("@truePrimary", MySqlDbType.Int32) { Value = truePrimaryKey });
            cmd3.Parameters.Add(new MySqlParameter("@updatedTime", MySqlDbType.DateTime) { Value = timeNow });

            rowsMod = cmd3.ExecuteNonQuery();

            if (rowsMod > 0)
            {
                Debug.WriteLine($"APPENDED ROW: Table: {histTable} at Key: fkiFundId - {truePrimaryKey}");
            }
        }

        // No need for audit when NEW entry - only modifications
        return;
    }

    private static string ParseDBDate(string dbdate) // Re-formats Glacier dates from M/d/yyyy to yyyy/M/d
    {
        string dateString = "";
        string formattedDate = "";

        dateString = dbdate.Trim();

        try
        {
            formattedDate = DateTime.ParseExact(dateString, "dd/MM/yyyy hh:mm:ss tt", null).ToString("yyyy-MM-dd");
            //Debug.WriteLine(formattedDate);
        }
        catch
        {
            formattedDate = null;
            //Debug.WriteLine("formattedDate is null");
        }

        return formattedDate;
    }

    private static decimal? ParseDBDecimal(decimal? currentValue) // Return formatted decimal or null value if "n/a"
    {
        decimal? currentVal = 0;
        if (currentValue != null)
        {
            currentVal = Convert.ToDecimal(currentValue / 1.0000000000000000000000000000m);
        }
        else
        {
            currentVal = null;
            //Debug.WriteLine("formatted decimal is null");
        }


        return currentVal;
    }

}

// By J. Koekemoer