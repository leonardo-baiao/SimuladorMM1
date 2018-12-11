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

        public Simulador(TipoFila tipoFila, double taxaChegada)
        {
            GeraFila(tipoFila);
            TAXA_CHEGADA = taxaChegada;
            listaEventos = new ListaEventos();
            estatisticaAtual = new Estatistica { Rodada = 0 };
            listaEstatisticas = new List<Estatistica>();
            _geradorEstatisticas = new GeradorEstatisticas();
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
            eventoAtual = listaEventos.NewProximoEvento();

            if (eventoAtual.Tempo == 0)
            {
                listaEventos.NewAdicionaEvento(CalculaChegadaFregues());
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

            listaEventos.NewAdicionaEvento(CalculaChegadaFregues());

        }

        private void EntradaServidor()
        {
            var cliente = fila.RetornaFregues();
            listaEventos.NewAdicionaEvento(CalculaSaidaServidor());

            if (Rodada.Equals(cliente.Tipo))
            {
                _geradorEstatisticas.CalculaSomaAmostras(ref estatisticaAtual, tempo - cliente.TempoChegada);
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

        private Evento CalculaEntradaServidor()
        {
            return new Evento
            {
                Tipo = TipoEvento.ENTRADA_SERVIDOR,
                Tempo = tempo
            };
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
            estatisticaAtual.TempoMedio = _geradorEstatisticas.CalculaMediaAmostral(estatisticaAtual.SomaAmostras, amostras);
            estatisticaAtual.QuantidadeMedia = _geradorEstatisticas.CalculaMediaAmostral(estatisticaAtual.QuantidadeMedia,tempo - tempoInicialRodada);
            listaEstatisticas.Add(estatisticaAtual);

            //Console.WriteLine("Rodada " + Rodada);
            //Console.WriteLine("Quantidade: " + fila.Quantidade);
            //Console.WriteLine("Tempo Medio: " + estatisticaAtual.TempoMedio);
            //Console.WriteLine("Quantidade Media: " + estatisticaAtual.QuantidadeMedia);

        }

        public void CalculaEstatisticasFinais()
        {
            double tempoMedioFinal;
            double varianciaTempoFinal;
            double somaTempoMedio = 0;

            double mediaPessoasFinal;
            double varianciaPessoasFinal;
            double somaQuantidadeMedia = 0;

            IntervaloConfianca icMedia;
            IntervaloConfianca icVariancia;
            IntervaloConfianca icPessoasMedia;
            IntervaloConfianca icPessoasVariancia;
            
            foreach (var estatistica in listaEstatisticas)
            {
                somaTempoMedio += estatistica.TempoMedio;
                somaQuantidadeMedia += estatistica.QuantidadeMedia;
            }

            tempoMedioFinal = _geradorEstatisticas.CalculaMediaAmostral(somaTempoMedio, listaEstatisticas.Count);
            varianciaTempoFinal = _geradorEstatisticas.CalculaVarianciaAmostral(listaEstatisticas.Select(l => l.TempoMedio).ToList(), tempoMedioFinal, listaEstatisticas.Count);

            mediaPessoasFinal = _geradorEstatisticas.CalculaMediaAmostral(somaQuantidadeMedia,listaEstatisticas.Count);
            varianciaPessoasFinal = _geradorEstatisticas.CalculaVarianciaAmostral(listaEstatisticas.Select(l => l.QuantidadeMedia).ToList(), mediaPessoasFinal, listaEstatisticas.Count);

            icMedia = _geradorEstatisticas.CalculaIC(tempoMedioFinal, varianciaTempoFinal, VariavelAleatoria.TSTUDENT, listaEstatisticas.Count);
            icVariancia = _geradorEstatisticas.CalculaIC(tempoMedioFinal, varianciaTempoFinal, VariavelAleatoria.CHIQUADRADO, listaEstatisticas.Count);

            icPessoasMedia = _geradorEstatisticas.CalculaIC(mediaPessoasFinal, varianciaPessoasFinal, VariavelAleatoria.TSTUDENT, listaEstatisticas.Count);
            icPessoasVariancia = _geradorEstatisticas.CalculaIC(mediaPessoasFinal, varianciaPessoasFinal, VariavelAleatoria.CHIQUADRADO, listaEstatisticas.Count);

            //shak altera

            double covTempo =_geradorEstatisticas.CalculaCovariancia(listaEstatisticas.Select(l => l.TempoMedio), tempoMedioFinal);
            double covPessoas = _geradorEstatisticas.CalculaCovariancia(listaEstatisticas.Select(l => l.QuantidadeMedia), mediaPessoasFinal);

            //shak altera

            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine("Rodadas: " + listaEstatisticas.Count + " KMIN: " + Constantes.KMIN + " Utilizacao: " + TAXA_CHEGADA);
            Console.WriteLine("");
            Console.WriteLine("Tempo Medio: " + tempoMedioFinal);
            Console.WriteLine("Variancia Tempo: " + varianciaTempoFinal);
            Console.WriteLine("Intervalo de Confiança Media:");
            Console.WriteLine("    L: {0}, U: {1}, P: {2}", icMedia.L, icMedia.U, icMedia.Precisao);
            Console.WriteLine("Intervalo de Confiança Variancia:");
            Console.WriteLine("    L: {0}, U: {1}, P: {2}", icVariancia.L, icVariancia.U, icVariancia.Precisao);
            Console.WriteLine("Cov: {0}", covTempo);

            Console.WriteLine("");

            Console.WriteLine("Numero de Pessoas Medio: " + mediaPessoasFinal);
            Console.WriteLine("Variancia do numero de pessoas: " + varianciaPessoasFinal);
            Console.WriteLine("Intervalo de Confiança numero de pessoas medio:");
            Console.WriteLine("    L: {0}, U: {1}, P: {2}", icPessoasMedia.L, icPessoasMedia.U, icPessoasMedia.Precisao);
            Console.WriteLine("Intervalo de Confiança variancia numero de pessoas:");
            Console.WriteLine("    L: {0}, U: {1}, P: {2}", icPessoasVariancia.L, icPessoasVariancia.U, icPessoasVariancia.Precisao);
            Console.WriteLine("Cov: {0}", covPessoas);
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
