namespace Itau.CompraProgramada.Domain.Entities
{
    public class ItemCesta : Entity
    {
        public long CestaId { get; private set; }
        public string Ticker { get; private set; }
        public decimal Percentual { get; private set; }

        public virtual CestaRecomendacao Cesta { get; private set; }

        protected ItemCesta() { }

        public ItemCesta(long cestaId, string ticker, decimal percentual)
        {
            CestaId = cestaId;
            Ticker = ticker;
            Percentual = percentual;
        }
    }
}
