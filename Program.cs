using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class Osobnik
    {
        public int ocena;
        public int[] trasa;

        public Osobnik(int[] n)
        {
            trasa = n;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string[] lines = System.IO.File.ReadAllLines("Berlin.txt");
            int l = Convert.ToInt32(lines[0]);
            int liczbaOsobników = 100;

            Random r = new Random();

            List<Osobnik> osobnicy = new List<Osobnik>();

            long t1 = DateTime.Now.Millisecond;

            for (int i = 0; i < liczbaOsobników; i++)
            {
                List<int> indeksy = new List<int>();
                int[] tab = new int[l];
                for (int k = 0; k < l; k++)
                {
                    indeksy.Add(k);
                }

                for (int j = 0; j < l ; j++)
                { 
                    tab[j] = indeksy[r.Next(indeksy.Count)];
                    indeksy.Remove(tab[j]);
                }
                osobnicy.Add(new Osobnik(tab));
            }

            int[,] distances = new int[l,l];
            for (int i = 1; i < l + 1; i++)
            {
                int[] line = GetLine(lines[i]);
                for(int j = 0; j < line.Length - 1; j++)
                {
                    distances[i - 1, j] = line[j];
                }
            }

            for (int i = 0; i < l - 1; i++)
            {
                for (int j = i + 1; j < l - 1; j++)
                {
                    distances[i, j] = distances[j, i];
                }
            }

            for (int i = 0; i < liczbaOsobników; i++)
            {
                int tmp = 0;
                for (int j = 0; j < l - 1; j++)
                {
                    tmp += distances[osobnicy[i].trasa[j],osobnicy[i].trasa[j + 1]];
                }
                tmp += distances[osobnicy[i].trasa[0],osobnicy[i].trasa[osobnicy[i].trasa.Count() - 1]];
                osobnicy[i].ocena = tmp;
            }

            int min = int.MaxValue;
            for (int i = 0; i < liczbaOsobników; i++)
            {
                if(min > osobnicy[i].ocena)
                {
                    min = osobnicy[i].ocena;
                }
            }

            long t2 = DateTime.Now.Millisecond;

            t2 -= t1;

            Console.WriteLine(t2);

            Console.WriteLine(min);

            Console.ReadKey();
        }

        static int[] GetLine(string line)
        { 
            string[] s = line.Split(' ');
            int[] n = new int[s.Length];
            for(int i = 0; i < s.Length - 1; i++)
            {
                n[i] = Convert.ToInt32(s[i]);
            }
            return n;
        }
    }
}
