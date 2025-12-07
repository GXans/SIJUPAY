using System;
using System.Collections.Generic;
using SIJUPAY.Models;

namespace SIJUPAY.Models.ViewModels
{
    public class ReporteVentasViewModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? IdCategoria { get; set; } 

        public IEnumerable<Categoria>? ListaCategorias { get; set; }

        public List<Venta> ResultadosVentas { get; set; } = new List<Venta>();
        public decimal TotalVentas { get; set; }
    }
}