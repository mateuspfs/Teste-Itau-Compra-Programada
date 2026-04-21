/**
 * Função auxiliar para buscar erro de campo em erros de validação do ASP.NET Core (case-insensitive)
 * @param validationError - Objeto de erro de validação retornado pela API
 * @param fieldName - Nome do campo para buscar o erro
 * @returns Mensagem de erro do campo ou undefined se não encontrado
 */
export const getFieldError = (validationError: any, fieldName: string): string | undefined => {
  const keys = Object.keys(validationError.errors);
  const key = keys.find(k => k.toLowerCase() === fieldName.toLowerCase());
  if (key) {
    const errors = validationError.errors[key];
    return errors && errors.length > 0 ? errors[0] : undefined;
  }
  return undefined;
};

