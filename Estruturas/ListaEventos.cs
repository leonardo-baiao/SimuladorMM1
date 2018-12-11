using System;
using System.Collections.Generic;
using System.Linq;

namespace Estruturas
{
    public class ListaEventos
    {
        private List<Evento> listaEventos;
        Evento proximaChegada;
        Evento proximaSaida;

        public ListaEventos()
        {
            listaEventos = new List<Evento>();
            proximaChegada = new Evento();
            proximaSaida = new Evento();
        }

        public Evento ProximoEvento()
        {
            try
            {
                var prox = listaEventos[0];
                RemoveEvento();
                return prox;
            }
            catch { return null; }
        }

        public void AdicionaEvento(Evento evento)
        {
            try
            {
                listaEventos.Insert(listaEventos.FindIndex(e => e.Tempo > evento.Tempo), evento);
            }
            catch (Exception) { listaEventos.Add(evento); }
        }

        public void RemoveEvento()
        {
            listaEventos.RemoveAt(0);
        }


        public void NewAdicionaEvento(Evento evento)
        {
            if (evento.Tipo == TipoEvento.CHEGADA_FREGUES)
            {
                proximaChegada = evento;
            }
            else
                proximaSaida = evento;
        }

        public Evento NewProximoEvento()
        {
            if (proximaChegada.Tempo <= proximaSaida.Tempo || proximaSaida.Tempo == 0)
                return proximaChegada;
            else
                return proximaSaida;
        }

    }
}