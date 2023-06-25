using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using UtilsNeobetel;
using Robotic_Process_Automation.Negocio;


using OpenPop.Mime;
using OpenPop.Pop3;
using System.Runtime.InteropServices.ComTypes;
using System.Net.Mail;

namespace Robotic_Process_Automation
{
    public partial class Form1 : Form
    {
        //--> Login
        private string sServer;
        private int uPorta;
        private string sEmail;
        private string sSenha;

        //-->Filtros

        private string sRemetente;
        private string sAssunto;
        private string sCorpo;
        private string sExtensao;

        //-> Geral
        private string sCaminhoDestino;


        //---------------------------------------------------------------------------
        //----------------------------> Barra titulo <-------------------------------
        // Importar APIs do Windows para o movimento do formulário
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HT_CAPTION = 0x0002;

        public Form1()
        {
            InitializeComponent();
        }

        private void pnlBarraTitulo_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMinimized_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //--------------------------> Tratamento dos txt <---------------------------

        private void Form1_Load(object sender, EventArgs e)
        {
            cbxExtensao.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void txtTimer_KeyPress(object sender, KeyPressEventArgs e)
        {
            UtilGeral.somenteNumeroTextBox(sender, e);
        }

        private void txtPorta_KeyPress(object sender, KeyPressEventArgs e)
        {
            UtilGeral.somenteNumeroTextBox(sender, e);
        }

        private void txtGlobal_KeyPress(object sender, KeyPressEventArgs e)
        {
            UtilGeral.somenteNumeroTextBox(sender, e);
        }

        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //------------------------------> Start/Pause <------------------------------
        private void btnStart_Click(object sender, EventArgs e)
        {
            timer1.Interval = Convert.ToInt32(txtTimer.Text);
            timer1.Enabled = true;
            lsbHistorico.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + "Timer iniciado");

            //--> Login
            sServer = txtServer.Text;
            uPorta = Convert.ToInt32(txtPorta.Text);
            sEmail = txtEmail.Text;
            sSenha = txtSenha.Text;

            //-->Filtros
            sRemetente = txtRemetente.Text;
            sAssunto = txtAssunto.Text;
            sCorpo = txtCorpo.Text;
            sExtensao = cbxExtensao.Text;
            
            pictureBox2.Visible = true;
            pictureBox3.Visible = false;        

        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            lsbHistorico.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + "Pause");

            pictureBox2.Visible = false;
            pictureBox3.Visible = true;
        }

        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //---------------------------------> Timer <---------------------------------

        private void timer1_Tick(object sender, EventArgs e)
        {

            try
            {
                Cursor = Cursors.WaitCursor;
                BuscarEmails(sServer, uPorta, sEmail, sSenha, sRemetente, sAssunto, sCorpo, sExtensao);

            }
            catch (Exception ex)
            {
                MessageBox.Show(UtilGeral.mensagemErro(ex), "Neobetel - ERRO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cursor = Cursors.Default;
                Application.DoEvents();
            }
            Cursor = Cursors.Default;

        }

        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //----------------------------------> Geral <--------------------------------
       

        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //----------------------------------> RPA <----------------------------------
        public void BuscarEmails(string servidorPop3, int porta, string email, string senha, string remetente, string assunto, string corpo, string extensao)
        {
            string sPastaDestino = sCaminhoDestino;
            string sCaminhoCompleto = "";
            try
            {

                Cursor = Cursors.WaitCursor;

                using (var client = new Pop3Client())
                {
                    // Conectar ao servidor POP3
                    client.Connect(servidorPop3, porta, true);

                    // Autenticar com as credenciais do email
                    client.Authenticate(email, senha);

                    // Obter o número total de emails na caixa de entrada
                    int count = client.GetMessageCount();

                    for (int i = 1; i <= count; i++)
                    {
                        // Obter o email atual
                        var message = client.GetMessage(i);

                        // Verificar se o email atende aos critérios de filtro
                        if ((!string.IsNullOrEmpty(remetente) && message.Headers.From.Address.Contains(remetente)) ||
                            (!string.IsNullOrEmpty(assunto) && message.Headers.Subject.Contains(assunto)) ||
                            (!string.IsNullOrEmpty(corpo) && message.MessagePart.GetBodyAsText().Contains(corpo)))
                        {
                            //--> Seleciona a pasta pela Global                     
                            verificaSePastaExiste(sPastaDestino);

                            // Verificar se o email possui anexos
                            if (message.FindAllAttachments().Count > 0)
                            {
                                // Percorrer todos os anexos do email
                                foreach (var attachment in message.FindAllAttachments())
                                {

                                    // Especificar o caminho e o nome do arquivo
                                    sCaminhoCompleto = Path.Combine(sPastaDestino, attachment.FileName);

                                    // Verificar se o arquivo já existe (foi baixado anteriormente) 
                                    if (verificaSeJaFoiBaixado(sPastaDestino, attachment.FileName) == false)
                                    {
                                        // Verificar se a extensão do arquivo vai ser filtrada
                                        if (extensao != "" && extensao != "TODOS")
                                        {
                                            if (Path.GetExtension(attachment.FileName).Equals(extensao, StringComparison.OrdinalIgnoreCase))
                                            {
                                                // Salvar o anexo em um arquivo local
                                                using (var stream = new FileStream(sCaminhoCompleto, FileMode.Create))
                                                {
                                                    attachment.Save(stream);
                                                }

                                                lsbHistorico.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + sCaminhoCompleto + " Arquivo baixado com sucesso!");
                                            }
                                        }
                                        else
                                        {
                                            // Salvar o anexo em um arquivo local
                                            using (var stream = new FileStream(sCaminhoCompleto, FileMode.Create))
                                            {
                                                attachment.Save(stream);
                                            }
                                            lsbHistorico.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + sCaminhoCompleto + " Arquivo baixado com sucesso!");
                                        }                                     
                                    } 
                                    else 
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    // Desconectar do servidor POP3
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                lsbHistorico.Items.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + "Falha: " + ex.Message);
                Cursor = Cursors.Default;
            }

            Cursor = Cursors.Default;
        }
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        //------------------------------> Validação <--------------------------------
        public bool verificaSeJaFoiBaixado(string pastaDestinoRPA, string nomeDoArquivo)
        {
      
            //-->Verifica na pasta destino do RPA e da pasta destino do Importador de XML se o arquivo existe
            if (File.Exists(Path.Combine(pastaDestinoRPA, nomeDoArquivo)))
            {                      
               return true;
            }
            else
            {
                return false;
            }
        }

        public void verificaSePastaExiste(string pasta)
        {
            if(!Directory.Exists(pasta))
            {
                Directory.CreateDirectory(pasta);
            }          
        }

        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
    }
}
