# Integração e Entrega Contínuas (CI/CD) - GitHub Actions

Este diretório contém os workflows do **GitHub Actions** responsáveis por automatizar os testes, o build e o deploy da aplicação e da infraestrutura na AWS. 

O objetivo destas esteiras é garantir que qualquer código integrado à branch principal (`master`) seja testado e entregue em produção de forma segura, rápida e padronizada.

## Workflows Disponíveis

### 1. Deploy da API (`deploy-api.yml`)
Esta esteira é acionada sempre que houver modificações na pasta `src`, `tests`, no `Dockerfile` ou neste próprio workflow.

**Etapas do Fluxo:**
1. **Testes Automatizados:** Restaura as dependências do .NET e executa as suítes de Testes de Unidade e Integração (usando TestContainers). Se os testes falharem, o deploy é bloqueado.
2. **Build e Push de Imagem:** Faz o build da imagem Docker otimizada em múltiplos estágios. Em seguida, autentica-se de forma segura na AWS e faz o push da imagem para o **Amazon ECR** (Elastic Container Registry), criando o repositório automaticamente caso ele não exista.
3. **Deploy Contínuo (EC2):** Conecta-se via SSH na instância EC2 de produção, copia as configurações do banco (`docker-compose.prod.yml`), realiza o login seguro no ECR (sem expor as chaves permanentes no servidor) e executa um `docker-compose pull` e `up -d` para atualizar a API sem downtime perceptível.

### 2. Deploy do Frontend (`deploy-frontend.yml`)
Esta esteira cuida exclusivamente da interface web do usuário, sendo acionada por mudanças na pasta `frontend`.

**Etapas do Fluxo:**
1. **Build Node.js:** Instala as dependências via `npm ci` e executa o processo de build otimizado pelo Vite, injetando as variáveis de ambiente necessárias (como a URL da API da EC2 gerada pelo Terraform).
2. **Sincronização com o S3:** Envia a pasta final (`dist/`) para o Bucket **Amazon S3** especificado nos segredos. Utiliza a flag `--delete` para remover do bucket arquivos antigos que não existem mais na nova versão, mantendo a hospedagem estática limpa.
3. **Invalidação de Cache (Opcional):** Caso o projeto utilize o Amazon CloudFront, realiza a invalidação do cache para garantir que os usuários recebam a nova versão imediatamente.

## Segurança e Gestão de Segredos

As esteiras não possuem chaves criptográficas "chumbadas" no código. Todas as integrações com a AWS, com o Banco de Dados e com os servidores dependem dos **GitHub Secrets**.

Para que os workflows funcionem corretamente, os seguintes *Secrets* devem estar configurados no repositório:

- **AWS Credentials:** `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION`, `AWS_ACCOUNT_ID`
- **Infraestrutura e Servidor:** `EC2_HOST` (IP público gerado pelo Terraform), `EC2_USERNAME`, `SSH_PRIVATE_KEY`
- **Banco de Dados:** `DB_HOST`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`
- **Frontend:** `S3_BUCKET_NAME`, `VITE_API_URL`
