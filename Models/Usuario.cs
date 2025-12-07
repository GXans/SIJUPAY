using System;
using System.Collections.Generic;

namespace SIJUPAY.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string Contrasena { get; set; } = null!;

    public string? Telefono { get; set; }

    public string TipoUsuario { get; set; } = null!;

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
