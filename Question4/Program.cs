using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

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
            //Сложность по времени: O(R) + O(P * log P) (линейный поиск по приёмам, сортировка PatientId)
            var selectIds = receptions
                .Where(rec => rec.ReceptionStart.CompareTo(s_yearLimit) < 0)
                .Select(rec => rec.PatientId)
                .Distinct();

            //вариант 1: по каждому Id ищем пациента линейным поиском. Нет ограничений на порядок и непрерывность Id пациентов.
            //Сложность по времени: O(R) + O(P * P)
            //Дополнительная память: O(1)

            Console.WriteLine("1)");
            GetResult(() => selectIds
                .Select(patId => patients.Where(pat => pat.Id == patId).FirstOrDefault())
                .Where(pat => pat != null)
                .ToList<object>());

            //вариант 2:  ищем пациентов используя знание, что их Id возрастают на 1, начиная с 1, 
            // то есть выбираем каждого пациента по индексу
            //Сложность по времени: O(R) + O(P * log P)
            //Дополнительная память: O(1)

            Console.WriteLine("2)");
            GetResult(() => selectIds
                .Select(patId => patients[patId - 1])
                .Where(pat => pat != null)
                .ToList<object>());

            //вариант 3: ищем пациентов используя словарь, в который их предварительно загрузили (ключ: Id) 
            //Сложность по времени:  O(R) + O(P * log P)
            //Дополнительная память: O(P)

            Console.WriteLine("3)");
            GetResult(() => {
                var dict = patients.ToDictionary(pat => pat.Id);
                return selectIds
                .Select(patId => dict[patId])
                .Where(pat => pat != null)
                .ToList<object>();
            });

            //вариант 4:  предполагаем, что пациенты предварительно отсортированы, как это следует из условия, но непрерывность Id не гарантирована.
            //Пациентов находим бинарным поиском
            //Сложность по времени: O(P * log P)
            //Дополнительная память: O(1)

            Console.WriteLine("4)");
            GetResult(() => {
                return selectIds
                .Select(patId => {
                    var l = 0;
                    var r = patients.Count - 1;
                    while(l < r)
                    {
                        int m = (l + r) / 2;
                        if (patients[m].Id < patId)
                        {
                            l = m + 1;
                        }
                        else
                        {
                            r = m;
                        }
                    }
                    return patients[l].Id == patId ? patients[l] : null;
                })
                .Where(pat => pat != null)
                .ToList<object>();
            });

            //вариант 5:  не предполагаем, что пациенты предварительно отсортированы, поэтому сначала сортируем. Непрерывность Id не гарантирована.
            //Пациентов находим бинарным поиском
            //Сложность по времени: O(P * log P)
            //Дополнительная память: O(P)

            Console.WriteLine("5)");
            GetResult(() => {
                var patients1 = patients.OrderBy(p => p.Id).ToList();
                return selectIds
                .Select(patId => {
                    var l = 0;
                    var r = patients1.Count - 1;
                    while (l < r)
                    {
                        int m = (l + r) / 2;
                        if (patients1[m].Id < patId)
                        {
                            l = m + 1;
                        }
                        else
                        {
                            r = m;
                        }
                    }
                    return patients1[l].Id == patId ? patients1[l] : null;
                })
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
