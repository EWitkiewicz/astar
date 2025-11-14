
using System;
using System.Collections.Generic;

namespace Aport // Upewnij się, że ta nazwa pasuje do nazwy projektu
{
    public class obiekt
    {
        // Współrzędne
        public int x { get; set; }
        public int y { get; set; }

        // Referencja do rodzica (do odtworzenia ścieżki)
        public obiekt rodzic { get; set; }

        // Koszty A*
        // f jest obliczane automatycznie jako suma g + heuristic
        public double f => g + heuristic;
        public double g { get; set; } // Koszt od startu
        public double heuristic { get; set; } // Koszt do celu (heurystyka)

        /// <summary>
        /// Konstruktor węzła (pola)
        /// </summary>
        public obiekt(int x, int y, obiekt rodzic)
        {
            this.x = x;
            this.y = y;
            this.rodzic = rodzic;

            // Domyślne wartości
            this.g = double.MaxValue;
            this.heuristic = 0;
        }
    }
}