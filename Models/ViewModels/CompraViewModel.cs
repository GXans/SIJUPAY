using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SIJUPAY.Models.ViewModels
{
    // Modelo principal para el formulario de Compra (Encabezado)
    public class CompraViewModel
    {
        public CompraViewModel()
        {
            Detalles = new List<CompraDetalleViewModel>();
        }

        // Se usa para identificar al proveedor (o usuario logueado, según tu lógica)
        [Display(Name = "Proveedor / Usuario")]
        public int IdProveedor { get; set; }

        [Display(Name = "Método de Pago")]
        public string? MetodoPago { get; set; }

        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        public string? Observaciones { get; set; }

        // Total estimado de la compra (calculado por la vista o controlador)
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }

        // Lista de productos seleccionados
        public List<CompraDetalleViewModel> Detalles { get; set; }
    }

    // Modelo para cada línea de producto en la compra
    public class CompraDetalleViewModel
    {
        public int IdProducto { get; set; }

        // Solo para mostrar el nombre en la vista (no se guarda en BD)
        public string? NombreProducto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioCompra { get; set; }

        // Propiedad calculada útil para mostrar en la tabla (no se envía al controlador)
        public decimal Subtotal => Cantidad * PrecioCompra;
    }
}
