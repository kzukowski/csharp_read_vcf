using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcfImporter
{
    class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public DBConnect()
        {

        }

        //Initialize values
        public void Initialize(string server, string datbase, string username, string password)
        {
            this.server = server;
            this.database = datbase;
            this.uid = username;
            this.password = password;
            string connectionString;
            connectionString = "server=" + server + ";" + "uid=" +
            uid + ";" + "pwd=" + password + ";" + "database=" + database + ";";
            Console.WriteLine(connectionString);

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                Console.WriteLine("connection opened");
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        // sending single query to database
        public void SendQuery(string command)
        {
            //open connection
            if (this.OpenConnection() == true)
            {
                Console.WriteLine(1);
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(command, connection);
                Console.WriteLine(2);

                //Execute command
                cmd.ExecuteNonQuery();
                Console.WriteLine(3);

                //close connection
                this.CloseConnection();
                Console.WriteLine(4);
            }
        }

        // makes one transaction with multiple queues in passed list
        // databases have query size limits, so it is necessary to divide them and send them in transaction
        public void queriesTransaction(List<string> commands)
        {
            //open connection
            if (this.OpenConnection() == true)
            {
                MySqlCommand myCommand = connection.CreateCommand();
                MySqlTransaction myTrans = connection.BeginTransaction();
                myCommand.Connection = connection;
                myCommand.Transaction = myTrans;
                try
                {
                    foreach (string item in commands)
                    {
                        myCommand.CommandText = item;
                        myCommand.ExecuteNonQuery();

                    }
                    myTrans.Commit();
                }
                catch(Exception e)
                {
                    try
                    {
                        myTrans.Rollback();
                    }
                    catch (SqlException ex)
                    {
                        if (myTrans.Connection != null)
                        {
                            Console.WriteLine("An exception of type " + ex.GetType() +
                            " was encountered while attempting to roll back the transaction.");
                        }
                    }

                    Console.WriteLine("An exception of type " + e.GetType() +
                    " was encountered while inserting the data.");
                    Console.WriteLine("Neither record was written to database.");
                }
                finally
                {
                    this.CloseConnection();
                }
            }
        }
    }
}
