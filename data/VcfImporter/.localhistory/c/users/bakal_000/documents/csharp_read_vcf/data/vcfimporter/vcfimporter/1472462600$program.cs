using System;
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
            
            //file has to be placed in debug folder of the project
            VcfManager reader = new VcfManager("example_big.vcf");
            reader.connect("127.0.0.1", "zootechnika", "root", "");
            //reader.readAllDataFromFile();
            reader.send("TRUNCATE TABLE `biodata`");
            //List<string> temp = reader.generateInsert("biodata",100);
            //reader.send(temp);
            Console.ReadKey();
        }
    }
}
