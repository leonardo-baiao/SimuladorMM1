using System;
using System.Collections.Generic;
using System.Text;

namespace Estruturas
{

    //Classe abstrata responsável pelo armazenamento de dados na fila. É usada como base para as classes FilaFCFS e FilaLCFS. 
    public abstract class Fila
    {
        protected List<Fregues> fila;

        public Fila()
        {
            fila = new List<Fregues>();
        }

        public int Quantidade{ get { return fila.Count; } }

        public abstract void AdicionaFregues(Fregues cliente);
        public abstract Fregues RetornaFregues();
    }

    //Enumerado responsável pela escolha do tipo de fila no inicio da simulação.
    public enum TipoFila
    {
        LCFS = 1,
        FCFS= 2
    }
}
