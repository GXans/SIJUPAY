

namespace SIJUPAY.Models.ViewModels
{
    public class VentaViewModel
    {
        
        public int IdCliente { get; set; } = 3;
        public string MetodoPago { get; set; } = "Efectivo";

        
        public List<VentaCatalogoItemViewModel> ItemsCatalogo { get; set; } = new List<VentaCatalogoItemViewModel>();
    }
}