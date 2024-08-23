using System;
using System.Net;
using System.Net.Mail;

namespace InoaPriceAlert
{
    public class EmailService
    {
        private readonly Config _config;

        public EmailService(Config config)
        {
            _config = config;
        }

        public void SendEmail(string mensagem)
        {
                if (_config.SmtpConfig == null || _config.EmailDestino == null)
                {
                    Console.WriteLine("Configuração de SMTP ou e-mail de destino não fornecida.");
                    return;
                }
            try
            {
                var smtpClient = new SmtpClient(_config.SmtpConfig.Host)
                {
                    Port = _config.SmtpConfig.Port,
                    Credentials = new NetworkCredential(
                        _config.SmtpConfig.Username,
                        _config.SmtpConfig.Password
                    ),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        _config.SmtpConfig.Username
                            ?? throw new ArgumentNullException(nameof(_config.SmtpConfig.Username))
                    ),
                    Subject = "Alerta de Preço",
                    Body = mensagem,
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(
                    _config.EmailDestino
                        ?? throw new ArgumentNullException(nameof(_config.EmailDestino))
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
