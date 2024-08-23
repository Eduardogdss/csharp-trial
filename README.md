## Como rodar o projeto

### Pré-requisitos

- .NET Core instalado em sua máquina
- Arquivos `config.json` e `.env` na raiz do projeto

### Passos para rodar o projeto

1. Clone o repositório:

   ```sh
   git clone <URL_DO_REPOSITORIO>
   cd <NOME_DO_REPOSITORIO>
   ```

2. Construa e execute o projeto:

   ```sh
   dotnet build
   dotnet run -- "ATIVO" "VALOR_VENDA" "VALOR_COMPRA"
   ```

   **OU**

   Vá até a pasta publish e execute o arquivo `StockQuoteAlert.exe` passando os argumentos:

   ```sh
   cd ./bin/Debug/net6.0/publish
   ./StockQuoteAlert.exe "ATIVO" "VALOR_VENDA" "VALOR_COMPRA"
   ```

### Modelo dos arquivos

- `config.json`

Crie um arquivo `config.json` na raiz do projeto com o seguinte conteúdo:

```json
{
	"EmailDestino": "email_destino@example.com",
	"SmtpConfig": {
		"Host": "smtp.host_example.com",
		"Port": 587,
		"Username": "email_destinatario@example.com",
		"Password": "senha_destinatario"
	},
	"ApiToken": "seu_token_aqui"
}
```

- `.env`

  Crie um arquivo `.env` na raiz do projeto com o seguinte conteúdo:
  Obs: é importante que, independente da URL, o token e o ativo estejam nos formatos abaixo.

  `API_TOKEN=SEU_TOKEN__BRAPI_DEV`
  `API_URL_SAMPLE="https://brapi.dev/api/quote/{active}?token={token}"`
