using System;
using System.Threading.Tasks;
using Itau.CompraProgramada.Application.DTOs.Admin;

namespace Itau.CompraProgramada.Application.Interfaces
{
    public interface IMotorCompraEngine
    {
        Task<MotorCompraResponse> ExecutarProcessamentoDiarioAsync(DateTime dataProcessamento);
    }
}
