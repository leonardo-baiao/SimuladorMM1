using System;
using System.Collections.Generic;
using System.Text;

namespace Estatisticas
{
    /* Classe responsável por armazenar os dados referentes as estatisticas de cada rodada. 
     * Armazena a Rodada, o tempo médio e a quantidade média para plotar os gráficos na tela 
     * e realizar os cálculos de variancia e intervalos de confiança no final.
     */
    public class Estatistica
    {
        public int Rodada { get; set; }
        public double SomaAmostras { get; set; }
        public double TempoMedio { get; set; }
        public double QuantidadeMedia { get; set; }
    }
}
