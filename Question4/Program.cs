using System;
using System.Collections.Generic;
using System.Linq;

namespace Question4
{
    internal class Program
    {
        private const int s_head = 5;
        private const int s_tail = 5;
        private const int s_count = 50000;
        private static DateTime s_yearLimit = new(2017, 1, 1);
        static void Main(string[] args)
        {

            var rand = new Random();
            var receptions = Enumerable.Range(1, s_count).SelectMany(pid => Enumerable.Range(1, rand.Next(0, 100)).Select(rid => new { PatientId = pid, ReceptionStart = new DateTime(2017, 06, 30).AddDays(-rand.Next(1, 500)) })).ToList();
            var patients = Enumerable.Range(1, s_count).Select(pid => new { Id = pid, Surname = string.Format("Иванов{0}", pid) }).ToList();

            // общая часть: фильтруем по времени приёмы, выбираем уникальные PatientId

            var selectIds = receptions
                .Where(rec => rec.ReceptionStart.CompareTo(s_yearLimit) < 0)
                .Select(rec => rec.PatientId)
                .Distinct();

            //вариант 1: ищем пациентов линейным поиском (считаю этот вариант плохим)

            Console.WriteLine("1)");
            GetResult(() => selectIds
                .Select(patId => patients.Where(pat => pat.Id == patId).FirstOrDefault())
                .Where(pat => pat != null)
                .ToList<object>());

            //вариант 2:  ищем пациентов используя знание, что их Id возрастают на 1, начиная с 1, 
            // то есть выбираем каждого пациента по индексу

            Console.WriteLine("2)");
            GetResult(() => selectIds
                .Select(patId => patients[patId - 1])
                .Where(pat => pat != null)
                .ToList<object>());

            //вариант 3:  ищем пациентов используя словарь, в который их предварительно загрузили (ключ: Id) 

            Console.WriteLine("3)");
            GetResult(() => {
                var dict = patients.ToDictionary(pat => pat.Id);
                return selectIds
                .Select(patId => dict[patId])
                .Where(pat => pat != null)
                .ToList<object>();
            });


        }
        private static void GetResult(Func<List<object>> query)
        {
            DateTime start = DateTime.Now;
            var patients = query();
            TimeSpan elapsed = DateTime.Now - start;
            Console.WriteLine("elapsed: {0}, found: {1}", elapsed, patients.Count);
            Console.WriteLine("first {0}:", s_head);
            foreach (var pat in patients.Take(s_head))
            {
                Console.WriteLine(pat);
            }
            Console.WriteLine("last {0}:", s_tail);
            foreach (var pat in patients.Skip(patients.Count - s_tail))
            {
                Console.WriteLine(pat);
            }
        }
    }
}
