using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SIJUPAY.Models.ViewModels
{
    
    public class CompraViewModel
    {
        public CompraViewModel()
        {
            Detalles = new List<CompraDetalleViewModel>();
        }

        
        [Display(Name = "Proveedor / Usuario")]
        public int IdProveedor { get; set; }

        [Display(Name = "Método de Pago")]
        public string? MetodoPago { get; set; }

        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        public string? Observaciones { get; set; }

        
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }

        
        public List<CompraDetalleViewModel> Detalles { get; set; }
    }

    
    public class CompraDetalleViewModel
    {
        public int IdProducto { get; set; }

        
        public string? NombreProducto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioCompra { get; set; }

        
        public decimal Subtotal => Cantidad * PrecioCompra;
    }
}
