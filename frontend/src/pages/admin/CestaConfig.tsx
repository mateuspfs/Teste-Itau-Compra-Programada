import { useEffect, useState } from 'react';
import { adminApi } from '../../services/api';
import type { CestaDto, CestaItemDto } from '../../types/api';
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
  Button,
  Input,
} from '../../components';
import { swal } from '../../utils/swal';
import { formatarDataBr } from '../../helpers/masks';
import { PlusIcon, TrashIcon } from '@heroicons/react/24/outline';

export default function CestaConfig() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [cestaAtual, setCestaAtual] = useState<CestaDto | null>(null);
  const [historico, setHistorico] = useState<CestaDto[]>([]);
  
  // Edit state
  const [isEditing, setIsEditing] = useState(false);
  const [editDescricao, setEditDescricao] = useState('');
  const [editItens, setEditItens] = useState<CestaItemDto[]>([]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [resAtual, resHist] = await Promise.all([
        adminApi.getCestaAtual(),
        adminApi.getHistoricoCestas()
      ]);

      if (resAtual.success && resAtual.data) {
        setCestaAtual(resAtual.data);
      }
      if (resHist.success && resHist.data) {
        setHistorico(resHist.data.cestas);
      }
    } catch (err) {
      setError('Erro ao carregar dados da cesta');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleStartEdit = () => {
    if (cestaAtual) {
      setEditDescricao(cestaAtual.nome + ' (Nova)');
      setEditItens(cestaAtual.itens.map(i => ({ ticker: i.ticker, percentual: i.percentual })));
    } else {
      setEditDescricao('Cesta Recomendada');
      setEditItens([{ ticker: '', percentual: 0 }]);
    }
    setIsEditing(true);
  };

  const handleAddItem = () => {
    setEditItens([...editItens, { ticker: '', percentual: 0 }]);
  };

  const handleRemoveItem = (index: number) => {
    setEditItens(editItens.filter((_, i) => i !== index));
  };

  const handleItemChange = (index: number, field: keyof CestaItemDto, value: any) => {
    const newItems = [...editItens];
    newItems[index] = { ...newItems[index], [field]: value };
    setEditItens(newItems);
  };

  const handleSave = async () => {
    const totalPercentual = editItens.reduce((acc, i) => acc + Number(i.percentual), 0);
    if (totalPercentual !== 100) {
      swal.errorToast(`A soma dos pesos deve ser 100%. Atual: ${totalPercentual}%`);
      return;
    }

    try {
      setLoading(true);
      const result = await adminApi.configurarCesta({
        nome: editDescricao,
        itens: editItens.map(i => ({ ticker: i.ticker.toUpperCase(), percentual: Number(i.percentual) }))
      });

      if (result.success) {
        swal.successToast('Nova cesta configurada com sucesso!');
        setIsEditing(false);
        loadData();
      } else {
        swal.errorToast(result.errors.join(', '));
      }
    } catch (err) {
        swal.errorToast('Erro ao salvar cesta');
    } finally {
        setLoading(false);
    }
  };

  if (loading && !isEditing) return <Loading message="Carregando configurações..." />;

  return (
    <Container>
      <PageHeader
        title="Gestão da Cesta Recomendada"
        subtitle="Defina os ativos e pesos para o rebalanceamento automático"
        actionLabel={isEditing ? 'Cancelar Edição' : 'Novos Pesos'}
        onAction={() => setIsEditing(!isEditing)}
      />

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Lado Esquerdo: Configuração Ativa ou Formulário */}
        <Box title={isEditing ? 'Configurar Nova Cesta' : 'Composição em Vigor'}>
          {isEditing ? (
            <div className="space-y-4">
               <Input 
                 label="Descrição / Nome da Cesta" 
                 value={editDescricao} 
                 onChange={(e) => setEditDescricao(e.target.value)}
                 placeholder="Ex: Top Five Abril 2024"
               />
               
               <div className="space-y-3">
                 <div className="flex justify-between items-center bg-slate-50 dark:bg-slate-800 p-2 rounded">
                    <span className="text-sm font-bold">Ativos e Pesos (%)</span>
                    <button onClick={handleAddItem} className="text-blue-600 hover:text-blue-800 flex items-center gap-1 text-xs font-bold">
                        <PlusIcon className="h-4 w-4" /> ADICIONAR
                    </button>
                 </div>
                 
                 {editItens.map((item, idx) => (
                   <div key={idx} className="flex gap-2 items-end">
                      <div className="flex-1">
                        <Input 
                          placeholder="TICKER" 
                          value={item.ticker} 
                          onChange={(e) => handleItemChange(idx, 'ticker', e.target.value)}
                          className="font-mono uppercase"
                        />
                      </div>
                      <div className="w-24">
                        <Input 
                          type="number" 
                          placeholder="%" 
                          value={item.percentual.toString()} 
                          onChange={(e) => handleItemChange(idx, 'percentual', e.target.value)}
                        />
                      </div>
                      <button 
                        onClick={() => handleRemoveItem(idx)}
                        className="mb-2 p-2 text-red-500 hover:bg-red-50 rounded"
                      >
                        <TrashIcon className="h-5 w-5" />
                      </button>
                   </div>
                 ))}
               </div>

               <div className="pt-4 border-t">
                  <div className="flex justify-between items-center mb-4">
                    <span className="text-sm">Soma dos pesos:</span>
                    <span className={`font-bold ${editItens.reduce((acc, i) => acc + Number(i.percentual), 0) === 100 ? 'text-green-600' : 'text-red-600'}`}>
                      {editItens.reduce((acc, i) => acc + Number(i.percentual), 0)}%
                    </span>
                  </div>
                  <Button variant="primary" className="w-full bg-[#EC7000] border-none" onClick={handleSave}>
                    Ativar Nova Configuração
                  </Button>
               </div>
            </div>
          ) : (
            <>
              {cestaAtual ? (
                <div className="space-y-6">
                  <div className="bg-blue-50 dark:bg-blue-900/20 p-4 rounded-xl">
                    <p className="text-sm font-bold text-blue-800 dark:text-blue-300">{cestaAtual.nome}</p>
                    <p className="text-xs text-blue-600 dark:text-blue-400">Ativada em {formatarDataBr(cestaAtual.dataCriacao)}</p>
                  </div>

                  <Table>
                    <Thead>
                      <Tr isHeader>
                        <Th>Ticker</Th>
                        <Th align="right">Peso Percentual</Th>
                        <Th align="right">Visual</Th>
                      </Tr>
                    </Thead>
                    <Tbody>
                      {cestaAtual.itens.map((item, i) => (
                        <Tr key={item.ticker} index={i}>
                          <Td className="font-mono font-bold">{item.ticker}</Td>
                          <Td align="right" className="font-bold">{item.percentual}%</Td>
                          <Td align="right">
                             <div className="w-24 h-1.5 bg-slate-100 rounded-full overflow-hidden">
                                <div className="h-full bg-orange-500" style={{ width: `${item.percentual}%` }}></div>
                             </div>
                          </Td>
                        </Tr>
                      ))}
                    </Tbody>
                  </Table>
                  
                  <button 
                    onClick={handleStartEdit}
                    className="w-full py-2 border-2 border-dashed border-slate-200 text-slate-400 hover:border-orange-500 hover:text-orange-500 rounded-xl transition-all font-bold text-sm"
                  >
                    CLONAR E EDITAR CESTA
                  </button>
                </div>
              ) : (
                <div className="py-10 text-center">
                   <p className="text-slate-400 mb-4">Nenhuma cesta configurada.</p>
                   <Button onClick={handleStartEdit}>Configurar Primeira Cesta</Button>
                </div>
              )}
            </>
          )}
        </Box>

        {/* Lado Direito: Histórico */}
        <Box title="Histórico de Recomendações">
           <div className="space-y-4">
              {historico.length > 0 ? (
                historico.slice(0, 5).map(c => (
                  <div key={c.cestaId} className="p-3 border rounded-lg hover:border-blue-300 transition-colors">
                     <div className="flex justify-between items-start mb-1">
                        <span className="text-sm font-bold">{c.nome}</span>
                        {c.ativa && <span className="bg-green-100 text-green-700 text-[10px] font-bold px-1.5 rounded">ATIVA</span>}
                     </div>
                     <p className="text-[10px] text-slate-400 mb-2">{formatarDataBr(c.dataCriacao)}</p>
                     <div className="flex gap-1 flex-wrap">
                        {c.itens.map(i => (
                          <span key={i.ticker} className="text-[9px] bg-slate-100 dark:bg-slate-800 px-1 rounded">
                            {i.ticker} ({i.percentual}%)
                          </span>
                        ))}
                     </div>
                  </div>
                ))
              ) : (
                <p className="text-slate-400 text-center py-10">Nenhum histórico disponível.</p>
              )}
           </div>
        </Box>
      </div>
    </Container>
  );
}
