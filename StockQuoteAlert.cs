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

namespace InoaPriceAlert
{
    class StockQuoteAlert
    {
        static async Task Main(string[] args)
        {
            Env.Load();
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            string? configContent = await File.ReadAllTextAsync("config.json");
            Config? config = JsonSerializer.Deserialize<Config>(configContent);
            if (config == null)
            {
                Console.WriteLine("Erro ao desserializar o arquivo de configuração.");
                return;
            }

            if (args.Length < 3 || args.Length > 4)
            {
                Console.WriteLine(
                    "Por favor, forneça 3 ou 4 argumentos: <ativo> <precoVenda> <precoCompra> [intervaloEmMs]"
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

            int interval = 3000;
            if (args.Length == 4 && !int.TryParse(args[3], out interval))
            {
                Console.WriteLine("4º argumento inválido. Usando valor padrão de 3000 ms.");
                interval = 3000;
            }
            var marketService = new MarketService();
            var emailService = new EmailService(config);

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
