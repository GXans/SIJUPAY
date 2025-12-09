

using System;
using System.Collections.Generic;
using SIJUPAY.Models; 
using SIJUPAY.Models.ViewModels; 

namespace SIJUPAY.Models.ViewModels
{
    public class ReporteComprasViewModel
    {
        // Parámetros de entrada
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? IdProveedor { get; set; } 

        
        public IEnumerable<Usuario>? ListaProveedores { get; set; }

        
        public List<Compra> ResultadosCompras { get; set; } = new List<Compra>();

        
        public decimal TotalCompras { get; set; }
    }
}