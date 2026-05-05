import json
import boto3
import urllib.parse
import os
import logging
from datetime import datetime

# Configuração de Logs para o CloudWatch
logger = logging.getLogger()
logger.setLevel(logging.INFO)

# Inicializando os clients da AWS
s3_client = boto3.client('s3')
sqs_client = boto3.client('sqs')

# Obtendo a URL da Fila SQS a partir das variáveis de ambiente (definidas no Terraform)
SQS_QUEUE_URL = os.environ.get('SQS_QUEUE_URL')

def parse_b3_line(line):
    """
    Faz o parse de uma linha do arquivo posicional da B3 (COTAHIST).
    Extrai as informações que importam para o nosso sistema de Compra Programada.
    """
    try:
        # Pula as linhas de Header (começam com '00') e Trailer (começam com '99')
        if line.startswith('00') or line.startswith('99'):
            return None

        # O layout da B3 (COTAHIST) é posicional. O Worker espera os seguintes campos:
        # Data do pregão: Posições 02 a 09
        # Ticker: Posições 12 a 23
        # Abertura: Posições 56 a 68
        # Máximo: Posições 69 a 81
        # Mínimo: Posições 82 a 94
        # Fechamento: Posições 108 a 120
        
        data_pregao_str = line[2:10].strip()
        ticker = line[12:24].strip()
        
        abertura = float(line[56:69].strip()) / 100.0
        maximo = float(line[69:82].strip()) / 100.0
        minimo = float(line[82:95].strip()) / 100.0
        fechamento = float(line[108:121].strip()) / 100.0

        # Formata a data para ISO 8601, mais amigável para o Worker .NET consumir
        data_formatada = datetime.strptime(data_pregao_str, '%Y%m%d').strftime('%Y-%m-%dT%H:%M:%S')

        return {
            "Data": data_formatada,
            "Ticker": ticker,
            "Abertura": abertura,
            "Fechamento": fechamento,
            "Maximo": maximo,
            "Minimo": minimo
        }

    except Exception as e:
        logger.warning(f"Erro ao fazer o parse da linha: {line}. Erro: {str(e)}")
        return None

def lambda_handler(event, context):
    """
    Ponto de entrada da AWS Lambda. É ativada automaticamente quando um arquivo
    cai no bucket S3 configurado.
    """
    logger.info("Iniciando processamento do arquivo B3...")

    # O objeto 'event' traz as informações do arquivo que acabou de cair no S3
    try:
        # Pega o nome do bucket e a chave (nome) do arquivo
        bucket = event['Records'][0]['s3']['bucket']['name']
        key = urllib.parse.unquote_plus(event['Records'][0]['s3']['object']['key'], encoding='utf-8')
        
        logger.info(f"Arquivo detectado: {key} no bucket {bucket}")

        # Puxa o conteúdo do arquivo txt da B3 diretamente para a memória da Lambda
        response = s3_client.get_object(Bucket=bucket, Key=key)
        
        # Lê o conteúdo (decodificando para string assumindo que a B3 usa utf-8 ou latin-1)
        # Para B3, normalmente é latin-1, mas vamos tentar utf-8 como padrão
        try:
            file_content = response['Body'].read().decode('utf-8')
        except UnicodeDecodeError:
            logger.info("Falha no decode UTF-8, tentando latin-1 (Padrão B3 Antigo)...")
            file_content = response['Body'].read().decode('latin-1')

        linhas = file_content.split('\n')
        
        cotacoes_processadas = 0
        lote_cotacoes = []
        BATCH_SIZE = 500
        
        # Função auxiliar para esvaziar o lote (enviar para o SQS)
        def enviar_lote():
            if not lote_cotacoes:
                return
            
            message_body = json.dumps(lote_cotacoes)
            
            # Publica o array JSON na Fila SQS para ser consumido em bulk
            sqs_client.send_message(
                QueueUrl=SQS_QUEUE_URL,
                MessageBody=message_body
            )
            lote_cotacoes.clear()

        # Processa as linhas do arquivo
        for linha in linhas:
            if not linha.strip():
                continue # Pula linhas vazias
                
            cotacao = parse_b3_line(linha)
            
            if cotacao:
                lote_cotacoes.append(cotacao)
                cotacoes_processadas += 1

                # Se o lote encheu, dispara para a AWS e limpa a lista
                if len(lote_cotacoes) >= BATCH_SIZE:
                    enviar_lote()

        # Ao final do loop, garante que o resto das cotações que não fecharam o número 500 sejam enviadas
        enviar_lote()

        logger.info(f"Sucesso! {cotacoes_processadas} cotações extraídas e enviadas em Lote para o SQS.")
        
        return {
            'statusCode': 200,
            'body': json.dumps(f'Processamento concluído: {cotacoes_processadas} registros enviados.')
        }

    except Exception as e:
        logger.error(f"Erro catastrófico no processamento do arquivo. Detalhes: {str(e)}")
        raise e
