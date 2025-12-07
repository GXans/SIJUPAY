using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIJUPAY.Models;
using SIJUPAY.Models.ViewModels;

namespace SIJUPAY.Controllers
{
    public class ComprasController : Controller
    {
        private readonly BDSijuPayContext _context;

        public ComprasController(BDSijuPayContext context)
        {
            _context = context;
        }

        // GET: Compras/Index (Página principal/Historial)
        public async Task<IActionResult> Index()
        {
            // Obtener todas las compras que ya fueron finalizadas.
            var comprasFinalizadas = await _context.Compras
                .Include(c => c.IdProveedorNavigation) // Para mostrar el nombre del usuario/proveedor
                .Where(c => c.Observaciones != "EN_CARRITO")
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return View(comprasFinalizadas);
        }
        // GET: Compras/Crear (Muestra el formulario)
        public async Task<IActionResult> Crear()
        {
            // 1. Cargamos el catálogo de productos
            var productos = await _context.Productos
                .Include(p => p.IdCategoriaNavigation) // O cualquier otra inclusión que necesites
                .ToListAsync();

            // 2. Enviamos la lista de productos (IEnumerable<Producto>) como el MODELO
            return View(productos); // <--- SOLUCIÓN: Enviamos la lista 'productos'
        }

        // POST: Compras/RegistrarCompra (Procesa el formulario)
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> RegistrarCompra(CompraViewModel model)
        {
            model.Detalles = model.Detalles.Where(d => d.Cantidad > 0 && d.IdProducto > 0 && d.PrecioCompra > 0).ToList();
            if (!ModelState.IsValid || !model.Detalles.Any())
            {
                
                return BadRequest("La compra no tiene detalles válidos o todos los campos están vacíos.");
            }

            
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    
                    var compra = new Compra
                    {
                        Fecha = DateTime.Now,
                        Total = model.Detalles.Sum(d => d.Cantidad * d.PrecioCompra),
                        IdProveedor = 3
                    };
                    _context.Compras.Add(compra);
                    await _context.SaveChangesAsync();

                    var idNuevaCompra = compra.IdCompra;

                    
                    foreach (var item in model.Detalles)
                    {
                        
                        var detalle = new CompraDetalle
                        {
                            IdCompra = idNuevaCompra,
                            IdProducto = item.IdProducto,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = item.PrecioCompra
                        };
                        _context.CompraDetalles.Add(detalle);

                        
                        var movimiento = new InventarioMovimiento
                        {
                            IdProducto = item.IdProducto,
                            Cantidad = item.Cantidad,
                            TipoMovimiento = "Entrada", 
                            Fecha = DateTime.Now,
                            IdUsuario = 3,
                            Referencia = $"Compra ID {idNuevaCompra}",
                            Observaciones = $"Entrada por compra. {item.Cantidad} unidades."
                        };
                        _context.InventarioMovimientos.Add(movimiento);

                        
                        var producto = await _context.Productos.FindAsync(item.IdProducto);
                        if (producto == null) throw new Exception($"Producto con ID {item.IdProducto} no encontrado.");

                        producto.Stock += item.Cantidad;
                        _context.Productos.Update(producto);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = $"Compra ID {idNuevaCompra} y stock actualizado correctamente." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { success = false, message = $"Error en la transacción: {ex.Message}" });
                }
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarAlCarrito(int idProducto, int cantidad = 1)
        {
            
            int idUsuarioActual = 3;

            var compraPendiente = await _context.Compras
                .FirstOrDefaultAsync(c => c.IdProveedor == idUsuarioActual && c.Observaciones == "EN_CARRITO");

            
            if (compraPendiente == null)
            {
                compraPendiente = new Compra
                {
                    IdProveedor = idUsuarioActual,
                    Fecha = DateTime.Now,
                    Total = 0,
                    MetodoPago = "Pendiente",
                    Observaciones = "EN_CARRITO" 
                };
                _context.Compras.Add(compraPendiente);
                await _context.SaveChangesAsync();
            }

            
            var detalleExistente = await _context.CompraDetalles
                .FirstOrDefaultAsync(cd => cd.IdCompra == compraPendiente.IdCompra && cd.IdProducto == idProducto);

            if (detalleExistente != null)
            {
                detalleExistente.Cantidad += cantidad;
                _context.CompraDetalles.Update(detalleExistente);
            }
            else
            {
                var producto = await _context.Productos.FindAsync(idProducto);
                var nuevoDetalle = new CompraDetalle
                {
                    IdCompra = compraPendiente.IdCompra,
                    IdProducto = idProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.Precio 
                };
                _context.CompraDetalles.Add(nuevoDetalle);
            }

            await _context.SaveChangesAsync();

            
            return RedirectToAction(nameof(Crear));
        }

        public async Task<IActionResult> PagarCarrito()
        {
            int idUsuarioActual = 3;

            
            var compraPendiente = await _context.Compras
                .Include(c => c.CompraDetalles)
                .ThenInclude(cd => cd.IdProductoNavigation)
                .FirstOrDefaultAsync(c => c.IdProveedor == idUsuarioActual && c.Observaciones == "EN_CARRITO");

            if (compraPendiente == null)
            {
                
                return View(new CompraViewModel());
            }

           
            var modelo = new CompraViewModel
            {
                
                IdProveedor = compraPendiente.IdProveedor, 
                Detalles = compraPendiente.CompraDetalles.Select(d => new CompraDetalleViewModel
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioCompra = d.PrecioUnitario,
                    NombreProducto = d.IdProductoNavigation.Nombre 
                }).ToList()
            };

            
            ViewBag.IdCompraPendiente = compraPendiente.IdCompra;

            return View(modelo);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarCompra(int idCompra, CompraViewModel model, string metodoDePago)
        {

            var compra = await _context.Compras
        .Include(c => c.CompraDetalles)
        .FirstOrDefaultAsync(c => c.IdCompra == idCompra);

            if (compra == null || compra.Observaciones != "EN_CARRITO")
            {
                return NotFound();
            }

            int idUsuarioResponsable = compra.IdProveedor;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    decimal totalCalculado = 0;

                    

                    foreach (var detalle in compra.CompraDetalles)
                    {
                        
                        var producto = await _context.Productos.FindAsync(detalle.IdProducto);
                        producto.Stock += detalle.Cantidad;
                        producto.Estado = true;
                        _context.Productos.Update(producto);

                       
                        var movimiento = new InventarioMovimiento
                        {
                            IdProducto = detalle.IdProducto,
                            Cantidad = detalle.Cantidad,
                            TipoMovimiento = "Entrada",
                            Fecha = DateTime.Now,
                            IdUsuario = 3,
                            Referencia = $"Compra ID {compra.IdCompra}",
                            Observaciones = "Compra Finalizada"
                        };
                        _context.InventarioMovimientos.Add(movimiento);

                        totalCalculado += detalle.Cantidad * detalle.PrecioUnitario;
                    }

                    
                    compra.Fecha = DateTime.Now;
                    compra.Total = totalCalculado;
                    compra.Observaciones = "FINALIZADA";

                    compra.MetodoPago = metodoDePago;

                    _context.Compras.Update(compra);

                    await _context.SaveChangesAsync();

                    
                    await transaction.CommitAsync();
                    
                    return RedirectToAction("DetalleFactura", new { id = compra.IdCompra });
                }
                catch (Exception ex)
                {
                    // Si la transacción ya se completó (commit fue exitoso), el Rollback lanzará InvalidOperationException.
                    // Es seguro atrapar el error de Rollback aquí, pero es mejor solo intentar el Rollback.
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (InvalidOperationException)
                    {
                        // Ignoramos la excepción si la transacción ya se completó.
                    }

                    // Aquí puedes registrar 'ex'
                    throw; // Lanza la excepción original para el Developer Page
                }

            }
        }

        // GET: Compras/DetalleFactura/{id}
        public async Task<IActionResult> DetalleFactura(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            // Buscamos la compra con todos sus detalles y relaciones necesarias (Producto y Usuario/Proveedor)
            var compra = await _context.Compras
                .Include(c => c.CompraDetalles)
                    .ThenInclude(cd => cd.IdProductoNavigation) // Para obtener el nombre y precio del producto
                .Include(c => c.IdProveedorNavigation) // Asumimos que IdProveedorNavigation es el Usuario
                .FirstOrDefaultAsync(m => m.IdCompra == id);

            if (compra == null)
            {
                return NotFound();
            }

            // Aseguramos que la factura se pueda ver solo si está FINALIZADA (opcional)
            if (compra.Observaciones == "EN_CARRITO")
            {
                return BadRequest("Esta compra aún no ha sido finalizada.");
            }

            // Usaremos el objeto Compra directamente como modelo para la vista.
            return View(compra);
        }
    }
}
