﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcfImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            
            VcfManager reader = new VcfManager("example_big.vcf");
            reader.establishConnectionWithDatabase("127.0.0.1", "zootechnika", "root", "");
            List<string> temp = reader.tableInsertQueryStringGenerator("biodata",100);
            foreach (string item in temp)
            {
                Console.WriteLine(item.Substring(0, 4));
            }
            //reader.tableInsertQueryStringGenerator("biodata");
            //reader.sendQuery(reader.tableInsertQueryStringGenerator("biodata"));
            Console.ReadKey();
        }
    }
}