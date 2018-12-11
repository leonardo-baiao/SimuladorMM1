using System;
using System.Collections.Generic;
using System.Text;

namespace Estruturas
{
    public class FilaFCFS : Fila
    {

        public override void AdicionaFregues(Fregues cliente)
        {
            fila.Add(cliente);
        }

        public override Fregues RetornaFregues()
        {
            var cliente = fila[0];
            fila.RemoveAt(0);
            return cliente;
        }
    }
}
