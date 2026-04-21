import { type ReactNode } from 'react';

// Box com borda, sombra e estilização padronizada
export default function Box({ 
  children, 
  className = '', 
  variant = 'default',
  title
}: { 
  children: ReactNode; 
  className?: string; 
  variant?: 'default' | 'dashed';
  title?: string;
}) {
  const baseClasses = 'rounded-xl border';
  const variantClasses = {
    default: `bg-white/95 backdrop-blur-sm dark:bg-slate-900 border-slate-200/80 shadow-lg dark:border-slate-800 dark:shadow-none`,
    dashed: 'border-dashed border-slate-200/60 dark:border-slate-800',
  };

  // Se houver bg- na className, removemos o fundo padrão do variant
  const hasCustomBg = className.includes('bg-');
  const finalVariantClasses = hasCustomBg 
    ? variantClasses[variant].replace('bg-white/95 backdrop-blur-sm', '').replace('dark:bg-slate-900', '')
    : variantClasses[variant];

  return (
    <div className={`relative ${baseClasses} ${finalVariantClasses} ${className}`}>
      {title && (
        <div className="border-b border-slate-100 dark:border-slate-800 px-6 py-4">
          <h3 className="text-lg font-bold text-slate-800 dark:text-blue-400">
            {title}
          </h3>
        </div>
      )}
      <div className={title ? 'p-6' : 'p-4'}>
        {children}
      </div>
    </div>
  );
}

