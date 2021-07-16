using LogGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using Xunit;

namespace TestLogGenerator
{
    public class TestLogGenerator
    {
        private string m_diretorio = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private const int TAMANHO_MAXIMO = 10 * 1024 * 1024; //10mb

        [Fact]
        public void GravarLog()
        {
            var texto = "Log";
            LogGenerator.LogGenerator.GravarLog(texto);

            Assert.True(File.Exists(m_diretorio + @"\Log.txt"));
            Assert.True(File.Exists(m_diretorio + @"\Config.txt"));

            var textFinal = File.ReadAllLines(m_diretorio + @"\Log.txt")[0][19..];
            Assert.NotEmpty(textFinal);
            Assert.Equal(texto, textFinal);

            Limpar();
        }

        [Fact]
        public void GravarLogMultiplasConfigs()
        {
            var texto = "Log";

            var configs = new Configs()
            {
                NomeArquivo = @".\Logs2.txt"
            };

            var config = new Configs[] { new Configs(), new Configs(), configs };

            var configPadrao = JsonSerializer.Serialize(config);

            using (StreamWriter arquivoLog = new(m_diretorio + @"\Config.txt"))
            {
                arquivoLog.WriteLine(configPadrao);
            }

            LogGenerator.LogGenerator.GravarLog(texto, configuracao: 2);

            Assert.True(File.Exists(m_diretorio + configs.NomeArquivo));
            Assert.True(File.Exists(m_diretorio + @"\Config.txt"));

            var textoFinal = File.ReadAllLines(m_diretorio + configs.NomeArquivo)[0][19..];

            Assert.NotEmpty(textoFinal);
            Assert.Equal(texto, textoFinal);

            Limpar();
        }

        [Fact]
        public void MudarConfiguracaoMultiplas()
        {
            var configs = new Configs()
            {
                NomeArquivo = @".\Logs2.txt"
            };

            var config = new Configs[] { new Configs(), new Configs(), configs };

            var configPadrao = JsonSerializer.Serialize(config);

            using (StreamWriter arquivoLog = new(m_diretorio + @"\Config.txt"))
            {
                arquivoLog.WriteLine(configPadrao);
            }

            LogGenerator.LogGenerator.SalvarConfigs(2, false);

            var arquivoDeserialize = JsonSerializer
                    .Deserialize<List<Configs>>(File.ReadAllText(m_diretorio + @"\Config.txt"));

            Assert.False(arquivoDeserialize[2].GravarArquivo);

            Limpar();
        }

        [Fact]
        public void MudarConfiguracao()
        {
            var config = new Configs[] { new Configs() };

            var configPadrao = JsonSerializer.Serialize(config);

            using (StreamWriter arquivoLog = new(m_diretorio + @"\Config.txt"))
            {
                arquivoLog.WriteLine(configPadrao);
            }

            LogGenerator.LogGenerator.SalvarConfigs(0, false);

            var arquivoDeserialize = JsonSerializer
                    .Deserialize<List<Configs>>(File.ReadAllText(m_diretorio + @"\Config.txt"));

            Assert.False(arquivoDeserialize[0].GravarArquivo);

            Limpar();
        }

        [Fact]
        public void GravarLogComParar()
        {
            Limpar();

            var texto = "Loga";
            LogGenerator.LogGenerator.GravarLog(texto);

            Assert.True(File.Exists(m_diretorio + @"\Log.txt"));
            Assert.True(File.Exists(m_diretorio + @"\Config.txt"));

            var textFinal = File.ReadAllLines(m_diretorio + @"\Log.txt", System.Text.Encoding.Latin1)[0][19..];
            Assert.NotEmpty(textFinal);
            Assert.Equal(texto, textFinal);

            LogGenerator.LogGenerator.SalvarConfigs(0, false);

            LogGenerator.LogGenerator.GravarLog(texto);
            LogGenerator.LogGenerator.GravarLog(texto);
            LogGenerator.LogGenerator.GravarLog(texto);
            LogGenerator.LogGenerator.GravarLog(texto);

            var lista = File.ReadAllLines(m_diretorio + @"\Log.txt", System.Text.Encoding.Latin1);

            Assert.Single(lista);

            LogGenerator.LogGenerator.SalvarConfigs(0, true);

            LogGenerator.LogGenerator.GravarLog(texto);
            textFinal = File.ReadAllLines(m_diretorio + @"\Log.txt", System.Text.Encoding.UTF8)[0][19..];
            Assert.NotEmpty(textFinal);
            Assert.Equal(texto, textFinal);

            textFinal = File.ReadAllLines(m_diretorio + @"\Log.txt", System.Text.Encoding.UTF8)[1][19..];
            Assert.NotEmpty(textFinal);
            Assert.Equal(texto, textFinal);

            Limpar();
        }

        [Fact]
        public void GravarLogPreventLoop()
        {
            Thread a = new Thread(x => {
                var texto = "Loga";

                while (true)
                {
                    LogGenerator.LogGenerator.GravarLog(texto);
                }
            });

            a.Start();


            Thread b = new Thread(x => {
                var texto = "Loga";

                while (true)
                {
                    LogGenerator.LogGenerator.GravarLog(texto);
                }
            });

            b.Start();

            var texto = "Loga";

            while (true)
            {
                LogGenerator.LogGenerator.GravarLog(texto);
            }

        }

        public void aaaa(string texto)
        {
           
        }

        [Fact]
        public void GravarTamanhoMaximo()
        {
            Limpar();

            long tamanho = 0;

            for (int i = 0; i < 3700; i++)
            {
                var texto = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
                LogGenerator.LogGenerator.GravarLog(texto);
            }

            using (FileStream fileStream = new(m_diretorio + @"\Log.txt", FileMode.Append))
            {
                tamanho = fileStream.Length;
            };

            Assert.True(tamanho > TAMANHO_MAXIMO);

            Limpar();
        }


        private void Limpar()
        {
            if (File.Exists(m_diretorio + @"\Log.txt"))
            {
                File.Delete(m_diretorio + @"\Log.txt");
            }

            if (File.Exists(m_diretorio + @"\Config.txt"))
            {
                File.Delete(m_diretorio + @"\Config.txt");
            }

            if (File.Exists(m_diretorio + @"\Logs2.txt"))
            {
                File.Delete(m_diretorio + @"\Logs2.txt");
            }
        }
    }
}
