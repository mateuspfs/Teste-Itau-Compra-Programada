import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { clientesApi } from '../../services/api';
import { Container, Box, PageHeader, Button, Loading } from '../../components';
import ClienteForm from './ClienteForm';
import { swal } from '../../utils/swal';
import { getFieldError } from '../../helpers/validation';

// Página de adesão ao produto
export default function ClientesAdesao() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    nome: '',
    cpf: '',
    email: '',
    valorMensal: 100,
  });
  const [errors, setErrors] = useState<{ 
    nome?: string; 
    cpf?: string; 
    email?: string; 
    valorMensal?: string 
  }>({});

  const handleFieldChange = (field: string, value: any) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (errors[field as keyof typeof errors]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});
    setLoading(true);

    try {
      const result = await clientesApi.aderir({
        nome: formData.nome.trim(),
        cpf: formData.cpf.trim(),
        email: formData.email.trim(),
        valorMensal: formData.valorMensal,
      });

      if (result.success) {
        swal.successToast('Adesão realizada com sucesso!');
        navigate('/clientes');
      } else {
        swal.errorToast(result.errors.join(', ') || 'Erro ao realizar adesão');
      }
    } catch (err: any) {
      if (err.response?.status === 400 && err.response?.data?.errors) {
        const validationError = err.response.data;
        const fieldErrors: any = {};
        
        ['nome', 'cpf', 'email', 'valorMensal'].forEach(field => {
            const error = getFieldError(validationError, field);
            if (error) fieldErrors[field] = error;
        });

        setErrors(fieldErrors);
      } else {
        swal.errorToast('Erro ao conectar com a API');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container>
      <PageHeader
        title="Adesão ao Produto"
        subtitle="Cadastre o cliente no sistema de Compra Programada Itaú"
      />

      <Box className="max-w-2xl border-0 shadow-lg">
        {loading && (
          <div className="absolute inset-0 flex items-center justify-center bg-white/50 backdrop-blur-sm dark:bg-slate-900/50 z-10">
            <Loading message="Processando adesão..." />
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <ClienteForm
            nome={formData.nome}
            cpf={formData.cpf}
            email={formData.email}
            valorMensal={formData.valorMensal}
            errors={errors}
            onNomeChange={(val) => handleFieldChange('nome', val)}
            onCpfChange={(val) => handleFieldChange('cpf', val)}
            onEmailChange={(val) => handleFieldChange('email', val)}
            onValorMensalChange={(val) => handleFieldChange('valorMensal', val)}
            disabled={loading}
          />

          <div className="flex gap-4 pt-4">
            <Button
              type="submit"
              variant="primary"
              disabled={loading}
              className="flex-1 bg-[#EC7000] hover:bg-orange-700 border-none"
            >
              Confirmar Adesão
            </Button>
            <Button
              type="button"
              variant="outline"
              onClick={() => navigate('/clientes')}
              disabled={loading}
              className="flex-1"
            >
              Voltar
            </Button>
          </div>
        </form>
      </Box>
    </Container>
  );
}
