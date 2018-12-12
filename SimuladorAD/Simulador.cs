using Estatisticas;
using Estruturas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimuladorAD
{
    public class Simulador
    {
        // variaveis principais
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


        //Construtor da classe
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

        /* Método principal da simulação. Chamada as rotinas de processamento de rodada transiente, processamento de eventos, calculo de estatisticas e incremento de rodadas. 
         * No final de todas as rodadas, chama a rotina de calculo de estatisticas finais.
         */
        public void IniciarSimulacao()
        {

            ProcessaRodadaTransiente();

            while (Rodada <= Constantes.MAX_RODADAS)
            {
                ProcessaEventos(Constantes.KMIN);

                CalculaEstatisticas();

                ProximaRodada();
            }
            CalculaEstatisticasFinais();
        }

        public int Rodada { get; private set; }
        
        //Responsável pelo processamento da rodada transiente. Utiliza um KMIN especifico para a rodada transiente.
        internal void ProcessaRodadaTransiente()
        {
            ProcessaEventos(Constantes.KTRANS);
            ProximaRodada();
        }

        //Método para inicialização de nova rodada. incrementa a variável de rodada e zera as variáveis de controle.
        public void ProximaRodada()
        {
            Rodada++;

            if (Rodada > Constantes.MAX_RODADAS)
                return;

            estatisticaAtual = new Estatistica { Rodada = Rodada };
            amostras = 0;
            tempoInicialRodada = tempo;
        }
        
        //Método que itera o tratamento dos eventos enquanto não atingir o número de amostras definido.
        public void ProcessaEventos(int K)
        {
            while(amostras < K)
            {
                TrataEvento();
            }
        }

        //Método para o tratamento de eventos. Recebe os novos eventos e gerencia o tipo de tratamento respectivo.
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


        /* Método para o tratamento da chegada de fregueses. 
         * Salva a quantidade média de pessoas na fila e insere um novo fregues na fila. 
         * Calcula o proximo evento de chegada.
         */
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

        //Método para o tratamento da entrada de fregueses no servidor. Retira o fregues da fila e calcula o proximo evento de saida do servidor.
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

        /* Método para o tratamento da saida de fregueses do servidor. 
         * Salva a quantidade média de pessoas na fila e insere um novo fregues na fila.
         * Inicia uma nova entrada no servidor, caso haja pessoas na fila de espera.
         */
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

        //Método para o calculo do tempo de atendimento no servidor. Calcula uma amostragem exponencial e salva no evento.
        private Evento CalculaSaidaServidor()
        {
            return new Evento
            {
                Tipo = TipoEvento.SAIDA_SERVIDOR,
                Tempo = tempo + _geradorEstatisticas.CalculaExponencial(Constantes.TAXA_SERVIDOR)
            };
        }

        //Método para o calculo da chegada de fregueses a fila. Calcula uma amostragem exponencial e salva no evento.
        private Evento CalculaChegadaFregues()
        {
            return new Evento
            {
                Tipo = TipoEvento.CHEGADA_FREGUES,
                Tempo = tempo + _geradorEstatisticas.CalculaExponencial(TAXA_CHEGADA)
            };
        }
        
        /* Método que calcula as estatisticas de cada Rodada.
         * Calcula a média e a variancia amostral do Tempo de espera na fila e da quantidade de pessoas na fila.
         */
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


        /* Método que calcula as estatisticas finais.
         * Calcula a media e a variancia final do tempo de espera na fila e da quantidade de fregueses na fila.
         * Calcula os ICs da média e da variancia e suas respectivas precisoes;
         */
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
        
        //Instancia uma nova fila, de acordo com o tipo de fila requisitado.
        private void GeraFila(TipoFila tipoFila)
        {
            if (tipoFila.Equals(TipoFila.FCFS))
                fila = new FilaFCFS();
            else
                fila = new FilaLCFS();
        }
    }
}
