using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Falco.BancoDados;

namespace Robotic_Process_Automation
{
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //if (args.Length > 0)
            //{
            //    clsBancoDados.Servidor = args[0];
            //    clsBancoDados.Banco = args[1];
            //    clsBancoDados.Usuario = args[2];
            //    clsBancoDados.Senha = args[3];
            //}
            //else
            //{
            //    clsBancoDados.Servidor = "192.168.10.14";
            //    clsBancoDados.Banco = "MASERP_DEV";
            //    clsBancoDados.Usuario = "JOSE.SILVA";
            //    clsBancoDados.Senha = "123";
            //    clsBancoDados.Conectar(true);
            //}

            Application.Run(new Form1());
        }


    }
}
