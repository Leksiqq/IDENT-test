using System;
using System.Collections.Generic;
using System.Linq;

namespace Question4
{
    internal class Program
    {
        private const int s_count = 500000;
        private const int passesCount = 5;
        private const int warmingPassesCount = 2;
        private static DateTime s_yearLimit = new(2017, 1, 1);
        static void Main(string[] args)
        {

            var rand = new Random();
            var receptions = Enumerable.Range(1, s_count).SelectMany(pid => Enumerable.Range(1, rand.Next(0, 100)).Select(rid => new { PatientId = pid, ReceptionStart = new DateTime(2017, 06, 30).AddDays(-rand.Next(1, 500)) })).ToList();
            var patients = Enumerable.Range(1, s_count).Select(pid => new { Id = pid, Surname = string.Format("Иванов{0}", pid) }).ToList();

            //Общая часть для вариантов 1-5: фильтруем по времени приёмы, выбираем уникальные PatientId
            //Сложность по времени: O(R) (линейный поиск по приёмам)
            //Дополнительная память: O(P) (думаю, что Distinct() сделан через HashSet)
            var selectIds = receptions
                .Where(rec => rec.ReceptionStart.CompareTo(s_yearLimit) < 0)
                .Select(rec => rec.PatientId)
                .Distinct();

            List<TimeSpan> elapsedSums = Enumerable.Range(0, 6).Select(i => TimeSpan.Zero).ToList();
            for (int i = 0; i < warmingPassesCount + passesCount; ++i)
            {
                Console.WriteLine("pass: {0} {1}", i + 1, i < warmingPassesCount ? "warming" : string.Empty);
                //вариант 1: по каждому Id ищем пациента линейным поиском. Нет ограничений на порядок и непрерывность Id пациентов.
                //Сложность по времени: O(R) + O(P * P)
                //Дополнительная память: O(P)

                Console.WriteLine("1) too long");
                //GetResult(() => selectIds
                //    .Select(patId => patients.Where(pat => pat.Id == patId).FirstOrDefault())
                //    .Where(pat => pat != null)
                //    .ToList<object>(), i < warmingPassesCount ? null : elapsedSums, 0);

                //вариант 2:  ищем пациентов используя знание, что их Id возрастают на 1, начиная с 1, 
                // то есть выбираем каждого пациента по индексу
                //Сложность по времени: O(R) + O(P)
                //Дополнительная память: O(P)

                Console.Write("2) ");
                GetResult(() => selectIds
                    .Select(patId => patients[patId - 1])
                    .Where(pat => pat != null)
                    .ToList<object>(), i < warmingPassesCount ? null : elapsedSums, 1);

                //вариант 3: ищем пациентов используя словарь, в который их предварительно загрузили (ключ: Id) 
                //Сложность по времени:  O(R) + O(P)
                //Дополнительная память: O(P)

                Console.Write("3) ");
                GetResult(() =>
                {
                    var dict = patients.ToDictionary(pat => pat.Id);
                    return selectIds
                    .Select(patId => dict[patId])
                    .Where(pat => pat != null)
                    .ToList<object>();
                }, i < warmingPassesCount ? null : elapsedSums, 2);

                //вариант 4:  предполагаем, что пациенты предварительно отсортированы, как это следует из условия,
                //но непрерывность Id не гарантирована.
                //Пациентов находим бинарным поиском
                //Сложность по времени: O(R) + O(P * log P)
                //Дополнительная память: O(P)

                Console.Write("4) ");
                GetResult(() =>
                {
                    return selectIds
                    .Select(patId =>
                    {
                        var l = 0;
                        var r = patients.Count - 1;
                        while (l < r)
                        {
                            var m = (l + r) / 2;
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
                }, i < warmingPassesCount ? null : elapsedSums, 3);

                //вариант 5:  не предполагаем, что пациенты предварительно отсортированы, поэтому сначала сортируем. Непрерывность Id не гарантирована.
                //Пациентов находим бинарным поиском
                //Сложность по времени: O(R) + O(P * log P)
                //Дополнительная память: O(P)

                Console.Write("5) ");
                GetResult(() =>
                {
                    var patients1 = patients.OrderBy(p => p.Id).ToList();
                    return selectIds
                    .Select(patId =>
                    {
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
                }, i < warmingPassesCount ? null : elapsedSums, 4);

                // Вариант 6: полагая, что Distinct() сделан через HashSet, реализуем Distinct() сами 
                // через HashSet, который теперь в наших руках. Проходим по клиентам и выбираем тех, которые в нём.
                Console.Write("6) ");
                GetResult(() =>
                {
                    HashSet<int> hs = new();
                    foreach (var rec in receptions.Where(rec => rec.ReceptionStart.CompareTo(s_yearLimit) < 0))
                    {
                        hs.Add(rec.PatientId);
                    }
                    List<object> ans = new();
                    foreach (var pat in patients)
                    {
                        if (hs.Contains(pat.Id))
                        {
                            ans.Add(pat);
                        }
                    }
                    return ans;
                }, i < warmingPassesCount ? null : elapsedSums, 5);
            }
            Console.WriteLine("avg elapsed:");
            for(int i = 1; i < 6; ++i)
            {
                Console.WriteLine("{0}) {1}", i + 1, elapsedSums[i] / passesCount);
            }
        }
        private static void GetResult(Func<List<object>> query, List<TimeSpan> elapsedSums, int pos)
        {
            DateTime start = DateTime.Now;
            var patients = query();
            TimeSpan elapsed = DateTime.Now - start;
            Console.WriteLine("elapsed: {0}, found: {1}", elapsed, patients.Count);
            if(elapsedSums != null)
            {
                elapsedSums[pos] += elapsed;
            }
        }
    }
}
