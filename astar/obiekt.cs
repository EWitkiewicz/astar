using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace astar
{
    public class obiekt
    {
        public obiekt(int x,int y,obiekt rodzic)
        {
               this.x = x;
               this.y = y;
            this.g = double.MaxValue; // Domyślnie "nieskończoność"
            this.rodzic = rodzic;
            this.heurystyka = liczHeurystyke();
        }
        public int x {  get; set; }
        public int y { get; set; }
        public obiekt rodzic { get; set; }
        //Krok 6)[...] Dla każdej takiej kratki obliczana jest wartość funkcji f.[...]-pdf
        public double f { get { return g + heurystyka; } }
        public double g { get; set; }//odleglosc od startu
        public double heurystyka { get; set; }

        //Krok 2) Wybierz heurystykę h, która spełnia warunek dopuszczalności.-pdf
        private double liczHeurystyke() 
        {
            heurystyka=Math.Sqrt(Math.Pow(this.x-19,2) + Math.Pow(this.y-19,2));
            return heurystyka;
        }
       
    }
}
