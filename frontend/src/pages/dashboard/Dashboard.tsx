import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { clientesApi, adminApi } from '../../services/api';
import type { ClienteDto, CustodiaMasterDto } from '../../types/api';
import { formatarMoeda } from '../../helpers/masks';
import {
  Container,
  Box,
  PageHeader,
  Loading,
} from '../../components';
import {
  UsersIcon,
  CurrencyDollarIcon,
  ClipboardDocumentCheckIcon,
  CpuChipIcon
} from '@heroicons/react/24/outline';

export default function Dashboard() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [clientes, setClientes] = useState<ClienteDto[]>([]);
  const [stats, setStats] = useState({ totalAtivos: 0, totalValorMensal: 0, totalResiduoMaster: 0 });
  const [itensMaster, setItensMaster] = useState<{ ticker: string; valorAtual: number }[]>([]);

  useEffect(() => {
    const loadDashboardData = async () => {
      try {
        setLoading(true);
        const result = await clientesApi.getResumo();

        if (result.success && result.data) {
          setStats({
            totalAtivos: result.data.totalAtivos || 0,
            totalValorMensal: result.data.totalValorMensal || 0,
            totalResiduoMaster: result.data.totalResiduoMaster || 0
          });
          setClientes(result.data.ultimosClientes || []);
          setItensMaster(result.data.itensMaster || []);
        }
      } finally {
        setLoading(false);
      }
    };

    loadDashboardData();
  }, []);

  if (loading) return <Loading message="Carregando visão geral..." />;

  const totalMonthly = stats.totalValorMensal;
  const activeCount = stats.totalAtivos;
  const masterResidue = stats.totalResiduoMaster;

  return (
    <Container>
      <PageHeader
        title="Painel de Controle"
        subtitle="Visão consolidada do produto Compra Programada Itaú"
      />

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <Box className="flex items-center gap-4 border-l-4 border-l-blue-600">
          <div className="p-3 bg-blue-100 rounded-lg dark:bg-blue-900/30">
            <UsersIcon className="h-6 w-6 text-blue-600" />
          </div>
          <div>
            <p className="text-xs text-slate-500 uppercase font-bold">Clientes Ativos</p>
            <p className="text-2xl font-bold">{activeCount}</p>
          </div>
        </Box>

        <Box className="flex items-center gap-4 border-l-4 border-l-green-600">
          <div className="p-3 bg-green-100 rounded-lg dark:bg-green-900/30">
            <CurrencyDollarIcon className="h-6 w-6 text-green-600" />
          </div>
          <div>
            <p className="text-xs text-slate-500 uppercase font-bold">Investimento Mensal Total</p>
            <p className="text-2xl font-bold">{formatarMoeda(totalMonthly)}</p>
          </div>
        </Box>

        <Box className="flex items-center gap-4 border-l-4 border-l-orange-500">
          <div className="p-3 bg-orange-100 rounded-lg dark:bg-orange-900/30">
            <ClipboardDocumentCheckIcon className="h-6 w-6 text-orange-600" />
          </div>
          <div>
            <p className="text-xs text-slate-500 uppercase font-bold">Resíduos na Master (Custódia)</p>
            <p className="text-2xl font-bold">{formatarMoeda(masterResidue)}</p>
          </div>
        </Box>

        <Box className="flex items-center gap-4 border-l-4 border-l-purple-600">
          <div className="p-3 bg-purple-100 rounded-lg dark:bg-purple-900/30">
            <CpuChipIcon className="h-6 w-6 text-purple-600" />
          </div>
          <div>
            <p className="text-xs text-slate-500 uppercase font-bold">Status do Motor</p>
            <p className="text-2xl font-bold text-green-600 uppercase text-sm">Operacional</p>
          </div>
        </Box>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <Box title="Últimas Adesões">
          <div className="divide-y divide-slate-100 dark:divide-slate-800">
            {clientes.map(cliente => (
              <div key={cliente.clienteId} className="py-3 flex justify-between items-center group cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800/50 px-2 rounded-lg transition-colors" onClick={() => navigate(`/clientes/detalhes/${cliente.clienteId}`)}>
                <div className="flex flex-col">
                  <span className="font-bold text-slate-800 dark:text-blue-400 group-hover:text-orange-600">{cliente.nome}</span>
                  <span className="text-xs text-slate-500">{cliente.email}</span>
                </div>
                  <div className="text-right">
                  <p className="font-bold text-slate-900 dark:text-white">{formatarMoeda(cliente.valorMensal)}</p>
                  <p className="text-[10px] text-slate-400 uppercase tracking-widest">{cliente.numeroConta}</p>
                </div>
              </div>
            ))}
          </div>
          <button
            onClick={() => navigate('/clientes')}
            className="w-full mt-4 py-2 text-sm font-bold text-[#003399] hover:bg-blue-50 rounded-lg transition-colors"
          >
            Ver todos os clientes
          </button>
        </Box>

        <Box title="Composição da Custódia Master">
          {itensMaster && itensMaster.length > 0 ? (
            <div className="space-y-4">
              {itensMaster.map(item => {
                const maxVal = Math.max(...itensMaster.map(i => i.valorAtual), 1);
                const percent = (item.valorAtual / maxVal) * 100;

                return (
                  <div key={item.ticker}>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="font-bold">{item.ticker}</span>
                      <span className="text-slate-600">{formatarMoeda(item.valorAtual)} (Total)</span>
                    </div>
                    <div className="w-full h-2 bg-slate-100 rounded-full dark:bg-slate-800 overflow-hidden">
                      <div
                        className="h-full bg-[#EC7000]"
                        style={{ width: `${percent}%` }}
                      ></div>
                    </div>
                  </div>
                );
              })}
              <p className="text-[10px] text-slate-400 mt-4 italic">
                * A conta master retém as frações de ações que não puderam ser distribuídas integralmente aos clientes durante o rebalanceamento.
              </p>
            </div>
          ) : (
            <div className="py-10 text-center text-slate-400">
              Nenhuma custódia residual na conta master.
            </div>
          )}
        </Box>
      </div>
    </Container>
  );
}
