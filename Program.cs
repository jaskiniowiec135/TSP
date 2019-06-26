using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TSP
{
    class Program
    {
        static void Main(string[] args)
        {
            TSP.Instance.PrepareVariables();

            List<Person> persons = new List<Person>();
            Person best = new Person();
            GenerateInitiatePopulation(ref persons);

            best.rate = int.MaxValue;
            best = TSP.Instance.FindMinimum(persons, best);

            long t1 = DateTime.Now.Ticks;

            while (TSP.Instance.finalIterationNumber > TSP.Instance.mainCounter)
            {
                TSP.Instance.Iterate(ref persons, ref best);
            }

            long t2 = DateTime.Now.Ticks;
            TimeSpan tz = new TimeSpan(t2 - t1);
            TSP.Instance.Summarize(tz, best);
            SaveToFile(best);

            Console.ReadKey();
        }

        static void GenerateInitiatePopulation(ref List<Person> persons)
        {
            persons.Clear();
            for (int i = 0; i < TSP.Instance.numberOfPersons; i++)
            {
                persons.Add(new Person());
                persons[i].FillPerson();
                persons[i].RatePerson(TSP.Instance.distances);
            }
        }

        static void SaveToFile(Person best)
        {
            StringBuilder result = new StringBuilder();
            foreach (int i in best.route)
            {
                result.Append(i + "-");
            }
            result.Remove(result.Length - 1, 1);
            result.Append(" " + best.rate);

            string s = result.ToString();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter output = new StreamWriter(Path.Combine(path, TSP.Instance.fileName + "_wynik.txt")))
            {
                output.WriteLine(s);
            }
        }
    }
}
