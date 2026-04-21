using Itau.CompraProgramada.Application.Common;
using Itau.CompraProgramada.Application.DTOs.Clientes;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Enums;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;
using Itau.CompraProgramada.Domain.ValueObjects;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Itau.CompraProgramada.Application.Services
{
    public class ClienteService(
        IClienteRepository clienteRepository,
        IContaGraficaRepository contaGraficaRepository,
        IHistoricoValorMensalRepository historicoRepository,
        ICestaRecomendacaoRepository cestaRepository,
        IItemCestaRepository itemCestaRepository,
        ICustodiaRepository custodiaRepository,
        ICotacaoRepository cotacaoRepository) : IClienteService
    {
        public async Task<Result<AdesaoClienteResponse>> AderirAoProdutoAsync(AdesaoClienteRequest request)
        {
            if (!Cpf.Validar(request.CPF))
                return Result<AdesaoClienteResponse>.Fail("CPF informado é inválido.", "CPF_INVALIDO");

            if (request.ValorMensal < 100)
                return Result<AdesaoClienteResponse>.Fail("O valor mensal minímo e de R$ 100,00.", "VALOR_MENSAL_INVALIDO");

            var cpfLimpo = new Cpf(request.CPF).Valor;
            var clienteExistente = await clienteRepository.GetByCpfAsync(cpfLimpo);
            if (clienteExistente != null)
                return Result<AdesaoClienteResponse>.Fail("CPF já cadastrado no sistema.", "CLIENTE_CPF_DUPLICADO");

            var cliente = new Cliente(request.Nome, request.CPF, request.Email, request.ValorMensal);
            await clienteRepository.AddAsync(cliente);
            await clienteRepository.SaveChangesAsync();

            // RN-004: Criar Conta Gráfica Filhote
            var numeroConta = $"FLH-{cliente.Id:D6}";
            var contaGrafica = new ContaGrafica(cliente.Id, numeroConta, ContaTipo.FILHOTE);
            await contaGraficaRepository.AddAsync(contaGrafica);
            await contaGraficaRepository.SaveChangesAsync();

            var response = new AdesaoClienteResponse
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

            return Result<AdesaoClienteResponse>.Success(response);
        }

        public async Task<Result<SaidaProdutoResponse>> SairDoProdutoAsync(long clienteId)
        {
            var cliente = await clienteRepository.GetByIdAsync(clienteId);
            
            if (cliente == null)
                return Result<SaidaProdutoResponse>.NotFound("Cliente não encontrado.", "CLIENTE_NAO_ENCONTRADO");
            
            if (!cliente.Ativo)
                return Result<SaidaProdutoResponse>.Fail("Cliente já havia saído do produto.", "CLIENTE_JA_INATIVO");

            cliente.Desativar();
            await clienteRepository.SaveChangesAsync();

            var response = new SaidaProdutoResponse
            {
                ClienteId = cliente.Id,
                Nome = cliente.Nome,
                Ativo = cliente.Ativo,
                DataSaida = DateTime.UtcNow,
                Mensagem = "Saída do produto realizada com sucesso."
            };

            return Result<SaidaProdutoResponse>.Success(response);
        }

        public async Task<Result<AlterarValorMensalResponse>> AlterarValorMensalAsync(long clienteId, AlterarValorMensalRequest request)
        {
            var cliente = await clienteRepository.GetByIdAsync(clienteId);
            
            if (cliente == null)
                return Result<AlterarValorMensalResponse>.NotFound("Cliente não encontrado.", "CLIENTE_NAO_ENCONTRADO");
            
            if (request.NovoValorMensal < 100)
                return Result<AlterarValorMensalResponse>.Fail("O valor mensal minímo e de R$ 100,00.", "VALOR_MENSAL_INVALIDO");
            
            var valorAnterior = cliente.ValorMensal;
            
            // RN-013: Manter histórico do valor anterior
            var historico = new HistoricoValorMensal(clienteId, valorAnterior, request.NovoValorMensal);
            await historicoRepository.AddAsync(historico);

            cliente.AlterarValorMensal(request.NovoValorMensal);
            await clienteRepository.SaveChangesAsync();

            var response = new AlterarValorMensalResponse
            {
                ClienteId = cliente.Id,
                ValorMensalAnterior = valorAnterior,
                ValorMensalNovo = cliente.ValorMensal,
                DataAlteracao = DateTime.UtcNow,
                Mensagem = "Valor mensal atualizado. O novo valor será considerado a partir da próxima data de compra."
            };

            return Result<AlterarValorMensalResponse>.Success(response);
        }

        public async Task<Result<RentabilidadeResponse>> ObterRentabilidadeAsync(long clienteId)
        {
            var cliente = await clienteRepository.GetByIdAsync(clienteId);
            
            if (cliente == null)
                return Result<RentabilidadeResponse>.NotFound("Cliente não encontrado.", "CLIENTE_NAO_ENCONTRADO");

            var todasContas = await contaGraficaRepository.GetAllAsync();
            var conta = todasContas.FirstOrDefault(c => c.ClienteId == clienteId && c.Tipo == ContaTipo.FILHOTE);

            if (conta == null)
                return Result<RentabilidadeResponse>.NotFound("Conta gráfica não encontrada para este cliente.", "CONTA_NAO_ENCONTRADA");

            var custodias = await custodiaRepository.GetByContaGraficaIdAsync(conta.Id);
            decimal valorInvestidoTotal = 0;
            decimal valorAtualTotal = 0;

            foreach (var custodia in custodias)
            {
                var cotacao = await cotacaoRepository.GetUltimaCotacaoAsync(custodia.Ticker);
                decimal precoAtual = cotacao?.PrecoFechamento ?? 0;

                valorInvestidoTotal += custodia.Quantidade * custodia.PrecoMedio;
                valorAtualTotal += custodia.Quantidade * precoAtual;
            }

            var response = new RentabilidadeResponse
            {
                ClienteId = cliente.Id,
                Nome = cliente.Nome,
                DataConsulta = DateTime.UtcNow,
                Rentabilidade = new RentabilidadeResumoDTO
                {
                    ValorTotalInvestido = valorInvestidoTotal,
                    ValorAtualCarteira = valorAtualTotal,
                    PLTotal = valorAtualTotal - valorInvestidoTotal,
                    RentabilidadePercentual = valorInvestidoTotal > 0 ? ((valorAtualTotal - valorInvestidoTotal) / valorInvestidoTotal) * 100 : 0
                }
            };

            // RN-020: Próximos aportes (05, 15 e 25) a partir de hoje
            var hoje = DateTime.UtcNow;
            var proximos = new List<HistoricoAporteDTO>();
            int[] diasAlvo = [5, 15, 25];
            DateTime dataRef = new(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            int encontrados = 0;

            while (encontrados < 3)
            {
                foreach (var dia in diasAlvo)
                {
                    var dataAporte = new DateTime(dataRef.Year, dataRef.Month, dia).AddHours(12);
                    if (dataAporte > hoje && encontrados < 3)
                    {
                        proximos.Add(new HistoricoAporteDTO
                        {
                            Data = dataAporte,
                            Valor = cliente.ValorMensal / 3,
                            Parcela = $"{(dia == 5 ? "1" : dia == 15 ? "2" : "3")}/3"
                        });
                        encontrados++;
                    }
                }
                dataRef = dataRef.AddMonths(1);
            }

            response.HistoricoAportes = proximos;

            response.EvolucaoCarteira = new List<EvolucaoCarteiraDTO>
            {
                new() { Data = DateTime.UtcNow.AddMonths(-1).AddDays(25), ValorInvestido = valorInvestidoTotal, ValorCarteira = valorAtualTotal, Rentabilidade = response.Rentabilidade.RentabilidadePercentual }
            };

            return Result<RentabilidadeResponse>.Success(response);
        }

        public async Task<Result<ResultadoPaginado<AdesaoClienteResponse>>> ObterTodosPaginaAsync(int pagina, int tamanhoPagina, bool ordemDesc = true)
        {
            var skip = (pagina - 1) * tamanhoPagina;
            var (clientes, totalRegistros) = await clienteRepository.GetPagedAsync(skip, tamanhoPagina, c => c.Id, ordemDesc);
            
            var todasContas = await contaGraficaRepository.GetAllAsync();

            var responseItems = clientes.Select(cliente =>
            {
                var conta = todasContas.FirstOrDefault(c => c.ClienteId == cliente.Id && c.Tipo == ContaTipo.FILHOTE);
                return new AdesaoClienteResponse
                {
                    ClienteId = cliente.Id,
                    Nome = cliente.Nome,
                    CPF = cliente.CPF,
                    Email = cliente.Email,
                    ValorMensal = cliente.ValorMensal,
                    Ativo = cliente.Ativo,
                    DataAdesao = cliente.DataAdesao,
                    ContaGrafica = conta != null ? new ContaGraficaDTO
                    {
                        Id = conta.Id,
                        NumeroConta = conta.NumeroConta,
                        Tipo = conta.Tipo.ToString(),
                        DataCriacao = conta.DataCriacao
                    } : null!
                };
            }).ToList();

            var resultado = new ResultadoPaginado<AdesaoClienteResponse>(responseItems, totalRegistros, pagina, tamanhoPagina);
            return Result<ResultadoPaginado<AdesaoClienteResponse>>.Success(resultado);
        }

        public async Task<Result<ClienteResumoResponse>> ObterResumoDashboardAsync()
        {
            var data = await clienteRepository.ObterResumoAsync();
            var resUltimos = await ObterTodosPaginaAsync(1, 5, true);
            
            var resumo = new ClienteResumoResponse
            {
                TotalAtivos = data.TotalAtivos,
                TotalValorMensal = data.TotalValorMensal,
                TotalResiduoMaster = data.ValorResiduoMaster,
                UltimosClientes = resUltimos.Data?.Itens.Select(c => new UltimoClienteDashboardDTO
                {
                    ClienteId = c.ClienteId,
                    Nome = c.Nome,
                    Email = c.Email,
                    ValorMensal = c.ValorMensal,
                    NumeroConta = c.ContaGrafica?.NumeroConta
                }).ToList() ?? [],
                ItensMaster = data.ItensMaster.Select(i => new ItemCustodiaMasterResumoDTO
                {
                    Ticker = i.Ticker,
                    Quantidade = i.Quantidade,
                    ValorAtual = i.ValorAtual
                }).ToList() ?? [],
            };

            return Result<ClienteResumoResponse>.Success(resumo);
        }
    }
}
