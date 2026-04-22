import axios from 'axios';
import type { 
  ApiResult, 
  ClienteDto, 
  CarteiraDto, 
  RentabilidadeDto, 
  CestaDto, 
  CestaItemDto, 
  CustodiaMasterDto,
  ResultadoPaginado,
  ClienteResumoDto
} from '../types/api';

// Configuração base do cliente HTTP para comunicação com a API
const baseURL = import.meta.env.VITE_API_URL || (import.meta.env.DEV ? 'http://localhost:5027' : '');
const api = axios.create({
  baseURL: baseURL.endsWith('/api') ? baseURL : `${baseURL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Serviço de API para Clientes
export const clientesApi = {
  // Lista todos os clientes com paginação (Nomes em português para combinar com os DTOs)
  getAll: async (pagina = 1, tamanhoPagina = 10, ordemDesc = true): Promise<ApiResult<ResultadoPaginado<ClienteDto>>> => {
    const response = await api.get<ResultadoPaginado<ClienteDto>>('/clientes', {
      params: { pagina, tamanhoPagina, ordemDesc }
    });
    return { success: true, errors: [], data: response.data };
  },

  // Busca resumo de métricas para o dashboard
  getResumo: async (): Promise<ApiResult<ClienteResumoDto>> => {
    const response = await api.get<ClienteResumoDto>('/clientes/resumo');
    return { success: true, errors: [], data: response.data };
  },

  // Busca carteira do cliente
  getCarteira: async (clienteId: number): Promise<ApiResult<CarteiraDto>> => {
    const response = await api.get<CarteiraDto>(`/clientes/${clienteId}/carteira`);
    return { success: true, errors: [], data: response.data };
  },

  // Busca rentabilidade do cliente
  getRentabilidade: async (clienteId: number): Promise<ApiResult<RentabilidadeDto>> => {
    const response = await api.get<RentabilidadeDto>(`/clientes/${clienteId}/rentabilidade`);
    return { success: true, errors: [], data: response.data };
  },

  // Realiza a adesão de um novo cliente
  aderir: async (request: { nome: string; cpf: string; email: string; valorMensal: number }): Promise<ApiResult<ClienteDto>> => {
    try {
      const response = await api.post<ClienteDto>('/clientes/adesao', request);
      return { success: true, errors: [], data: response.data };
    } catch (error: any) {
      return { success: false, errors: [error.response?.data?.detail || 'Erro na adesão'], data: null };
    }
  },

  // Cancela participação
  sair: async (clienteId: number): Promise<ApiResult<any>> => {
    const response = await api.post(`/clientes/${clienteId}/saida`);
    return { success: true, errors: [], data: response.data };
  },

  // Altera valor mensal
  alterarValor: async (clienteId: number, novoValor: number): Promise<ApiResult<any>> => {
    const response = await api.put(`/clientes/${clienteId}/valor-mensal`, { novoValorMensal: novoValor });
    return { success: true, errors: [], data: response.data };
  },
};

// Serviço de API para Administração (Cesta Recomendada)
export const adminApi = {
  // Busca a cesta atual
  getCestaAtual: async (): Promise<ApiResult<CestaDto>> => {
    const response = await api.get<CestaDto>('/admin/cesta/atual');
    return { success: true, errors: [], data: response.data };
  },

  // Configura uma nova cesta
  configurarCesta: async (request: { nome: string; itens: CestaItemDto[] }): Promise<ApiResult<CestaDto>> => {
    try {
      const response = await api.post<CestaDto>('/admin/cesta', request);
      return { success: true, errors: [], data: response.data };
    } catch (error: any) {
      return { success: false, errors: [error.response?.data?.detail || 'Erro ao configurar cesta'], data: null };
    }
  },

  // Busca histórico de cestas
  getHistoricoCestas: async (): Promise<ApiResult<{ cestas: CestaDto[] }>> => {
    const response = await api.get<{ cestas: CestaDto[] }>('/admin/cesta/historico');
    return { success: true, errors: [], data: response.data };
  },

  // Busca custódia master
  getCustodiaMaster: async (): Promise<ApiResult<CustodiaMasterDto>> => {
    const response = await api.get<CustodiaMasterDto>('/admin/conta-master/custodia');
    return { success: true, errors: [], data: response.data };
  },
};

// Serviço de API para o Motor de Compra (Controle manual)
export const motorApi = {
  // Dispara o processamento diário
  executar: async (data?: string): Promise<ApiResult<any>> => {
    const params = data ? { data } : {};
    const response = await api.post('/motorcompra/executar-teste', null, { params });
    return { success: true, errors: [], data: response.data };
  },
};

export default api;
