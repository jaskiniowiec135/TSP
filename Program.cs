using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TSP
{
    public class Osobnik
    {
        public int ocena;
        public static int liczbaMiast;
        public int[] trasa = new int[liczbaMiast];
        static Random r = new Random();

        public Osobnik()
        {

        }

        public Osobnik(Osobnik o)
        {
            this.ocena = o.ocena;
            this.trasa = o.trasa;
        }

        public void WypełnijOsobnika()
        {
            List<int> indeksy = new List<int>();
            int[] tab = new int[liczbaMiast];
            for (int k = 0; k < liczbaMiast; k++)
            {
                indeksy.Add(k);
            }

            for (int j = 0; j < liczbaMiast; j++)
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
            lines = File.ReadAllLines("Berlin.txt");
            Osobnik.liczbaMiast = Convert.ToInt32(TSP.Instancja.lines[0]);
            distances = TSP.UstawOdległości();
        }

        public int liczbaOsobników = 40;
        public int uczestnicyTurnieju = 3;
        public int parametrKrzyżowania = 15;
        public int parametrMutacji = 985;
        public int licznikGłówny = 0;
        public int DocelowaLiczbaIteracji = 10000;

        #endregion

        private static int[,] UstawOdległości()
        {
            int[,] distances = new int[Osobnik.liczbaMiast, Osobnik.liczbaMiast];
            for (int i = 1; i < Osobnik.liczbaMiast + 1; i++)
            {
                int[] line = GetLine(TSP.Instancja.lines[i]);
                for (int j = 0; j < line.Length - 1; j++)
                {
                    distances[i - 1, j] = line[j];
                }
            }

            for (int i = 0; i < Osobnik.liczbaMiast + 1; i++)
            {
                for (int j = i + 1; j < Osobnik.liczbaMiast; j++)
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

            rodzice = TSP.Wybierz(osobnicy, najlepszy);

            rodzice = TSP.Krzyżuj(rodzice, parametrKrzyżowania);

            rodzice = TSP.Mutuj(rodzice, parametrMutacji);

            najlepszy = new Osobnik(ZnajdźMinimumWPętli(rodzice, najlepszy, licznikGłówny));
            osobnicy = rodzice;
            licznikGłówny++;
        }

        #region Selekcja

        private static List<Osobnik> Wybierz(List<Osobnik> osobnicy, Osobnik najlepszy)
        {
            List<Osobnik> rodzice = new List<Osobnik>();

            if(TSP.Instancja.licznikGłówny % 500 == 0)
            {
                rodzice = TSP.WybierzRuletka(osobnicy, najlepszy);
            }
            else
            {
                rodzice = TSP.WybierzTurniej(osobnicy);
            }

            return rodzice;
        }

        private static List<Osobnik> WybierzRuletka(List<Osobnik> osobnicy, Osobnik najlepszy)
        {
            List<Osobnik> rodzice = new List<Osobnik>();
            double sumaOcen = 0;
            foreach(Osobnik o in osobnicy)
            {
                sumaOcen += o.ocena;
            }
            
            for (int i = 0; i < osobnicy.Count; i++)
            {
                double licznik = RNG.Next(99);
                double ocena = 0;
                int j = -1;
                double tmp = -1;
                while (licznik >= tmp)
                {
                    j++;
                    ocena += osobnicy[j].ocena;
                    tmp = ((ocena / sumaOcen)*100) + 1;
                    //tmp = Math.Round(100 - (((sumaOcen - ocena + 1) / sumaOcen) * 100));
                    // TODO
                }
                rodzice.Add(osobnicy[j]);
            }
            return rodzice;
        }

        private static List<Osobnik> WybierzTurniej(List<Osobnik> osobnicy)
        {
            List<Osobnik> rodzice = new List<Osobnik>();
            for (int i = 0; i < osobnicy.Count; i++)
            {
                List<Osobnik> turniej = new List<Osobnik>();
                for (int j = 0; j < TSP.Instancja.uczestnicyTurnieju; j++)
                {
                    turniej.Add(osobnicy[RNG.Next(osobnicy.Count - 1)]);
                }
                rodzice.Add(new Osobnik(Instancja.ZnajdźMinimum(turniej, osobnicy[i])));
            }
            return rodzice;
        }

        #endregion

        #region Krzyżowanie

        private static List<Osobnik> Krzyżuj(List<Osobnik> rodzice, int parametrKrzyżowania)
        {
            Osobnik o1 = new Osobnik();
            Osobnik o2 = new Osobnik();

            for (int i = 0; i < rodzice.Count; i++)
            {
                Osobnik o = new Osobnik();

                if (RNG.Next(100) > parametrKrzyżowania)
                {
                    if (TSP.Instancja.licznikGłówny % 500 == 0)
                    {
                        o = KrzyżujParęPMX(i, rodzice);
                    }
                    else
                    {
                        o = KrzyżujParęOX(i, rodzice);
                    }
                }
                else
                {
                    o = rodzice[i];
                }

                if (i % 2 == 0)
                {
                    o1 = o;
                }
                else
                {
                    o2 = o;
                    rodzice[i - 1] = o1;
                    rodzice[i] = o2;
                }
            }

            return rodzice;
        }

        private static Osobnik KrzyżujParęPMX(int indeksRodziców, List<Osobnik> rodzice)
        {
            Osobnik o = new Osobnik();
            for (int i = 0; i < Osobnik.liczbaMiast; i++)
            {
                o.trasa[i] = -1;
            }
            int któryRodzic = 0;
            if (indeksRodziców % 2 == 0)
            {
                któryRodzic = 1;
            }
            else
            {
                któryRodzic = -1;
            }

            int początekKrzyżowania = 0, koniecKrzyżowania = 0;
            początekKrzyżowania = RNG.Next(Osobnik.liczbaMiast - 1);
            koniecKrzyżowania = RNG.Next(Osobnik.liczbaMiast - 1);

            if (początekKrzyżowania > koniecKrzyżowania)
            {
                int tmp = koniecKrzyżowania;
                koniecKrzyżowania = początekKrzyżowania;
                początekKrzyżowania = tmp;
            }

            for (int i = początekKrzyżowania; i <= koniecKrzyżowania; i++)
            {
                o.trasa[i] = rodzice[indeksRodziców].trasa[i];
            }
            

            for(int i = początekKrzyżowania; i <= koniecKrzyżowania; i++)
            {
                if(!o.trasa.Contains(rodzice[indeksRodziców + któryRodzic].trasa[i]))
                {
                    int indeksPrzypisanejWartości = Array.IndexOf(rodzice[indeksRodziców + któryRodzic].trasa, o.trasa[i]);
                    
                    while (o.trasa[indeksPrzypisanejWartości] != -1)
                    {
                        indeksPrzypisanejWartości = Array.IndexOf(rodzice[indeksRodziców + któryRodzic].trasa, o.trasa[indeksPrzypisanejWartości]);
                        //int tmp = Array.IndexOf(rodzice[indeksRodziców + któryRodzic].trasa, o.trasa[indeksPrzypisanejWartości]);
                        //indeksPrzypisanejWartości = Array.IndexOf(rodzice[indeksRodziców + któryRodzic].trasa, tmp);
                        // TODO do poprawy generacja populacji bazowej
                    }
                    o.trasa[indeksPrzypisanejWartości] = rodzice[indeksRodziców + któryRodzic].trasa[i];
                }                
            }

            for (int i = 0; i < Osobnik.liczbaMiast; i++)
            {
                int n = rodzice[indeksRodziców + któryRodzic].trasa[i];
                if(!o.trasa.Contains(n))
                {
                    o.trasa[Array.IndexOf(o.trasa, -1)] = n;
                }
            }

            return o;
        }

        private static Osobnik KrzyżujParęOX(int indeksRodziców, List<Osobnik> rodzice)
        {
            Osobnik o = new Osobnik();
            int któryRodzic = 0;
            if(indeksRodziców%2==0)
            {
                któryRodzic = 1;
            }
            else
            {
                któryRodzic = -1;
            }

            int początekKrzyżowania = 0, koniecKrzyżowania = 0;
            początekKrzyżowania = RNG.Next(Osobnik.liczbaMiast - 1);
            koniecKrzyżowania = RNG.Next(Osobnik.liczbaMiast - 1);

            if(początekKrzyżowania > koniecKrzyżowania)
            {
                int tmp = koniecKrzyżowania;
                koniecKrzyżowania = początekKrzyżowania;
                początekKrzyżowania = tmp;
            }

            for (int j = początekKrzyżowania; j <= koniecKrzyżowania; j++)
            {
                o.trasa[j] = rodzice[indeksRodziców].trasa[j];
            }

            int licznikRodzica = koniecKrzyżowania + 1, licznikOsobnika = koniecKrzyżowania + 1;

            while(Array.IndexOf(o.trasa,0) != Array.LastIndexOf(o.trasa,0))
            {
                if (!o.trasa.Contains(rodzice[indeksRodziców + któryRodzic].trasa[licznikRodzica]))
                {
                    o.trasa[licznikOsobnika] = rodzice[indeksRodziców + któryRodzic].trasa[licznikRodzica];
                    licznikOsobnika++;
                }

                licznikRodzica++;
                if(licznikRodzica > Osobnik.liczbaMiast - 1)
                {
                    licznikRodzica = 0;
                }
                if(licznikOsobnika > Osobnik.liczbaMiast - 1)
                {
                    licznikOsobnika = 0;
                }
            }

            return o;
        }

        #endregion

        #region Mutacja

        private static List<Osobnik> Mutuj(List<Osobnik> rodzice, int parametrMutacji)
        {
            if(TSP.Instancja.licznikGłówny % 1000 == 0)
            {
                TSP.Instancja.parametrMutacji += 1;
            }
            for (int i = 0; i < rodzice.Count; i++)
            {
                foreach (int j in rodzice[i].trasa)
                {
                    if (RNG.Next(1000) > parametrMutacji)
                    {
                        int m = RNG.Next(0, Osobnik.liczbaMiast - 1);
                        int tmp = rodzice[i].trasa[j];
                        rodzice[i].trasa[j] = rodzice[i].trasa[m];
                        rodzice[i].trasa[m] = tmp;
                    }
                }
            }
            return rodzice;
        }

        #endregion

        #endregion

        public Osobnik ZnajdźMinimum(List<Osobnik> osobnicy, Osobnik najlepszy)
        {
            for (int i = 0; i < osobnicy.Count; i++)
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
                rodzice[i].OceńOsobnika(distances);
                if (najlepszy.ocena > rodzice[i].ocena)
                {
                    najlepszy = rodzice[i];
                    StringBuilder iteracja = new StringBuilder();
                    iteracja.Append("Iteracja: ");
                    iteracja.Append(licznikGłówny.ToString());
                    iteracja.Append((licznikGłówny > 100000) ? "\t" : "\t\t");
                    Console.Write(iteracja);
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

            for (int i = 0; i < Osobnik.liczbaMiast; i++)
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
            foreach(int i in najlepszy.trasa)
            {
                Console.Write(i + " ");
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
            najlepszy = TSP.Instancja.ZnajdźMinimum(osobnicy, najlepszy);

            long t1 = DateTime.Now.Ticks;

            while (TSP.Instancja.DocelowaLiczbaIteracji > TSP.Instancja.licznikGłówny)
            {
                TSP.Instancja.WykonajIterację(ref osobnicy, ref najlepszy);
            }

            TimeSpan tz = new TimeSpan(DateTime.Now.Ticks - t1);
            TSP.Instancja.Podsumuj(tz, najlepszy);
            ZapiszDoPliku(najlepszy);

            Console.ReadKey();
        }

        static void WygenerujPopulacjęPoczątkową(ref List<Osobnik> osobnicy)
        {
            osobnicy.Clear();
            for (int i = 0; i < TSP.Instancja.liczbaOsobników; i++)
            {
                osobnicy.Add(new Osobnik());
                osobnicy[i].WypełnijOsobnika();
                osobnicy[i].OceńOsobnika(TSP.Instancja.distances);
            }
        }

        static void ZapiszDoPliku(Osobnik najlepszy)
        {
            StringBuilder wynik = new StringBuilder();
            foreach (int i in najlepszy.trasa)
            {
                wynik.Append(i + "-");
            }
            wynik.Remove(wynik.Length - 1, 1);
            wynik.Append(" " + najlepszy.ocena);

            string s = wynik.ToString();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter output = new StreamWriter(Path.Combine(path, "Wynik.txt")))
            {
                output.WriteLine(s);
            }
        }
    }
}
