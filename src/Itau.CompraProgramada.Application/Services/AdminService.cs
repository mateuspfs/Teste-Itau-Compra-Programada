using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itau.CompraProgramada.Application.DTOs.Admin;
using Itau.CompraProgramada.Application.Exceptions;
using Itau.CompraProgramada.Application.Interfaces;
using Itau.CompraProgramada.Domain.Entities;
using Itau.CompraProgramada.Domain.Interfaces.Respositories;

namespace Itau.CompraProgramada.Application.Services
{
    public class AdminService(
        ICestaRecomendacaoRepository cestaRepository,
        IItemCestaRepository itemCestaRepository,
        IRebalanceamentoService rebalanceamentoService) : IAdminService
    {
        public async Task<CestaCadastroResponse> CadastrarAlterarCestaAsync(CestaRequest request)
        {
            if (request.Itens.Count != 5)
                throw new ValidationException($"A cesta deve conter exatamente 5 ativos. Quantidade informada: {request.Itens.Count}.", "QUANTIDADE_ATIVOS_INVALIDA");

            var somaPercentuais = request.Itens.Sum(i => i.Percentual);
            if (somaPercentuais != 100)
                throw new ValidationException($"A soma dos percentuais deve ser exatamente 100%. Soma atual: {somaPercentuais}%.", "PERCENTUAIS_INVALIDOS");

            if (request.Itens.Any(i => i.Percentual <= 0))
                throw new ValidationException("Cada ativo na cesta deve ter um percentual maior que 0%.", "PERCENTUAL_POSITIVO_REQUERIDO");

            var cestaAtual = await cestaRepository.FirstOrDefaultAsync(c => c.Ativa);
            CestaResumoDTO? resumoAnterior = null;
            List<string>? ativosRemovidos = null;
            List<string>? ativosAdicionados = null;

            if (cestaAtual != null)
            {
                var itensAtuais = await itemCestaRepository.GetByCestaIdAsync(cestaAtual.Id);
                var tickersAtuais = itensAtuais.Select(i => i.Ticker).ToList();
                var novosTickers = request.Itens.Select(i => i.Ticker).ToList();

                ativosRemovidos = [.. tickersAtuais.Except(novosTickers)];
                ativosAdicionados = [.. novosTickers.Except(tickersAtuais)];

                cestaAtual.Desativar();
                await cestaRepository.SaveChangesAsync();

                resumoAnterior = new CestaResumoDTO
                {
                    CestaId = cestaAtual.Id,
                    Nome = cestaAtual.Nome,
                    DataDesativacao = cestaAtual.DataDesativacao ?? DateTime.UtcNow
                };
            }

            var novaCesta = new CestaRecomendacao(request.Nome);
            await cestaRepository.AddAsync(novaCesta);
            await cestaRepository.SaveChangesAsync();

            var itens = request.Itens.Select(i => new ItemCesta(novaCesta.Id, i.Ticker, i.Percentual)).ToList();
            await itemCestaRepository.AddRangeAsync(itens);
            await itemCestaRepository.SaveChangesAsync();

            var response = new CestaCadastroResponse
            {
                CestaId = novaCesta.Id,
                Nome = novaCesta.Nome,
                Ativa = novaCesta.Ativa,
                DataCriacao = novaCesta.DataCriacao,
                Itens = itens.Select(i => new ItemCestaDTO { Ticker = i.Ticker, Percentual = i.Percentual }).ToList()
            };
            
            var houveRebalanceamento = resumoAnterior != null;
            response.RebalanceamentoDisparado = houveRebalanceamento;
            
            if (houveRebalanceamento)
            {
                response.CestaAnteriorDesativada = resumoAnterior;
                response.AtivosRemovidos = ativosRemovidos;
                response.AtivosAdicionados = ativosAdicionados;
                response.Mensagem = "Cesta atualizada. Rebalanceamento disparado.";
            }
            else
            {
                response.Mensagem = "Primeira cesta cadastrada com sucesso.";
            }

            // RN-019: A alteração da cesta deve disparar o processo de rebalanceamento
            if (resumoAnterior != null)
            {
                await rebalanceamentoService.ProcessarRebalanceamentoPorMudancaCestaAsync(resumoAnterior.CestaId, novaCesta.Id);
            }

            return response;
        }

        public async Task<CestaDetalhesResponse> ObterCestaAtualAsync()
        {
            var cesta = await cestaRepository.FirstOrDefaultAsync(c => c.Ativa);
            if (cesta == null)
                throw new NotFoundException("Nenhuma cesta ativa encontrada.", "CESTA_NAO_ENCONTRADA");

            var itens = await itemCestaRepository.GetByCestaIdAsync(cesta.Id);
            
            return new CestaDetalhesResponse
            {
                CestaId = cesta.Id,
                Nome = cesta.Nome,
                Ativa = cesta.Ativa,
                DataCriacao = cesta.DataCriacao,
                Itens = itens.Select(i => new ItemCestaDetalhesDTO 
                { 
                    Ticker = i.Ticker, 
                    Percentual = i.Percentual,
                    CotacaoAtual = null
                }).ToList()
            };
        }

        public async Task<IEnumerable<CestaHistoricoResponse>> ObterHistoricoCestasAsync()
        {
            var cestas = await cestaRepository.GetHistoricoAsync();
            var responses = new List<CestaHistoricoResponse>();

            foreach (var cesta in cestas)
            {
                var itens = await itemCestaRepository.GetByCestaIdAsync(cesta.Id);
                responses.Add(new CestaHistoricoResponse
                {
                    CestaId = cesta.Id,
                    Nome = cesta.Nome,
                    Ativa = cesta.Ativa,
                    DataCriacao = cesta.DataCriacao,
                    DataDesativacao = cesta.DataDesativacao,
                    Itens = itens.Select(i => new ItemCestaDTO { Ticker = i.Ticker, Percentual = i.Percentual }).ToList()
                });
            }

            return responses;
        }
    }
}
