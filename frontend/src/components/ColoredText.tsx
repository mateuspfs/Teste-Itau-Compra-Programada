interface ColoredTextProps {
  children: React.ReactNode;
  color?: 'green' | 'red' | 'default';
  className?: string;
}

// Componente para exibir texto com cores (verde, vermelho ou padrão)
export default function ColoredText({ children, color = 'default', className = '' }: ColoredTextProps) {
  const colorClasses = {
    green: 'text-green-600 dark:text-green-400',
    red: 'text-red-600 dark:text-red-400',
    default: 'text-slate-700 dark:text-slate-300',
  };

  return (
    <span className={`${colorClasses[color]} ${className}`}>
      {children}
    </span>
  );
}

