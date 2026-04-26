output "ec2_public_ip" {
  value       = aws_instance.api_server.public_ip
  description = "IP Público da API"
}

output "rds_endpoint" {
  value       = aws_db_instance.mysql.endpoint
  description = "Endpoint do Banco de Dados"
}

output "s3_website_url" {
  value       = aws_s3_bucket_website_configuration.frontend_web.website_endpoint
  description = "URL do Frontend no S3"
}
