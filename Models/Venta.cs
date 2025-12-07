using System;
using System.Collections.Generic;

namespace SIJUPAY.Models;

public partial class Venta
{
    public int IdVenta { get; set; }

    public int IdCliente { get; set; }

    public DateTime? Fecha { get; set; }

    public decimal Total { get; set; }

    public string? MetodoPago { get; set; }
    public string? Observaciones { get; set; }

    public virtual Usuario IdClienteNavigation { get; set; } = null!;

    public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
}
