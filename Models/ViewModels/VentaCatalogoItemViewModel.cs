// SIJUPAY.Models.ViewModels/VentaCatalogoItemViewModel.cs


namespace SIJUPAY.Models.ViewModels
{
    public class VentaCatalogoItemViewModel
    {
       
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal Precio { get; set; }
        public int? Stock { get; set; }
        public string? ImagenUrl { get; set; }

        
        public int CantidadAComprar { get; set; }
    }
}