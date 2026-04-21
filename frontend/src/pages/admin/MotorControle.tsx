import { useState } from 'react';
import { motorApi } from '../../services/api';
import {
  Container,
  Box,
  PageHeader,
  Button,
  Input,
  Loading,
} from '../../components';
import { swal } from '../../utils/swal';
import { PlayIcon, CpuChipIcon, CalendarIcon } from '@heroicons/react/24/outline';

export default function MotorControle() {
  const [loading, setLoading] = useState(false);
  const [dataProcessamento, setDataProcessamento] = useState(new Date().toISOString().split('T')[0]);
  const [logs, setLogs] = useState<string[]>([]);

  const handleExecutar = async () => {
    const confirm = await swal.confirm(
      `Deseja realmente disparar o processamento para a data ${dataProcessamento}? Isso executará as rotinas de aporte, frações e rebalanceamento.`,
      'Executar Motor',
      'Disparar Agora',
      'Cancelar'
    );

    if (!confirm.isConfirmed) return;

    try {
      setLoading(true);
      setLogs(prev => [...prev, `[${new Date().toLocaleTimeString()}] Iniciando processamento para ${dataProcessamento}...`]);
      
      const result = await motorApi.executar(dataProcessamento);

      if (result.success) {
        swal.success('Motor executado com sucesso!', 'Processamento concluído');
        setLogs(prev => [...prev, `[${new Date().toLocaleTimeString()}] SUCESSO: ${JSON.stringify(result.data)}`]);
      } else {
        swal.error('Erro ao executar motor', result.errors.join(', '));
        setLogs(prev => [...prev, `[${new Date().toLocaleTimeString()}] ERRO: ${result.errors.join(', ')}`]);
      }
    } catch (err) {
      swal.errorToast('Erro de conexão com o motor');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container>
      <PageHeader
        title="Controle do Motor de Compra"
        subtitle="Ferramentas administrativas para teste e simulação de rebalanceamento"
      />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-1 space-y-6">
            <Box title="Disparo Manual">
                <p className="text-sm text-slate-600 mb-6">
                    O motor processa automaticamente nos dias 5, 15 e 25. Use esta ferramenta para forçar a execução em datas específicas.
                </p>

                <div className="space-y-4">
                    <Input 
                        label="Data de Simulação"
                        type="date"
                        value={dataProcessamento}
                        onChange={(e) => setDataProcessamento(e.target.value)}
                        disabled={loading}
                    />

                    <Button 
                        onClick={handleExecutar}
                        disabled={loading}
                        className="w-full flex items-center justify-center gap-2 bg-[#003399]"
                    >
                        {loading ? <Loading compact /> : <PlayIcon className="h-5 w-5" />}
                        Executar Ciclo Completo
                    </Button>
                </div>
            </Box>

            <Box className="bg-orange-50 border-orange-200 border-2">
                <div className="flex gap-3">
                    <CpuChipIcon className="h-6 w-6 text-orange-600 shrink-0" />
                    <div>
                        <p className="text-sm font-bold text-orange-800 uppercase">Atenção</p>
                        <p className="text-xs text-orange-700">
                            A execução manual afetará as ordens de compra e os saldos reais (ou simulados) de todos os clientes ativos no sistema.
                        </p>
                    </div>
                </div>
            </Box>
        </div>

        <div className="lg:col-span-2">
            <Box title="Log de Execução" className="h-full flex flex-col">
                <div className="bg-slate-900 rounded-lg p-4 font-mono text-xs text-green-400 h-[400px] overflow-y-auto space-y-1">
                    {logs.length === 0 && (
                        <p className="text-slate-500 italic">Nenhuma atividade registrada nesta sessão...</p>
                    )}
                    {logs.map((log, i) => (
                        <p key={i}>{log}</p>
                    ))}
                    {loading && (
                        <p className="animate-pulse">_ Processando...</p>
                    )}
                </div>
                <button 
                  onClick={() => setLogs([])}
                  className="mt-3 text-xs text-slate-400 hover:text-slate-600 self-end"
                >
                    Limpar Logs
                </button>
            </Box>
        </div>
      </div>

      <div className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6">
         <div className="p-4 bg-white dark:bg-slate-800 rounded-xl shadow-sm border flex items-center gap-4">
            <div className="bg-blue-50 p-2 rounded-lg"><CalendarIcon className="h-5 w-5 text-blue-600" /></div>
            <div>
                <p className="text-[10px] uppercase font-bold text-slate-400">Próxima Compra</p>
                <p className="text-sm font-bold">25 de Abril, 2024</p>
            </div>
         </div>
         {/* More cards can go here */}
      </div>
    </Container>
  );
}
