// SIJUPAY.Models.ViewModels/VentaCatalogoItemViewModel.cs

namespace SIJUPAY.Models.ViewModels
{
    public class VentaCatalogoItemViewModel
    {
        // Propiedades del Producto (Vienen de la base de datos)
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal Precio { get; set; }
        public int? Stock { get; set; }
        public string? ImagenUrl { get; set; }

        // Propiedad que el usuario llena en la vista (el input de cantidad)
        public int CantidadAComprar { get; set; }
    }
}