import { type ReactNode } from 'react';
import Button from './Button';

// Cabeçalho padronizado de página com título, subtítulo e ação
interface PageHeaderProps {
  title: string;
  subtitle?: string;
  actionLabel?: string;
  onAction?: () => void;
  actionIcon?: ReactNode;
}

export default function PageHeader({ title, subtitle, actionLabel, onAction, actionIcon }: PageHeaderProps) {
  return (
    <div className="flex items-center justify-between">
      <div>
        <h1 className="text-2xl font-semibold text-slate-800 dark:text-slate-100">{title}</h1>
        {subtitle && (
          <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{subtitle}</p>
        )}
      </div>
      {actionLabel && onAction && (
        <Button onClick={onAction} variant="primary">
          {actionIcon && <span className="mr-2">{actionIcon}</span>}
          {actionLabel}
        </Button>
      )}
    </div>
  );
}

