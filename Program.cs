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
        static async Task Main(string[] args)
        {
            Env.Load();
            var marketService = new MarketService();
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // Ler a configuração do arquivo JSON
            string? configContent = await File.ReadAllTextAsync("config.json");
            Config? config = JsonSerializer.Deserialize<Config>(configContent);
            if (config == null)
            {
                Console.WriteLine("Erro ao desserializar o arquivo de configuração.");
                return;
            }
            var emailService = new EmailService(config);

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
                Console.WriteLine("2º argumento inválido.");
                return;
            }
            if (!float.TryParse(args[2], out float buyRef))
            {
                Console.WriteLine("3º argumento inválido.");
                return;
            }

            while (true)
            {
                await Task.Delay(3000);
                float? price = await marketService.GetRegularMarketPriceAsync(active);
                if (price != null)
                {
                    if (price > sellRef)
                    {
                        Console.WriteLine($"Venda {price}");
                        emailService.SendEmail($"Preço atual: {price} - Venda recomendada");
                    }
                    if (price < buyRef)
                    {
                        Console.WriteLine($"Compra {price}");
                        emailService.SendEmail($"Preço atual: {price} - Compra recomendada");
                    }
                }
            }
        }
    }
}
