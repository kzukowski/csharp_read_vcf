﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VcfImporter
{
    class VcfManager
    {
        string fileLocation;
        int linesCounter = 0;
        Dictionary<String, List<Dictionary<string, string>>> headerData; // header data structure
        List<string> tableRowNames; //records data structure
        List<List<string>> tableValues;
        
        // file location is the path to the file 
        public VcfManager(string fileLocation)
        {
            this.fileLocation = fileLocation;
            headerData = new Dictionary<String, List<Dictionary<string, string>>>();
            tableRowNames = new List<string>();
            tableValues = new List<List<string>>();
            readDataFromFile();
        }

        public void readDataFromFile()
        {

            // Open the text file using a stream reader.
            using (StreamReader file = new StreamReader(fileLocation))
            {
                string line;
                int tempCharacterPosition = 0;
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
                            //Console.WriteLine("Added: " + headerName);
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
                                //Console.WriteLine(fieldName + ":" + fieldValue);
                                tempSubString = tempSubString.Substring(tempCharacterPosition + 1);
                            }
                            List<Dictionary<string, string>> tempList;
                            headerData.TryGetValue(headerName, out tempList);
                            tempList.Add(tempDictionary);
                            //foreach (KeyValuePair<string, string> kvp in tempDictionary)
                            //{
                            //    //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                            //    Console.WriteLine(string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value));
                            //}
                        }
                        else // Adds to header dictionary headername with only one value, without value name
                        {
                            tempSubString = line.Substring(line.IndexOf("=") + 1);
                            headerData[headerName].Add(new Dictionary<string, string>() { { "", tempSubString } });
                            //Console.WriteLine(tempSubString);
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
                                //Console.WriteLine(fieldValue);
                                if (tempSubString.Length > tempCharacterPosition + 1)
                                {
                                    tempSubString = tempSubString.Substring(tempCharacterPosition + 1);
                                }
                                else
                                {
                                    tempSubString = "";
                                }
                            }
                            //Console.WriteLine(string.Join("\t", tableRowNames.ToArray()));
                        }
                        else
                        {
                            tempSubString = line;
                            List<string> tempValues = new List<string>();
                            while (tempSubString != "") // gets every each of parameters in multiple parameters row
                            {
                                fieldValue = tempSubString.Substring(0, (tempCharacterPosition = (tempSubString.IndexOf("\t") < 0 ? tempSubString.Length : tempSubString.IndexOf("\t"))));
                                tempValues.Add(fieldValue);
                                if (tempSubString.Length > tempCharacterPosition + 1)
                                {
                                    tempSubString = tempSubString.Substring(tempCharacterPosition + 1);
                                }
                                else
                                {
                                    tempSubString = "";
                                }
                            }
                            //Console.WriteLine(string.Join("\t", tempValues.ToArray()));
                        }
                    }
                    linesCounter++;
                }
            }
            //Console.ReadKey();
        }
    }
}
