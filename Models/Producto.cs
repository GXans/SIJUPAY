using System;
using System.Collections.Generic;

namespace SIJUPAY.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public byte[]? Foto { get; set; }

    public decimal Precio { get; set; }

    public string? Descripcion { get; set; }

    public int IdCategoria { get; set; }

    public bool? Estado { get; set; }

    public int? Stock { get; set; }

    public virtual ICollection<CompraDetalle> CompraDetalles { get; set; } = new List<CompraDetalle>();

    public virtual Categoria? IdCategoriaNavigation { get; set; }

    public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();

    public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
}
