# Infraestrutura como Código (IaC) - Terraform

Este diretório contém toda a definição da infraestrutura do projeto na AWS, gerenciada como código através do **Terraform**. A utilização de IaC garante que o ambiente seja reprodutível, seguro e perfeitamente documentado.

## Arquitetura da Infraestrutura

A infraestrutura foi desenhada pensando em segurança e uso eficiente do nível gratuito (Free Tier) da AWS.

- **VPC (`vpc.tf`)**: Rede isolada para a aplicação, contendo uma Subnet Pública e duas Subnets Privadas.
- **EC2 (`ec2.tf`)**: Servidor `t3.micro` rodando Ubuntu 22.04 LTS. Localizado na Subnet Pública, ele atua como host para a API e os serviços auxiliares via Docker. O script de inicialização (`user_data`) já instala automaticamente o Docker e o Docker Compose.
- **RDS (`rds.tf`)**: Banco de dados MySQL 8.0 gerenciado (`db.t3.micro`). Localizado nas Subnets Privadas para garantir segurança máxima (não acessível diretamente pela internet).
- **Security Groups (`security_groups.tf`)**: Regras de firewall estritas. O Banco de Dados apenas aceita conexões oriundas do Security Group da API.
- **S3 (`s3.tf`)**: Bucket configurado para hospedagem de site estático (Static Website Hosting), onde o Frontend React é armazenado e servido publicamente.

## Como Executar e Provisionar

### Pré-requisitos
1. [Terraform](https://developer.hashicorp.com/terraform/downloads) instalado na sua máquina.
2. AWS CLI instalado e configurado com suas credenciais (`aws configure`).

### Passos para Implantação

1. **Inicializar o Terraform** (Baixar plugins e configurar o backend local):
   ```bash
   terraform init
   ```

2. **Validar e Planejar** (Visualizar o que será criado/alterado):
   ```bash
   terraform plan
   ```

3. **Aplicar a Infraestrutura** (Provisionar na AWS):
   ```bash
   terraform apply
   ```
   *Confirme digitando `yes` quando solicitado.*

### Pós-Implantação

Ao final da execução do `terraform apply`, o console exibirá três saídas (Outputs):
- `ec2_public_ip`: O IP do servidor da API.
- `rds_endpoint`: O endereço de conexão do banco de dados MySQL.
- `s3_website_url`: O link de acesso do Frontend.

**Atenção:** Estes valores devem ser copiados e inseridos nos **Secrets** do repositório no GitHub (`EC2_HOST`, `DB_HOST` e `S3_BUCKET_NAME`) para que a esteira de CI/CD funcione corretamente com a nova infraestrutura.

## Limpeza de Recursos

Para apagar completamente todos os recursos criados basta rodar:

```bash
terraform destroy
```
*O bucket S3 está configurado com `force_destroy = true` para facilitar a exclusão mesmo se contiver arquivos.*
