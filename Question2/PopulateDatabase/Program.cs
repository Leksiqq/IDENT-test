using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopulateDatabase
{
    internal class Program
    {
        private const string s_connectionString = "Server=.\\sqlexpress;Database=IDENTClinic4;Trusted_Connection=True;Encrypt=no;";
        private const int s_count = 50000;
        private const double s_doctorsRatio = .01;
        private const int s_minDoctorsReceptions = 1000;
        private const int s_maxDoctorsReceptions = 20000;
        private const int s_100yearsDays = 36525;
        private const int s_18yearsMinutes = 9467280;
        static void Main(string[] args)
        {
            Random rnd = new();
            SqlConnection connection = new(s_connectionString);
            SqlTransaction? sqlTransaction = null;
            try
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                DateTime start = DateTime.Now;
                Console.WriteLine("Starting clear DB");
                command.CommandText = "DELETE FROM Receptions";
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM Doctors";
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM Patients";
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM Persons";
                command.ExecuteNonQuery();
                Console.WriteLine("done at {0}", DateTime.Now - start);
                SqlCommand insertPersonCommand = connection.CreateCommand();
                insertPersonCommand.CommandText = @"
INSERT INTO Persons (ID, Name, LastName, BirthDate) VALUES (@ID, @Name, @LastName, @BirthDate)
";
                insertPersonCommand.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                insertPersonCommand.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 50);
                insertPersonCommand.Parameters.Add("@LastName", System.Data.SqlDbType.NVarChar, 50);
                insertPersonCommand.Parameters.Add("@BirthDate", System.Data.SqlDbType.DateTime);
                insertPersonCommand.Prepare();

                SqlCommand insertDoctorCommand = connection.CreateCommand();
                insertDoctorCommand.CommandText = "INSERT INTO Doctors (ID, ITN) VALUES (@ID, @ITN)";
                insertDoctorCommand.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                insertDoctorCommand.Parameters.Add("@ITN", System.Data.SqlDbType.Char, 12);
                insertDoctorCommand.Prepare();

                SqlCommand insertPatientCommand = connection.CreateCommand();
                insertPatientCommand.CommandText = "INSERT INTO Patients (ID, Card) VALUES (@ID, @Card)";
                insertPatientCommand.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                insertPatientCommand.Parameters.Add("@Card", System.Data.SqlDbType.VarChar, 50);
                insertPatientCommand.Prepare();

                Console.WriteLine("Starting populate DB");
                sqlTransaction = connection.BeginTransaction();
                insertPersonCommand.Transaction = sqlTransaction;
                insertDoctorCommand.Transaction = sqlTransaction;
                insertPatientCommand.Transaction = sqlTransaction;

                List<int> doctorIds = new();
                start = DateTime.Now;
                foreach (
                    var item in 
                    Enumerable
                        .Range(1, s_count)
                        .Select(pid => new { Id = pid, Name = "Иван", LastName = string.Format("Иванов{0}", pid) })
                )
                {
                    insertPersonCommand.Parameters["@ID"].Value = item.Id;
                    insertPersonCommand.Parameters["@Name"].Value = item.Name;
                    insertPersonCommand.Parameters["@LastName"].Value = item.LastName;
                    insertPersonCommand.Parameters["@BirthDate"].Value = new DateTime(2024, 1, 1).AddDays(-rnd.Next(s_100yearsDays));
                    insertPersonCommand.ExecuteNonQuery();

                    insertPatientCommand.Parameters["@ID"].Value = item.Id;
                    while (true)
                    {
                        insertPatientCommand.Parameters["@Card"].Value = string.Format("{0,8}", rnd.Next(100000000));
                        try
                        {
                            insertPatientCommand.ExecuteNonQuery();
                            break;
                        }
                        catch (SqlException ex)
                        {
                            if(ex.HResult != -2146232060)
                            {
                                throw;
                            }
                        }
                    }
                    if (rnd.NextDouble() <= s_doctorsRatio)
                    {
                        insertDoctorCommand.Parameters["@ID"].Value = item.Id;
                        doctorIds.Add(item.Id);
                        while (true)
                        {
                            insertDoctorCommand.Parameters["@ITN"].Value = string.Format("{0,6}{1,6}", rnd.Next(10000000), rnd.Next(10000000));
                            try
                            {
                                insertDoctorCommand.ExecuteNonQuery();
                                break;
                            }
                            catch (SqlException ex) 
                            {
                                if (ex.HResult != -2146232060)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
                SqlCommand insertReceptionCommand = connection.CreateCommand();
                insertReceptionCommand.CommandText = @"
INSERT INTO Receptions (ID, ID_Doctors, ID_Patients, StartDateTime) VALUES (@ID, @ID_Doctors, @ID_Patients, @StartDateTime)
";
                insertReceptionCommand.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                insertReceptionCommand.Parameters.Add("@ID_Doctors", System.Data.SqlDbType.Int);
                insertReceptionCommand.Parameters.Add("@ID_Patients", System.Data.SqlDbType.Int);
                insertReceptionCommand.Parameters.Add("@StartDateTime", System.Data.SqlDbType.DateTime);

                insertReceptionCommand.Transaction = sqlTransaction;
                int receptionId = 0;
                foreach (int id in doctorIds) 
                {
                    for (int i = rnd.Next(s_minDoctorsReceptions, s_maxDoctorsReceptions + 1); i >= 0; --i) 
                    {
                        int patientId = rnd.Next(1, s_count);
                        if(patientId != id)
                        {
                            insertReceptionCommand.Parameters["@ID"].Value = ++receptionId;
                            insertReceptionCommand.Parameters["@ID_Doctors"].Value = id;
                            insertReceptionCommand.Parameters["@ID_Patients"].Value = patientId;
                            insertReceptionCommand.Parameters["@StartDateTime"].Value = 
                                DateTime.Now.AddMinutes(-rnd.Next(s_18yearsMinutes));
                            insertReceptionCommand.ExecuteNonQuery();
                        }
                    }
                }
                sqlTransaction.Commit();
                Console.WriteLine("done at {0}", DateTime.Now - start);


            }
            catch (SqlException ex)
            {
                sqlTransaction?.Rollback();
                Console.WriteLine(ex);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
