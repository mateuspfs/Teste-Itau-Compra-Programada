import { MoonIcon, SunIcon } from '@heroicons/react/24/outline';
import { useTheme } from '../hooks/useTheme';

type Props = {
  compact?: boolean;
};

export default function ThemeToggle({ compact }: Props) {
  const { theme, toggleTheme } = useTheme();

  return (
    <button
      type="button"
      onClick={toggleTheme}
      className={[
        'inline-flex items-center gap-2 rounded-full border text-sm font-medium transition-colors',
        // Estilização condicional baseada no modo compacto
        compact
          ? 'border-slate-200 bg-white px-2.5 py-1 text-slate-700 hover:bg-slate-100 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:hover:bg-slate-700'
          : 'border-slate-200 bg-white px-4 py-2 text-slate-800 shadow-sm hover:bg-slate-100 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:hover:bg-slate-700',
      ].join(' ')}
      aria-label="Alternar tema"
    >
      {/* Ícone e texto condicionais baseados no tema atual */}
      {theme === 'dark' ? (
        <>
          <MoonIcon className="h-5 w-5" />
          {!compact && <span>Tema escuro</span>}
        </>
      ) : (
        <>
          <SunIcon className="h-5 w-5" />
          {!compact && <span>Tema claro</span>}
        </>
      )}
    </button>
  );
}
