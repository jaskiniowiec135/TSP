using System;
using System.Collections.Generic;

namespace TSP
{
    public class Person
    {
        public int rate;
        public static int numberOfCities;
        public int[] route = new int[numberOfCities];
        static Random r = new Random();

        public Person()
        {

        }

        public Person(Person o)
        {
            this.rate = o.rate;
            this.route = o.route;
        }

        public void FillPerson()
        {
            List<int> indexes = new List<int>();
            int[] tab = new int[numberOfCities];
            for (int k = 0; k < numberOfCities; k++)
            {
                indexes.Add(k);
            }

            for (int j = 0; j < numberOfCities; j++)
            {
                tab[j] = indexes[r.Next(indexes.Count)];
                indexes.Remove(tab[j]);
            }
            route = tab;
        }

        public void RatePerson(int[,] distances)
        {
            rate = 0;
            for (int i = 0; i < route.Length - 1; i++)
            {
                rate += distances[route[i], route[i + 1]];
            }
            rate += distances[route[0], route[route.Length - 1]];
        }
    }
}
