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
            
            VcfManager reader = new VcfManager("example_small.vcf");
            Console.WriteLine(reader.tableInsertQueryStringGenerator("db"));
            Console.ReadKey();
        }
    }
}