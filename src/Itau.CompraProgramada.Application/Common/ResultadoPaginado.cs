using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Itau.CompraProgramada.Application.Common
{
    public class ResultadoPaginado<T>
    {
        [JsonPropertyName("itens")]
        public IEnumerable<T> Itens { get; set; } = new List<T>();

        [JsonPropertyName("totalRegistros")]
        public int TotalRegistros { get; set; }

        [JsonPropertyName("pagina")]
        public int Pagina { get; set; }

        [JsonPropertyName("tamanhoPagina")]
        public int TamanhoPagina { get; set; }

        [JsonPropertyName("totalPaginas")]
        public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamanhoPagina);

        public ResultadoPaginado() { }

        public ResultadoPaginado(IEnumerable<T> itens, int totalRegistros, int pagina, int tamanhoPagina)
        {
            Itens = itens;
            TotalRegistros = totalRegistros;
            Pagina = pagina;
            TamanhoPagina = tamanhoPagina;
        }
    }
}
