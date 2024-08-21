using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MeuPrograma
{
    class Program
    {
        static async Task<float?> GetRegularMarketPriceAsync(string active)
        {
            string baseUrl =
                "https://brapi.dev/api/quote/" + active + "?token=jjM3uGcXaAJ7Raq3i26xxz";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(baseUrl))
                    {
                        using (HttpContent content = res.Content)
                        {
                            var data = await content.ReadAsStringAsync();
                            if (data != null)
                            {
                                var dataObj = JObject.Parse(data);
                                var regularMarketPriceToken = dataObj["results"]
                                    ?[0]
                                    ?["regularMarketPrice"];
                                if (regularMarketPriceToken != null)
                                {
                                    return regularMarketPriceToken.Value<float>();
                                }
                                else
                                {
                                    Console.WriteLine("Preço atual não encontrado.");
                                    return null;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Sem dados.");
                                return null;
                            }
                        }
                    }
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
            Config config = JsonSerializer.Deserialize<Config>(configContent);
            Console.WriteLine($"Configuração: {config.EmailDestino}");

            if (args.Length < 3)
            {
                Console.WriteLine("Por favor, forneça 3 argumentos: <baseUrl> <arg2> <arg3>");
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
                    From = new MailAddress(config.SmtpConfig.Username),
                    Subject = "Alerta de Preço",
                    Body = mensagem,
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(config.EmailDestino);
                Console.WriteLine($"Eemaill {mailMessage}");
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
