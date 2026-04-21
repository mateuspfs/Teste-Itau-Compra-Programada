import { type ReactNode } from 'react';
import Box from './Box';

// Componente de estado vazio padronizado
interface EmptyStateProps {
  message: string;
  icon?: ReactNode;
  action?: {
    label: string;
    onClick: () => void;
  };
}

export default function EmptyState({ message, icon, action }: EmptyStateProps) {
  return (
    <Box variant="dashed" className="p-12 text-center bg-slate-50/50 dark:bg-slate-900">
      {icon && <div className="mb-4 flex justify-center">{icon}</div>}
      <p className="text-slate-700 dark:text-slate-400">{message}</p>
      {action && (
        <button
          onClick={action.onClick}
          className="mt-4 text-sm font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300"
        >
          {action.label}
        </button>
      )}
    </Box>
  );
}

