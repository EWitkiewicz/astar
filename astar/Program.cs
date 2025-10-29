using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;


namespace astar
{
    internal class Program
    {
        //Krok 5) Stwórz dwie listy pomocnicze: listę otwartą[...]-pdf
        static List<obiekt> listaOtwarta = new List<obiekt>();//poczekalnia
        static List<obiekt> listaZamknieta = new List<obiekt>();
        
        static void Main(string[] args)
        {
            Console.WriteLine("Podaj sciezke do pliku txt z mapką:\nw tym projekcie to:\nC:\\Users\\Win11\\source\\repos\\astar\\astar\\grid.txt");
            
            string nazwapliku =Console.ReadLine();
            
            var zawartosc_pliku =File.ReadAllText(nazwapliku);

            int[,] mapa=zamienNaMacierz(zawartosc_pliku);
            drukujMape(mapa);

            // Krok 4) Wskaż punkt startowy [...]-pdf
            obiekt start = new obiekt(0, 0, null);
            start.g = 0;
            //Krok 5) Stwórz dwie listy pomocnicze: listę otwartą,[...], oraz listę zamkniętą, inicjowaną polem startowym[...]
            listaZamknieta.Add(start);
            sprawdzWszystkichSasiadow(start, mapa);

            bool czyZnalezionoMete = false;

            // Krok 8) Jeśli cel nie został osiągnięty i lista otwarta zawiera przynajmniej jeden element, przejdź do Kroku 6.-pdf
            while (listaOtwarta.Count > 0) 
            {
               obiekt obecnePole= znajdzNanizszyKoszt(listaOtwarta);

                //Krok 5) Stwórz dwie listy pomocnicze: listę otwartą, początkowo pustą, która będzie zawierała kratki rozważane jako pola do ekspansji, oraz listę zamkniętą, inicjowanwaną polem startowym,
                //!!! do której trafią odwiedzone pola, jednocześnie usuwając jez listy otwarnej.!!!
                listaOtwarta.Remove(obecnePole);
                listaZamknieta.Add(obecnePole);

                //Krok 9) Algorytm może zakończyć się na dwa sposoby:1.Jeśli cel został osiągnięty, wróć wstecz przez kolejne kratki rodziców do kratki startowej, wyznaczając w ten sposób optymalną ścieżkę od startu do celu [...]-pdf
                if (czyToMeta(obecnePole)) 
                {
                    List<obiekt> finalnaSciezka = zwrocSciezke(obecnePole);

                    //wyprintuj współrzędne ścieżki
                    Console.WriteLine("\nZnaleziona ścieżka (od Startu do Celu):");
                    foreach (obiekt pole in finalnaSciezka)
                    {
                        Console.WriteLine($"-> ({pole.x}, {pole.y})");
                    }

                    //zaznaczamy sciezke 3kami
                    foreach (obiekt pole in finalnaSciezka)
                    {
                        // Sprawdzamy, czy to nie Start lub meta (żeby ich nie nadpisać)
                        if ((pole.x != 0 || pole.y != 0) && !czyToMeta(pole))
                        {
                            mapa[pole.y, pole.x] = 3;
                        }
                    }

                    //wyswietla zatrójkowaną mape mapę
                    Console.WriteLine("\nMapa z wyznaczoną trasą (3):");
                    drukujMape(mapa);
                    czyZnalezionoMete = true;
                    break;
                }

                sprawdzWszystkichSasiadow(obecnePole, mapa);
            }
            // Krok 9) Algorytm może zakończyć się na dwa sposoby:
            //[...]
            //2. Jeśli lista otwarta jest pusta, a cel nie został osiągnięty, zwróć komunikat o niemożliwości dotarcia do celu.
            if (!czyZnalezionoMete)
            {
                Console.WriteLine("\n Lista otwarta jest pusta. Nie można znaleźć ścieżki do celu.");
            }
        }
        public static List<obiekt>zwrocSciezke(obiekt obiektMeta)
         {
                 List<obiekt> sciezka=new List<obiekt>();

            obiekt obecnePole = obiektMeta;

            while (obecnePole != null) 
            { 
                sciezka.Add(obecnePole);
                obecnePole = obecnePole.rodzic;
            }
            sciezka.Reverse();

            return sciezka;
        }
        //Krok 3) Określ sposoby rozwiązywania konfliktów, które mogą powstać podczas eksploracji siatki.-pdf
        // Krok 7) Do listy zamkniętej trafia kratka z listy otwartej o najmniejszej wartościf.Konflikty rozwiązywane są hierarchicznie — spośród kratek z tym samym oszacowaniem f wybieramy pole, które zostało odwiedzone najpóźniej.
        public static obiekt znajdzNanizszyKoszt(List<obiekt> lista)
        {

            //lista nie jest pusta (ogarnia to pętla while)
            //for leci od ostatniego elementu jako tymczasowego minimum
            obiekt obZnajlepszymKosztem = lista[lista.Count - 1];

            // iterujemy od końca (przedostatniego elementu) do początku
            //i>= zaobezpiecza przypadek krancowy
            for (int i = lista.Count - 2; i >= 0; i--)
            {
                // Sprawdzamy bieżący element (lista[i])
                if (lista[i].f < obZnajlepszymKosztem.f)
                {
                    // Znaleźliśmy nowe, niższe f
                    obZnajlepszymKosztem = lista[i];
                }
                // Jeśli f jest równe, nic nie robimy, trzymamy się tego znalezionego później
            }

            return obZnajlepszymKosztem;
        }
        public static void sprawdzWszystkichSasiadow(obiekt obecnyObiekt, int[,] mapa)
{
            // Krok 1) Ustal sposób poruszania się agenta, koszt poszczególnych ruchów oraz kolejność przeszukiwania pól wokół aktualnej pozycji.-pdf

            //góra
            sprawdzSasiada(obecnyObiekt, obecnyObiekt.x, obecnyObiekt.y + 1, mapa);
    
            //dół
            sprawdzSasiada(obecnyObiekt, obecnyObiekt.x, obecnyObiekt.y - 1, mapa);

            //lewo
            sprawdzSasiada(obecnyObiekt, obecnyObiekt.x - 1, obecnyObiekt.y, mapa);

            //prawo
            sprawdzSasiada(obecnyObiekt, obecnyObiekt.x + 1, obecnyObiekt.y, mapa);
}
        public static void sprawdzSasiada(obiekt obecnyObiekt,int sasiadX,int sasiadY, int[,] mapa)
        {
            obiekt sasiad = new obiekt(sasiadX,sasiadY, obecnyObiekt);
            
            if (czyNaMapie(sasiad) && !czyPrzeszkoda(sasiad, mapa))
            {
                // "Czy lista zamknięta ma JAKIKOLWIEK obiekt 'pole', dla którego x i y pasują do sasiad?"
                bool juzJestNaLiscieZamknietej = listaZamknieta.Any(pole => pole.x == sasiad.x && pole.y == sasiad.y);
                //contains sprawdza czy ten dokladnie oobiekt a przeciez dopiero co go tworzymy wiec na pewno go nie ma , funkcja Any sprawdza czy istnieje JAKIKOLWIEK który ...(i dowolny warunek,a w tym przypadku ktory ma takie same wspolrzedne)
                bool juzJestNaLiscieOtwartej = listaOtwarta.Any(pole => pole.x == sasiad.x && pole.y == sasiad.y);
                if (!juzJestNaLiscieZamknietej)
                {
                    //tutaj a nie w obiekcie bo tu juz wiemy kto jest rodzicem a tak to bylby problemy z wezlem startu bo ma rodzica jako null
                    if (juzJestNaLiscieOtwartej)
                    {
                        //Krok 6) Kratki otaczające ostatnio dodane pole do listy zamkniętej, które możemy odwiedzić, trafiają na listę otwartą.Zachowują one informację o ”rodzicu”,czyli kratce, przez którą zostały dodane do listy otwartej. Dla każdej takiej kratki obliczana jest wartość funkcji f. Jeśli kratka już ma przypisaną wartość f z innego pola dodanego wcześniej do listy zamkniętej, porównujemy nową wartość z aktualną.Zmieniamy ”rodzica” tylko wtedy, gdy nowa wartość f jest mniejsza niż poprzednia-pdf
                        obiekt tenCoJusJestNaLiscieOtwartej = listaOtwarta.Find(pole => pole.x == sasiad.x && pole.y == sasiad.y);

                        double noweG = obecnyObiekt.g + 1;
                        if (noweG < tenCoJusJestNaLiscieOtwartej.g)
                        {
                            tenCoJusJestNaLiscieOtwartej.g = noweG;
                            tenCoJusJestNaLiscieOtwartej.rodzic = obecnyObiekt;
                        }
                    }
                    else
                    {
                        sasiad.g = obecnyObiekt.g + 1;
                        listaOtwarta.Add(sasiad);
                    }
                } 
            }
        }
        //Krok 4) Wskaż punkt startowy i cel.-pdf
        public static bool czyToMeta(obiekt obecnePole)
        {
            if (obecnePole.x == 19 && obecnePole.y == 19) return true;
            return false; ;
        }
        public static bool czyPrzeszkoda(obiekt obecnePole, int[,] mapa)
        {
            if (mapa[obecnePole.y,obecnePole.x] == 5) return true;
            return false;
        }
        public static bool czyNaMapie(obiekt obecnePole)
        {
            if (obecnePole.x < 20 && obecnePole.x >= 0 && obecnePole.y < 20 && obecnePole.y >= 0) return true;
            return false;
        }
        public static int[,] zamienNaMacierz(string zawartoscpliku )
        {
            int[,] mapa=new int[20,20];

            string[] liczbyJakoStringi = zawartoscpliku.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

            // 3. Wypełniamy mapę 2D
            int index = 0; // Płaski indeks do przesuwania się po 'liczbyJakoStringi'

            for (int y = 19; y >=0; y--) // Pętla po wierszach (Y)
            {
                for (int x = 0; x < 20; x++) // Pętla po kolumnach (X)
                {
                    // 4. Zamieniamy string (np. "5") na liczbę (int 5)
                    mapa[y, x] = int.Parse(liczbyJakoStringi[index]);

                    // Przesuwamy się na kolejną liczbę z pliku
                    index++;
                }
            }
            return mapa;

            

        }
        public static void drukujMape(int[,] mapa)
        {// Zaczynamy od y=19 (GÓRNY wiersz logiczny) i idziemy w dół do 0
            for (int y = 19; y >= 0; y--)
            {
                for (int x = 0; x < 20; x++)
                {
                    // Console.Write drukuje bez przechodzenia do nowej linii
                    Console.Write(mapa[y, x] + " ");
                }
                // Console.WriteLine puste - robi przejście do nowej linii na końcu wiersza
                Console.WriteLine();
            }
        }


    }
}
