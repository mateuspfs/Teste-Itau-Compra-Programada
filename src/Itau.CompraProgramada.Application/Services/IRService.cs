using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Messaging;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;

namespace Itau.CompraProgramada.Application.Services
{
    public class IRService(
        IClienteRepository clienteRepository,
        IEventoIRRepository eventoIRRepository,
        IMessagingService messagingService,
        IRebalanceamentoRepository rebalanceamentoRepository) : IIRService
    {
        private const decimal ALIQUOTA_DEDO_DURO = 0.00005m;
        private const decimal LIMITE_ISENCAO_VENDA = 20000.00m;
        private const decimal ALIQUOTA_IR_LUCRO = 0.20m;

        public async Task ProcessarIRDedoDuroAsync(long clienteId, string ticker, decimal valorOperacao, string tipoOperacao, int quantidade, decimal precoUnitario)
        {
            var cliente = await clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null) return;

            decimal valorIR = Math.Round(valorOperacao * ALIQUOTA_DEDO_DURO, 2);
            if (valorIR < 0.01m) valorIR = 0.01m;

            var evento = new EventoIR(clienteId, EventoIRTipo.DEDO_DURO, valorOperacao, valorIR);
            await eventoIRRepository.AddAsync(evento);
            await eventoIRRepository.SaveChangesAsync();

            // RN-056: Publicar no Kafka
            var message = new
            {
                tipo = "IR_DEDO_DURO",
                clienteId = cliente.Id,
                cpf = cliente.CPF,
                ticker,
                tipoOperacao,
                quantidade,
                precoUnitario,
                valorOperacao,
                aliquota = ALIQUOTA_DEDO_DURO,
                valorIR,
                dataOperacao = DateTime.UtcNow
            };

            await messagingService.PublishAsync("ir-events", message);
        }

        public async Task ProcessarIRVendaMensalAsync(long clienteId, int mes, int ano)
        {
            var cliente = await clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null) return;

            var rebalanceamentos = await rebalanceamentoRepository.GetAllAsync();
            var vendasMes = rebalanceamentos
                .Where(r => r.ClienteId == clienteId && 
                            r.TickerVendido != null &&
                            r.DataRebalanceamento.Month == mes &&
                            r.DataRebalanceamento.Year == ano)
                .ToList();

            decimal totalVendas = vendasMes.Sum(v => v.ValorVenda);

            if (totalVendas <= LIMITE_ISENCAO_VENDA) return;

            // RN-059/RN-060: 20% sobre lucro líquido Real
            decimal lucroTotal = vendasMes.Sum(v => v.Lucro);
            decimal valorIR = lucroTotal > 0 ? Math.Round(lucroTotal * ALIQUOTA_IR_LUCRO, 2) : 0;

            if (valorIR <= 0) return;

            var message = new
            {
                tipo = "IR_VENDA",
                clienteId = cliente.Id,
                cpf = cliente.CPF,
                mesReferencia = $"{ano}-{mes:D2}",
                totalVendasMes = totalVendas,
                lucroLiquido = lucroTotal,
                aliquota = ALIQUOTA_IR_LUCRO,
                valorIR,
                dataCalculo = DateTime.UtcNow
            };

            await messagingService.PublishAsync("ir-events", message);
        }
    }
}
