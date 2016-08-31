using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace VcfImporter
{
    class VcfManagerMySQL : VcfManagerAbstract
    {
        public VcfManagerMySQL(string fileLocation) : base(fileLocation)
        {
            dbConnect = new DatabaseConnectMySQL();
        }

        public override void createTableFromVCFData(string tableName)
        {
            throw new NotImplementedException();
        }
    }
}
