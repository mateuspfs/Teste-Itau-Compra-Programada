import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { clientesApi } from '../../services/api';
import type { ClienteDto, CarteiraDto, RentabilidadeDto } from '../../types/api';
import { formatarMoeda, formatarDataBr } from '../../helpers/masks';
import {
  Container,
  Box,
  PageHeader,
  Table,
  Thead,
  Tbody,
  Tr,
  Th,
  Td,
  Loading,
  ErrorMessage,
  ColoredText,
} from '../../components';

export default function ClienteDetalhes() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [carteira, setCarteira] = useState<CarteiraDto | null>(null);
  const [rentabilidade, setRentabilidade] = useState<RentabilidadeDto | null>(null);

  const loadData = async () => {
    if (!id) return;
    try {
      setLoading(true);
      setError(null);
      
      const [resCarteira, resRent] = await Promise.all([
        clientesApi.getCarteira(Number(id)),
        clientesApi.getRentabilidade(Number(id))
      ]);

      if (resCarteira.success && resCarteira.data) {
        setCarteira(resCarteira.data);
      } else {
        setError('Erro ao carregar carteira');
      }

      if (resRent.success && resRent.data) {
        setRentabilidade(resRent.data);
      }
    } catch (err) {
      setError('Erro ao conectar com a API');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [id]);

  if (loading) return <Loading message="Carregando detalhes do cliente..." />;
  if (error || !carteira) return <ErrorMessage message={error || 'Cliente não encontrado'} onRetry={loadData} />;

  return (
    <Container>
      <PageHeader
        title={`Portfólio: ${carteira.nome}`}
        subtitle={`Conta: ${carteira.contaGrafica} | Consulta em ${formatarDataBr(carteira.dataConsulta)}`}
        actionLabel="Voltar"
        onAction={() => navigate('/clientes')}
      />

      {/* Cards de Resumo */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <Box className="bg-white dark:bg-slate-900 border-l-4 border-l-blue-600">
          <p className="text-xs text-slate-500 uppercase font-bold mb-1">Total Investido</p>
          <p className="text-xl font-bold text-slate-900 dark:text-white">
            {formatarMoeda(carteira.resumo.valorTotalInvestido)}
          </p>
        </Box>
        <Box className="bg-white dark:bg-slate-900 border-l-4 border-l-[#EC7000]">
          <p className="text-xs text-slate-500 uppercase font-bold mb-1">Valor de Mercado</p>
          <p className="text-xl font-bold text-[#EC7000]">
            {formatarMoeda(carteira.resumo.valorAtualCarteira)}
          </p>
        </Box>
        <Box className="bg-white dark:bg-slate-900 border-l-4 border-l-green-600">
          <p className="text-xs text-slate-500 uppercase font-bold mb-1">Lucro/Prejuízo (P/L)</p>
          <ColoredText color={carteira.resumo.plTotal >= 0 ? 'green' : 'red'} className="text-xl font-bold">
            {carteira.resumo.plTotal >= 0 ? '+' : ''} {formatarMoeda(carteira.resumo.plTotal)}
          </ColoredText>
        </Box>
        <Box className="bg-white dark:bg-slate-900 border-l-4 border-l-[#003399]">
          <p className="text-xs text-slate-500 uppercase font-bold mb-1">Rentabilidade</p>
          <p className="text-2xl font-bold text-[#003399]">
            {carteira.resumo.rentabilidadePercentual.toFixed(2)}%
          </p>
        </Box>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Tabela de Ativos */}
        <div className="lg:col-span-2">
            <Box title="Detalhamento da Custódia">
                <Table>
                    <Thead>
                        <Tr isHeader>
                            <Th>Ticker</Th>
                            <Th align="center">Qtd</Th>
                            <Th align="right">Preço Médio</Th>
                            <Th align="right">Cotação</Th>
                            <Th align="right">P/L %</Th>
                            <Th align="right">Composição</Th>
                        </Tr>
                    </Thead>
                    <Tbody>
                        {carteira.ativos.map((ativo, idx) => (
                            <Tr key={ativo.ticker} index={idx}>
                                <Td className="font-bold">{ativo.ticker}</Td>
                                <Td align="center">{ativo.quantidade}</Td>
                                <Td align="right">{formatarMoeda(ativo.precoMedio)}</Td>
                                <Td align="right">{formatarMoeda(ativo.cotacaoAtual)}</Td>
                                <Td align="right">
                                    <ColoredText color={ativo.plPercentual >= 0 ? 'green' : 'red'} className="font-bold">
                                        {ativo.plPercentual >= 0 ? '+' : ''}{ativo.plPercentual.toFixed(2)}%
                                    </ColoredText>
                                </Td>
                                <Td align="right">
                                    <div className="flex flex-col items-end">
                                        <span className="text-xs font-bold">{ativo.composicaoCarteira.toFixed(1)}%</span>
                                        <div className="w-16 h-1 bg-slate-200 rounded-full mt-1 overflow-hidden">
                                            <div className="h-full bg-orange-500" style={{ width: `${ativo.composicaoCarteira}%` }}></div>
                                        </div>
                                    </div>
                                </Td>
                            </Tr>
                        ))}
                    </Tbody>
                </Table>
            </Box>
        </div>

        {/* Histórico e Evolução Side info */}
        <div className="space-y-6">
             {rentabilidade && (
                 <>
                    <Box title="Próximos Aportes" className="border-t-4 border-t-blue-600">
                        <div className="space-y-3">
                            {rentabilidade.historicoAportes.map((aporte, i) => (
                                <div key={i} className="flex justify-between items-center border-b pb-2 last:border-0">
                                    <div>
                                        <p className="text-sm font-medium">{formatarDataBr(aporte.data)}</p>
                                        <p className="text-xs text-slate-500">Parcela {aporte.parcela}</p>
                                    </div>
                                    <p className="font-bold text-slate-700">{formatarMoeda(aporte.valor)}</p>
                                </div>
                            ))}
                        </div>
                    </Box>

                    <Box title="Ações Recomendadas" className="bg-slate-50 dark:bg-slate-800 border-dashed border-2">
                         <div className="text-center py-4">
                             <p className="text-sm text-slate-600 mb-2">Seu capital é distribuído automaticamente entre as 5 empresas com maior potencial do mês.</p>
                             <button 
                                onClick={() => navigate('/admin/cesta')}
                                className="text-xs font-bold text-[#003399] uppercase hover:underline"
                             >
                                 Ver Cesta Atual
                             </button>
                         </div>
                    </Box>
                 </>
             )}
        </div>
      </div>
    </Container>
  );
}
