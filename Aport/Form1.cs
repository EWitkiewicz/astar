using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Aport 
{
    public partial class Form1 : Form
    {
       //zmienne globalne

        const int ROZMIAR_MAPY_X = 40;
        const int ROZMIAR_MAPY_Y = 30;
        int[,] mapa = new int[ROZMIAR_MAPY_Y, ROZMIAR_MAPY_X];
        int rozmiarPola = 30;
        Point pozycjaGracza = new Point(1, 1);
        Point pozycjaPsa = new Point(1, 2);
        Point pozycjaPatyka = new Point(-1, -1);
        List<obiekt> listaOtwarta = new List<obiekt>();
        List<obiekt> listaZamknieta = new List<obiekt>();
        List<obiekt> sciezkaPsa = new List<obiekt>();
        Image imgKrzew, imgPatyk, imgEryk, imgEryk_Odbity, imgErykZPatykiem, imgErykZPatykiem_Odbity, imgKora, imgKora_Odbita, imgKoraZPatykiem, imgKoraZPatykiem_Odbita;
        bool czyPiesMaPatyk = false;
        bool czyGraczMaPatyk = true;
        bool czyPiesPatrzyWPrawo = true;
        bool czyGraczPatrzyWPrawo = true;
        SolidBrush pedzelPodloga = new SolidBrush(Color.LightGreen);//koloruje nazielono kratki
        SolidBrush pedzelSciezka = new SolidBrush(Color.FromArgb(100, 135, 206, 250));//sciezka psa
        Pen pedzelSiatka = new Pen(Color.FromArgb(50, 0, 0, 0));//obramowanie kratek
        System.Windows.Forms.Timer timerGry;


        //konstuktor + podpiecie eventów

        public Form1()
        {
            InitializeComponent();

            this.ClientSize = new Size(ROZMIAR_MAPY_X * rozmiarPola, ROZMIAR_MAPY_Y * rozmiarPola);//pole powierzchni mapy
            this.Text = "A*port!";
            this.DoubleBuffered = true;//"najpierw narysował całą grafikę w ukrytej pamięci, a dopiero potem pokazał gotowy obraz na ekranie. Zapobiega to denerwującemu migotaniu animacji."

            ZaladujObrazy();

            this.Paint += new PaintEventHandler(this.Form1_Paint);
            this.KeyDown += new KeyEventHandler(this.Form1_KeyDown);//wcisniecia klawisza
            this.MouseClick += new MouseEventHandler(this.Form1_MouseClick);//klikniecie

            timerGry = new System.Windows.Forms.Timer();
            timerGry.Interval = 100;//logika gry będzie się aktualizować 10 razy na sekundę (1000ms / 100ms = 10 FPS).
            timerGry.Tick += new EventHandler(this.timer1_Tick);//""Za każdym razem, gdy 'tykniesz' (co 100ms), wywołaj funkcję timer1_Tick." (To jest funkcja, która m.in. przesuwa psa)."
            timerGry.Start();

            // ZMIANA: Pokazujemy instrukcję na starcie
            PokazInstrukcje();
        }

        private void PokazInstrukcje()
        {
            string instrukcje = "Witaj w grze 'Aport!'\n\n" +
                              "--- STEROWANIE ---\n\n" +
                              "● Ruch postaci (Eryka) : \tW, S, A, D lub Strzałki\n" +
                              "● Rzut patykiem: \tLewy Przycisk Myszy (LPM)\n" +
                              "    (Musisz mieć patyk, aby rzucić!)\n\n" +
                              "● Buduj/Usuń krzak: \tPrawy Przycisk Myszy (PPM)\n\n" +
                              "● Pokaż tę pomoc: \tKlawisz 'i'\n\n" +
                              "--- CEL ---\n" +
                              "Rzucać Korze patyk :)";

            // Zatrzymujemy timer gry, gdy wyświetlamy MessageBox, żeby piesek się nie ruszał
            timerGry.Stop();
            MessageBox.Show(instrukcje, "Instrukcja Gry");
            timerGry.Start(); // Wznawiamy timer po zamknięciu okienka
        }

        private void ZaladujObrazy()
        {
            try
            {
                imgKrzew = Properties.Resources.krzew;
                imgPatyk = Properties.Resources.patyk;
                imgEryk = Properties.Resources.Eryk;
                imgErykZPatykiem = Properties.Resources.Eryk_z_patykiem;
                imgKora = Properties.Resources.Kora;
                imgKoraZPatykiem = Properties.Resources.Kora_z_Patykiem;
                imgEryk_Odbity = (Image)imgEryk.Clone();
                imgEryk_Odbity.RotateFlip(RotateFlipType.RotateNoneFlipX);
                imgErykZPatykiem_Odbity = (Image)imgErykZPatykiem.Clone();
                imgErykZPatykiem_Odbity.RotateFlip(RotateFlipType.RotateNoneFlipX);
                imgKora_Odbita = (Image)imgKora.Clone();
                imgKora_Odbita.RotateFlip(RotateFlipType.RotateNoneFlipX);
                imgKoraZPatykiem_Odbita = (Image)imgKoraZPatykiem.Clone();
                imgKoraZPatykiem_Odbita.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Nie udało się wczytać obrazów z zasobów. Upewnij się, że dodałeś je do Properties->Resources.\nBłąd: {e.Message}");
                Application.Exit();
            }
        }

       
        // Główna pętla rysująca
       
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;// Pobiera"płótno" (Graphics), na którym będziemy rysować. Możeszmy myśleć o g jak o cyfrowej kartce papieru, po której teraz możemy malować.
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;// ustawienie graficzne  dla pixel artu. Mówi systemowi: "Jeśli kiedykolwiek będziesz musiał rozciągnąć obrazek (np. Kora.png), nie próbuj go wygładzać ani rozmywać. Zachowaj ostre, kwadratowe krawędzie." To sprawia, że pixel art wygląda czysto.

            // 1. Podłoga
            for (int y = 0; y < ROZMIAR_MAPY_Y; y++)
            {
                for (int x = 0; x < ROZMIAR_MAPY_X; x++)
                {
                    g.FillRectangle(pedzelPodloga, x * rozmiarPola, y * rozmiarPola, rozmiarPola, rozmiarPola);
                }
            }
            // 2. Ścieżka psa
            if (sciezkaPsa != null)
            {
                foreach (obiekt pole in sciezkaPsa)
                {
                    g.FillRectangle(pedzelSciezka, pole.x * rozmiarPola, pole.y * rozmiarPola, rozmiarPola, rozmiarPola);
                }
            }
            // 3. Krzewy
            for (int y = 0; y < ROZMIAR_MAPY_Y; y++)
            {
                for (int x = 0; x < ROZMIAR_MAPY_X; x++)
                {
                    if (mapa[y, x] == 5)
                    {
                        g.DrawImage(imgKrzew, x * rozmiarPola, y * rozmiarPola, rozmiarPola, rozmiarPola);
                    }
                }
            }
            // 4. Siatka
            for (int y = 0; y < ROZMIAR_MAPY_Y; y++)
            {
                for (int x = 0; x < ROZMIAR_MAPY_X; x++)
                {
                    g.DrawRectangle(pedzelSiatka, x * rozmiarPola, y * rozmiarPola, rozmiarPola, rozmiarPola);
                }
            }
            // 5. Patyk
            if (pozycjaPatyka.X != -1)
            {
                g.DrawImage(imgPatyk, pozycjaPatyka.X * rozmiarPola, pozycjaPatyka.Y * rozmiarPola, rozmiarPola, rozmiarPola);
            }
            // 6. Gracz
            Image obrazekGracza = czyGraczMaPatyk ?
                (czyGraczPatrzyWPrawo ? imgErykZPatykiem : imgErykZPatykiem_Odbity) :
                (czyGraczPatrzyWPrawo ? imgEryk : imgEryk_Odbity);
            g.DrawImage(obrazekGracza, pozycjaGracza.X * rozmiarPola, pozycjaGracza.Y * rozmiarPola, rozmiarPola, rozmiarPola);
            // 7. Pies
            Image obrazekPsa = czyPiesMaPatyk ?
                (czyPiesPatrzyWPrawo ? imgKoraZPatykiem : imgKoraZPatykiem_Odbita) :
                (czyPiesPatrzyWPrawo ? imgKora : imgKora_Odbita);
            g.DrawImage(obrazekPsa, pozycjaPsa.X * rozmiarPola, pozycjaPsa.Y * rozmiarPola, rozmiarPola, rozmiarPola);
        }


        //Główna pętla logiki gry
        private void timer1_Tick(object sender, EventArgs e)
        {
            //czy Kora ma gdzie iść?
            if (sciezkaPsa != null && sciezkaPsa.Count > 0)
            {
                //sprawdzanie czy droga jest zablokowana
                obiekt nastepnyKrok = sciezkaPsa[0];
                if (mapa[nastepnyKrok.y, nastepnyKrok.x] == 5)
                {
                    sciezkaPsa.Clear();
                    Point cel = (pozycjaPatyka.X != -1) ? pozycjaPatyka : pozycjaGracza;
                    var nowaSciezka = ZnajdzSciezke(pozycjaPsa, cel);
                    if (nowaSciezka == null)
                    {
                        if (pozycjaPatyka.X != -1)
                            MessageBox.Show("Kora nie może dobiec do patyka! Usuń krzaczek.");
                        else
                            MessageBox.Show("Kora nie może znaleźć drogi powrotnej! Usuń krzaczek.");
                    }
                    else
                    {
                        sciezkaPsa = nowaSciezka;
                        if (sciezkaPsa.Count > 0) sciezkaPsa.RemoveAt(sciezkaPsa.Count - 1);
                    }
                    this.Invalidate();
                    return;
                }
                //Normalny ruch + logika obracania Kory
                if (nastepnyKrok.x > pozycjaPsa.X) czyPiesPatrzyWPrawo = true;
                else if (nastepnyKrok.x < pozycjaPsa.X) czyPiesPatrzyWPrawo = false;
                pozycjaPsa = new Point(nastepnyKrok.x, nastepnyKrok.y);
                sciezkaPsa.RemoveAt(0);

                //psiak dociera do celu
                if (sciezkaPsa.Count == 0)
                {
                    if (pozycjaPatyka.X != -1 && !czyPiesMaPatyk)//Sprawdza, czy celem był patyk.
                    {
                        //Sprawdza, czy jest obok patyka.
                        if (Math.Abs(pozycjaPsa.X - pozycjaPatyka.X) <= 1 && Math.Abs(pozycjaPsa.Y - pozycjaPatyka.Y) <= 1)
                        {
                            //"Podnosi" patyk (patyk znika z mapy).
                            pozycjaPatyka = new Point(-1, -1);
                            //Ustawia flagę, że piesek ma patyk (zmieni się jego obrazek na Kora z Patykiem).
                            czyPiesMaPatyk = true;
                            //Natychmiast wyznacza nową ścieżkę powrotną do gracza i sprawdza, czy nie jest zablokowana (wyświetlając MessageBox, jeśli jest).
                            var nowaSciezka = ZnajdzSciezke(pozycjaPsa, pozycjaGracza);
                            if (nowaSciezka == null)
                            {
                                MessageBox.Show("Kora podniosła patyk, ale nie może znaleźć drogi powrotnej! Usuń krzaczek.");
                            }
                            else
                            {
                                sciezkaPsa = nowaSciezka;
                                if (sciezkaPsa.Count > 0) sciezkaPsa.RemoveAt(sciezkaPsa.Count - 1);
                            }
                        }
                    }
                    else if (czyPiesMaPatyk)
                    {
                        //...i właśnie dotarł obok gracza...
                        if (Math.Abs(pozycjaPsa.X - pozycjaGracza.X) <= 1 && Math.Abs(pozycjaPsa.Y - pozycjaGracza.Y) <= 1)
                        {
                            //..oddaje patyk (zmienia obrazek).
                            czyPiesMaPatyk = false;
                            //...gracz dostaje patyk (zmienia swój obrazek na Eryk z patykiem).
                            czyGraczMaPatyk = true;
                        }
                    }
                }
            }
            this.Invalidate();
        }


        // Obsługa kliknięć myszką
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            //Przeliczenie Współrzędnych
            int gridX = e.X / rozmiarPola;
            int gridY = e.Y / rozmiarPola;
            if (!czyNaMapie(gridX, gridY)) return;

            //Logika dla Prawego Przycisku Myszy (Budowanie)
            if (e.Button == MouseButtons.Right)
            {
                if (gridX == pozycjaGracza.X && gridY == pozycjaGracza.Y) return;
                if (gridX == pozycjaPsa.X && gridY == pozycjaPsa.Y) return;
                bool czyUsunietoKrzak = (mapa[gridY, gridX] == 5);
                mapa[gridY, gridX] = (mapa[gridY, gridX] == 0) ? 5 : 0;

                if (czyUsunietoKrzak && (sciezkaPsa == null || sciezkaPsa.Count == 0))
                {
                    Point cel = (pozycjaPatyka.X != -1) ? pozycjaPatyka : pozycjaGracza;
                    var nowaSciezka = ZnajdzSciezke(pozycjaPsa, cel);
                    if (nowaSciezka != null)
                    {
                        sciezkaPsa = nowaSciezka;
                        if (sciezkaPsa.Count > 0) sciezkaPsa.RemoveAt(sciezkaPsa.Count - 1);
                    }
                }
            }
            //Logika dla Lewego Przycisku Myszy (Rzucanie Patyka)
            if (e.Button == MouseButtons.Left && czyGraczMaPatyk)
            {
                if (mapa[gridY, gridX] == 5) return;
                if (gridX == pozycjaPsa.X && gridY == pozycjaPsa.Y) return;

                pozycjaPatyka = new Point(gridX, gridY);
                czyGraczMaPatyk = false;
                var nowaSciezka = ZnajdzSciezke(pozycjaPsa, pozycjaPatyka);

                if (nowaSciezka != null)
                {
                    sciezkaPsa = nowaSciezka;
                    if (sciezkaPsa.Count > 0) sciezkaPsa.RemoveAt(sciezkaPsa.Count - 1);
                }
                else
                {
                    MessageBox.Show("Kora nie może znaleźć drogi do patyka! Spróbuj rzucić gdzie indziej.");
                    pozycjaPatyka = new Point(-1, -1);
                    czyGraczMaPatyk = true;
                }
            }
        }


        // Obsługa klawiatury (ruch gracza)
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //Sprawdzamy klawisz 'i' na początku
            if (e.KeyCode == Keys.I)
            {
                PokazInstrukcje();
                return; // Zatrzymujemy dalsze przetwarzanie klawiszy
            }
            //Przygotowanie do Ruchu
            Point nowaPozycja = pozycjaGracza;
            bool ruszylSie = false;

            //Sprawdzenie Klawiszy Ruchu
            switch (e.KeyCode)
            {
                case Keys.W:
                case Keys.Up:
                    nowaPozycja.Y--; ruszylSie = true; break;
                case Keys.S:
                case Keys.Down:
                    nowaPozycja.Y++; ruszylSie = true; break;
                case Keys.A:
                case Keys.Left:
                    nowaPozycja.X--; czyGraczPatrzyWPrawo = false; ruszylSie = true; break;
                case Keys.D:
                case Keys.Right:
                    nowaPozycja.X++; czyGraczPatrzyWPrawo = true; ruszylSie = true; break;
            }
            //Weryfikacja i Wykonanie Ruchu
            if (ruszylSie)
            {
                //Sprawdzenie Kolizji
                bool czyPolePsa = (nowaPozycja.X == pozycjaPsa.X && nowaPozycja.Y == pozycjaPsa.Y);
                if (czyNaMapie(nowaPozycja.X, nowaPozycja.Y) && mapa[nowaPozycja.Y, nowaPozycja.X] == 0 && !czyPolePsa)
                {
                    //Wykonanie Ruchu i Aktualizacja AI Psa
                    pozycjaGracza = nowaPozycja;

                    if (pozycjaPatyka.X == -1)
                    {
                        var nowaSciezka = ZnajdzSciezke(pozycjaPsa, pozycjaGracza);
                        if (nowaSciezka == null)
                        {
                            sciezkaPsa?.Clear();
                        }
                        else
                        {
                            sciezkaPsa = nowaSciezka;
                            if (sciezkaPsa.Count > 0) sciezkaPsa.RemoveAt(sciezkaPsa.Count - 1);
                        }
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "A*port!";
            this.ResumeLayout(false);

        }




        //logika A*

        public List<obiekt> ZnajdzSciezke(Point startPos, Point celPos)
        {
            listaOtwarta.Clear();
            listaZamknieta.Clear();
            if (!czyNaMapie(celPos.X, celPos.Y) || mapa[celPos.Y, celPos.X] == 5) return null;
            obiekt start = new obiekt(startPos.X, startPos.Y, null);
            start.g = 0;
            start.heuristic = LiczHeurystyke(startPos, celPos);
            listaZamknieta.Add(start);
            sprawdzWszystkichSasiadow(start, mapa, celPos);
            while (listaOtwarta.Count > 0)
            {
                obiekt obecnePole = znajdzNajnizszyKoszt(listaOtwarta);
                listaOtwarta.Remove(obecnePole);
                listaZamknieta.Add(obecnePole);
                if (obecnePole.x == celPos.X && obecnePole.y == celPos.Y)
                {
                    return zwrocSciezke(obecnePole); // SUKCES
                }
                sprawdzWszystkichSasiadow(obecnePole, mapa, celPos);
            }
            return null; // Porażka
        }
        private double LiczHeurystyke(Point A, Point B)
        {
            return Math.Sqrt(Math.Pow(A.X - B.X, 2) + Math.Pow(A.Y - B.Y, 2));
        }
        public void sprawdzWszystkichSasiadow(obiekt obecnyObiekt, int[,] mapa, Point celPos)
        {
            sprawdzSasiada(obecnyObiekt, obecnyObiekt.x, obecnyObiekt.y + 1, mapa, celPos);
            sprawdzSasiada(obecnyObiekt, obecnyObiekt.x, obecnyObiekt.y - 1, mapa, celPos);
            sprawdzSasiada(obecnyObiekt, obecnyObiekt.x - 1, obecnyObiekt.y, mapa, celPos);
            sprawdzSasiada(obecnyObiekt, obecnyObiekt.x + 1, obecnyObiekt.y, mapa, celPos);
        }
        public void sprawdzSasiada(obiekt obecnyObiekt, int sasiadX, int sasiadY, int[,] mapa, Point celPos)
        {
            obiekt sasiad = new obiekt(sasiadX, sasiadY, obecnyObiekt);
            if (!czyNaMapie(sasiad) || czyPrzeszkoda(sasiad, mapa))
            {
                return;
            }
            bool juzJestNaLiscieZamknietej = listaZamknieta.Any(pole => pole.x == sasiad.x && pole.y == sasiad.y);
            if (juzJestNaLiscieZamknietej)
            {
                return;
            }
            double noweG = obecnyObiekt.g + 1;
            obiekt tenCoJusJestNaLiscieOtwartej = listaOtwarta.FirstOrDefault(pole => pole.x == sasiad.x && pole.y == sasiad.y);
            if (tenCoJusJestNaLiscieOtwartej != null)
            {
                if (noweG < tenCoJusJestNaLiscieOtwartej.g)
                {
                    tenCoJusJestNaLiscieOtwartej.g = noweG;
                    tenCoJusJestNaLiscieOtwartej.rodzic = obecnyObiekt;
                }
            }
            else
            {
                sasiad.g = noweG;
                sasiad.heuristic = LiczHeurystyke(new Point(sasiad.x, sasiad.y), celPos);
                listaOtwarta.Add(sasiad);
            }
        }
        public List<obiekt> zwrocSciezke(obiekt obiektMeta)
        {
            List<obiekt> sciezka = new List<obiekt>();
            obiekt obecnePole = obiektMeta;
            while (obecnePole != null)
            {
                sciezka.Add(obecnePole);
                obecnePole = obecnePole.rodzic;
            }
            sciezka.Reverse();
            return sciezka;
        }
        public obiekt znajdzNajnizszyKoszt(List<obiekt> lista)
        {
            obiekt obZnajlepszymKosztem = lista[lista.Count - 1];
            for (int i = lista.Count - 2; i >= 0; i--)
            {
                if (lista[i].f < obZnajlepszymKosztem.f)
                {
                    obZnajlepszymKosztem = lista[i];
                }
            }
            return obZnajlepszymKosztem;
        }
        public bool czyPrzeszkoda(obiekt obecnePole, int[,] mapa)
        {
            return mapa[obecnePole.y, obecnePole.x] == 5;
        }
        public bool czyNaMapie(obiekt obecnePole)
        {
            return (obecnePole.x < ROZMIAR_MAPY_X && obecnePole.x >= 0 &&
                    obecnePole.y < ROZMIAR_MAPY_Y && obecnePole.y >= 0);
        }
        public bool czyNaMapie(int x, int y)
        {
            return (x < ROZMIAR_MAPY_X && x >= 0 &&
                    y < ROZMIAR_MAPY_Y && y >= 0);
        }
    }
}