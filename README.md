API de Cálculo de Gols no Futebol - Questionário 2

Visão Geral
Esta solução implementa uma API REST moderna para calcular a quantidade total de gols marcados por times de futebol em anos específicos. A implementação utiliza a abordagem Minimal API do .NET 9, proporcionando uma estrutura mais leve e eficiente.
Requisitos do Questionário
O projeto atende a todos os requisitos do Questionário 2:

Desenvolver uma aplicação que calcule a quantidade de gols marcados por time em um ano
Consumir a API externa: https://jsonmock.hackerrank.com/api/football_matches
Calcular os gols quando o time atua tanto como mandante quanto como visitante
Entregar dois resultados específicos:

Paris Saint-Germain em 2013: 109 gols
Chelsea em 2014: 92 gols



Arquitetura
A solução adota uma arquitetura moderna baseada em Minimal API:
Principais Componentes:

Program.cs: Configuração e definição de endpoints
Services/FootballGoalsService.cs: Lógica de negócio principal
Models/: Classes de modelo para dados e respostas
Interfaces/: Contratos para injeção de dependência

Endpoints:

GET /api/football/goals: Calcula gols para um time específico em um determinado ano
GET /api/football/predefined-goals: Retorna resultados para os times predefinidos
GET /health: Endpoint para verificação de status

Tecnologias Utilizadas

.NET 9: Framework mais recente da Microsoft
Minimal API: Abordagem concisa para criação de APIs
Swagger/OpenAPI: Documentação automática da API
Injeção de Dependência: Para acoplamento fraco entre componentes
HttpClient: Para comunicação com APIs externas
xUnit: Framework de testes
Moq: Biblioteca de mocking para testes

Destaques Técnicos

Idempotência: Os endpoints são projetados para serem idempotentes
Tratamento de Erros: Implementação robusta para captura e tratamento de exceções
Logging: Logging estruturado para monitoramento e depuração
Parametrização: Os endpoints aceitam parâmetros para consultas flexíveis
Documentação: Swagger completamente configurado com descrições detalhadas

Como Executar

Pré-requisitos:

.NET 9 SDK ou superior


Clonar o repositório:
git clone [URL_DO_REPOSITORIO]

Navegar até a pasta do projeto:
cd FootballGoal.API

Restaurar as dependências:
dotnet restore

Executar a aplicação:
dotnet run

Acessar a API:

Swagger UI: https://localhost:PORTA/swagger
Endpoints diretos:

https://localhost:SUAPORTA/api/football/goals?teamName=Paris%20Saint-Germain&year=2013
https://localhost:SUAPORTA/api/football/predefined-goals
https://localhost:SUAPORTA/health





Testes
A solução inclui testes automatizados:
dotnet test
Os testes incluem:

Testes de integração usando WebApplicationFactory
Verificação dos endpoints principais
Simulação de serviços com mocking

Nota Adicional
Esta implementação demonstra como criar uma API REST eficiente e moderna usando o padrão Minimal API do .NET 9, que representa uma abordagem mais direta e com menos boilerplate em comparação com o padrão tradicional de controllers.
A estrutura do projeto segue as melhores práticas atuais de desenvolvimento .NET, incluindo separação de responsabilidades, testes automatizados e documentação completa.
