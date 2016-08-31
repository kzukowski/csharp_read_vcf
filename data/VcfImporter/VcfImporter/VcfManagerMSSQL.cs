using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace VcfImporter
{
    class VcfManagerMSSQL : VcfManagerAbstract
    {
        public VcfManagerMSSQL(string fileLocation) : base(fileLocation)
        {
            dbConnect = new DatabaseConnectMSSQL();
        }

        public override void createTableFromVCFData(string tableName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("CREATE TABLE " + tableName + "([");
            foreach (var tableRowName in tableRowNames)
            {
                stringBuilder.Append(tableRowName + "] text NULL, [");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            foreach (KeyValuePair< string, string> additionalRow in additionalRowNames)
            {
                stringBuilder.Append("[" + additionalRow.Key + "] " + additionalRow.Value + " NULL, ");
            }

            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(")");
            Console.WriteLine(stringBuilder.ToString());

            dbConnect.sendQuery(stringBuilder.ToString());

        }

        public override List<string> generateInsert(string tableLocationAndName, int divideEveryColumn)
        {
            List<string> queriesList = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            for (int currentRow = 0; currentRow < tableValues.Count; currentRow++)
            {
                stringBuilder.Append("INSERT INTO [");
                stringBuilder.Append(tableLocationAndName + "] ([");
                foreach (string item in tableRowNames)
                {
                    stringBuilder.Append(item + "], [");
                }

                foreach (string item in additionalRowNames.Keys)
                {
                    stringBuilder.Append(item + "], [");
                }
                stringBuilder.Remove(stringBuilder.Length - 3, 3);
                stringBuilder.Append(") VALUES ");
                for (int currentRowInPart = 0; currentRowInPart < divideEveryColumn && currentRow < tableValues.Count; currentRowInPart++)
                {
                    stringBuilder.Append("(");
                    foreach (string element in tableValues[currentRow])
                    {
                        stringBuilder.Append("'" + element + "', ");
                    }
                    foreach (string element in additionalRowNames.Keys)
                    {
                        string tempValue = "";
                        if(additionalInfoValues[currentRow].TryGetValue(element, out tempValue))
                        {
                            stringBuilder.Append("'" + tempValue + "', ");
                        }
                        else
                        {
                            stringBuilder.Append("NULL, ");
                        }
                    }
                    stringBuilder.Remove(stringBuilder.Length - 2, 2);
                    stringBuilder.Append("), ");
                    currentRow += 1;
                }
                currentRow -= 1;
                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                queriesList.Add(stringBuilder.ToString());
                stringBuilder.Clear();
            }
            return queriesList;
        }
    }
}
