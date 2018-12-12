using Estatisticas;
using Estruturas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimuladorAD
{
    public class Simulador
    {
        private double tempo = 0;
        private double tempoUltimoEvento = 0;
        private double tempoInicialRodada = 0;
        private int amostras = 0;
        private bool servidor;
        private Evento eventoAtual;
        private double TAXA_CHEGADA;
        private ListaEventos listaEventos;
        private Fila fila;
        private Estatistica estatisticaAtual;
        public List<Estatistica> listaEstatisticas;
        private readonly GeradorEstatisticas _geradorEstatisticas;


        // variaveis para estimativa da variancia por rodada
        public List<double> listaVarianciaP;
        public List<double> listaVarianciaT;
        public List<double> listaMediaTempoRodada;
        public List<double> listaMediaPessoasRodada;
        double SomTempoAtual = 0;
        double SomQTempoAtual = 0;
        double SomPessoasAtual = 0;
        double SomQPessoasAtual = 0;
        // fim variaveis


        // variaveis para parametros finais
        public double tempoMedioFinal;
        public double varianciaTempoFinal;
        public double somaTempoMedio = 0;

        public double mediaPessoasFinal;
        public double varianciaPessoasFinal;
        public double somaQuantidadeMedia = 0;

        public IntervaloConfianca icMedia;
        public IntervaloConfianca icVariancia;
        public IntervaloConfianca icPessoasMedia;
        public IntervaloConfianca icPessoasVariancia;
        // fim variaveis

        public Simulador(TipoFila tipoFila, double taxaChegada)
        {
            GeraFila(tipoFila);
            TAXA_CHEGADA = taxaChegada;
            listaEventos = new ListaEventos();
            estatisticaAtual = new Estatistica { Rodada = 0 };
            listaEstatisticas = new List<Estatistica>();
            _geradorEstatisticas = new GeradorEstatisticas();

            
            listaVarianciaP = new List<double>();
            listaVarianciaT = new List<double>();
            listaMediaTempoRodada = new List<double>();
            listaMediaPessoasRodada = new List<double>();
        }

        public void IniciarSimulacao()
        {

            ProcessaRodadaTransiente();

            while (Rodada <= Constantes.MAX_RODADAS)
            {
                ProcessaEventos();

                CalculaEstatisticas();

                ProximaRodada();
            }
            CalculaEstatisticasFinais();
        }

        public int Rodada { get; private set; }

        internal void ProcessaRodadaTransiente()
        {
            ProcessaEventos();
            ProximaRodada();
        }


        public void ProximaRodada()
        {
            Rodada++;

            if (Rodada > Constantes.MAX_RODADAS)
                return;

            estatisticaAtual = new Estatistica { Rodada = Rodada };
            amostras = 0;
            tempoInicialRodada = tempo;
        }
        
        public void ProcessaEventos()
        {
            while(amostras < Constantes.KMIN)
            {
                TrataEvento();
            }
        }

        private void TrataEvento()
        {
            eventoAtual = listaEventos.ProximoEvento();

            if (eventoAtual.Tempo == 0)
            {
                listaEventos.AdicionaEvento(CalculaChegadaFregues());
                return;
            }

            tempo = eventoAtual.Tempo;

            if (eventoAtual.Tipo == TipoEvento.CHEGADA_FREGUES)
            {
                ChegadaFregues();
            }
            else
            {
                SaidaServidor();
            }

            tempoUltimoEvento = tempo;
        }



        private void ChegadaFregues()
        {
            estatisticaAtual.QuantidadeMedia += servidor ? (fila.Quantidade + 1) * (tempo - tempoUltimoEvento) : fila.Quantidade * (tempo - tempoUltimoEvento);

            fila.AdicionaFregues(new Fregues { Tipo = Rodada, TempoChegada = tempo });

            if (!servidor)
            {
                EntradaServidor();
                servidor = true;
            }

            listaEventos.AdicionaEvento(CalculaChegadaFregues());

        }

        private void EntradaServidor()
        {
            var cliente = fila.RetornaFregues();
            listaEventos.AdicionaEvento(CalculaSaidaServidor());

            if (Rodada.Equals(cliente.Tipo))
            {
                estatisticaAtual.SomaAmostras += tempo - cliente.TempoChegada;
                amostras++;
            }
        }

        private void SaidaServidor()
        {
            estatisticaAtual.QuantidadeMedia += (fila.Quantidade + 1) * (tempo - tempoUltimoEvento);

            if (fila.Quantidade == 0)
            {
                servidor = false;
                eventoAtual.Tempo = 0;
                return;
            }
            EntradaServidor();
        }


        private Evento CalculaSaidaServidor()
        {
            return new Evento
            {
                Tipo = TipoEvento.SAIDA_SERVIDOR,
                Tempo = tempo + _geradorEstatisticas.CalculaExponencial(Constantes.TAXA_SERVIDOR)
            };
        }

        private Evento CalculaChegadaFregues()
        {
            return new Evento
            {
                Tipo = TipoEvento.CHEGADA_FREGUES,
                Tempo = tempo + _geradorEstatisticas.CalculaExponencial(TAXA_CHEGADA)
            };
        }
        
        public void CalculaEstatisticas()
        {
            estatisticaAtual.TempoMedio = estatisticaAtual.SomaAmostras/amostras;
            estatisticaAtual.QuantidadeMedia = estatisticaAtual.QuantidadeMedia/(tempo - tempoInicialRodada);
            listaEstatisticas.Add(estatisticaAtual);

            SomTempoAtual += estatisticaAtual.TempoMedio;
            SomQTempoAtual += Math.Pow(estatisticaAtual.TempoMedio, 2);
            SomPessoasAtual += estatisticaAtual.QuantidadeMedia;
            SomQPessoasAtual += Math.Pow(estatisticaAtual.QuantidadeMedia, 2);

            listaMediaTempoRodada.Add(SomTempoAtual / Rodada);
            listaMediaPessoasRodada.Add(SomPessoasAtual / Rodada);

            listaVarianciaP.Add(_geradorEstatisticas.CalculaEstimativaVariancia(SomQPessoasAtual, SomPessoasAtual, Rodada));
            listaVarianciaT.Add(_geradorEstatisticas.CalculaEstimativaVariancia(SomQTempoAtual, SomTempoAtual, Rodada));
        }

        public void CalculaEstatisticasFinais()
        {            
            foreach (var estatistica in listaEstatisticas)
            {
                somaTempoMedio += estatistica.TempoMedio;
                somaQuantidadeMedia += estatistica.QuantidadeMedia;
            }

            tempoMedioFinal = somaTempoMedio/listaEstatisticas.Count;
            varianciaTempoFinal = _geradorEstatisticas.CalculaVarianciaAmostral(listaEstatisticas.Select(l => l.TempoMedio).ToList(), tempoMedioFinal, listaEstatisticas.Count);

            mediaPessoasFinal = somaQuantidadeMedia/listaEstatisticas.Count;
            varianciaPessoasFinal = _geradorEstatisticas.CalculaVarianciaAmostral(listaEstatisticas.Select(l => l.QuantidadeMedia).ToList(), mediaPessoasFinal, listaEstatisticas.Count);

            icMedia = _geradorEstatisticas.CalculaIC(tempoMedioFinal, varianciaTempoFinal, VariavelAleatoria.TSTUDENT, listaEstatisticas.Count);
            icVariancia = _geradorEstatisticas.CalculaIC(tempoMedioFinal, varianciaTempoFinal, VariavelAleatoria.CHIQUADRADO, listaEstatisticas.Count);

            icPessoasMedia = _geradorEstatisticas.CalculaIC(mediaPessoasFinal, varianciaPessoasFinal, VariavelAleatoria.TSTUDENT, listaEstatisticas.Count);
            icPessoasVariancia = _geradorEstatisticas.CalculaIC(mediaPessoasFinal, varianciaPessoasFinal, VariavelAleatoria.CHIQUADRADO, listaEstatisticas.Count);

            double covTempo =_geradorEstatisticas.CalculaCovariancia(listaEstatisticas.Select(l => l.TempoMedio), tempoMedioFinal);
            double covPessoas = _geradorEstatisticas.CalculaCovariancia(listaEstatisticas.Select(l => l.QuantidadeMedia), mediaPessoasFinal);

        }

        private void GeraFila(TipoFila tipoFila)
        {
            if (tipoFila.Equals(TipoFila.FCFS))
                fila = new FilaFCFS();
            else
                fila = new FilaLCFS();
        }
    }
}
