using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetEnv;
using Newtonsoft.Json.Linq;

namespace MeuPrograma
{
    class Program
    {
        static async Task<float?> GetRegularMarketPriceAsync(string active)
        {
            Env.Load();
            string token = Environment.GetEnvironmentVariable("API_TOKEN");
            string baseUrl = $"https://brapi.dev/api/quote/{active}?token={token}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage res = await client.GetAsync(baseUrl);
                    res.EnsureSuccessStatusCode();
                    string data = await res.Content.ReadAsStringAsync();
                    var dataObj = JObject.Parse(data);
                    var regularMarketPriceToken = dataObj["results"]?[0]?["regularMarketPrice"];
                    return regularMarketPriceToken?.Value<float>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro: {e.Message}");
                return null;
            }
        }

        static async Task Main(string[] args)
        {
            // Definir a cultura para InvariantCulture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // Ler a configuração do arquivo JSON
            string configContent = await File.ReadAllTextAsync("config.json");
            if (string.IsNullOrWhiteSpace(configContent))
            {
                Console.WriteLine("Arquivo de configuração vazio.");
                return;
            }
            Config config = JsonSerializer.Deserialize<Config>(configContent);
            if (config == null)
            {
                Console.WriteLine("Erro ao desserializar o arquivo de configuração.");
                return;
            }

            if (args.Length < 3)
            {
                Console.WriteLine(
                    "Por favor, forneça 3 argumentos: <ativo> <precoVenda> <precoCompra>"
                );
                return;
            }

            string active = args[0];
            if (!float.TryParse(args[1], out float sellRef))
            {
                Console.WriteLine("O segundo argumento deve ser um número válido.");
                return;
            }
            if (!float.TryParse(args[2], out float buyRef))
            {
                Console.WriteLine("O terceiro argumento deve ser um número válido.");
                return;
            }

            while (true)
            {
                await Task.Delay(3000);
                float? price = await GetRegularMarketPriceAsync(active);
                if (price != null)
                {
                    if (price > sellRef)
                    {
                        Console.WriteLine($"Venda {price}");
                        EnviarEmail(config, $"Preço atual: {price} - Venda recomendada");
                    }
                    if (price < buyRef)
                    {
                        Console.WriteLine($"Compra {price}");
                        EnviarEmail(config, $"Preço atual: {price} - Compra recomendada");
                    }
                }
            }
        }

        static void EnviarEmail(Config config, string mensagem)
        {
            try
            {
                if (config.SmtpConfig == null || config.EmailDestino == null)
                {
                    Console.WriteLine("Configuração de SMTP ou e-mail de destino não fornecida.");
                    return;
                }

                var smtpClient = new SmtpClient(config.SmtpConfig.Host)
                {
                    Port = config.SmtpConfig.Port,
                    Credentials = new NetworkCredential(
                        config.SmtpConfig.Username,
                        config.SmtpConfig.Password
                    ),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        config.SmtpConfig.Username
                            ?? throw new ArgumentNullException(nameof(config.SmtpConfig.Username))
                    ),
                    Subject = "Alerta de Preço",
                    Body = mensagem,
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(
                    config.EmailDestino
                        ?? throw new ArgumentNullException(nameof(config.EmailDestino))
                );
                smtpClient.Send(mailMessage);
                Console.WriteLine("E-mail enviado com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
            }
        }
    }
}
