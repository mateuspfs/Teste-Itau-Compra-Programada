import { type InputHTMLAttributes, forwardRef, useState, useEffect, useRef } from 'react';

interface CurrencyInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type' | 'value' | 'onChange'> {
  label?: string;
  error?: string;
  value: string | number;
  onChange: (value: string) => void;
}

// Componente de input para valores monetários com formato bancário (os dois últimos dígitos são sempre centavos)
const CurrencyInput = forwardRef<HTMLInputElement, CurrencyInputProps>(
  ({ label, error, value, onChange, className = '', onBlur, onFocus, disabled, placeholder, ...props }, ref) => {
    const decimalToCents = (val: string | number): number => {
      const numVal = typeof val === 'string' ? parseFloat(val) : val;
      if (isNaN(numVal) || numVal === 0) return 0;
      return Math.round(numVal * 100);
    };

    const centsToDecimal = (cents: number): number => {
      return cents / 100;
    };

    const formatCents = (cents: number): string => {
      if (cents === 0) return '0,00';
      const decimal = centsToDecimal(cents);
      return new Intl.NumberFormat('pt-BR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
      }).format(decimal);
    };

    const [centsString, setCentsString] = useState<string>('');
    const [displayValue, setDisplayValue] = useState<string>('0,00');
    const previousValueRef = useRef<string | number>('');

    useEffect(() => {
      if (value !== previousValueRef.current) {
        const cents = decimalToCents(value);
        const centsStr = cents.toString();
        setCentsString(centsStr);
        setDisplayValue(formatCents(cents));
        previousValueRef.current = value;
      }
    }, [value]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const inputValue = e.target.value;
      const rawValue = inputValue.replace(/[^\d]/g, '');
      
      if (!rawValue) {
        setCentsString('');
        setDisplayValue('0,00');
        onChange('0');
        previousValueRef.current = '0';
        return;
      }
      
      setCentsString(rawValue);
      
      const cents = parseInt(rawValue, 10);
      setDisplayValue(formatCents(cents));
      
      const decimalValue = centsToDecimal(cents);
      onChange(decimalValue.toString());
      previousValueRef.current = decimalValue.toString();
    };

    const handleBlurEvent = (e: React.FocusEvent<HTMLInputElement>) => {
      if (centsString) {
        const cents = parseInt(centsString, 10);
        setDisplayValue(formatCents(cents));
      }
      onBlur?.(e);
    };

    const handleFocusEvent = (e: React.FocusEvent<HTMLInputElement>) => {
      onFocus?.(e);
    };

    return (
      <div className="w-full">
        {label && (
          <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
            {label}
            {props.required && <span className="ml-1 text-red-500">*</span>}
          </label>
        )}
        <input
          ref={ref}
          type="text"
          inputMode="numeric"
          value={displayValue}
          onChange={handleChange}
          onBlur={handleBlurEvent}
          onFocus={handleFocusEvent}
          disabled={disabled}
          placeholder={placeholder || "0,00"}
          className={`w-full rounded-lg border px-4 py-2.5 text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-offset-1 ${
            error
              ? 'border-red-300 bg-red-50 text-red-900 placeholder-red-300 focus:border-red-500 focus:ring-red-500 dark:border-red-700 dark:bg-red-900/20 dark:text-red-200 dark:placeholder-red-500'
              : 'border-slate-300 bg-white text-slate-900 placeholder-slate-400 focus:border-brand-500 focus:ring-brand-500 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100 dark:placeholder-slate-500'
          } ${className}`}
          {...props}
        />
        {error && (
          <p className="mt-1.5 text-xs font-medium text-red-600 dark:text-red-400">{error}</p>
        )}
      </div>
    );
  }
);

CurrencyInput.displayName = 'CurrencyInput';

export default CurrencyInput;

