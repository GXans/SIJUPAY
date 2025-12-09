using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIJUPAY.Models;
using SIJUPAY.Models.ViewModels;


namespace SIJUPAY.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly BDSijuPayContext _context;

        public VentasController(BDSijuPayContext context)
        {
            _context = context;
        }
        // GET: Ventas/Index (Página principal/Historial)
        public async Task<IActionResult> Index()
        {
            // Obtener todas las ventas que ya fueron FINALIZADAS.
            var ventasFinalizadas = await _context.Venta // Asegúrate que tu modelo se llama 'Venta'
                .Include(v => v.IdClienteNavigation)    // Incluye la información del Cliente
                .Where(v => v.Observaciones == "FINALIZADA") // Filtra solo las ventas completadas
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View(ventasFinalizadas);
        }

        // --- ACCIÓN INDEX (Historial) ---
        public async Task<IActionResult> Crear()
        {
            // 1. Obtener solo productos con stock y activos
            var productos = await _context.Productos
                .Where(p => p.Stock > 0 && p.Estado == true)
                .ToListAsync();

            // 2. Mapear los productos al VentaViewModel
            var modelo = new VentaViewModel
            {
                IdCliente = 3, // Asigna un cliente por defecto
                ItemsCatalogo = productos.Select(p => new VentaCatalogoItemViewModel
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.Nombre,
                    Precio = p.Precio, 
                    Stock = p.Stock,
                    ImagenUrl = p.Foto != null
                        ? Convert.ToBase64String(p.Foto) // Convierte byte[] a string Base64
                        : null,
                    CantidadAComprar = 0 
                }).ToList()
            };

            return View(modelo);
        }
        // POST: Ventas/RegistrarVenta 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarVenta(VentaViewModel model)
        {
            // 1. Filtrar solo los ítems donde el usuario ingresó una cantidad > 0
            var detallesValidos = model.ItemsCatalogo
                .Where(d => d.CantidadAComprar > 0 && d.CantidadAComprar <= d.Stock) // Validar stock
                .ToList();

            if (!ModelState.IsValid || !detallesValidos.Any())
            {
                // Si no hay ítems válidos, puedes redirigir con un mensaje de error
                TempData["Error"] = "Debe seleccionar al menos un producto con una cantidad válida que no exceda el stock.";
                return RedirectToAction(nameof(Crear));
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 2. Crear la Venta principal
                    var venta = new Venta
                    {
                        Fecha = DateTime.Now,
                        Total = detallesValidos.Sum(d => d.CantidadAComprar * d.Precio),
                        IdCliente = model.IdCliente,
                        Observaciones = "FINALIZADA",
                        MetodoPago = model.MetodoPago
                    };
                    _context.Venta.Add(venta);
                    await _context.SaveChangesAsync();

                    var idNuevaVenta = venta.IdVenta;

                    // 3. Procesar detalles, stock y movimientos
                    foreach (var item in detallesValidos)
                    {
                        // Crear Detalle de Venta
                        var detalle = new VentaDetalle
                        {
                            IdVenta = idNuevaVenta,
                            IdProducto = item.IdProducto,
                            Cantidad = item.CantidadAComprar,
                            PrecioUnitario = item.Precio
                        };
                        _context.VentaDetalles.Add(detalle);

                        // Descontar Stock (Salida)
                        var productoEnDB = await _context.Productos.FindAsync(item.IdProducto);
                        productoEnDB.Stock -= item.CantidadAComprar;
                        _context.Productos.Update(productoEnDB);

                        // Crear Movimiento de Inventario (Salida)
                        var movimiento = new InventarioMovimiento
                        {
                            IdProducto = item.IdProducto,
                            Cantidad = item.CantidadAComprar,
                            TipoMovimiento = "Salida", // ¡Importante! En venta es Salida
                            Fecha = DateTime.Now,
                            IdUsuario = venta.IdCliente, // o el usuario que registra la venta
                            Referencia = $"Venta ID {idNuevaVenta}",
                            Observaciones = "Venta Directa"
                        };
                        _context.InventarioMovimientos.Add(movimiento);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"Venta #{idNuevaVenta} registrada con éxito.";
                    return RedirectToAction("DetalleFactura", new { id = idNuevaVenta });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Error al registrar la venta: {ex.Message}";
                    return RedirectToAction(nameof(Crear));
                }
            }
        }


        // --- ACCIÓN AGREGAR AL CARRITO ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarAlCarritoVenta(int idProducto, int cantidad, int idCliente)
        {
            var producto = await _context.Productos.FindAsync(idProducto);
            if (producto == null || cantidad <= 0 || cantidad > producto.Stock)
            {
                TempData["Error"] = "Cantidad inválida o stock insuficiente.";
                return RedirectToAction(nameof(Crear));
            }

            var venta = await _context.Venta
                .Include(v => v.VentaDetalles)
                .FirstOrDefaultAsync(v => v.IdCliente == idCliente && v.Observaciones == "EN_CARRITO");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (venta == null)
                    {
                        venta = new Venta
                        {
                            IdCliente = idCliente,
                            Observaciones = "EN_CARRITO",
                            Fecha = DateTime.Now,
                            Total = 0
                        };
                        _context.Venta.Add(venta);
                        await _context.SaveChangesAsync();
                    }

                    var detalleExistente = venta.VentaDetalles
                        .FirstOrDefault(d => d.IdProducto == idProducto);

                    if (detalleExistente != null)
                    {
                        detalleExistente.Cantidad += cantidad;
                        _context.VentaDetalles.Update(detalleExistente);
                    }
                    else
                    {
                        var detalle = new VentaDetalle
                        {
                            IdVenta = venta.IdVenta,
                            IdProducto = idProducto,
                            Cantidad = cantidad,
                            // USAMOS PrecioVenta si existe, o Precio si usaste ese campo para ventas
                            PrecioUnitario = producto.Precio
                        };
                        _context.VentaDetalles.Add(detalle);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    TempData["Success"] = "Producto agregado al carrito de venta.";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Error al agregar producto: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Crear));
        }

        // --- ACCIÓN PAGAR CARRITO ---
        public async Task<IActionResult> PagarCarrito(int idVenta)
        {
            var venta = await _context.Venta
                .Include(v => v.IdClienteNavigation)
                .Include(v => v.VentaDetalles)
                .ThenInclude(vd => vd.IdProductoNavigation)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta && v.Observaciones == "EN_CARRITO");

            if (venta == null || !venta.VentaDetalles.Any())
            {
                TempData["Error"] = "El carrito de venta está vacío o no se encontró.";
                return RedirectToAction(nameof(Crear));
            }

            // Calculo seguro del total, usando GetValueOrDefault() para PriceUnitario
            decimal totalCalculado = venta.VentaDetalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            ViewBag.TotalVenta = totalCalculado;
            ViewBag.IdVentaPendiente = idVenta;

            return View(venta);
        }

        // --- ACCIÓN FINALIZAR VENTA (Salida de Stock) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarVenta(int idVenta, string metodoDePago)
        {
            var venta = await _context.Venta
                .Include(v => v.VentaDetalles)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta && v.Observaciones == "EN_CARRITO");

            if (venta == null || !venta.VentaDetalles.Any())
            {
                TempData["Error"] = "Venta no encontrada o carrito vacío.";
                return RedirectToAction(nameof(Crear));
            }
            int idUsuarioResponsable = venta.IdCliente;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    decimal totalCalculado = 0;

                    foreach (var detalle in venta.VentaDetalles)
                    {
                        var producto = await _context.Productos.FindAsync(detalle.IdProducto);

                        // LÓGICA CRÍTICA: DISMINUIR EL STOCK (SALIDA)
                        if (producto.Stock < detalle.Cantidad)
                        {
                            throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre}. Solo quedan {producto.Stock}.");
                        }

                        producto.Stock -= detalle.Cantidad;
                        _context.Productos.Update(producto);

                        
                        var movimiento = new InventarioMovimiento
                        {
                            IdProducto = detalle.IdProducto,
                            Cantidad = detalle.Cantidad,
                            TipoMovimiento = "Salida",
                            Fecha = DateTime.Now,
                            IdUsuario = idUsuarioResponsable,
                            Referencia = $"Venta ID {venta.IdVenta}",
                            Observaciones = "Venta Finalizada"
                        };
                        _context.InventarioMovimientos.Add(movimiento);


                        totalCalculado += detalle.Cantidad * detalle.PrecioUnitario;
                    }

                    
                    venta.Fecha = DateTime.Now;
                    venta.Total = totalCalculado;
                    venta.Observaciones = "FINALIZADA";
                    venta.MetodoPago = metodoDePago;

                    _context.Venta.Update(venta);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"Venta #{venta.IdVenta} finalizada con éxito. Stock actualizado.";
                    return RedirectToAction("DetalleFactura", new { id = venta.IdVenta });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = ex.Message;
                    
                    return RedirectToAction(nameof(PagarCarrito), new { idVenta = idVenta });
                }
            }
        }

        // --- ACCIÓN DETALLE FACTURA ---
        public async Task<IActionResult> DetalleFactura(int id)
        {
            if (id == 0) return NotFound();

            var venta = await _context.Venta
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.IdProductoNavigation)
                .Include(v => v.IdClienteNavigation)
                .FirstOrDefaultAsync(m => m.IdVenta == id);

            if (venta == null) return NotFound();

            if (venta.Observaciones == "EN_CARRITO")
            {
                return BadRequest("Esta venta aún no ha sido finalizada.");
            }

            return View(venta);
        }
    }
}