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
        int rodadas;
        DateTime data_hora_comeco;
        DateTime data_hora_fim;

        public Form1()
        {
            InitializeComponent();

            rodadas = 0;
            estatisticas = new List<Estatistica>();
            fila = TipoFila.FCFS;
            utilizacao = 0.2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool calculando;
            button1.Enabled = false;
            comboBox1.Enabled = false;
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;

            kayChart numeroPessoas = new kayChart(chart1, 600);

            kayChart tempoMedio = new kayChart(chart2, 600);

            numeroPessoas.serieName = "N° Pessoas";
            tempoMedio.serieName = "Tempo Médio";

            _simulacao = new Simulador(fila, utilizacao);

            calculando = true;
            data_hora_comeco = DateTime.Now;
            Task.Factory.StartNew(() =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    label2.Text = "Calculando rodada: " + rodadas + "  Tempo comeco: " + data_hora_comeco.ToLongTimeString();
                });
                _simulacao.IniciarSimulacao();
                calculando = false;
            });

            Task.Factory.StartNew(() =>
            {
                while (calculando)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        label2.Text = "Calculando rodada: " + rodadas + "  Tempo comeco: " + data_hora_comeco.ToLongTimeString();
                    });
                    rodadas = _simulacao.listaEstatisticas.Count;
                }
                this.Invoke((MethodInvoker)delegate
                {
                    List<double> list = new List<double>(_simulacao.listaEstatisticas.Select(l => l.QuantidadeMedia));
                    chart1.Series["N° Pessoas"].Points.DataBindY(list);
                });
                data_hora_fim = DateTime.Now;
                this.Invoke((MethodInvoker)delegate
                {
                    label2.Text = "Fim da simulação" + "  Tempo comeco: " + data_hora_comeco.ToLongTimeString() + "  Tempo final: " + data_hora_fim.ToLongTimeString() + "\n\n"
                                + "Tempo Médio Final: ";
                });
            });

            Task.Factory.StartNew(() =>
            {
                while (calculando) ;
                this.Invoke((MethodInvoker)delegate
                {
                    List<double> list = new List<double>(_simulacao.listaEstatisticas.Select(l => l.TempoMedio));
                    chart2.Series["Tempo Médio"].Points.DataBindY(list);
                });
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


        private void label2_Click(object sender, EventArgs e)
        {
            
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

    }
}
