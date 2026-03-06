using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itau.CompraProgramada.Application.DTOs.Clientes;
using Itau.CompraProgramada.Application.Exceptions;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Entities;
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
        public async Task<CarteiraResponse> ObterCarteiraPorClienteAsync(long clienteId)
        {
            var cliente = await clienteRepository.GetByIdAsync(clienteId) 
                ?? throw new NotFoundException("Cliente não encontrado.", "CLIENTE_NAO_ENCONTRADO");

            var contas = await contaGraficaRepository.GetAllAsync();
            var conta = contas.FirstOrDefault(c => c.ClienteId == clienteId && c.Tipo == ContaTipo.FILHOTE);

            if (conta == null)
                throw new NotFoundException("Conta gráfica não encontrada para este cliente.", "CONTA_NAO_ENCONTRADA");

            var custodias = await custodiaRepository.GetByContaGraficaIdAsync(conta.Id);
            
            var response = new CarteiraResponse();
            decimal valorInvestidoTotal = 0;
            decimal valorAtualTotal = 0;

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
                    PL = (precoAtual - custodia.PrecoMedio) * custodia.Quantidade
                };

                valorInvestidoTotal += custodia.Quantidade * custodia.PrecoMedio;
                valorAtualTotal += ativoDto.ValorAtual;

                response.Ativos.Add(ativoDto);
            }

            response.ValorInvestidoTotal = valorInvestidoTotal;
            response.ValorAtualTotal = valorAtualTotal;
            response.PLTotal = valorAtualTotal - valorInvestidoTotal;
            
            if (valorInvestidoTotal > 0)
            {
                response.RentabilidadePercentual = ((valorAtualTotal - valorInvestidoTotal) / valorInvestidoTotal) * 100;
            }

            // RN-070: Composição percentual real
            if (valorAtualTotal > 0)
            {
                foreach (var ativo in response.Ativos)
                {
                    ativo.ComposicaoPercentual = (ativo.ValorAtual / valorAtualTotal) * 100;
                }
            }

            return response;
        }
    }
}
