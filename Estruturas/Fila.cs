using System;
using System.Collections.Generic;
using System.Text;

namespace Estruturas
{
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

    public enum TipoFila
    {
        LCFS = 1,
        FCFS= 2
    }
}
