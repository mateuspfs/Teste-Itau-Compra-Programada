using Itau.CompraProgramada.Application.DTOs.Admin;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Generic;
using Microsoft.Extensions.Logging;

namespace Itau.CompraProgramada.Worker.Services
{
    public class MotorCompraEngine(
        IUnitOfWork uow,
        IIRService irService,
        ILogger<MotorCompraEngine> logger) : IMotorCompraEngine
    {
        public async Task<MotorCompraResponse> ExecutarProcessamentoDiarioAsync(DateTime dataProcessamento)
        {
            var response = new MotorCompraResponse
            {
                DataExecucao = dataProcessamento,
                Mensagem = "Hoje não é um dia de execução programada."
            };

            if (!DiaDeExecucao(dataProcessamento))
            {
                logger.LogInformation("Hoje ({data}) não é um dia de execução programada (5, 15 ou 25).", dataProcessamento.ToShortDateString());
                return response;
            }

            logger.LogInformation("Iniciando processamento do Motor de Compra para {data}...", dataProcessamento.ToShortDateString());

            var cestaAtiva = await uow.Cestas.FirstOrDefaultAsync(c => c.Ativa);
            if (cestaAtiva == null)
            {
                response.Mensagem = "Nenhuma cesta de recomendação ativa encontrada.";
                return response;
            }

            var itensCesta = (await uow.ItensCesta.GetByCestaIdAsync(cestaAtiva.Id)).ToList();
            if (itensCesta.Count == 0)
            {
                response.Mensagem = "A cesta ativa não possui itens cadastrados.";
                return response;
            }

            var contaMaster = await uow.Contas.FirstOrDefaultAsync(c => c.Tipo == ContaTipo.MASTER);
            if (contaMaster == null)
            {
                response.Mensagem = "Conta Master não encontrada.";
                return response;
            }

            var clientesAtivos = await uow.Clientes.ToListAsync(c => c.Ativo && c.Id != contaMaster.ClienteId);
            if (clientesAtivos.Count == 0)
            {
                response.Mensagem = "Nenhum cliente ativo encontrado.";
                return response;
            }

            // RN-025: O valor de cada cliente para a data e: ValorMensal / 3
            decimal valorTotalConsolidado = clientesAtivos.Sum(c => c.ValorMensal / 3);
            response.TotalClientes = clientesAtivos.Count;
            response.TotalConsolidado = valorTotalConsolidado;

            // Inicializar distribuicoes no response por cliente
            foreach (var cliente in clientesAtivos)
            {
                response.Distribuicoes.Add(new MotorDistribuicaoDTO
                {
                    ClienteId = cliente.Id,
                    Nome = cliente.Nome,
                    ValorAporte = cliente.ValorMensal / 3
                });
            }

            int totalEventosIR = 0;

            foreach (var item in itensCesta)
            {
                logger.LogInformation("Processando ativo {ticker}...", item.Ticker);
                totalEventosIR += await ProcessarItemCestaAsync(item, valorTotalConsolidado, clientesAtivos, contaMaster, response);
            }

            // Consultar resíduos finais na Master
            var custodiasMaster = await uow.Custodias.GetByContaGraficaIdAsync(contaMaster.Id);
            foreach (var cust in custodiasMaster.Where(c => c.Quantidade > 0))
            {
                response.ResiduosCustMaster.Add(new MotorResiduoMasterDTO
                {
                    Ticker = cust.Ticker,
                    Quantidade = cust.Quantidade
                });
            }

            await uow.CommitAsync();
            
            response.EventosIRPublicados = totalEventosIR;
            response.Mensagem = $"Compra programada executada com sucesso para {clientesAtivos.Count} clientes.";
            
            logger.LogInformation("Processamento do Motor de Compra finalizado com sucesso.");
            return response;
        }

        private async Task<int> ProcessarItemCestaAsync(ItemCesta item, decimal valorTotalConsolidado, List<Cliente> clientesAtivos, ContaGrafica contaMaster, MotorCompraResponse response)
        {
            decimal valorAlvoConsolidado = valorTotalConsolidado * (item.Percentual / 100);
            
            var cotacao = await uow.Cotacoes.GetUltimaCotacaoAsync(item.Ticker);
            decimal preco = cotacao?.PrecoFechamento ?? 0;
            if (preco <= 0) return 0;

            // RN-028/029: Quantidade Alvo e considerar saldo Master
            int qtdAlvoTotal = (int)(valorAlvoConsolidado / preco);
            var custodiasMaster = await uow.Custodias.GetByContaGraficaIdAsync(contaMaster.Id);
            var custodiaMasterAtivo = custodiasMaster.FirstOrDefault(c => c.Ticker == item.Ticker);
            int saldoMasterAnterior = custodiaMasterAtivo?.Quantidade ?? 0;

            // RN-030: Se houver saldo master, descontar da quantidade a comprar
            int qtdAComprar = qtdAlvoTotal - saldoMasterAnterior;
            if (qtdAComprar < 0) qtdAComprar = 0;

            List<OrdemCompra> ordensCriadas = [];
            if (qtdAComprar > 0)
            {
                ordensCriadas = await ExecutarOrdensMasterAsync(contaMaster.Id, item.Ticker, qtdAComprar, preco);
                
                var motorOrdem = new MotorOrdemCompraDTO
                {
                    Ticker = item.Ticker,
                    QuantidadeTotal = qtdAComprar,
                    PrecoUnitario = preco,
                    ValorTotal = qtdAComprar * preco
                };

                foreach (var o in ordensCriadas)
                {
                    motorOrdem.Detalhes.Add(new MotorOrdemDetalheDTO
                    {
                        Tipo = o.TipoMercado.ToString(),
                        Ticker = o.Ticker,
                        Quantidade = o.Quantidade
                    });
                }
                response.OrdensCompra.Add(motorOrdem);
            }

            int qtdDisponivelParaDistribuir = qtdAComprar + saldoMasterAnterior;
            if (qtdDisponivelParaDistribuir <= 0) return 0;

            var ordemPrincipal = ordensCriadas.FirstOrDefault();
            
            return await DistribuirParaClientesAsync(item.Ticker, preco, qtdDisponivelParaDistribuir, valorTotalConsolidado, clientesAtivos, contaMaster.Id, ordemPrincipal?.Id, response);
        }

        private async Task<List<OrdemCompra>> ExecutarOrdensMasterAsync(long contaMasterId, string ticker, int quantidade, decimal preco)
        {
            var ordens = new List<OrdemCompra>();

            // RN-031 e RN-032: Separação entre mercado de lote padrão (>=100) e fracionário
            int lotes = (quantidade / 100) * 100;
            int fracionario = quantidade % 100;

            if (lotes > 0)
            {
                var ordemLote = new OrdemCompra(contaMasterId, ticker, lotes, preco, TipoMercado.LOTE);
                await uow.Ordens.AddAsync(ordemLote);
                ordens.Add(ordemLote);
            }

            if (fracionario > 0)
            {
                var tickerFracionario = ticker.EndsWith('F') ? ticker : ticker + "F";
                var ordemFrac = new OrdemCompra(contaMasterId, tickerFracionario, fracionario, preco, TipoMercado.FRACIONARIO);
                await uow.Ordens.AddAsync(ordemFrac);
                ordens.Add(ordemFrac);
            }

            await uow.CommitAsync();

            var custodiasMaster = await uow.Custodias.GetByContaGraficaIdAsync(contaMasterId);
            var custodia = custodiasMaster.FirstOrDefault(c => c.Ticker == ticker);

            if (custodia == null)
            {
                await uow.Custodias.AddAsync(new Custodia(contaMasterId, ticker, quantidade, preco));
            }
            else
            {
                int novaQtd = custodia.Quantidade + quantidade;
                // RN-042: Fórmula de Preço Médio: (Qtd Ant x PM Ant + Qtd Nova x Preco Nova) / (Qtd Ant + Qtd Nova)
                decimal novoPM = (custodia.Quantidade * custodia.PrecoMedio + quantidade * preco) / novaQtd;
                custodia.AtualizarPosicao(novaQtd, novoPM);
            }

            return ordens;
        }

        private async Task<int> DistribuirParaClientesAsync(string ticker, decimal preco, int qtdTotalDisponivel, decimal valorTotalAportes, List<Cliente> clientes, long contaMasterId, long? ordemCompraId, MotorCompraResponse response)
        {
            int qtdJaDistribuida = 0;
            int eventosIR = 0;

            foreach (var cliente in clientes)
            {
                // RN-025: O valor de cada cliente para a data e: ValorMensal / 3
                decimal aporteNoDia = cliente.ValorMensal / 3;
                decimal proporcao = aporteNoDia / valorTotalAportes;
                // RN-036: Quantidade por cliente = TRUNCAR(Proporcao x Quantidade Total Disponivel)
                int qtdCliente = (int)(proporcao * qtdTotalDisponivel);

                if (qtdCliente <= 0) continue;

                var contaFilhote = await uow.Contas.FirstOrDefaultAsync(c => c.ClienteId == cliente.Id && c.Tipo == ContaTipo.FILHOTE);
                if (contaFilhote == null) continue;

                var custodias = await uow.Custodias.GetByContaGraficaIdAsync(contaFilhote.Id);
                var custodia = custodias.FirstOrDefault(c => c.Ticker == ticker);

                if (custodia == null)
                {
                    custodia = new Custodia(contaFilhote.Id, ticker, qtdCliente, preco);
                    await uow.Custodias.AddAsync(custodia);
                }
                else
                {
                    int novaQtd = custodia.Quantidade + qtdCliente;
                    decimal novoPM = (custodia.Quantidade * custodia.PrecoMedio + qtdCliente * preco) / novaQtd;
                    custodia.AtualizarPosicao(novaQtd, novoPM);
                }

                await uow.CommitAsync();

                if (ordemCompraId.HasValue)
                {
                    var dist = new Distribuicao(ordemCompraId.Value, custodia.Id, ticker, qtdCliente, preco);
                    await uow.Distribuicoes.AddAsync(dist);
                }
                
                var distDto = response.Distribuicoes.First(d => d.ClienteId == cliente.Id);
                distDto.Ativos.Add(new MotorAtivoDTO { Ticker = ticker, Quantidade = qtdCliente });

                qtdJaDistribuida += qtdCliente;

                // RN-053: Alíquota de 0,005% (IR Dedo-Duro) sobre o valor total da operação
                await irService.ProcessarIRDedoDuroAsync(cliente.Id, ticker, qtdCliente * preco, "COMPRA", qtdCliente, preco);
                eventosIR++;
            }

            var custodiaMaster = await uow.Custodias.FirstOrDefaultAsync(c => c.ContaGraficaId == contaMasterId && c.Ticker == ticker);
            if (custodiaMaster != null)
            {
                custodiaMaster.AtualizarPosicao(custodiaMaster.Quantidade - qtdJaDistribuida, custodiaMaster.PrecoMedio);
            }

            return eventosIR;
        }

        private static bool DiaDeExecucao(DateTime data)
        {
            // RN-020: Compras ocorrem em 3 datas por mês: dias 5, 15 e 25
            int[] diasAlvo = [5, 15, 25];

            foreach (var dia in diasAlvo)
            {
                var dataAlvoOriginal = new DateTime(data.Year, data.Month, dia, 0, 0, 0, DateTimeKind.Local);
                var dataAjustada = AjustarParaProximoDiaUtil(dataAlvoOriginal);

                if (data.Date == dataAjustada.Date)
                    return true;
            }

            return false;
        }

        private static DateTime AjustarParaProximoDiaUtil(DateTime data)
        {
            // RN-021: Se cair em fim de semana, executar no próximo dia útil (segunda-feira)
            if (data.DayOfWeek == DayOfWeek.Saturday)
                return data.AddDays(2);
            if (data.DayOfWeek == DayOfWeek.Sunday)
                return data.AddDays(1);
            return data;
        }
    }
}
