import { type ReactNode } from 'react';

// Container principal com espaçamento padronizado
export default function Container({ children, className = '' }: { children: ReactNode; className?: string }) {
  return <div className={`space-y-4 ${className}`}>{children}</div>;
}

