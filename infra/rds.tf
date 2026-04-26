resource "aws_db_instance" "mysql" {
  allocated_storage    = 20
  db_name              = "compra_programada"
  engine               = "mysql"
  engine_version       = "8.0"
  instance_class       = "db.t3.micro" # db.t3.micro garantido no Free Tier
  username             = "admin"
  password             = "mudar_senha_123" # TODO: Mover para variável sensível ou Secret Manager
  parameter_group_name = "default.mysql8.0"
  skip_final_snapshot  = true
  
  db_subnet_group_name   = aws_db_subnet_group.rds_subnet_group.name
  vpc_security_group_ids = [aws_security_group.rds_sg.id]

  tags = {
    Name = "itau-rds-mysql"
  }
}