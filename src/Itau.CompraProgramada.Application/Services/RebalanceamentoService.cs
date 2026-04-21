using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;

namespace Itau.CompraProgramada.Application.Services
{
    public class RebalanceamentoService(
        IClienteRepository clienteRepository,
        IContaGraficaRepository contaGraficaRepository,
        ICustodiaRepository custodiaRepository,
        IItemCestaRepository itemCestaRepository,
        ICotacaoRepository cotacaoRepository,
        IRebalanceamentoRepository rebalanceamentoRepository,
        IIRService irService) : IRebalanceamentoService
    {
        public async Task ProcessarRebalanceamentoPorMudancaCestaAsync(long cestaAnteriorId, long novaCestaId)
        {
            var clientesAtivos = await clienteRepository.GetAtivosAsync();
            var itensNovos = await itemCestaRepository.GetByCestaIdAsync(novaCestaId);

            foreach (var cliente in clientesAtivos)
            {
                await RebalancearClienteAsync(cliente, itensNovos);
                await irService.ProcessarIRVendaMensalAsync(cliente.Id, DateTime.UtcNow.Month, DateTime.UtcNow.Year);
            }
        }

        private async Task RebalancearClienteAsync(Cliente cliente, IEnumerable<ItemCesta> itensNovos)
        {
            var conta = await contaGraficaRepository.FirstOrDefaultAsync(c => c.ClienteId == cliente.Id && c.Tipo == ContaTipo.FILHOTE);
            if (conta == null) return;

            var custodiasAtuais = (await custodiaRepository.GetByContaGraficaIdAsync(conta.Id)).ToList();
            
            decimal valorTotalCarteira = 0;
            var precosAtuais = new Dictionary<string, decimal>();

            foreach (var custodia in custodiasAtuais)
            {
                var cotacao = await cotacaoRepository.GetUltimaCotacaoAsync(custodia.Ticker);
                decimal preco = cotacao?.PrecoFechamento ?? custodia.PrecoMedio;
                precosAtuais[custodia.Ticker] = preco;
                valorTotalCarteira += custodia.Quantidade * preco;
            }

            if (valorTotalCarteira <= 0) return;

            var tickersNovos = itensNovos.Select(i => i.Ticker).ToList();
            var ativosSairam = custodiasAtuais.Where(c => !tickersNovos.Contains(c.Ticker) && c.Quantidade > 0).ToList();

            foreach (var ativo in ativosSairam)
            {
                var precoVenda = precosAtuais[ativo.Ticker];
                decimal valorVenda = ativo.Quantidade * precoVenda;
                decimal lucro = (precoVenda - ativo.PrecoMedio) * ativo.Quantidade;

                // Registrar rebalanceamento como MUDANCA_CESTA
                var rebal = new Rebalanceamento(cliente.Id, RebalanceamentoTipo.MUDANCA_CESTA, ativo.Ticker, null, valorVenda, lucro);
                await rebalanceamentoRepository.AddAsync(rebal);

                await irService.ProcessarIRDedoDuroAsync(cliente.Id, ativo.Ticker, valorVenda, "VENDA", ativo.Quantidade, precoVenda);

                custodiaRepository.Remove(ativo);
            }

            foreach (var itemNovo in itensNovos)
            {
                decimal valorAlvo = valorTotalCarteira * (itemNovo.Percentual / 100);
                var cotacao = await cotacaoRepository.GetUltimaCotacaoAsync(itemNovo.Ticker);
                decimal precoAtual = cotacao?.PrecoFechamento ?? 0;

                if (precoAtual <= 0) continue;

                int qtdAlvo = (int)(valorAlvo / precoAtual);
                var custodiaExistente = custodiasAtuais.FirstOrDefault(c => c.Ticker == itemNovo.Ticker);

                if (custodiaExistente == null)
                {
                    if (qtdAlvo > 0)
                    {
                        var novaCustodia = new Custodia(conta.Id, itemNovo.Ticker, qtdAlvo, precoAtual);
                        await custodiaRepository.AddAsync(novaCustodia);
                        
                        var rebal = new Rebalanceamento(cliente.Id, RebalanceamentoTipo.MUDANCA_CESTA, null, itemNovo.Ticker, 0); // ValorVenda=0 para compra
                        await rebalanceamentoRepository.AddAsync(rebal);

                        await irService.ProcessarIRDedoDuroAsync(cliente.Id, itemNovo.Ticker, qtdAlvo * precoAtual, "COMPRA", qtdAlvo, precoAtual);
                    }
                }
                else
                {
                    int diffQtd = qtdAlvo - custodiaExistente.Quantidade;

                    if (diffQtd > 0)
                    {
                        decimal novoPM = (custodiaExistente.Quantidade * custodiaExistente.PrecoMedio + diffQtd * precoAtual) / qtdAlvo;
                        custodiaExistente.AtualizarPosicao(qtdAlvo, novoPM);

                        var rebal = new Rebalanceamento(cliente.Id, RebalanceamentoTipo.MUDANCA_CESTA, null, itemNovo.Ticker, 0);
                        await rebalanceamentoRepository.AddAsync(rebal);

                        await irService.ProcessarIRDedoDuroAsync(cliente.Id, itemNovo.Ticker, diffQtd * precoAtual, "COMPRA", diffQtd, precoAtual);
                    }
                    else if (diffQtd < 0)
                    {
                        int qtdVendida = Math.Abs(diffQtd);
                        decimal valorVenda = qtdVendida * precoAtual;
                        decimal lucro = (precoAtual - custodiaExistente.PrecoMedio) * qtdVendida;
                        
                        custodiaExistente.AtualizarPosicao(qtdAlvo, custodiaExistente.PrecoMedio);

                        var rebal = new Rebalanceamento(cliente.Id, RebalanceamentoTipo.MUDANCA_CESTA, itemNovo.Ticker, null, valorVenda, lucro);
                        await rebalanceamentoRepository.AddAsync(rebal);

                        await irService.ProcessarIRDedoDuroAsync(cliente.Id, itemNovo.Ticker, valorVenda, "VENDA", qtdVendida, precoAtual);
                    }
                }
            }

            await rebalanceamentoRepository.SaveChangesAsync();
            await custodiaRepository.SaveChangesAsync();
        }
    }
}
