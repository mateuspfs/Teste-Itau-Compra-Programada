using System;
using System.Linq;
using System.Threading.Tasks;
using Itau.CompraProgramada.Application.DTOs.Clientes;
using Itau.CompraProgramada.Application.Exceptions;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Domain.ValueObjects;

namespace Itau.CompraProgramada.Application.Services
{
    public class ClienteService(
        IClienteRepository clienteRepository,
        IContaGraficaRepository contaGraficaRepository,
        IHistoricoValorMensalRepository historicoRepository,
        ICestaRecomendacaoRepository cestaRepository,
        IItemCestaRepository itemCestaRepository,
        ICustodiaRepository custodiaRepository) : IClienteService
    {
        public async Task<AdesaoClienteResponse> AderirAoProdutoAsync(AdesaoClienteRequest request)
        {
            if (!Cpf.Validar(request.CPF))
                throw new ValidationException("CPF informado é inválido.", "CPF_INVALIDO");

            if (request.ValorMensal < 100)
                throw new ValidationException("O valor mensal minímo e de R$ 100,00.", "VALOR_MENSAL_INVALIDO");

            var cpfLimpo = new Cpf(request.CPF).Valor;
            var clienteExistente = await clienteRepository.GetByCpfAsync(cpfLimpo);
            if (clienteExistente != null)
                throw new ValidationException("CPF já cadastrado no sistema.", "CLIENTE_CPF_DUPLICADO");

            var cliente = new Cliente(request.Nome, request.CPF, request.Email, request.ValorMensal);
            await clienteRepository.AddAsync(cliente);
            await clienteRepository.SaveChangesAsync();

            // RN-004: Criar Conta Gráfica Filhote
            var numeroConta = $"FLH-{cliente.Id:D6}";
            var contaGrafica = new ContaGrafica(cliente.Id, numeroConta, ContaTipo.FILHOTE);
            await contaGraficaRepository.AddAsync(contaGrafica);
            await contaGraficaRepository.SaveChangesAsync();

            // RN-004: Criar a Custódia associada à Conta Gráfica Filhote (Inicialmente com 0)
            var cestaAtiva = await cestaRepository.FirstOrDefaultAsync(c => c.Ativa);
            if (cestaAtiva != null)
            {
                var itensCesta = await itemCestaRepository.GetByCestaIdAsync(cestaAtiva.Id);
                foreach (var item in itensCesta)
                {
                    var custodia = new Custodia(contaGrafica.Id, item.Ticker, 0, 0);
                    await custodiaRepository.AddAsync(custodia);
                }
                await custodiaRepository.SaveChangesAsync();
            }

            return new AdesaoClienteResponse
            {
                ClienteId = cliente.Id,
                Nome = cliente.Nome,
                CPF = cliente.CPF,
                Email = cliente.Email,
                ValorMensal = cliente.ValorMensal,
                Ativo = cliente.Ativo,
                DataAdesao = cliente.DataAdesao,
                ContaGrafica = new ContaGraficaDTO
                {
                    Id = contaGrafica.Id,
                    NumeroConta = contaGrafica.NumeroConta,
                    Tipo = contaGrafica.Tipo.ToString(),
                    DataCriacao = contaGrafica.DataCriacao
                }
            };
        }

        public async Task<SaidaProdutoResponse> SairDoProdutoAsync(long clienteId)
        {
            var cliente = await clienteRepository.GetByIdAsync(clienteId) ?? throw new NotFoundException("Cliente não encontrado.", "CLIENTE_NAO_ENCONTRADO");
            
            if (!cliente.Ativo)
                throw new ValidationException("Cliente já havia saído do produto.", "CLIENTE_JA_INATIVO");

            cliente.Desativar();
            await clienteRepository.SaveChangesAsync();

            return new SaidaProdutoResponse
            {
                ClienteId = cliente.Id,
                Nome = cliente.Nome,
                Ativo = cliente.Ativo,
                DataSaida = DateTime.UtcNow,
                Mensagem = "Saída do produto realizada com sucesso."
            };
        }

        public async Task<AlterarValorMensalResponse> AlterarValorMensalAsync(long clienteId, AlterarValorMensalRequest request)
        {
            var cliente = await clienteRepository.GetByIdAsync(clienteId) ?? throw new NotFoundException("Cliente não encontrado.", "CLIENTE_NAO_ENCONTRADO");
            
            if (request.NovoValorMensal < 100)
                throw new ValidationException("O valor mensal minímo e de R$ 100,00.", "VALOR_MENSAL_INVALIDO");
            
            var valorAnterior = cliente.ValorMensal;
            
            // RN-013: Manter histórico do valor anterior
            var historico = new HistoricoValorMensal(clienteId, valorAnterior, request.NovoValorMensal);
            await historicoRepository.AddAsync(historico);

            cliente.AlterarValorMensal(request.NovoValorMensal);
            await clienteRepository.SaveChangesAsync();

            return new AlterarValorMensalResponse
            {
                ClienteId = cliente.Id,
                ValorMensalAnterior = valorAnterior,
                ValorMensalNovo = cliente.ValorMensal,
                DataAlteracao = DateTime.UtcNow,
                Mensagem = "Valor mensal atualizado. O novo valor será considerado a partir da próxima data de compra."
            };
        }
    }
}
