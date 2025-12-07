using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIJUPAY.Models;
using SIJUPAY.Models.ViewModels;


namespace SIJUPAY.Controllers
{
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
            
            var ventasFinalizadas = await _context.Venta 
                .Include(v => v.IdClienteNavigation)    
                .Where(v => v.Observaciones == "FINALIZADA")
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View(ventasFinalizadas);
        }

        
        public async Task<IActionResult> Crear()
        {
            
            var productos = await _context.Productos
                .Where(p => p.Stock > 0 && p.Estado == true)
                .ToListAsync();

            
            var modelo = new VentaViewModel
            {
                IdCliente = 3, 
                ItemsCatalogo = productos.Select(p => new VentaCatalogoItemViewModel
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.Nombre,
                    Precio = p.Precio, 
                    Stock = p.Stock,
                    ImagenUrl = p.Foto != null
                        ? Convert.ToBase64String(p.Foto) 
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
            
            var detallesValidos = model.ItemsCatalogo
                .Where(d => d.CantidadAComprar > 0 && d.CantidadAComprar <= d.Stock) 
                .ToList();

            if (!ModelState.IsValid || !detallesValidos.Any())
            {
                
                TempData["Error"] = "Debe seleccionar al menos un producto con una cantidad válida que no exceda el stock.";
                return RedirectToAction(nameof(Crear));
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    
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

                    
                    foreach (var item in detallesValidos)
                    {
                        
                        var detalle = new VentaDetalle
                        {
                            IdVenta = idNuevaVenta,
                            IdProducto = item.IdProducto,
                            Cantidad = item.CantidadAComprar,
                            PrecioUnitario = item.Precio
                        };
                        _context.VentaDetalles.Add(detalle);

                        
                        var productoEnDB = await _context.Productos.FindAsync(item.IdProducto);
                        productoEnDB.Stock -= item.CantidadAComprar;
                        _context.Productos.Update(productoEnDB);

                        
                        var movimiento = new InventarioMovimiento
                        {
                            IdProducto = item.IdProducto,
                            Cantidad = item.CantidadAComprar,
                            TipoMovimiento = "Salida", 
                            Fecha = DateTime.Now,
                            IdUsuario = venta.IdCliente, 
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

            
            decimal totalCalculado = venta.VentaDetalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            ViewBag.TotalVenta = totalCalculado;
            ViewBag.IdVentaPendiente = idVenta;

            return View(venta);
        }

        
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

        [HttpGet]
        public async Task<IActionResult> Reporte()
        {
            var model = new ReporteVentasViewModel
            {
                ListaCategorias = await _context.Categoria.Where(c => c.Estado == true).ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Reporte(ReporteVentasViewModel model)
        {
            
            model.ListaCategorias = await _context.Categoria.Where(c => c.Estado == true).ToListAsync();

            
            DateTime fechaInicio = model.FechaInicio ?? DateTime.MinValue;
            DateTime fechaFin = model.FechaFin?.AddDays(1) ?? DateTime.MaxValue;

            
            var query = _context.Venta
                .Include(v => v.IdClienteNavigation)
                .Include(v => v.VentaDetalles)
                    .ThenInclude(vd => vd.IdProductoNavigation) 
                .Where(v => v.Fecha >= fechaInicio &&
                            v.Fecha < fechaFin &&
                            v.Observaciones == "FINALIZADA")
                .AsQueryable();

            
            if (model.IdCategoria.HasValue && model.IdCategoria.Value > 0)
            {
                
                query = query.Where(v => v.VentaDetalles.Any(vd =>
                    vd.IdProductoNavigation.IdCategoria == model.IdCategoria.Value));
            }

            
            var ventas = await query.OrderByDescending(v => v.Fecha).ToListAsync();

            var dataGrafico = ventas
            .SelectMany(v => v.VentaDetalles) 
            .GroupBy(vd => vd.IdProductoNavigation.IdCategoriaNavigation.Nombre) 
            .Select(g => new GraficoDataViewModel
            {
                Etiqueta = g.Key, 
                Valor = g.Sum(vd => vd.Cantidad * vd.PrecioUnitario) 
            })
            .OrderByDescending(d => d.Valor)
            .ToList();

            model.ResultadosVentas = ventas;
            model.TotalVentas = ventas.Sum(v => v.Total);

            ViewBag.DatosGraficoJson = Newtonsoft.Json.JsonConvert.SerializeObject(dataGrafico);
            return View(model);
        }
    }
}