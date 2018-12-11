using System;
using System.Collections.Generic;
using System.Text;

namespace Estatisticas
{
    //Classe usada para armazenar o intervalo de confiança da média e da variancia.
    public class IntervaloConfianca
    {
        public double L { get; set; }
        public double U { get; set; }
        public double Precisao { get; set; }
    }
}
