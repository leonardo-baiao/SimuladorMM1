using System;
using System.Collections.Generic;
using System.Text;

namespace Estruturas
{
    //Classe responsável por armazenar os dados do evento.
    public class Evento
    {
        public TipoEvento Tipo { get; set; }
        public double Tempo { get; set; }
    }

    //Enumerado usado para a escolha dos métodos de calculo de eventos. 
    public enum TipoEvento
    {
        CHEGADA_FREGUES = 1,
        SAIDA_SERVIDOR = 2,
    }
}
