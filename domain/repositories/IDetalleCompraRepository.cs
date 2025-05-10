
using System.Collections.Generic;
using tallerc.domain.entities;



namespace tallerc.domain.repositories
{
    public interface IDetalleCompraRepository : IGenericRepository<DetalleCompra>
    {
        List<DetalleCompra> GetByCompraId(int compraId);
        // Podrías añadir un método para agregar un rango de detalles si es necesario
        // bool AddRange(IEnumerable<DetalleCompra> detalles);
    }
}