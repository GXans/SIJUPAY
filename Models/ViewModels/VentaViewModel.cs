// SIJUPAY.Models.ViewModels/VentaViewModel.cs

namespace SIJUPAY.Models.ViewModels
{
    public class VentaViewModel
    {
        // Información del encabezado de la venta
        public int IdCliente { get; set; } = 3;
        public string MetodoPago { get; set; } = "Efectivo";

        // Lista de todos los productos del catálogo que se mostrarán en la vista
        public List<VentaCatalogoItemViewModel> ItemsCatalogo { get; set; } = new List<VentaCatalogoItemViewModel>();
    }
}