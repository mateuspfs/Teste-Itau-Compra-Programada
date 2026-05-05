# ==============================================================================
# INFRAESTRUTURA PARA INGESTÃO DE COTAÇÕES DA B3 (SERVERLESS DATA PIPELINE)
# ==============================================================================

# 1. BUCKET S3: Onde os arquivos TXT da B3 serão depositados
resource "aws_s3_bucket" "b3_quotes_bucket" {
  bucket = "itau-b3-quotes-bucket-unique-123" # Em prod, garanta que seja único
  force_destroy = true
  
  tags = {
    Name = "Bucket para Ingestao B3"
    Projeto = "Compra Programada Itau"
  }
}

# 2. FILA SQS: Receberá os eventos limpos (JSON) enviados pela Lambda
resource "aws_sqs_queue" "b3_quotes_events" {
  name = "b3-quotes-events-queue"
  
  # Tempo que a mensagem fica invisível enquanto o .NET processa
  visibility_timeout_seconds = 30
}

# 3. IAM ROLE: A identidade/permissão que a Lambda vai assumir ao rodar
resource "aws_iam_role" "lambda_exec_role" {
  name = "b3_processor_lambda_role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
}

# Anexando a política básica para a Lambda poder escrever logs no CloudWatch
resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.lambda_exec_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# Política Customizada: Permitir que a Lambda LEIA do S3 e ESCREVA no SQS
resource "aws_iam_role_policy" "lambda_s3_sqs_policy" {
  name = "LambdaS3AndSQSPolicy"
  role = aws_iam_role.lambda_exec_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject"
        ]
        Resource = "${aws_s3_bucket.b3_quotes_bucket.arn}/*"
      },
      {
        Effect = "Allow"
        Action = [
          "sqs:SendMessage"
        ]
        Resource = aws_sqs_queue.b3_quotes_events.arn
      }
    ]
  })
}

# 4. ZIP DA LAMBDA: O Terraform precisa empacotar a pasta src/lambdas antes de subir
data "archive_file" "lambda_zip" {
  type        = "zip"
  source_dir  = "${path.module}/../src/lambdas/B3QuotesProcessor"
  output_path = "${path.module}/b3_processor.zip"
}

# 5. AWS LAMBDA FUNCTION: O recurso principal
resource "aws_lambda_function" "b3_processor" {
  filename         = data.archive_file.lambda_zip.output_path
  function_name    = "b3-quotes-processor"
  role             = aws_iam_role.lambda_exec_role.arn
  handler          = "lambda_function.lambda_handler" # arquivo_nome.nome_da_funcao
  
  # Escolhemos Python 3.12, uma versão moderna e estável
  runtime          = "python3.12"
  timeout          = 30 # Até 30 segundos para processar um arquivo grande

  # Gera um hash para o terraform saber se o script mudou e precisa atualizar
  source_code_hash = data.archive_file.lambda_zip.output_base64sha256

  environment {
    variables = {
      # Passamos a URL do SQS como variável de ambiente para a Lambda
      SQS_QUEUE_URL = aws_sqs_queue.b3_quotes_events.url
    }
  }
}

# 6. PERMISSÃO PARA O S3 ACIONAR A LAMBDA
resource "aws_lambda_permission" "allow_s3_invocation" {
  statement_id  = "AllowExecutionFromS3Bucket"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.b3_processor.arn
  principal     = "s3.amazonaws.com"
  source_arn    = aws_s3_bucket.b3_quotes_bucket.arn
}

# 7. S3 EVENT NOTIFICATION: O Gatilho que amarra tudo
resource "aws_s3_bucket_notification" "b3_bucket_notification" {
  bucket = aws_s3_bucket.b3_quotes_bucket.id

  lambda_function {
    lambda_function_arn = aws_lambda_function.b3_processor.arn
    events              = ["s3:ObjectCreated:*"]
    filter_suffix       = ".TXT" # Só reage se for um arquivo TXT
  }

  # Aguarda a permissão do S3 invocar a lambda ser criada antes de atrelar o evento
  depends_on = [aws_lambda_permission.allow_s3_invocation]
}
