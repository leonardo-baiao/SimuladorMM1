using System;
using System.Collections.Generic;
using System.Text;

namespace Estruturas
{
    //Classe que herda o construtor e os métodos da classe Fila. Modificando os métodos de inserção e remoção de fregueses na fila para se comportar como FCFS.
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
