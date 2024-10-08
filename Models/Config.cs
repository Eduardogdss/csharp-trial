namespace InoaPriceAlert
{
    public class SmtpConfig
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class Config
    {
        public string? EmailDestino { get; set; }
        public SmtpConfig? SmtpConfig { get; set; }
    }
}
