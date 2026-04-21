using Itau.CompraProgramada.Application.Common;
using Itau.CompraProgramada.Application.DTOs.Clientes;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;

namespace Itau.CompraProgramada.Application.Services
{
    public class CarteiraService(
        IClienteRepository clienteRepository,
        IContaGraficaRepository contaGraficaRepository,
        ICustodiaRepository custodiaRepository,
        ICotacaoRepository cotacaoRepository) : ICarteiraService
    {
        public async Task<Result<CarteiraResponse>> ObterCarteiraPorClienteAsync(long clienteId)
        {
            var cliente = await clienteRepository.GetByIdAsync(clienteId);
            
            if (cliente == null)
                return Result<CarteiraResponse>.NotFound("Cliente não encontrado.", "CLIENTE_NAO_ENCONTRADO");

            var contas = await contaGraficaRepository.GetAllAsync();
            var conta = contas.FirstOrDefault(c => c.ClienteId == clienteId && c.Tipo == ContaTipo.FILHOTE);

            if (conta == null)
                return Result<CarteiraResponse>.NotFound("Conta gráfica não encontrada para este cliente.", "CONTA_NAO_ENCONTRADA");

            var custodias = await custodiaRepository.GetByContaGraficaIdAsync(conta.Id);
            
            var response = new CarteiraResponse
            {
                ClienteId = cliente.Id,
                Nome = cliente.Nome,
                ContaGrafica = conta.NumeroConta,
                DataConsulta = DateTime.UtcNow
            };

            decimal valorTotalInvestido = 0;
            decimal valorAtualCarteira = 0;

            foreach (var custodia in custodias)
            {
                var cotacao = await cotacaoRepository.GetUltimaCotacaoAsync(custodia.Ticker);
                decimal precoAtual = cotacao?.PrecoFechamento ?? 0;

                var ativoDto = new AtivoCarteiraDTO
                {
                    Ticker = custodia.Ticker,
                    Quantidade = custodia.Quantidade,
                    PrecoMedio = custodia.PrecoMedio,
                    CotacaoAtual = precoAtual,
                    ValorAtual = custodia.Quantidade * precoAtual,
                    PL = (precoAtual - custodia.PrecoMedio) * custodia.Quantidade,
                    PLPercentual = custodia.PrecoMedio > 0 ? ((precoAtual - custodia.PrecoMedio) / custodia.PrecoMedio) * 100 : 0
                };

                valorTotalInvestido += custodia.Quantidade * custodia.PrecoMedio;
                valorAtualCarteira += ativoDto.ValorAtual;

                response.Ativos.Add(ativoDto);
            }

            response.Resumo.ValorTotalInvestido = valorTotalInvestido;
            response.Resumo.ValorAtualCarteira = valorAtualCarteira;
            response.Resumo.PLTotal = valorAtualCarteira - valorTotalInvestido;
            
            if (valorTotalInvestido > 0)
            {
                response.Resumo.RentabilidadePercentual = ((valorAtualCarteira - valorTotalInvestido) / valorTotalInvestido) * 100;
            }

            // RN-070: Composição percentual real
            if (valorAtualCarteira > 0)
            {
                foreach (var ativo in response.Ativos)
                {
                    ativo.ComposicaoCarteira = (ativo.ValorAtual / valorAtualCarteira) * 100;
                }
            }
            
            return Result<CarteiraResponse>.Success(response);
        }
    }
}
