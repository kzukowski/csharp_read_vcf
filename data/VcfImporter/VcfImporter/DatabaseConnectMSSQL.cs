using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace VcfImporter
{
    class DatabaseConnectMSSQL : DatabaseConnectInterface
    {
        private SqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public DatabaseConnectMSSQL()
        {

        }

        //Initialize values
        public void initialize(string connectionParameters)
        {
            connection = new SqlConnection(connectionParameters);
        }

        //open connection to database
        public bool openConnection()
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
        public bool closeConnection()
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
            finally
            {
                Console.WriteLine("connection closed");
            }
        }


        // sending single query to database
        public void sendQuery(string command)
        {
            //open connection
            if (this.openConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                SqlCommand cmd = new SqlCommand(command,connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.closeConnection();
            }
        }

        // makes one transaction with multiple queues in passed list
        // databases have query size limits, so it is necessary to divide them and sendToDatabase them in transaction
        public void queriesInTransaction(List<string> commands)
        {
            //open connection
            if (this.openConnection() == true)
            {
                SqlCommand myCommand = connection.CreateCommand();
                SqlTransaction myTrans = connection.BeginTransaction();
                myCommand.Connection = connection;
                myCommand.Transaction = myTrans;
                string tempString = "";
                try
                {
                    foreach (string item in commands)
                    {
                        tempString = item;
                        myCommand.CommandText = item;
                        myCommand.ExecuteNonQuery();

                    }
                    myTrans.Commit();
                }
                catch (Exception e)
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
                            Console.Write(tempString);
                        }
                    }

                    Console.WriteLine("An exception of type " + e.GetType() +
                    " was encountered while inserting the data.");
                    Console.WriteLine("Neither record was written to database.");
                }
                finally
                {
                    this.closeConnection();
                }
            }
        }
    }
}
