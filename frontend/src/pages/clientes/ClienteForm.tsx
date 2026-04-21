import { Input } from '../../components';

interface ClienteFormProps {
  nome: string;
  cpf: string;
  email: string;
  valorMensal: number;
  errors: {
    nome?: string;
    cpf?: string;
    email?: string;
    valorMensal?: string;
  };
  onNomeChange: (value: string) => void;
  onCpfChange: (value: string) => void;
  onEmailChange: (value: string) => void;
  onValorMensalChange: (value: number) => void;
  disabled?: boolean;
}

export default function ClienteForm({
  nome,
  cpf,
  email,
  valorMensal,
  errors,
  onNomeChange,
  onCpfChange,
  onEmailChange,
  onValorMensalChange,
  disabled,
}: ClienteFormProps) {
  return (
    <div className="space-y-4">
      <Input
        label="Nome Completo"
        placeholder="Digite o nome completo"
        value={nome}
        onChange={(e) => onNomeChange(e.target.value)}
        error={errors.nome}
        disabled={disabled}
        required
      />

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Input
          label="CPF"
          placeholder="000.000.000-00"
          value={cpf}
          onChange={(e) => onCpfChange(e.target.value)}
          error={errors.cpf}
          disabled={disabled}
          required
        />

        <Input
          label="E-mail"
          type="email"
          placeholder="exemplo@email.com"
          value={email}
          onChange={(e) => onEmailChange(e.target.value)}
          error={errors.email}
          disabled={disabled}
          required
        />
      </div>

      <Input
        label="Investimento Mensal (BRL)"
        type="number"
        placeholder="Mínimo R$ 100,00"
        value={valorMensal.toString()}
        onChange={(e) => onValorMensalChange(Number(e.target.value))}
        error={errors.valorMensal}
        disabled={disabled}
        required
        min={100}
        step={0.01}
      />
      
      <p className="text-xs text-slate-500 italic">
        * O valor mensal será utilizado para a compra das ações no Top Five nos dias 5, 15 e 25 de cada mês.
      </p>
    </div>
  );
}
