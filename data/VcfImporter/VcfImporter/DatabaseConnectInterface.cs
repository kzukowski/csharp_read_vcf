using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcfImporter
{
    interface DatabaseConnectInterface
    {
        // saves connection parameters into class instance
        void initialize(string connectionParameters);

        //establishes connection
        bool openConnection();
        
        bool closeConnection();

        //sends single querry passed in string
        void sendQuery(string command);

        //makes one transaction and sends multiple queries in it
        void queriesInTransaction(List<string> commands);

    }
}
