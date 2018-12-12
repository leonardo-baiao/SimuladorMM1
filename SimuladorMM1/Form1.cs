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

            kayChart medias = new kayChart(chart1, 600);

            medias.serieName = "N° Pessoas";

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
                        label2.Text = "Calculando rodada: " + rodadas + "  Tempo de começo: " + data_hora_comeco.ToLongTimeString();
                    });
                    rodadas = _simulacao.listaEstatisticas.Count;
                }
                this.Invoke((MethodInvoker)delegate
                {
                    //double tempoMax = _simulacao.listaMediaTempoRodada.Max();
                    //double pessoasMax = _simulacao.listaMediaPessoasRodada.Max();

                    //double tempoMin = _simulacao.listaMediaTempoRodada.Min();
                    //double pessoasMin = _simulacao.listaMediaPessoasRodada.Min();

                    //chart1.ChartAreas[0].AxisY.Maximum = tempoMax > pessoasMax ? tempoMax : pessoasMax;
                    //chart1.ChartAreas[0].AxisY.Minimum = tempoMin > pessoasMin ? pessoasMin : tempoMin;

                    chart1.Series["N° Pessoas"].Points.DataBindY(_simulacao.listaMediaPessoasRodada);

                    chart3.Series["Variância de Pessoas"].Points.DataBindY(_simulacao.listaVarianciaP);

                    chart1.Series["Tempo Médio"].Points.DataBindY(_simulacao.listaMediaTempoRodada);

                    chart3.Series["Variância do Tempo"].Points.DataBindY(_simulacao.listaVarianciaT);
                });
                data_hora_fim = DateTime.Now;
                this.Invoke((MethodInvoker)delegate
                {
                    label2.Text = "Fim da simulação" + "  Tempo de começo: " + data_hora_comeco.ToLongTimeString() + "  Tempo final: " + data_hora_fim.ToLongTimeString();

                    label4.Text = "E: " + _simulacao.mediaPessoasFinal + "  V: " + _simulacao.varianciaPessoasFinal + "\n"
                                + "L(E): " + _simulacao.icPessoasMedia.L + "  U(E): " + _simulacao.icPessoasMedia.U + "  P(E): " + _simulacao.icPessoasMedia.Precisao + "\n"
                                + "L(V): " + _simulacao.icPessoasVariancia.L + "  U(V):  " + _simulacao.icPessoasVariancia.U + "  P(V): " + _simulacao.icPessoasVariancia.Precisao;

                    label5.Text = "E: " + _simulacao.tempoMedioFinal + "  V: " + _simulacao.varianciaTempoFinal + "\n"
                                + "L(E): " + _simulacao.icMedia.L + "  U(E): " + _simulacao.icMedia.U + "  P(E): " + _simulacao.icMedia.Precisao + "\n"
                                + "L(V): " + _simulacao.icVariancia.L + "  U(V):  " + _simulacao.icVariancia.U + "  P(V): " + _simulacao.icVariancia.Precisao;
                });
            });          
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {
            
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}
