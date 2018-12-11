using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Estruturas;
using SimuladorAD;
using rtChart;
using System.Diagnostics;
using Estatisticas;
using System.Threading;

namespace SimuladorMM1
{
    public partial class Form1 : Form
    {
        private Simulador _simulacao;

        private double utilizacao;
        private TipoFila fila;
        private List<Estatistica> estatisticas;
        int index, index2;


        public Form1()
        {
            InitializeComponent();

            index = 0;
            estatisticas = new List<Estatistica>();
            fila = TipoFila.FCFS;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            kayChart numeroPessoas = new kayChart(chart1, 600);

            kayChart tempoMedio = new kayChart(chart2, 600);

            numeroPessoas.serieName = "N° Pessoas";
            tempoMedio.serieName = "Tempo Médio";

            _simulacao = new Simulador(fila, utilizacao);


            Task.Factory.StartNew(() =>
            {
                _simulacao.IniciarSimulacao();
            });

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                numeroPessoas.updateChart(updatePessoas, 20);
            });

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                tempoMedio.updateChart(updateTempo, 20);
            });
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            utilizacao = double.Parse(comboBox.SelectedItem.ToString());
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            fila = TipoFila.FCFS;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            fila = TipoFila.LCFS;
        }

        private double updatePessoas()
        {
            if(index + 1 < _simulacao.listaEstatisticas.Count)
            {
                index++;
            }
            return _simulacao.listaEstatisticas[index].QuantidadeMedia;
        }

        private double updateTempo()
        {
            if(index2 + 1 < _simulacao.listaEstatisticas.Count)
            {
                index2++;
            }
            return _simulacao.listaEstatisticas[index2].TempoMedio;
        }
    }
}
