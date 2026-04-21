import { type ReactNode } from 'react';

// Componente base de tabela com estilização padronizada
export function Table({ children, className = '' }: { children: ReactNode; className?: string }) {
  return (
    <div className="overflow-x-auto">
      <table className={`min-w-full divide-y divide-slate-200 dark:divide-slate-800 ${className}`}>
        {children}
      </table>
    </div>
  );
}

// Cabeçalho da tabela com cor diferenciada
export function Thead({ children }: { children: ReactNode }) {
  return <thead className="bg-[#0083c0] dark:bg-slate-700">{children}</thead>;
}

// Corpo da tabela
export function Tbody({ children }: { children: ReactNode }) {
  return <tbody className="divide-y divide-slate-200/60 bg-white dark:divide-slate-800 dark:bg-slate-900">{children}</tbody>;
}

// Linha da tabela com cores alternadas (zebra striping)
export function Tr({ 
  children, 
  className = '', 
  onClick,
  index,
  isHeader = false
}: { 
  children: ReactNode; 
  className?: string; 
  onClick?: () => void;
  index?: number;
  isHeader?: boolean;
}) {
  // Se for header, não aplica background (usa o do Thead)
  if (isHeader) {
    return (
      <tr className={className}>
        {children}
      </tr>
    );
  }

  // Cores alternadas: linhas pares mais claras, ímpares mais escuras
  const isEven = index !== undefined && index % 2 === 0;
  const baseBg = isEven 
    ? 'bg-slate-50/50 dark:bg-slate-800/30' 
    : 'bg-white dark:bg-slate-900';
  
  const hoverBg = onClick 
    ? 'hover:bg-slate-100 dark:hover:bg-slate-800/70' 
    : '';

  return (
    <tr
      className={`transition-colors ${baseBg} ${hoverBg} ${onClick ? 'cursor-pointer' : ''} ${className}`}
      onClick={onClick}
    >
      {children}
    </tr>
  );
}

// Célula de cabeçalho com cor diferenciada
export function Th({ children, className = '', align = 'left' }: { children: ReactNode; className?: string; align?: 'left' | 'center' | 'right' }) {
  const alignClass = {
    left: 'text-left',
    center: 'text-center',
    right: 'text-right',
  }[align];

  return (
    <th
      className={`px-6 py-3 text-xs font-semibold uppercase tracking-wider text-white dark:text-slate-100 ${alignClass} ${className}`}
    >
      {children}
    </th>
  );
}

// Célula de dados
export function Td({ children, className = '', align = 'left' }: { children: ReactNode; className?: string; align?: 'left' | 'center' | 'right' }) {
  const alignClass = {
    left: 'text-left',
    center: 'text-center',
    right: 'text-right',
  }[align];

  return (
    <td className={`px-6 py-4 whitespace-nowrap text-sm text-slate-700 dark:text-slate-100 ${alignClass} ${className}`}>
      {children}
    </td>
  );
}

