# VPC da aplicação (Ohio)
resource "aws_vpc" "itau_vpc" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = {
    Name = "itau-compra-programada-vpc"
  }
}

# Subnet Pública: Onde a API vai morar (acesso externo)
resource "aws_subnet" "public_subnet" {
  vpc_id                  = aws_vpc.itau_vpc.id
  cidr_block              = "10.0.1.0/24"
  map_public_ip_on_launch = true
  availability_zone       = "us-east-2a"

  tags = {
    Name = "itau-public-subnet"
  }
}

# Subnets Privadas: Isolamento para o Banco de Dados (RDS)
resource "aws_subnet" "private_subnet_1" {
  vpc_id            = aws_vpc.itau_vpc.id
  cidr_block        = "10.0.2.0/24"
  availability_zone = "us-east-2a"

  tags = {
    Name = "itau-private-subnet-1"
  }
}

resource "aws_subnet" "private_subnet_2" {
  vpc_id            = aws_vpc.itau_vpc.id
  cidr_block        = "10.0.3.0/24"
  availability_zone = "us-east-2b"

  tags = {
    Name = "itau-private-subnet-2"
  }
}

# Gateway para permitir saída/entrada de internet na VPC
resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.itau_vpc.id

  tags = {
    Name = "itau-igw"
  }
}

# Tabela de roteamento para direcionar tráfego para a internet
resource "aws_route_table" "public_rt" {
  vpc_id = aws_vpc.itau_vpc.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
  }

  tags = {
    Name = "itau-public-rt"
  }
}

# Ligar a subnet pública à tabela de roteamento da internet
resource "aws_route_table_association" "public_assoc" {
  subnet_id      = aws_subnet.public_subnet.id
  route_table_id = aws_route_table.public_rt.id
}

# Agrupamento de subnets para o RDS (Exige pelo menos 2 AZs diferentes)
resource "aws_db_subnet_group" "rds_subnet_group" {
  name       = "itau-rds-subnet-group"
  subnet_ids = [aws_subnet.private_subnet_1.id, aws_subnet.private_subnet_2.id]

  tags = {
    Name = "Itau RDS Subnet Group"
  }
}
