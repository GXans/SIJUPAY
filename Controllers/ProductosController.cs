using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIJUPAY.Models;
using System;
using System.Collections.Generic;
using System.IO; 
using System.Linq;
using System.Threading.Tasks;

namespace SIJUPAY.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly BDSijuPayContext _context;

        public ProductosController(BDSijuPayContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index(string search, int? categoria)
        {
            var query = _context.Productos.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Nombre.Contains(search));

            if (categoria.HasValue)
                query = query.Where(p => p.IdCategoria == categoria.Value);

            ViewBag.Categorias = new SelectList(_context.Categoria, "IdCategoria", "Nombre");
            ViewBag.Search = search;
            ViewBag.Categoria = categoria;

            return View(await query.ToListAsync());
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "Nombre");
            return View();
        }

        // POST: Productos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProducto,Nombre,Precio,Descripcion,IdCategoria,Stock")] Producto producto, IFormFile fotoCargada)
        {
            if (ModelState.IsValid)
            {
               
                if (fotoCargada != null && fotoCargada.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await fotoCargada.CopyToAsync(memoryStream);
                        producto.Foto = memoryStream.ToArray(); 
                    }
                }

               
                producto.Estado = (producto.Stock > 0);

               
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "Nombre", producto.IdCategoria);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "Nombre", producto.IdCategoria);
            return View(producto);
        }

        // POST: Productos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto, IFormFile? fotoCargada)
        {
            if (id != producto.IdProducto)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    
                    var productoDb = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);
                    if (productoDb == null)
                        return NotFound();

                    
                    productoDb.Nombre = producto.Nombre;
                    productoDb.Precio = producto.Precio;
                    productoDb.Descripcion = producto.Descripcion;
                    productoDb.Stock = producto.Stock;
                    productoDb.IdCategoria = producto.IdCategoria;
                    productoDb.Estado = producto.Stock > 0;
                    productoDb.Estado = producto.Estado;

                    
                    if (fotoCargada != null && fotoCargada.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await fotoCargada.CopyToAsync(memoryStream);
                            productoDb.Foto = memoryStream.ToArray();
                        }
                    }

                    
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    throw;
                }
            }

            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "Nombre", producto.IdCategoria);
            return View(producto);
        }


        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.IdProducto == id);
        }
    }
}
