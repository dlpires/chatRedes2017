using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;


namespace chatCliente
{
    public partial class frmCliente : Form
    {
        private string NomeUsuario = "desconhecido";
        private StreamWriter stwEnviador;
        private StreamReader strReceptor;
        private TcpClient tcpServidor;

        private delegate void AtualizaLogCallBack(string strMensagem);

        private delegate void FechaConexaoCallBack(string strMotivo);

        private Thread mensagemThread;
        private IPAddress enderecoIP;
        private bool Conectado;

        public frmCliente()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            if (Conectado == false)
            {
                InicializaConexao();
            }
            else
            {
                FechaConexao("Desconectado a pedido do usuário");
            }
        }

        private void InicializaConexao()
        {
            try
            {


                enderecoIP = IPAddress.Parse(txtServidorIP.Text);

                tcpServidor = new TcpClient();
                tcpServidor.Connect(enderecoIP, 2502);

                Conectado = true;

                NomeUsuario = txtUsuario.Text;

                txtServidorIP.Enabled = false;
                txtUsuario.Enabled = false;
                txtMensagem.Enabled = true;
                btnEnviar.Enabled = true;
                btnConectar.Text = "Desconectado";

                stwEnviador = new StreamWriter(tcpServidor.GetStream());
                stwEnviador.WriteLine(txtUsuario.Text);
                stwEnviador.Flush();

                mensagemThread = new Thread(new ThreadStart(RecebeMensagens));
                mensagemThread.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Erro : " + ex.Message, "Erro na conexão com servidor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RecebeMensagens()
        {
            strReceptor = new StreamReader(tcpServidor.GetStream());
            string ConResposta = strReceptor.ReadLine();
            
            if (ConResposta[0] == '1')
            {
                this.Invoke(new AtualizaLogCallBack(this.AtualizaLog), new object[] { "Conectado com sucesso!" });
            }
            else
            {
                string Motivo = "Não Conectado: ";

                Motivo += ConResposta.Substring(2, ConResposta.Length - 2);

                this.Invoke(new FechaConexaoCallBack(this.FechaConexao), new object[] { Motivo });

                return;
            }

            while (Conectado)
            {
                this.Invoke(new AtualizaLogCallBack(this.AtualizaLog), new object[] { strReceptor.ReadLine() });
            }
        }

        private void AtualizaLog(string strMensagem)
        {
            txtLog.AppendText(strMensagem + "\r\n");
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            EnviaMensagem();
        }

        private void txtMensagem_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                EnviaMensagem();
            }
        }

        private void EnviaMensagem()
        {
            if (txtMensagem.Lines.Length >= 1)
            {
                stwEnviador.WriteLine(txtMensagem.Text);
                stwEnviador.Flush();
                txtMensagem.Lines = null;
            }
            txtMensagem.Text = "";
        }

        private void FechaConexao(string Motivo)
        {
            txtLog.AppendText(Motivo + "\r\n");

            txtServidorIP.Enabled = true;
            txtUsuario.Enabled = true;
            txtMensagem.Enabled = false;
            btnEnviar.Enabled = false;
            btnConectar.Text = "Conectado";

            Conectado = false;
            stwEnviador.Close();
            strReceptor.Close();
            tcpServidor.Close();
        }
        
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Conectado == true)
            {
                Conectado = false;
                stwEnviador.Close();
                strReceptor.Close();
                tcpServidor.Close();
            }
        }
    }
}
