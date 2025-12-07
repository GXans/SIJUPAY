using System;
using System.Collections.Generic;

namespace SIJUPAY.Models;

public partial class InventarioMovimiento
{
    public int IdMovimiento { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public DateTime? Fecha { get; set; }

    public int IdUsuario { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
