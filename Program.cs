using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    public class Osobnik
    {
        public int ocena;
        public static int suma;
        public static int liczbaMiast;
        public int[] trasa = new int[liczbaMiast];
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

    public sealed class TSP
    {
        #region Deklaracja singletona

        private static TSP instancja = new TSP();

        static TSP()
        {

        }

        public static TSP Instancja
        {
            get
            {
                return instancja;   
            }
        }

        private TSP()
        {
        }

        #endregion

        #region Deklaracja zmiennych 
        static Random RNG;
        string[] lines;
        public int[,] distances;

        public void PrepareVariables()
        {
            RNG = new Random();
            lines = System.IO.File.ReadAllLines("Berlin.txt");
            Osobnik.liczbaMiast = Convert.ToInt32(TSP.Instancja.lines[0]);
            distances = TSP.UstawOdległości(TSP.Instancja.lines, Osobnik.liczbaMiast);
        }

        public int liczbaOsobników = 40;
        int uczestnicyTurnieju = 3;
        int parametrMutacji = 995;
        public int licznikGłówny = 0;
        public int DocelowaLiczbaIteracji = 200000;

        #endregion

        private static int[,] UstawOdległości(string[] lines, int l)
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

        #region Algorytm TSP

        public void WykonajIterację(ref List<Osobnik> osobnicy, ref Osobnik najlepszy)
        {
            List<Osobnik> rodzice = new List<Osobnik>();

            rodzice = TSP.WybierzTurniej(osobnicy, TSP.Instancja.distances, TSP.Instancja.uczestnicyTurnieju);

            rodzice = TSP.KrzyżujOX(rodzice);

            rodzice = TSP.Mutuj(rodzice, Osobnik.liczbaMiast, TSP.Instancja.parametrMutacji);

            foreach (Osobnik o in rodzice)
            {
                o.OceńOsobnika(TSP.Instancja.distances);
            }

            najlepszy = new Osobnik(TSP.Instancja.ZnajdźMinimumWPętli(osobnicy, najlepszy, TSP.Instancja.licznikGłówny));
            osobnicy = rodzice;
            TSP.Instancja.licznikGłówny++;
        }

        private static List<Osobnik> WybierzRuletka(List<Osobnik> osobnicy)
        {
            List<Osobnik> rodzice = new List<Osobnik>();
            int sumaOcen = 0;
            foreach (Osobnik o in osobnicy)
            {
                sumaOcen += o.ocena;
            }
            for (int i = 0; i < osobnicy.Count; i++)
            {
                int licznik = RNG.Next(100);
                int ocena = 0;
                int j = -1;
                while (licznik >= (ocena * 100) / sumaOcen && j < osobnicy.Count - 1)
                {
                    j++;
                    ocena += osobnicy[j].ocena;
                }
                rodzice.Add(osobnicy[j]);
            }

            return rodzice;
        }

        private static List<Osobnik> WybierzTurniej(List<Osobnik> osobnicy, int[,] distances, int uczestnicyTurnieju)
        {
            List<Osobnik> rodzice = new List<Osobnik>();
            for (int i = 0; i < osobnicy.Count; i++)
            {
                List<Osobnik> turniej = new List<Osobnik>();
                for (int j = 0; j < uczestnicyTurnieju; j++)
                {
                    turniej.Add(osobnicy[RNG.Next(osobnicy.Count - 1)]);
                }

                foreach (Osobnik o in turniej)
                {
                    o.OceńOsobnika(distances);
                }

                rodzice.Add(new Osobnik(Instancja.ZnajdźMinimum(turniej, osobnicy[i])));
            }

            return rodzice;
        }

        private static List<Osobnik> KrzyżujOX(List<Osobnik> rodzice)
        {
            Osobnik o1 = new Osobnik();
            Osobnik o2 = new Osobnik();

            for (int i = 0; i < rodzice.Count - 1; i++)
            {
                Osobnik o = KrzyżujParęOX(i, rodzice);

                if (i % 2 == 0)
                {
                    o1 = o;
                }
                else
                {
                    o2 = o;
                    rodzice[i - 1] = o1;
                    rodzice[i] = o2;
                    o1 = new Osobnik();
                    o2 = new Osobnik();
                }
            }

            return rodzice;
        }

        private static Osobnik KrzyżujParęOX(int indeksRodziców, List<Osobnik> rodzice)
        {
            Osobnik o = new Osobnik();
            int indeksKrzyżowania = 0;
            indeksKrzyżowania = RNG.Next(Osobnik.liczbaMiast / 2);
            for (int j = indeksKrzyżowania; j < indeksKrzyżowania + Osobnik.liczbaMiast / 2; j++)
            {
                o.trasa[j] = rodzice[indeksRodziców].trasa[j];
            }
            for (int j = indeksKrzyżowania; j < indeksKrzyżowania + Osobnik.liczbaMiast / 2; j++)
            {
                if (Array.IndexOf(o.trasa, rodzice[indeksRodziców + 1].trasa[j]) == -1)
                {
                    int indeksPrzepisanejWartości = Array.IndexOf(rodzice[indeksRodziców + 1].trasa, o.trasa[j]);
                    while (o.trasa[indeksPrzepisanejWartości] != 0)
                    {
                        indeksPrzepisanejWartości = Array.IndexOf(rodzice[indeksRodziców + 1].trasa, o.trasa[indeksPrzepisanejWartości]);
                    }
                    o.trasa[indeksPrzepisanejWartości] = rodzice[indeksRodziców + 1].trasa[j];
                }
            }
            for (int j = 0; j < Osobnik.liczbaMiast; j++)
            {
                if (Array.IndexOf(o.trasa, rodzice[indeksRodziców + 1].trasa[j]) == -1)
                {
                    o.trasa[Array.IndexOf(o.trasa, 0)] = rodzice[indeksRodziców + 1].trasa[j];
                }
            }

            return o;
        }

        private static List<Osobnik> Mutuj(List<Osobnik> rodzice, int liczbaMiast, int parametrMutacji)
        {
            for (int i = 0; i < rodzice.Count; i++)
            {
                foreach (int j in rodzice[i].trasa)
                {
                    if (RNG.Next(1000) > parametrMutacji)
                    {
                        int m = RNG.Next(liczbaMiast - 1);
                        if (m == j)
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

        #endregion

        public Osobnik ZnajdźMinimum(List<Osobnik> osobnicy, Osobnik najlepszy)
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

        private Osobnik ZnajdźMinimumWPętli(List<Osobnik> rodzice, Osobnik najlepszy, int licznikGłówny)
        {
            for (int i = 0; i < rodzice.Count; i++)
            {
                if (najlepszy.ocena > rodzice[i].ocena)
                {
                    najlepszy = rodzice[i];
                    StringBuilder iteracja = new StringBuilder();
                    iteracja.Append("Iteracja: ");
                    iteracja.Append(licznikGłówny.ToString());
                    iteracja.Append((licznikGłówny > 100000) ? "\t" : "\t\t");
                    Console.Write(iteracja);
                    //Console.Write("Iteracja: " + licznikGłówny + "\t");
                    Console.Write("Minimum wynosi: " + najlepszy.ocena + "\t");
                    for (int j = 0; j < najlepszy.ocena / 1000; j++)
                    {
                        Console.Write("*");
                    }
                    Console.WriteLine();
                }
            }
            return najlepszy;
        }

        #region Podsumowanie

        public void Podsumuj(TimeSpan tz, Osobnik najlepszy)
        {
            Console.WriteLine("Czas wykonania w sekundach: " + tz.Seconds);

            Console.WriteLine("Liczba iteracji: " + TSP.Instancja.licznikGłówny);

            Console.WriteLine("Minimmalna ocena wynosi: " + najlepszy.ocena);

            ZwróćPoprawnąTrasę(najlepszy);
        }

        private void ZwróćPoprawnąTrasę(Osobnik najlepszy)
        {
            if (TSP.SprawdźPoprawnośćTrasy(najlepszy))
            {
                Console.WriteLine("Najlepsza znaleziona poprawna trasa: ");
                TSP.WyświetlNajlepszego(najlepszy);
            }
            else
            {
                Console.WriteLine("Znaleziona trasa nie jest poprawna.");
            }
        }

        private static bool SprawdźPoprawnośćTrasy(Osobnik najlepszy)
        {
            bool result = true;

            for (int i = 0; i < najlepszy.trasa.Length; i++)
            {
                if (!najlepszy.trasa.Contains(i))
                {
                    result = false;
                }
            }

            return result;
        }

        private static void WyświetlNajlepszego(Osobnik najlepszy)
        {
            foreach (int i in najlepszy.trasa)
            {
                Console.Write(najlepszy.trasa[i] + " ");
            }
            Console.WriteLine();
        }

        #endregion
    }

    class Program
    {
        static void Main(string[] args)
        {
            TSP.Instancja.PrepareVariables();

            List<Osobnik> osobnicy = new List<Osobnik>();
            Osobnik najlepszy = new Osobnik();

            WygenerujPopulacjęPoczątkową(ref osobnicy);

            najlepszy.ocena = int.MaxValue;
            najlepszy =  TSP.Instancja.ZnajdźMinimum(osobnicy,najlepszy);

            Console.WriteLine("Minimum w populacji początkowej wynosi: " + najlepszy.ocena);

            long t1 = DateTime.Now.Ticks;

            while (TSP.Instancja.DocelowaLiczbaIteracji > TSP.Instancja.licznikGłówny)
            {
                TSP.Instancja.WykonajIterację(ref osobnicy, ref najlepszy);
            }

            TimeSpan tz = new TimeSpan(DateTime.Now.Ticks - t1);

            TSP.Instancja.Podsumuj(tz, najlepszy);

            Console.ReadKey();
        }

        static void WygenerujPopulacjęPoczątkową(ref List<Osobnik> osobnicy)
        {
            for (int i = 0; i < TSP.Instancja.liczbaOsobników; i++)
            {
                osobnicy.Add(new Osobnik());
                osobnicy[i].WypełnijOsobnika(Osobnik.liczbaMiast);
                osobnicy[i].OceńOsobnika(TSP.Instancja.distances);
            }
        }
    }
}
