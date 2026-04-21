import { MagnifyingGlassIcon, XMarkIcon } from '@heroicons/react/24/outline';
import { useState, useEffect } from 'react';

// Componente de busca padronizado com debounce
interface SearchProps {
  placeholder?: string;
  onSearch: (searchTerm: string) => void;
  debounceMs?: number;
  className?: string;
  value?: string;
}

export default function Search({
  placeholder = 'Buscar...',
  onSearch,
  debounceMs = 500,
  className = '',
  value: controlledValue,
}: SearchProps) {
  const [internalValue, setInternalValue] = useState(controlledValue || '');
  
  // Sincroniza estado interno quando valor controlado muda externamente
  useEffect(() => {
    if (controlledValue !== undefined) {
      setInternalValue(controlledValue);
    }
  }, [controlledValue]);

  useEffect(() => {
    const timer = setTimeout(() => {
      onSearch(internalValue);
    }, debounceMs);

    return () => clearTimeout(timer);
  }, [internalValue, debounceMs, onSearch]);

  const handleChange = (newValue: string) => {
    setInternalValue(newValue);
  };

  const handleClear = () => {
    setInternalValue('');
    onSearch('');
  };

  return (
    <div className={`relative ${className}`}>
      <div className="relative">
        <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
          <MagnifyingGlassIcon className="h-5 w-5 text-slate-400" />
        </div>
        <input
          type="text"
          value={internalValue}
          onChange={(e) => handleChange(e.target.value)}
          placeholder={placeholder}
          className="block w-full rounded-lg border border-slate-300/80 bg-white/90 backdrop-blur-sm py-2 pl-10 pr-10 text-sm text-slate-700 placeholder-slate-400 shadow-sm transition-all focus:border-brand-500 focus:bg-white focus:outline-none focus:ring-2 focus:ring-brand-500/20 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:placeholder-slate-500 dark:focus:border-brand-400 dark:backdrop-blur-none dark:shadow-none"
        />
        {internalValue && (
          <button
            type="button"
            onClick={handleClear}
            className="absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300"
            aria-label="Limpar busca"
          >
            <XMarkIcon className="h-5 w-5" />
          </button>
        )}
      </div>
    </div>
  );
}

