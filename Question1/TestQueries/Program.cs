using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestQueries
{
    internal class Program
    {
        private const string s_connectionString = "Server=.\\sqlexpress;Database=IDENTClinic4;Trusted_Connection=True;Encrypt=no;";
        static void Main(string[] args)
        {
            SqlConnection connection = new(s_connectionString);
            try
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = @"
with days as (
select dateadd(day, value, cast('2015-01-01' as date)) as 'day' from generate_series(0, 364, 1)
)
select count(*) as 'Cnt', days.day as 'Day' from receptions inner join days 
on CONVERT(DATE, Receptions.StartDateTime)=days.day group by days.day
order by Day
";
                List<DayCount> list5 = new();

                DateTime start = DateTime.Now;
                Console.WriteLine("1.5)");
                SqlDataReader reader = command.ExecuteReader();
                Console.WriteLine("done at {0}", DateTime.Now - start);
                while (reader.Read())
                {
                    list5.Add(new DayCount { Day = reader.GetDateTime(1), Count = reader.GetInt32(0) });
                }
                reader.Close();
                Console.WriteLine("count rows: {0}, sum: {1}", list5.Count, list5.Select(x => x.Count).Sum());

                command.CommandText = @"
select 
d.id as 'id_doc',
per.lastname as 'doc_lastname', 
p.id as 'pat_id',
per1.lastname as 'pat_lastname' 
from
doctors d
inner join 
patients p
	on d.id=(select top 1 id_doctors from receptions where id_patients=p.id order by StartDateTime desc)
inner join
persons per
	on d.id=per.id
inner join
persons per1	
	on p.id=per1.id
";
                List<LastReception> list6 = new();

                start = DateTime.Now;
                Console.WriteLine("2.1)");
                reader = command.ExecuteReader();
                Console.WriteLine("done at {0}", DateTime.Now - start);
                while (reader.Read())
                {
                    list6.Add(new LastReception {
                        IdDoctor = reader.GetInt32(0),
                        DoctorLastName = reader.GetString(1),
                        IdPatient = reader.GetInt32(2),
                        PatientLastName = reader.GetString(3),
                    });
                }
                reader.Close();
                Console.WriteLine("count rows: {0}", list6.Count);

                command.CommandText = @"
select
d.id as 'id_doc',
per.lastname as 'doc_lastname', 
p.id as 'pat_id',
per1.lastname as 'pat_lastname',
r.StartDateTime
from 
doctors d
inner join
receptions r
on d.id=r.id_doctors
inner join
patients p
on r.id=(select top 1 id from receptions where id_patients=p.id order by StartDateTime desc)
inner join
persons per
	on d.id=per.id
inner join
persons per1	
	on p.id=per1.id
";
                List<LastReception> list7 = new();

                start = DateTime.Now;
                Console.WriteLine("2.2)");
                reader = command.ExecuteReader();
                Console.WriteLine("done at {0}", DateTime.Now - start);
                while (reader.Read())
                {
                    list7.Add(new LastReception
                    {
                        IdDoctor = reader.GetInt32(0),
                        DoctorLastName = reader.GetString(1),
                        IdPatient = reader.GetInt32(2),
                        PatientLastName = reader.GetString(3),
                        StartDayTime = reader.GetDateTime(4),
                    });
                }
                reader.Close();
                Console.WriteLine("count rows: {0}", list7.Count);

                command.CommandText = @"
select 
d.id as 'id_doc',
per.lastname as 'doc_lastname', 
p.id as 'pat_id',
per1.lastname as 'pat_lastname' 
from
doctors d
inner join 
patients p
	on d.id=(select top 1 id_doctors from receptions where id_patients=p.id and StartDateTime=(select max(StartDateTime) from receptions where id_patients=p.id))
inner join
persons per
	on d.id=per.id
inner join
persons per1	
	on p.id=per1.id
";
                List<LastReception> list8 = new();

                start = DateTime.Now;
                Console.WriteLine("2.3)");
                reader = command.ExecuteReader();
                Console.WriteLine("done at {0}", DateTime.Now - start);
                while (reader.Read())
                {
                    list6.Add(new LastReception
                    {
                        IdDoctor = reader.GetInt32(0),
                        DoctorLastName = reader.GetString(1),
                        IdPatient = reader.GetInt32(2),
                        PatientLastName = reader.GetString(3),
                    });
                }
                reader.Close();
                Console.WriteLine("count rows: {0}", list6.Count);

            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                connection.Close();
            }
        }
    }
    internal class DayCount
    {
        internal int Count { get; set; }
        internal DateTime Day { get; set; }
    }
    internal class LastReception
    {
        internal int IdDoctor { get; set; }
        internal string DoctorLastName { get; set; }
        internal int IdPatient { get; set; }
        internal string PatientLastName { get; set; }
        internal DateTime StartDayTime { get; set; }
    }
}