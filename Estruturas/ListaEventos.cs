using System;
using System.Collections.Generic;
using System.Linq;

namespace Estruturas
{
    //Classe responsável pela lógica da lista de eventos. Utiliza duas variaveis que são atualizadas com os novos eventos de chegada de fregues e saida do servidor.   
    public class ListaEventos
    {
        Evento proximaChegada;
        Evento proximaSaida;

        //Construtor da classe.
        public ListaEventos()
        {
            proximaChegada = new Evento();
            proximaSaida = new Evento();
        }
        
        //Adiciona o evento na variável responsável pelo tipo do evento.
        public void AdicionaEvento(Evento evento)
        {
            if (evento.Tipo == TipoEvento.CHEGADA_FREGUES)
            {
                proximaChegada = evento;
            }
            else
                proximaSaida = evento;
        }

        //Retorna o evento com menor tempo.
        public Evento ProximoEvento()
        {
            if (proximaChegada.Tempo <= proximaSaida.Tempo || proximaSaida.Tempo == 0)
                return proximaChegada;
            else
                return proximaSaida;
        }

    }
}