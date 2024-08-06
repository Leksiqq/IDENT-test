using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestQueries
{
    internal class Program
    {
        private const string s_connectionString = 
            "Server=.\\sqlexpress;Database=IDENTClinic4;Trusted_Connection=True;Encrypt=no;";
        static void Main(string[] args)
        {
            SqlConnection connection = new(s_connectionString);
            try
            {
                connection.Open();

                Action query1_1 = () =>
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandTimeout = 600;
                    command.CommandText = @"
with days as (
  select dateadd(day, value, cast('2015-01-01' as date)) as 'day' from   generate_series(0, 364, 1)
)
select days.day as 'Day', count(id) as 'Cnt' from 
days 
  left join 
receptions
  on 
  DATEPART(year, Receptions.StartDateTime)=DATEPART(year, days.day)
  and DATEPART(DAYOFYEAR, Receptions.StartDateTime)=DATEPART(DAYOFYEAR, days.day)

group by days.day
order by Day
";
                    List<DayCount> list = new();

                    DateTime start = DateTime.Now;
                    SqlDataReader reader = command.ExecuteReader();
                    TimeSpan readerExecuted = DateTime.Now - start;
                    while (reader.Read())
                    {
                        list.Add(new DayCount { Day = reader.GetDateTime(0), Count = reader.GetInt32(1) });
                    }
                    reader.Close();
                    Console.WriteLine(
                        $@"1.1) reader executed at {readerExecuted
                        }
done at {DateTime.Now - start
                        }, count rows: {list.Count
                        }, sum: {list.Select(x => x.Count).Sum()}"
                    );
                };

                Action query1_2 = () =>
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandTimeout = 600;
                    command.CommandText = @"
EXEC GetDailyReceptionsNum '20150101', '20151231'
";
                    List<DayCount> list = new();

                    DateTime start = DateTime.Now;
                    SqlDataReader reader = command.ExecuteReader();
                    TimeSpan readerExecuted = DateTime.Now - start;
                    while (reader.Read())
                    {
                        list.Add(new DayCount { Day = reader.GetDateTime(0), Count = reader.GetInt32(1) });
                    }
                    reader.Close();
                    Console.WriteLine(
                        $@"1.1) reader executed at {readerExecuted
                        }
done at {DateTime.Now - start
                        }, count rows: {list.Count
                        }, sum: {list.Select(x => x.Count).Sum()}"
                    );
                };

                Action query2_1 = () =>
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandTimeout = 600;
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
                    List<LastReception> list = new();

                    DateTime start = DateTime.Now;
                    SqlDataReader reader = command.ExecuteReader();
                    TimeSpan readerExecuted = DateTime.Now - start;
                    while (reader.Read())
                    {
                        list.Add(new LastReception
                        {
                            IdDoctor = reader.GetInt32(0),
                            DoctorLastName = reader.GetString(1),
                            IdPatient = reader.GetInt32(2),
                            PatientLastName = reader.GetString(3),
                        });
                    }
                    reader.Close();
                    Console.WriteLine(
                        $@"2.1) reader executed at {readerExecuted
                        }
done at {DateTime.Now - start
                        }, count rows: {list.Count}"
                    );
                };
                Action query2_2 = () =>
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandTimeout = 600;
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
    on r.id=(select top 1 id from receptions where id_patients=p.id and StartDateTime=(
    	select max(StartDateTime) from receptions where id_patients=p.id
  	  )
    )
    inner join
  persons per
    on d.id=per.id
    inner join
  persons per1    
    on p.id=per1.id
";
                    List<LastReception> list = new();

                    DateTime start = DateTime.Now;
                    SqlDataReader reader = command.ExecuteReader();
                    TimeSpan readerExecuted = DateTime.Now - start;
                    while (reader.Read())
                    {
                        list.Add(new LastReception
                        {
                            IdDoctor = reader.GetInt32(0),
                            DoctorLastName = reader.GetString(1),
                            IdPatient = reader.GetInt32(2),
                            PatientLastName = reader.GetString(3),
                            StartDayTime = reader.GetDateTime(4),
                        });
                    }
                    reader.Close();
                    Console.WriteLine(
                        $@"2.2) reader executed at {readerExecuted
                        }
done at {DateTime.Now - start
                        }, count rows: {list.Count}"
                    );
                };

                Action query2_3 = () =>
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandTimeout = 600;
                    command.CommandText = @"
select
  d.id as 'id_doc',
  per.lastname as 'doc_lastname', 
  r1.id_patients as 'pat_id',
  per1.lastname as 'pat_lastname',
  r1.StartDateTime
from 
  doctors d
    inner join
  (select r.id_patients, r.id_doctors, r.StartDateTime from 
    (select id_patients, id_doctors, StartDateTime, row_number() over (partition by id_patients ORDER BY StartDateTime desc) as rownumber from Receptions) r
    where r.rownumber=1
  ) r1
    on d.id=r1.id_doctors
    inner join
  persons per
	on d.id=per.id
    inner join
  persons per1	
	on r1.id_patients=per1.id
";
                    List<LastReception> list = new();

                    DateTime start = DateTime.Now;
                    SqlDataReader reader = command.ExecuteReader();
                    TimeSpan readerExecuted = DateTime.Now - start;
                    while (reader.Read())
                    {
                        list.Add(new LastReception
                        {
                            IdDoctor = reader.GetInt32(0),
                            DoctorLastName = reader.GetString(1),
                            IdPatient = reader.GetInt32(2),
                            PatientLastName = reader.GetString(3),
                            StartDayTime = reader.GetDateTime(4),
                        });
                    }
                    reader.Close();
                    Console.WriteLine(
                        $@"2.3) reader executed at {readerExecuted
                        }
done at {DateTime.Now - start
                        }, count rows: {list.Count}"
                    );
                };

                Action query2_4 = () =>
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandTimeout = 600;
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
    on r.id=(select top 1 first_value(id) over (partition by id_patients ORDER BY StartDateTime desc) as id from receptions where id_patients=p.id)
    inner join
  persons per
    on d.id=per.id
    inner join
  persons per1    
    on p.id=per1.id";
                    List<LastReception> list = new();

                    DateTime start = DateTime.Now;
                    SqlDataReader reader = command.ExecuteReader();
                    TimeSpan readerExecuted = DateTime.Now - start;
                    while (reader.Read())
                    {
                        list.Add(new LastReception
                        {
                            IdDoctor = reader.GetInt32(0),
                            DoctorLastName = reader.GetString(1),
                            IdPatient = reader.GetInt32(2),
                            PatientLastName = reader.GetString(3),
                            StartDayTime = reader.GetDateTime(4),
                        });
                    }
                    reader.Close();
                    Console.WriteLine(
                        $@"2.4) reader executed at {readerExecuted
                        }
done at {DateTime.Now - start
                        }, count rows: {list.Count}"
                    );
                };
                query1_1();
                query1_2();
                query2_1();
                query2_2();
                query2_3();
                query2_4();

            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                connection.CloseAsync();
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