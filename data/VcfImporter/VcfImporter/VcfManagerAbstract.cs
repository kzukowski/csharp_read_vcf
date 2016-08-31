using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace VcfImporter
{
    abstract class VcfManagerAbstract
    {
        string fileLocation;
        int linesCounter = 0;
        Dictionary<string, List<Dictionary<string, string>>> headerData; // header data structure
        public Dictionary<string, string> additionalRowNames;
        public List<Dictionary<string, string>> additionalInfoValues;
        public List<string> tableRowNames; //records data structure
        public List<List<string>> tableValues;
        public DatabaseConnectInterface dbConnect;

        // file location is the path to the file 
        public VcfManagerAbstract(string fileLocation)
        {
            this.fileLocation = fileLocation;
            headerData = new Dictionary<string, List<Dictionary<string, string>>>();
            tableRowNames = new List<string>();
            tableValues = new List<List<string>>();
            additionalRowNames = new Dictionary<string, string>();
            additionalInfoValues = new List<Dictionary<string, string>>();
        }

        public abstract void createTableFromVCFData(string tableName);

        public void readAllDataFromFile(string fileLocation)
        {
            this.fileLocation = fileLocation;
            readAllDataFromFile();
        }

        public void sendToDatabase(string query)
        {
            dbConnect.sendQuery(query);
        }

        public void sendToDatabase(List<string> queries)
        {
            dbConnect.queriesInTransaction(queries);
        }

        public void connect(string connectionParameters)
        {
            dbConnect.initialize(connectionParameters);
        }



        //divideEveryColumn is the value of rows in one query in returned list
        public virtual List<string> generateInsert(string tableLocationAndName, int divideEveryColumn)//when database is too large it is necessary to sendToDatabase database in multiple packets
        {
            List<string> queriesList = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            for(int currentRow = 0; currentRow < tableValues.Count; currentRow++)
            { 
                stringBuilder.Append("INSERT INTO `");
                stringBuilder.Append(tableLocationAndName + "` (`");
                foreach (string item in tableRowNames)
                {
                    stringBuilder.Append(item + "`, `");
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
                    stringBuilder.Remove(stringBuilder.Length - 2, 2);
                    stringBuilder.Append("), ");
                    currentRow+=1;
                }
                currentRow -= 1;
                stringBuilder.Remove(stringBuilder.Length - 6, 6);
                stringBuilder.Append("')");
                queriesList.Add(stringBuilder.ToString());
                stringBuilder.Clear();
            }
            return queriesList;
        }

        public void readAllDataFromFile()
        {

            // Open the text file using a stream reader.
            using (StreamReader file = new StreamReader(fileLocation))
            {
                string line;
                int tempCharacterPosition = 0, infoRowPosition = 0, currentRowPosition = 0;
                while ((line = file.ReadLine()) != null)
                {
                    tempCharacterPosition = 0;
                    //getting header data
                    if (line.Substring(0, 2) == "##")
                    {
                        string headerName, tempSubString = "", fieldName = "", fieldValue = "";
                        //checks if name of the parameter is in the dictionary
                        if (!headerData.ContainsKey(headerName = line.Substring(line.IndexOf("##") + 2, (line.IndexOf("=")) - 2)))
                        {
                            headerData.Add(headerName, new List<Dictionary<string, string>>());
                        }
                        if (line[line.Length - 1] == '>') // check if row has multiple parameters
                        {
                            tempSubString = line.Substring(line.IndexOf("<") + 1);
                            Dictionary<string, string> tempDictionary = new Dictionary<string, string>();
                            while (tempSubString != "") // gets every each of parameters in multiple parameters row
                            {
                                fieldName = tempSubString.Substring(0, tempCharacterPosition = tempSubString.IndexOf("="));
                                if (tempSubString[tempCharacterPosition + 1] == '"') // gets string in quotations mark taking into account inside quotation marks
                                {
                                    Regex regex = new Regex(@"""[^""\\]*(?:\\.[^""\\]*)*""");
                                    fieldValue = regex.Match(tempSubString).ToString();
                                    tempCharacterPosition = tempCharacterPosition + fieldValue.Length + 1;
                                }
                                else // if there are no quotation marks in specific value
                                {
                                    fieldValue = tempSubString.Substring(tempSubString.IndexOf("=") + 1, (tempCharacterPosition = (tempSubString.IndexOf(",") > 0 ? tempSubString.IndexOf(",") : tempSubString.IndexOf(">"))) - tempSubString.IndexOf("=") - 1);
                                }
                                tempDictionary.Add(fieldName, fieldValue);
                                tempSubString = tempSubString.Substring(tempCharacterPosition + 1);
                            }
                            List<Dictionary<string, string>> tempList;
                            headerData.TryGetValue(headerName, out tempList);
                            tempList.Add(tempDictionary);
                        }
                        else // Adds to header dictionary headername with only one value, without value name
                        {
                            tempSubString = line.Substring(line.IndexOf("=") + 1);
                            headerData[headerName].Add(new Dictionary<string, string>() { { "", tempSubString } });
                        }

                    }
                    else// getting database data
                    {
                        string tempSubString, fieldValue = "";
                        if (line.Substring(0, 1) == "#")
                        {
                            tempSubString = line.Substring(1);
                            while (tempSubString != "") // gets every each of parameters in multiple parameters row
                            {
                                fieldValue = tempSubString.Substring(0, (tempCharacterPosition = (tempSubString.IndexOf("\t") < 0 ? tempSubString.Length : tempSubString.IndexOf("\t"))));
                                tableRowNames.Add(fieldValue);
                                if (tempSubString.Length > tempCharacterPosition + 1)
                                {
                                    tempSubString = tempSubString.Substring(tempCharacterPosition + 1);
                                }
                                else
                                {
                                    tempSubString = "";
                                }
                                if(fieldValue=="INFO") // additionalRowNames generated fields from info
                                {
                                    infoRowPosition = currentRowPosition;
                                }
                                currentRowPosition++;
                            }
                        }
                        else
                        {
                            currentRowPosition = 0;
                            tempSubString = line;
                            List<string> tempValues = new List<string>();
                            while (tempSubString != "") // gets every each of parameters in multiple parameters row
                            {
                                fieldValue = tempSubString.Substring(0, (tempCharacterPosition = (tempSubString.IndexOf("\t") < 0 ? tempSubString.Length : tempSubString.IndexOf("\t"))));
                                
                                tempValues.Add(fieldValue);
                                if (currentRowPosition == infoRowPosition)
                                {
                                    List<string> tempInfoValues = new List<string>();
                                    Dictionary<string, string> additionalTempValues = new Dictionary<string, string>();
                                    string tempInfoField = fieldValue, tempInfo = fieldValue;
                                    int tempInfoPointerPosition = 0;
                                    while (tempInfo != "") // gets every each of parameters in multiple parameters row
                                    {
                                        tempInfoField = tempInfo.Substring(0, (tempInfoPointerPosition = (tempInfo.IndexOf(";") < 0 ? tempInfo.Length : tempInfo.IndexOf(";"))));
                                        
                                        if(tempInfoField.IndexOf("=")<0)
                                        {
                                            additionalRowNames[tempInfoField] = "text";
                                            additionalTempValues[tempInfoField] = "";
                                            //Console.WriteLine(tempInfoField);
                                        }
                                        else
                                        {
                                            string tempValue = "";
                                            float tempValueF = 0f;
                                            if(additionalRowNames.TryGetValue(tempInfoField.Substring(0, tempInfoField.IndexOf("=")), out tempValue))
                                            {
                                                if(tempValue != "text")
                                                {
                                                    if(!float.TryParse(tempInfoField.Substring(tempInfoField.IndexOf("=") + 1, tempInfoField.Length - tempInfoField.IndexOf("=") - 1), out tempValueF)
                                                        || tempInfoField.Substring(tempInfoField.IndexOf("=") + 1, tempInfoField.Length - tempInfoField.IndexOf("=") - 1).IndexOf(",") > -1)
                                                    {
                                                        //Console.WriteLine(tempInfoField.Substring(0, tempInfoField.IndexOf("=")) + "text");
                                                        additionalRowNames[tempInfoField.Substring(0, tempInfoField.IndexOf("="))] = "text";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (float.TryParse(tempInfoField.Substring(tempInfoField.IndexOf("=") + 1, tempInfoField.Length - tempInfoField.IndexOf("=") - 1), out tempValueF)
                                                    && tempInfoField.Substring(tempInfoField.IndexOf("=") + 1, tempInfoField.Length - tempInfoField.IndexOf("=") - 1).IndexOf(",") < 0)
                                                {
                                                    additionalRowNames[tempInfoField.Substring(0, tempInfoField.IndexOf("="))] = "float";
                                                    //Console.WriteLine(tempInfoField.Substring(0, tempInfoField.IndexOf("=")) + " float");
                                                }
                                                else
                                                {
                                                    additionalRowNames[tempInfoField.Substring(0, tempInfoField.IndexOf("="))] = "text";
                                                    //Console.WriteLine(tempInfoField.Substring(0, tempInfoField.IndexOf("=")) + " text");
                                                }
                                            }

                                            additionalTempValues[tempInfoField.Substring(0, tempInfoField.IndexOf("="))] = tempInfoField.Substring(tempInfoField.IndexOf("=") + 1, tempInfoField.Length - tempInfoField.IndexOf("=") - 1);
                                            //Console.WriteLine(tempInfoField.Substring(0, tempInfoField.IndexOf("=")) + " " + tempInfoField.Substring(tempInfoField.IndexOf("=") + 1, tempInfoField.Length - tempInfoField.IndexOf("=") - 1));
                                        }
                                        if (tempInfo.Length > tempInfoPointerPosition + 1)
                                        {
                                            tempInfo = tempInfo.Substring(tempInfoPointerPosition + 1);
                                        }
                                        else
                                        {
                                            tempInfo = "";
                                        }
                                        currentRowPosition++;
                                    }
                                    additionalInfoValues.Add(additionalTempValues);

                                }
                                if (tempSubString.Length > tempCharacterPosition + 1)
                                {
                                    tempSubString = tempSubString.Substring(tempCharacterPosition + 1);
                                }
                                else
                                {
                                    tempSubString = "";
                                }
                                currentRowPosition++;
                            }
                            tableValues.Add(tempValues);
                        }
                    }
                    linesCounter++;
                }
            }
        }
    }
}
