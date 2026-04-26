resource "aws_instance" "api_server" {
  # Ubuntu 22.04 LTS em Ohio
  ami           = "ami-0c55b159cbfafe1f0"
  instance_type = "t3.micro"
  
  subnet_id                   = aws_subnet.public_subnet.id
  vpc_security_group_ids      = [aws_security_group.api_sg.id]
  associate_public_ip_address = true
  
  # Chave SSH já criada no console da AWS (us-east-2)
  key_name = "api-itau"

  tags = {
    Name = "itau-api-server"
  }

  # Script de inicialização: Instala Docker e configura permissões no boot
  user_data = <<-EOF
              #!/bin/bash
              sudo apt-get update
              sudo apt-get install -y docker.io
              sudo systemctl start docker
              sudo systemctl enable docker
              sudo usermod -aG docker ubuntu
              EOF
}
