import Swal from 'sweetalert2';

// Helper para alertas e confirmações usando SweetAlert2
export const swal = {
  // Alerta de sucesso
  success: (message: string, title = 'Sucesso!') => {
    return Swal.fire({
      icon: 'success',
      title,
      text: message,
      confirmButtonColor: '#2f5cf5',
      confirmButtonText: 'OK',
    });
  },

  // Alerta de erro
  error: (message: string, title = 'Erro!') => {
    return Swal.fire({
      icon: 'error',
      title,
      text: message,
      confirmButtonColor: '#dc2626',
      confirmButtonText: 'OK',
    });
  },

  // Alerta de informação
  info: (message: string, title = 'Informação') => {
    return Swal.fire({
      icon: 'info',
      title,
      text: message,
      confirmButtonColor: '#2f5cf5',
      confirmButtonText: 'OK',
    });
  },

  // Alerta de aviso
  warning: (message: string, title = 'Atenção!') => {
    return Swal.fire({
      icon: 'warning',
      title,
      text: message,
      confirmButtonColor: '#f59e0b',
      confirmButtonText: 'OK',
    });
  },

  // Confirmação
  confirm: (message: string, title = 'Confirmar ação', confirmText = 'Confirmar', cancelText = 'Cancelar') => {
    return Swal.fire({
      icon: 'question',
      title,
      text: message,
      showCancelButton: true,
      confirmButtonColor: '#dc2626',
      cancelButtonColor: '#6b7280',
      confirmButtonText: confirmText,
      cancelButtonText: cancelText,
    });
  },

  // Toast minimal para erros (canto superior direito, desaparece automaticamente)
  errorToast: (message: string, duration = 4000) => {
    return Swal.fire({
      icon: 'error',
      title: message,
      toast: true,
      position: 'top-end',
      showConfirmButton: false,
      timer: duration,
      timerProgressBar: true,
      didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer);
        toast.addEventListener('mouseleave', Swal.resumeTimer);
      },
    });
  },

  // Toast minimal para sucesso (canto superior direito, desaparece automaticamente)
  successToast: (message: string, duration = 4000) => {
    return Swal.fire({
      icon: 'success',
      title: message,
      toast: true,
      position: 'top-end',
      showConfirmButton: false,
      timer: duration,
      timerProgressBar: true,
      didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer);
        toast.addEventListener('mouseleave', Swal.resumeTimer);
      },
    });
  },
};

