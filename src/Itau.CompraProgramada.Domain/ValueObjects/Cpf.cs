using System;
using System.Linq;

namespace Itau.CompraProgramada.Domain.ValueObjects
{
    public record Cpf
    {
        public string Valor { get; }

        public Cpf(string valor)
        {
            if (!Validar(valor))
                throw new Exception("CPF invalido.");

            Valor = Limpar(valor);
        }

        public static bool Validar(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;

            var cpfLimpo = Limpar(cpf);

            if (cpfLimpo.Length != 11) return false;

            if (cpfLimpo.All(c => c == cpfLimpo[0])) return false;

            var multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCpf = cpfLimpo.Substring(0, 9);
            var soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            var resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            var digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpfLimpo.EndsWith(digito);
        }

        private static string Limpar(string cpf)
        {
            return new string(cpf.Where(char.IsDigit).ToArray());
        }

        public override string ToString() => Valor;
        
        public static implicit operator string(Cpf cpf) => cpf.Valor;
        public static implicit operator Cpf(string valor) => new Cpf(valor);
    }
}
