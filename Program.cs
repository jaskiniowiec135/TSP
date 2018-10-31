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
        public static int suma;
        public int id;
        static Random r = new Random();

        public Osobnik()
        {

        }

        public Osobnik(Osobnik o)
        {
            this.id = o.id;
            this.ocena = o.ocena;
            this.trasa = o.trasa;
        }

        public void WypełnijOsobnika(int l)
        {
            id = suma;
            suma++;
            List<int> indeksy = new List<int>();
            int[] tab = new int[l];
            for (int k = 0; k < l; k++)
            {
                indeksy.Add(k);
            }

            for (int j = 0; j < l; j++)
            {
                tab[j] = indeksy[r.Next(indeksy.Count)];
                indeksy.Remove(tab[j]);
            }
            trasa = tab;
        }
        
        public void OceńOsobnika(int[,] distances)
        {
            ocena = 0;

            for (int i = 0; i < trasa.Length - 1; i++)
            {
                ocena += distances[trasa[i], trasa[i + 1]];
            }
            ocena += distances[trasa[0], trasa[trasa.Length - 1]];
        }
    }

    class Program
    {
        static readonly int liczbaOsobników = 100;
        static readonly int DocelowaLiczbaIteracji = 100000;
        static int licznikGłówny = 0;
        static int liczbaMiast = 0;
        static readonly int parametrMutacji = 90;
        static int parametrKrzyżowania = 0;
        static readonly int uczestnicyTurnieju = 5;

        static void Main(string[] args)
        {
            string[] lines = System.IO.File.ReadAllLines("Berlin.txt");
            liczbaMiast = Convert.ToInt32(lines[0]);
            parametrKrzyżowania = liczbaMiast / 2;
            List<Osobnik> osobnicy = new List<Osobnik>();
            Osobnik najlepszy = new Osobnik();

            int[,] distances = UstawOdległości(lines, liczbaMiast);

            for (int i = 0; i < liczbaOsobników; i++)
            {
                osobnicy.Add(new Osobnik());
                osobnicy[i].WypełnijOsobnika(liczbaMiast);
                osobnicy[i].OceńOsobnika(distances);
            }

            najlepszy.ocena = int.MaxValue;
            najlepszy = new Osobnik(ZnajdźMinimum(osobnicy,najlepszy));

            Console.WriteLine("Minimum w populacji początkowej wynosi: " + najlepszy.ocena);

            long t1 = DateTime.Now.Ticks;

            while(DocelowaLiczbaIteracji > licznikGłówny)
            {
                licznikGłówny++;
                Random r = new Random();
                List<Osobnik> rodzice = new List<Osobnik>();

                rodzice = WybierzRuletka(osobnicy);

                rodzice = Krzyżuj(rodzice);

                rodzice = Mutuj(rodzice);
                
                foreach(Osobnik o in rodzice)
                {
                    o.OceńOsobnika(distances);
                }

                najlepszy = new Osobnik(ZnajdźMinimum(osobnicy, najlepszy, licznikGłówny));

                osobnicy = rodzice;
            }

            TimeSpan tz = new TimeSpan(DateTime.Now.Ticks - t1);

            Console.WriteLine("Czas wykonania w sekundach: " + tz.Seconds);

            Console.WriteLine("Liczba iteracji: " + licznikGłówny);
            
            Console.WriteLine("Minimmalna ocena wynosi: " + najlepszy.ocena);

            if(SprawdźPoprawnośćTrasy(najlepszy))
            {
                Console.WriteLine("Najlepsza znaleziona poprawna trasa: ");
                WyświetlNajlepszego(najlepszy);
            }
            Console.ReadKey();
        }

        static List<Osobnik> WybierzRuletka(List<Osobnik> osobnicy)
        {
            List<Osobnik> rodzice = new List<Osobnik>();
            Random r = new Random();
            int sumaOcen = 0;
            foreach (Osobnik o in osobnicy)
            {
                sumaOcen += o.ocena;
            }
            for (int i = 0; i < liczbaOsobników; i++)
            {
                int licznik = r.Next(100);
                int ocena = 0;
                int j = -1;
                while (licznik >= (ocena * 100) / sumaOcen && j < liczbaOsobników - 1)
                {
                    j++;
                    ocena += osobnicy[j].ocena;
                }
                rodzice.Add(osobnicy[j]);
            }

            return rodzice;
        }

        static List<Osobnik> WybierzTurniej(List<Osobnik> osobnicy)
        {
            List<Osobnik> rodzice = new List<Osobnik>();
            Random r = new Random();
            for (int i = 0; i < liczbaOsobników; i++)
            {
                List<Osobnik> turniej = new List<Osobnik>();
                for (int j = 0; j < uczestnicyTurnieju; j++)
                {
                    turniej.Add(osobnicy[r.Next(liczbaOsobników - 1)]);
                }
            }

            return rodzice;
        }

        static List<Osobnik> Krzyżuj(List<Osobnik> rodzice)
        {
            for (int i = 0; i < liczbaOsobników;)
            {
                int tmp = 0;

                for (int j = 0; j < parametrKrzyżowania; j++)
                {
                    tmp = rodzice[i].trasa[j];
                    rodzice[i].trasa[j] = rodzice[i + 1].trasa[j];
                    rodzice[i + 1].trasa[j] = tmp;
                }
                i += 2;
            }

            return rodzice;
        }

        static List<Osobnik> Mutuj(List<Osobnik> rodzice)
        {
            Random r = new Random();
            for (int i = 0; i < liczbaOsobników; i++)
            {
                foreach (int j in rodzice[i].trasa)
                {
                    if (r.Next(100) > parametrMutacji)
                    {
                        int m = r.Next(liczbaMiast - 1);
                        if(m == j)
                        {
                            int tmp = rodzice[i].trasa[j];
                            rodzice[i].trasa[j] = rodzice[i].trasa[0];
                            rodzice[i].trasa[0] = tmp;
                        }
                        else
                        {
                            int tmp = rodzice[i].trasa[j];
                            rodzice[i].trasa[j] = rodzice[i].trasa[m];
                            rodzice[i].trasa[m] = tmp;
                        }
                    }
                }
            }
            return rodzice;
        }

        static Osobnik ZnajdźMinimum(List<Osobnik> osobnicy, Osobnik najlepszy)
        {
            for (int i = 0; i < osobnicy.Count - 1; i++)
            {
                if (najlepszy.ocena > osobnicy[i].ocena)
                {
                    najlepszy = osobnicy[i];
                }
            }

            return najlepszy;
        }

        static Osobnik ZnajdźMinimum(List<Osobnik> rodzice,Osobnik najlepszy, int licznikGłówny)
        {
            for (int i = 0; i < rodzice.Count; i++)
            {
                if (najlepszy.ocena > rodzice[i].ocena)
                {
                    najlepszy = rodzice[i];
                    Console.WriteLine("Iteracja: " + licznikGłówny);
                    Console.WriteLine("Minimum wynosi: " + najlepszy.ocena);
                    for (int j = 0; j < najlepszy.ocena/1000; j++)
                    {
                        Console.Write("*");
                    }
                    Console.WriteLine();
                }
            }
            return najlepszy;
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

        static void WyświetlNajlepszego(Osobnik najlepszy)
        {
            foreach(int i in najlepszy.trasa)
            {
                Console.Write(najlepszy.trasa[i] + " ");
            }
            Console.WriteLine();
        }

        static int[,] UstawOdległości(string[] lines,int l)
        {
            int[,] distances = new int[l, l];
            for (int i = 1; i < l + 1; i++)
            {
                int[] line = GetLine(lines[i]);
                for (int j = 0; j < line.Length - 1; j++)
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
            return distances;
        }

        static bool SprawdźPoprawnośćTrasy(Osobnik najlepszy)
        {
            bool result = true;

            for (int i = 0; i < najlepszy.trasa.Length; i++)
            {
                if(!najlepszy.trasa.Contains(i))
                {
                    result = false;
                }
            }

            return result;
        }
    }
}
