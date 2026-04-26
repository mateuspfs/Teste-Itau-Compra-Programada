# Regras de Firewall para a EC2 (API)
resource "aws_security_group" "api_sg" {
  name        = "api-sg"
  description = "Liberar portas para API e SSH"
  vpc_id      = aws_vpc.itau_vpc.id

  # Porta 80: Acesso Web
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Porta 22: Acesso SSH (Manutenção)
  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Saída: Liberar tudo para a instância conseguir baixar pacotes
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "itau-api-sg"
  }
}

# Regras de Firewall para o RDS (Banco de Dados)
resource "aws_security_group" "rds_sg" {
  name        = "rds-sg"
  description = "Segurança: Banco só aceita conexões vindas da API"
  vpc_id      = aws_vpc.itau_vpc.id

  # MySQL: Só permite entrada se vier de quem estiver no SG da API
  ingress {
    from_port       = 3306
    to_port         = 3306
    protocol        = "tcp"
    security_groups = [aws_security_group.api_sg.id]
  }

  tags = {
    Name = "itau-rds-sg"
  }
}
