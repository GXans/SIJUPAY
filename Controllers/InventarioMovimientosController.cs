using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIJUPAY.Models;

namespace SIJUPAY.Controllers
{
    public class InventarioMovimientosController : Controller
    {
        private readonly BDSijuPayContext _context;

        public InventarioMovimientosController(BDSijuPayContext context)
        {
            _context = context;
        }

        // GET: InventarioMovimientos
        public async Task<IActionResult> Index()
        {
            var bDSijuPayContext = _context.InventarioMovimientos.Include(i => i.IdProductoNavigation).Include(i => i.IdUsuarioNavigation);
            return View(await bDSijuPayContext.ToListAsync());
        }

        // GET: InventarioMovimientos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventarioMovimiento = await _context.InventarioMovimientos
                .Include(i => i.IdProductoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
            if (inventarioMovimiento == null)
            {
                return NotFound();
            }

            return View(inventarioMovimiento);
        }

        // GET: InventarioMovimientos/Create
        public IActionResult Create()
        {
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto");
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario");
            return View();
        }

        // POST: InventarioMovimientos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMovimiento,IdProducto,Cantidad,Fecha,IdUsuario,TipoMovimiento")] InventarioMovimiento inventarioMovimiento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inventarioMovimiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", inventarioMovimiento.IdProducto);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", inventarioMovimiento.IdUsuario);
            return View(inventarioMovimiento);
        }

        // GET: InventarioMovimientos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventarioMovimiento = await _context.InventarioMovimientos.FindAsync(id);
            if (inventarioMovimiento == null)
            {
                return NotFound();
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", inventarioMovimiento.IdProducto);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", inventarioMovimiento.IdUsuario);
            return View(inventarioMovimiento);
        }

        // POST: InventarioMovimientos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdMovimiento,IdProducto,Cantidad,Fecha,IdUsuario,TipoMovimiento")] InventarioMovimiento inventarioMovimiento)
        {
            if (id != inventarioMovimiento.IdMovimiento)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventarioMovimiento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventarioMovimientoExists(inventarioMovimiento.IdMovimiento))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", inventarioMovimiento.IdProducto);
            ViewData["IdUsuario"] = new SelectList(_context.Usuarios, "IdUsuario", "IdUsuario", inventarioMovimiento.IdUsuario);
            return View(inventarioMovimiento);
        }

        // GET: InventarioMovimientos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventarioMovimiento = await _context.InventarioMovimientos
                .Include(i => i.IdProductoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
            if (inventarioMovimiento == null)
            {
                return NotFound();
            }

            return View(inventarioMovimiento);
        }

        // POST: InventarioMovimientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventarioMovimiento = await _context.InventarioMovimientos.FindAsync(id);
            if (inventarioMovimiento != null)
            {
                _context.InventarioMovimientos.Remove(inventarioMovimiento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InventarioMovimientoExists(int id)
        {
            return _context.InventarioMovimientos.Any(e => e.IdMovimiento == id);
        }
    }
}
