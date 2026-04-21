// Tipos TypeScript correspondentes aos DTOs e helpers da API

export interface ApiResult<T> {
  success: boolean;
  errors: string[];
  data: T | null;
}

export interface ResultadoPaginado<T> {
  itens: T[];
  totalRegistros: number;
  pagina: number;
  tamanhoPagina: number;
  totalPaginas: number;
}

export interface ClienteResumoDto {
  totalAtivos: number;
  totalValorMensal: number;
  totalResiduoMaster: number;
  ultimosClientes: {
    clienteId: number;
    nome: string;
    email: string;
    valorMensal: number;
    numeroConta?: string;
  }[];
  itensMaster: {
    ticker: string;
    quantidade: number;
    valorAtual: number;
  }[];
  dataReferencia: string;
}

export interface ClienteDto {
  clienteId: number;
  nome: string;
  cpf: string;
  email: string;
  valorMensal: number;
  ativo: boolean;
  dataAdesao: string;
  contaGrafica: {
    id: number;
    numeroConta: string;
    tipo: string;
    dataCriacao: string;
  };
}

export interface RentabilidadeDto {
  clienteId: number;
  nome: string;
  dataConsulta: string;
  rentabilidade: {
    valorTotalInvestido: number;
    valorAtualCarteira: number;
    plTotal: number;
    rentabilidadePercentual: number;
  };
  historicoAportes: Array<{
    data: string;
    valor: number;
    parcela: string;
  }>;
  evolucaoCarteira: Array<{
    data: string;
    valorInvestido: number;
    valorCarteira: number;
    rentabilidade: number;
  }>;
}

export interface AtivoCarteiraDto {
  ticker: string;
  quantidade: number;
  precoMedio: number;
  cotacaoAtual: number;
  valorAtual: number;
  pl: number;
  plPercentual: number;
  composicaoCarteira: number;
}

export interface CarteiraDto {
  clienteId: number;
  nome: string;
  contaGrafica: string;
  dataConsulta: string;
  resumo: {
    valorTotalInvestido: number;
    valorAtualCarteira: number;
    plTotal: number;
    rentabilidadePercentual: number;
  };
  ativos: AtivoCarteiraDto[];
}

export interface CestaItemDto {
  id?: number;
  ticker: string;
  percentual: number;
}

export interface CestaDto {
  cestaId: number;
  nome: string;
  ativa: boolean;
  dataCriacao: string;
  itens: CestaItemDto[];
}

export interface CustodiaMasterDto {
  dataConsulta: string;
  totalAtivos: number;
  ativos: Array<{
    ticker: string;
    quantidadeTotal: number;
  }>;
}

