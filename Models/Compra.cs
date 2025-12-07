using System;
using System.Collections.Generic;

namespace SIJUPAY.Models;

public partial class Compra
{
    public int IdCompra { get; set; }

    public int IdProveedor { get; set; }

    public DateTime? Fecha { get; set; }

    public decimal Total { get; set; }

    public string? MetodoPago { get; set; }

    public string? Observaciones { get; set; }

    public virtual ICollection<CompraDetalle> CompraDetalles { get; set; } = new List<CompraDetalle>();

    public virtual Usuario IdProveedorNavigation { get; set; } = null!;
}
