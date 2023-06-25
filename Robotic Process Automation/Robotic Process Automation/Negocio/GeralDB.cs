using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Falco.BancoDados;

namespace Robotic_Process_Automation.Negocio
{
    public class GeralDB
    {

        public DataSet selecionarGlobal(int identificador)
        {
            try
            {
                clsBancoDados.LimparParametros();
                clsBancoDados.AdicionarParametro("@identificador_IN", identificador);

                return clsBancoDados.Executar("usp_SelecionarGlobal");
            }
            catch (Exception exc)
            {
                throw new Exception(exc.Message);
            }
        }


    }
}
