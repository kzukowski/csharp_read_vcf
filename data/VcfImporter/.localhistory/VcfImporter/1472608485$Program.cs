using System;
using System.Collections.Generic;

namespace VcfImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //file has to be placed in debug folder of the project
            VcfManagerAbstract reader = new VcfManagerMSSQL(@"C:\Users\bakal_000\Documents\csharp_read_vcf\data\VcfImporter\VcfImporter\bin\Debug\example_big.vcf");
            //connection parameters
            reader.connect("Data Source=MICHAL-PC;Initial Catalog=zootechnika;Integrated Security=True");
            reader.readAllDataFromFile();
            reader.sendToDatabase("DROP TABLE biodata");
            reader.createTableFromVCFData("biodata");
            List<string> temp = reader.generateInsert("biodata",100);
            reader.sendToDatabase(temp);
            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
