import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { clientesApi } from '../../services/api';
import type { ClienteDto } from '../../types/api';
import { formatarMoeda, formatarDataBr } from '../../helpers/masks';
import {
  Container,
  Box,
  PageHeader,
  EmptyState,
  Table,
  Thead,
  Tbody,
  Tr,
  Th,
  Td,
  Loading,
  ErrorMessage,
} from '../../components';

// Página de listagem de clientes
export default function ClientesList() {
  const navigate = useNavigate();
  const [clientes, setClientes] = useState<ClienteDto[]>([]);
  const [totalRegistros, setTotalRegistros] = useState(0);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [paginaAtual, setPaginaAtual] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const TAMANHO_PAGINA = 10;

  // Carrega a lista de clientes
  const loadClientes = async (pagina: number) => {
    try {
      setLoading(true);
      setError(null);
      const result = await clientesApi.getAll(pagina, TAMANHO_PAGINA, true);

      if (result.success && result.data) {
        setClientes(result.data.itens || []);
        setTotalRegistros(result.data.totalRegistros || 0);
        setTotalPaginas(result.data.totalPaginas || 0);
        setPaginaAtual(result.data.pagina || 1);
      } else {
        setError(result.errors.join(', ') || 'Erro ao carregar clientes');
      }
    } catch (err) {
      setError('Erro ao conectar com a API');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadClientes(1);
  }, []);

  const handlePageChange = (novaPagina: number) => {
    if (novaPagina >= 1 && novaPagina <= totalPaginas) {
      loadClientes(novaPagina);
    }
  };

  if (error && clientes.length === 0) {
    return <ErrorMessage message={error} onRetry={() => loadClientes(1)} />;
  }

  return (
    <Container>
      <PageHeader
        title="Clientes Ativos"
        subtitle={`${totalRegistros} ${totalRegistros === 1 ? 'cliente associado' : 'clientes associados'}`}
        actionLabel="Nova Adesão"
        onAction={() => navigate('/clientes/novo')}
      />

      {loading && clientes.length === 0 ? (
        <Loading message="Carregando clientes..." />
      ) : (
        <>
          {clientes.length === 0 ? (
            <EmptyState message="Nenhum cliente cadastrado no produto" />
          ) : (
            <div className="flex flex-col gap-4">
              <Box>
                <Table>
                  <Thead>
                    <Tr isHeader>
                      <Th>Nome / CPF</Th>
                      <Th align="center">Conta</Th>
                      <Th align="right">Investimento Mensal</Th>
                      <Th align="center">Data Adesão</Th>
                      <Th align="center">Status</Th>
                      <Th align="right">Ações</Th>
                    </Tr>
                  </Thead>
                  <Tbody>
                    {clientes.map((cliente, index) => (
                      <Tr key={cliente.clienteId} index={index}>
                        <Td>
                          <div className="flex flex-col">
                            <span className="font-bold text-[#003399] dark:text-blue-400">{cliente.nome}</span>
                            <span className="text-xs text-slate-500">{cliente.cpf}</span>
                          </div>
                        </Td>
                        <Td align="center">
                          <span className="font-mono text-sm bg-slate-100 px-2 py-1 rounded dark:bg-slate-800">
                            {cliente.contaGrafica?.numeroConta || 'N/A'}
                          </span>
                        </Td>
                        <Td align="right">
                          <span className="font-semibold text-slate-900 dark:text-white">
                            {formatarMoeda(cliente.valorMensal)}
                          </span>
                        </Td>
                        <Td align="center">{formatarDataBr(cliente.dataAdesao)}</Td>
                        <Td align="center">
                          <span className={`px-2 py-1 rounded-full text-xs font-bold ${cliente.ativo ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                            {cliente.ativo ? 'ATIVO' : 'INATIVO'}
                          </span>
                        </Td>
                        <Td align="right">
                          <div className="flex justify-end gap-2">
                            <button
                              onClick={() => navigate(`/clientes/detalhes/${cliente.clienteId}`)}
                              className="text-[#EC7000] hover:text-orange-700 font-medium text-sm"
                            >
                              Detalhes / Carteira
                            </button>
                          </div>
                        </Td>
                      </Tr>
                    ))}
                  </Tbody>
                </Table>
              </Box>

              {/* Controles de Paginação */}
              {totalPaginas > 1 && (
                <div className="flex items-center justify-between px-2">
                  <span className="text-sm text-slate-600 dark:text-slate-400">
                    Página <strong>{paginaAtual}</strong> de <strong>{totalPaginas}</strong>
                  </span>

                  <div className="flex gap-2">
                    <button
                      onClick={() => handlePageChange(paginaAtual - 1)}
                      disabled={paginaAtual === 1 || loading}
                      className="px-4 py-2 border rounded-md text-sm font-medium bg-white dark:bg-slate-800 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
                    >
                      Anterior
                    </button>

                    <div className="flex items-center gap-1">
                      {Array.from({ length: totalPaginas }, (_, i) => i + 1).map(p => (
                        <button
                          key={p}
                          onClick={() => handlePageChange(p)}
                          disabled={loading}
                          className={`w-8 h-8 rounded-md text-sm font-medium transition-colors ${paginaAtual === p
                              ? 'bg-[#EC7000] text-white'
                              : 'bg-white dark:bg-slate-800 border hover:bg-slate-50'
                            }`}
                        >
                          {p}
                        </button>
                      ))}
                    </div>

                    <button
                      onClick={() => handlePageChange(paginaAtual + 1)}
                      disabled={paginaAtual === totalPaginas || loading}
                      className="px-4 py-2 border rounded-md text-sm font-medium bg-white dark:bg-slate-800 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
                    >
                      Próximo
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}
        </>
      )}
    </Container>
  );
}
