using Estruturas;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

namespace Estatisticas
{

    /* Classe responsável pelo cálculo matemático das estatisticas necessárias para a analise de desempenho da fila. 
     * Todas os cálculos matemáticos são realizados através dessa classe.
     */
    public class GeradorEstatisticas
    {
        private Random gerador;

        //Construtor da classe.
        public GeradorEstatisticas()
        {
            gerador = new Random(DateTime.Now.Millisecond);
        }
        
        //Método para o calculo da Precisao do intervalo de confiança.
        private void CalculaPrecisao(ref IntervaloConfianca ic)
        {
             ic.Precisao = (ic.U - ic.L)/(ic.U + ic.L);
        }

        /* Método geral para o calculo do intervalo de confiança. 
         * Responsável por escolher entre TStudent e ChiQuadrado através do parametro va, que informa qual o calculo requerido.
         */
        public IntervaloConfianca CalculaIC(double media, double variancia, VariavelAleatoria va, int n)
        {
            var resultado = new IntervaloConfianca();

            switch (va)
            {
                case VariavelAleatoria.TSTUDENT:
                    CalculaICTStudent(media,variancia, n, ref resultado);
                    break;
                case VariavelAleatoria.CHIQUADRADO:
                    CalculaICChiQuadrado(variancia, n, ref resultado);
                    break;
                default:
                    Console.WriteLine("Variavel aleatória nula");
                    break;
            }

            CalculaPrecisao(ref resultado);

            return resultado;
        }

        //Método que calcula o intervalo de confiança da variancia através da fórmula da ChiQuadrado.
        private void CalculaICChiQuadrado(double variancia, int n, ref IntervaloConfianca ic)
        {
            ChiSquared cs = new ChiSquared(n-1);
            ic.U = ((n - 1) * variancia) / cs.InverseCumulativeDistribution(0.025);
            ic.L = ((n - 1) * variancia) / cs.InverseCumulativeDistribution(0.975);
        }

        //Método que calcula o intervalo de confiança da média através da fórmula da TStudent.
        private void CalculaICTStudent(double media, double variancia, int n, ref IntervaloConfianca ic)
        {
            StudentT ts = new StudentT(0,1,n-1);
            var percentil = ts.InverseCumulativeDistribution(0.975);

            ic.U = media + percentil * Math.Sqrt(variancia/n);
            ic.L = media - percentil * Math.Sqrt(variancia/n);
        }

        /*Método usado para se obter uma amostra aleatória de tempo através do cálculo da inversa da exponencial. 
        *Usado para calcular a proxima chegada de fregueses e o tempo de atendimento no servidor. 
        */
        public double CalculaExponencial(double taxa)
        {
            var amostra = GeraAmostra();
            return Math.Log(amostra)/(-taxa);
        }

        //Gera uma amostra aleatória através do gerador de números aleatórios da classe Random do C#. Utiliza o tempo como semente.
        private double GeraAmostra()
        {
            return gerador.NextDouble();
        }
        
        //Calcula a variancia iterando a partir de uma lista de médias de rodada e da média amostral geral. 
        public double CalculaVarianciaAmostral(List<double> listaMediaRodadas, double media, double n)
        {
            var soma = 0.0;

            foreach(var mediaRodada in listaMediaRodadas)
            {
                soma += Math.Pow(mediaRodada - media, 2);
            }

            return soma/(n-1);
        }

        //Calcula a estimativa da variancia
        public double CalculaEstimativaVariancia(double somaQ, double soma, int nRodadas)
        {
            double varianciaEstimada;

            varianciaEstimada = somaQ / (nRodadas - 1) - Math.Pow(soma, 2) / (nRodadas * (nRodadas - 1));

            return varianciaEstimada;
        }

        //Calcula a covariancia iterando a partir de uma lista de médias de rodada e da média amostral geral.
        public double CalculaCovariancia(IEnumerable<double> mediasRodadas, double mediaAmostral)
        {
            double autocov  = 0;
            List<double> mediasLista = mediasRodadas.ToList();

            for (var i = 0; i < mediasRodadas.Count() - 1; i++)
            {
                autocov += (mediasLista[i] - mediaAmostral) * (mediasLista[i+1] - mediaAmostral);
            }

            autocov = autocov / (mediasLista.Count - 2);

            return autocov;
        }

    }
}
