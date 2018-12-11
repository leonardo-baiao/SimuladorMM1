using System;
using System.Collections.Generic;
using System.Text;

namespace Estruturas
{
    public class FilaLCFS : Fila
    {

        public override void AdicionaFregues(Fregues cliente)
        {
            fila.Add(cliente);
        }

        public override Fregues RetornaFregues()
        {
            var cliente = fila[fila.Count - 1];
            fila.RemoveAt(fila.Count - 1);
            return cliente;
        }

    }
}
