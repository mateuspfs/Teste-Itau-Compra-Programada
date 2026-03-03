# Sistema de Compra Programada de Ações - Itaú Corretora

Este é um sistema desenvolvido para o desafio técnico da Itaú Corretora, permitindo que clientes invistam de forma recorrente em uma carteira recomendada ("Top Five").

## Tecnologias Utilizadas

- **Backend:** .NET 8 (C#)
- **Banco de Dados:** MySQL 8.0
- **Mensageria:** Apache Kafka
- **Arquitetura:** Clean Architecture + DDD (Domain-Driven Design)
- **Documentação:** Swagger/OpenAPI

## Estrutura do Projeto

O projeto segue os princípios da Clean Architecture, dividido em:

- **`Itau.CompraProgramada.Domain`**: Camada central contendo entidades, regras de negócio e interfaces.
- **`Itau.CompraProgramada.Application`**: Casos de uso e orquestração da lógica de negócio.
- **`Itau.CompraProgramada.Infrastructure`**: Implementações de persistência (EF Core), integração com Kafka e parser da B3.
- **`Itau.CompraProgramada.API`**: Endpoints REST para clientes e administração.
- **`Itau.CompraProgramada.Worker`**: Serviço de fundo que executa o motor de compra nos dias 5, 15 e 25.

## Como Executar

### Pré-requisitos
- Docker e Docker Compose instalados.
- SDK do .NET 8.

### Passo 1: Subir a Infraestrutura
Na raiz do projeto, execute:
```bash
docker-compose up -d
```
Isso iniciará o MySQL (porta 3307) e o ambiente Kafka/Zookeeper.

### Passo 2: Executar a API
Navegue até a pasta da API e execute:
```bash
cd src/Itau.CompraProgramada.API
dotnet run
```
A documentação Swagger estará disponível em: `http://localhost:5000/swagger` (ou porta configurada).

## Testes
Para rodar os testes unitários e de integração:
```bash
dotnet test
```

---
*Desenvolvido como parte do processo seletivo para Engenharia de Software - Itau Corretora.*
