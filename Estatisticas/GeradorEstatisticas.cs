using Estruturas;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

namespace Estatisticas
{
    public class GeradorEstatisticas
    {
        private Random gerador;
        private GeradorPlanilhas geradorPlanilhas;

        public GeradorEstatisticas()
        {
            geradorPlanilhas = new GeradorPlanilhas();
            gerador = new Random();
        }

        public void SalvaEstatisticas(List<Estatistica> estatisticas)
        {
            geradorPlanilhas.Exportar(estatisticas);
        }

        private void CalculaPrecisao(ref IntervaloConfianca ic)
        {
             ic.Precisao = (ic.U - ic.L)/(ic.U + ic.L);
        }

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


        private void CalculaICChiQuadrado(double variancia, int n, ref IntervaloConfianca ic)
        {
            ChiSquared cs = new ChiSquared(n-1);
            //ic.U = (Constantes.KMIN * (n - 1) * variancia) / cs.InverseCumulativeDistribution(0.025);
            // ic.L = (Constantes.KMIN * (n - 1) * variancia) / cs.InverseCumulativeDistribution(0.975);
            ic.U = ((n - 1) * variancia) / cs.InverseCumulativeDistribution(0.025);
            ic.L = ((n - 1) * variancia) / cs.InverseCumulativeDistribution(0.975);
        }

        private void CalculaICTStudent(double media, double variancia, int n, ref IntervaloConfianca ic)
        {
            StudentT ts = new StudentT(0,1,n-1);
            var percentil = ts.InverseCumulativeDistribution(0.975);

            ic.U = media + percentil * Math.Sqrt(variancia/n);
            ic.L = media - percentil * Math.Sqrt(variancia/n);
        }

        public double CalculaExponencial(double taxa)
        {
            var amostra = GeraAmostra();
            return Math.Log(amostra)/(-taxa);
        }

        private double GeraAmostra()
        {
            return gerador.NextDouble();
        }

        public void CalculaSomaAmostras(ref Estatistica estatistica, double x)
        {
            estatistica.SomaAmostras += x;
        }

        public double CalculaMediaAmostral(double somAmostras, double n)
        {
            return (somAmostras)/n;
        }

        public double CalculaVarianciaAmostral(List<double> listaMediaRodadas, double media, double n)
        {
            var soma = 0.0;

            foreach(var mediaRodada in listaMediaRodadas)
            {
                soma += Math.Pow(mediaRodada - media, 2);
            }

            return soma/(n-1);
        }

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
