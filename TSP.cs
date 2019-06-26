using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TSP
{
    public sealed class TSP
    {
        #region Singleton declaration

        private static TSP instance = new TSP();


        static TSP()
        {

        }

        public static TSP Instance
        {
            get
            {
                return instance;
            }
        }

        private TSP()
        {

        }

        #endregion

        #region Variables declaration 

        static Random RNG;
        string[] lines;
        public string fileName = "";
        public int[,] distances;

        public void PrepareVariables()
        {
            RNG = new Random();
            fileName = "berlin";
            lines = File.ReadAllLines(fileName + ".txt");
            Person.numberOfCities = Convert.ToInt32(TSP.Instance.lines[0]);
            distances = TSP.SetDistances();
        }

        public int numberOfPersons = 40;
        public int tournamentParticipants = 3;
        public int crossingParameter = 5;
        public int mutationParameter = 999995;
        public int mainCounter = 0;
        public int finalIterationNumber = 100000;

        #endregion

        private static int[,] SetDistances()
        {
            int[,] distances = new int[Person.numberOfCities, Person.numberOfCities];
            for (int i = 1; i < Person.numberOfCities + 1; i++)
            {
                int[] line = GetLine(TSP.Instance.lines[i]);
                for (int j = 0; j < line.Length - 1; j++)
                {

                    distances[i - 1, j] = line[j];
                }
            }

            for (int i = 0; i < Person.numberOfCities + 1; i++)
            {
                for (int j = i + 1; j < Person.numberOfCities; j++)
                {
                    distances[i, j] = distances[j, i];
                }
            }

            return distances;
        }

        private static int[] GetLine(string line)
        {
            string[] s = line.Split(' ');
            int[] n = new int[s.Length];
            for (int i = 0; i < s.Length - 1; i++)
            {
                n[i] = Convert.ToInt32(s[i]);
            }
            return n;
        }

        #region TSP algorithm

        public void Iterate(ref List<Person> persons, ref Person best)
        {
            List<Person> parents = new List<Person>();

            parents = TSP.Choose(persons, best);

            parents = TSP.Cross(parents, crossingParameter);

            parents = TSP.Mutate(parents, mutationParameter);

            best = new Person(FindMinimumInsideLoop(parents, best, mainCounter));
            persons = parents;
            mainCounter++;
        }

        #region Selection

        private static List<Person> Choose(List<Person> persons, Person best)
        {
            List<Person> parents = new List<Person>();
            parents = TSP.ChooseTournament(persons);

            return parents;
        }

        private static List<Person> ChooseRoulette(List<Person> persons, Person best)
        {
            List<Person> parents = new List<Person>();
            double ratesSum = 0;
            foreach (Person o in persons)
            {
                ratesSum += o.rate;
            }

            for (int i = 0; i < persons.Count; i++)
            {
                double counter = RNG.Next(99);
                double rate = 0;
                int j = -1;
                double tmp = -1;
                while (counter >= tmp)
                {
                    j++;
                    rate += persons[j].rate;
                    tmp = ((rate / ratesSum) * 100) + 1;
                }
                parents.Add(persons[j]);
            }
            return parents;
        }

        private static List<Person> ChooseTournament(List<Person> persons)
        {
            List<Person> parents = new List<Person>();
            for (int i = 0; i < persons.Count; i++)
            {
                List<Person> tournament = new List<Person>();
                for (int j = 0; j < TSP.Instance.tournamentParticipants; j++)
                {
                    tournament.Add(persons[RNG.Next(persons.Count - 1)]);
                }
                parents.Add(new Person(Instance.FindMinimum(tournament, persons[i])));
            }
            return parents;
        }

        #endregion

        #region Crossing

        private static List<Person> Cross(List<Person> parents, int crossingParameter)
        {
            Person o1 = new Person();
            Person o2 = new Person();

            for (int i = 0; i < parents.Count; i++)
            {
                Person o = new Person();

                if (RNG.Next(100) > crossingParameter)
                {
                    o = CrossPairOX(i, parents);
                }
                else
                {
                    o = parents[i];
                }

                if (i % 2 == 0)
                {
                    o1 = o;
                }
                else
                {
                    o2 = o;
                    parents[i - 1] = o1;
                    parents[i] = o2;
                }
            }

            return parents;
        }

        private static Person CrossPairPMX(int parentIndex, List<Person> parents)
        {
            Person o = new Person();
            for (int i = 0; i < Person.numberOfCities; i++)
            {
                o.route[i] = -1;
            }
            int whichParent = 0;
            if (parentIndex % 2 == 0)
            {
                whichParent = 1;
            }
            else
            {
                whichParent = -1;
            }

            int crossingStart = 0, crossingEnd = 0;
            crossingStart = RNG.Next(Person.numberOfCities - 1);
            crossingEnd = RNG.Next(Person.numberOfCities - 1);

            if (crossingStart > crossingEnd)
            {
                int tmp = crossingEnd;
                crossingEnd = crossingStart;
                crossingStart = tmp;
            }

            for (int i = crossingStart; i <= crossingEnd; i++)
            {
                o.route[i] = parents[parentIndex].route[i];
            }


            for (int i = crossingStart; i <= crossingEnd; i++)
            {
                if (!o.route.Contains(parents[parentIndex + whichParent].route[i]))
                {
                    int assignedValueIndex = Array.IndexOf(parents[parentIndex + whichParent].route, o.route[i]);

                    while (o.route[assignedValueIndex] != -1)
                    {
                        assignedValueIndex = Array.IndexOf(parents[parentIndex + whichParent].route, o.route[assignedValueIndex]);
                    }
                    o.route[assignedValueIndex] = parents[parentIndex + whichParent].route[i];
                }
            }

            for (int i = 0; i < Person.numberOfCities; i++)
            {
                int n = parents[parentIndex + whichParent].route[i];
                if (!o.route.Contains(n))
                {
                    o.route[Array.IndexOf(o.route, -1)] = n;
                }
            }

            return o;
        }

        private static Person CrossPairOX(int parentIndex, List<Person> parents)
        {
            Person o = new Person();
            int whichParent = 0;
            if (parentIndex % 2 == 0)
            {
                whichParent = 1;
            }
            else
            {
                whichParent = -1;
            }

            int crossingStart = 0, crossingEnd = 0;
            crossingStart = RNG.Next(Person.numberOfCities - 1);
            crossingEnd = RNG.Next(Person.numberOfCities - 1);

            if (crossingStart > crossingEnd)
            {
                int tmp = crossingEnd;
                crossingEnd = crossingStart;
                crossingStart = tmp;
            }

            for (int j = crossingStart; j <= crossingEnd; j++)
            {
                o.route[j] = parents[parentIndex].route[j];
            }

            int parentCounter = crossingEnd + 1, personCounter = crossingEnd + 1;

            while (Array.IndexOf(o.route, 0) != Array.LastIndexOf(o.route, 0))
            {
                if (!o.route.Contains(parents[parentIndex + whichParent].route[parentCounter]))
                {
                    o.route[personCounter] = parents[parentIndex + whichParent].route[parentCounter];
                    personCounter++;
                }

                parentCounter++;
                if (parentCounter > Person.numberOfCities - 1)
                {
                    parentCounter = 0;
                }
                if (personCounter > Person.numberOfCities - 1)
                {
                    personCounter = 0;
                }
            }

            return o;
        }

        #endregion

        #region Mutation

        private static List<Person> Mutate(List<Person> parents, int mutationParameter)
        {
            for (int i = 0; i < parents.Count; i++)
            {
                foreach (int j in parents[i].route)
                {
                    if (RNG.Next(1000000) > mutationParameter)
                    {
                        int m = RNG.Next(0, Person.numberOfCities - 1);
                        int tmp = parents[i].route[j];
                        parents[i].route[j] = parents[i].route[m];
                        parents[i].route[m] = tmp;
                    }
                }
            }

            return parents;
        }

        #endregion

        #endregion

        public Person FindMinimum(List<Person> persons, Person best)
        {
            for (int i = 0; i < persons.Count; i++)
            {
                if (best.rate > persons[i].rate)
                {
                    best.rate = persons[i].rate;
                    Array.Copy(persons[i].route, best.route,persons[i].route.Length);
                }
            }
            return best;
        }

        private Person FindMinimumInsideLoop(List<Person> parents, Person best, int mainCounter)
        {
            for (int i = 0; i < parents.Count; i++)
            {
                if (IsRouteCorrect(parents[i]))
                {
                    parents[i].RatePerson(distances);
                    if (best.rate > parents[i].rate)
                    {
                        best = parents[i];
                        StringBuilder iteration = new StringBuilder();
                        iteration.Append("Iteracja: ");
                        iteration.Append(mainCounter.ToString());
                        iteration.Append((mainCounter > 100000) ? "\t" : "\t\t");
                        Console.Write(iteration);
                        Console.Write("Minimum is: " + best.rate + "\t");
                        for (int j = 0; j < best.rate / 1000; j++)
                        {
                            Console.Write("*");
                        }
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("Not working");
                }

            }
            return best;
        }

        #region Summary

        public void Summarize(TimeSpan tz, Person best)
        {
            Console.WriteLine("Execution time in seconds: " + tz.Seconds);

            Console.WriteLine("Iteration count: " + TSP.Instance.mainCounter);

            Console.WriteLine("Best rate is: " + best.rate);

            ReturnCorrectRoute(best);
        }

        private void ReturnCorrectRoute(Person best)
        {
            if (TSP.IsRouteCorrect(best))
            {
                Console.WriteLine("Best found correct route: ");
                TSP.PrintBest(best);
            }
            else
            {
                Console.WriteLine("Found route is incorrect.");
            }
        }

        private static bool IsRouteCorrect(Person best)
        {
            bool result = true;

            for (int i = 0; i < Person.numberOfCities; i++)
            {
                if (!best.route.Contains(i))
                {
                    result = false;
                }
            }
            return result;
        }

        private static void PrintBest(Person best)
        {
            foreach (int i in best.route)
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();
        }
        #endregion
    }
}
