using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using LogGenerator.Models;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogGenerator
{
    public static class LogGenerator
    {
        #region Declaracoes 

        private static object m_safe = new();
        private static string m_diretorio = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string m_nomeArquivo = "";
        private static bool m_deveExecutar = false;
        private const string ARQUIVO_CONFIGURACAO = @"\Config.txt";

        private const int TAMANHO_MAXIMO = 10 * 1024 * 1024; //10mb
        private const int NUMERO_MAXIMO_CHAMADAS = 10;

        private static Dictionary<string, int> tarefas = new();

        #endregion

        #region public 

        public static void GravarLog(string textoLog, int configuracao = 0, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            Inicializar(configuracao);

            if (!m_deveExecutar)
            {
                return;
            }

            try
            {
                Adicionar(memberName);

                lock (m_safe)
                {

                    var data = DateTime.Now.ToShortDateString();
                    var hora = DateTime.Now.ToShortTimeString();

                    var bytesLogs = Encoding.Default.GetBytes(data + " " + hora + " : " + textoLog + Environment.NewLine);

                    using FileStream fileStream = new(m_diretorio + m_nomeArquivo, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                    var posicaoFinal = Math.Max(fileStream.Length - 1, 0);

                    var posicaoComecoTamanhoMaximo = Convert.ToInt32(posicaoFinal + bytesLogs.Length) - TAMANHO_MAXIMO;

                    if (posicaoComecoTamanhoMaximo > 0 && posicaoFinal >= posicaoComecoTamanhoMaximo)
                    {
                        var buffer = new byte[posicaoFinal - posicaoComecoTamanhoMaximo];

                        fileStream.Seek(posicaoComecoTamanhoMaximo, SeekOrigin.Begin);
                        fileStream.Read(buffer, 0, buffer.Length);

                        fileStream.Seek(0, SeekOrigin.Begin);
                        fileStream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        fileStream.Seek(0, SeekOrigin.End);
                    }

                    fileStream.Write(bytesLogs, 0, bytesLogs.Length);

                    Remover(memberName, configuracao);
                }
            }
            catch (Exception ex)
            {
                GravarExceptionEvent(ex);
            }
        }

        private static void Remover(string memberName, int configuracao)
        {
            if (tarefas.TryGetValue(memberName, out int valor))
            {
                tarefas[memberName] = valor - 1;

                if (valor > NUMERO_MAXIMO_CHAMADAS)
                {
                    SalvarConfigs(configuracao, false);
                }
            }
        }

        public static void GravarLog(object obj)
        {
            try
            {
                var objSerialize = JsonSerializer.Serialize(obj);

                GravarLog(objSerialize);
            }
            catch (Exception ex)
            {
                GravarExceptionEvent(ex);
            }
        }

        public static void SalvarConfigs(int config, bool valor)
        {
            try
            {
                var arquivo = m_diretorio + ARQUIVO_CONFIGURACAO;

                var arquivoDeserialize = JsonSerializer.Deserialize<List<Configs>>(File.ReadAllText(arquivo)).ToArray();

                if (config < arquivoDeserialize.Length && config >= 0)
                {
                    arquivoDeserialize[config].GravarArquivo = valor;

                    using StreamWriter arquivoLog = new(arquivo, false);
                    arquivoLog.WriteLine(JsonSerializer.Serialize(arquivoDeserialize));
                }
            }
            catch (Exception ex)
            {
                GravarExceptionEvent(ex);
            }
        }

        #endregion

        #region private 

        private static void GravarExceptionEvent(Exception ex)
        {
            if (OperatingSystem.IsWindows())
            {
                EventLog.WriteEntry("LogGenerator", "Erro ao Gravar Logs: \n\n" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        private static void Adicionar(string memberName)
        {
            if (tarefas.TryGetValue(memberName, out int valor))
            {
                tarefas[memberName] = valor + 1;

                return;
            }

            tarefas.TryAdd(memberName, 1);
        }

        private static void Inicializar(int configuracao)
        {
            var arquivo = m_diretorio + ARQUIVO_CONFIGURACAO;

            try
            {
                if (!File.Exists(arquivo))
                {
                    var config = new Configs[] { new Configs() };
                    var configPadrao = JsonSerializer.Serialize(config);

                    using StreamWriter arquivoLog = new(arquivo);
                    arquivoLog.WriteLine(configPadrao);

                    m_nomeArquivo = config[0].NomeArquivo;
                    m_deveExecutar = config[0].GravarArquivo;
                    return;
                }

                var arquivoDeserialize = JsonSerializer.Deserialize<List<Configs>>(File.ReadAllText(arquivo));

                if (configuracao < arquivoDeserialize.Count && configuracao >= 0)
                {
                    m_nomeArquivo = arquivoDeserialize[configuracao].NomeArquivo;
                    m_deveExecutar = arquivoDeserialize[configuracao].GravarArquivo;
                    return;
                }

                m_nomeArquivo = arquivoDeserialize[0].NomeArquivo;
                m_deveExecutar = arquivoDeserialize[0].GravarArquivo;
            }
            catch (Exception ex)
            {
                GravarExceptionEvent(ex);
                var padrao = new Configs();

                m_nomeArquivo = padrao.NomeArquivo;
                m_deveExecutar = padrao.GravarArquivo;
            }
        }

        #endregion

    }
}

