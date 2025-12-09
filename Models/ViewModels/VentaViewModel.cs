// SIJUPAY.Models.ViewModels/VentaViewModel.cs


namespace SIJUPAY.Models.ViewModels
{
    public class VentaViewModel
    {
        
        public int IdCliente { get; set; } = 0 ;
        public string MetodoPago { get; set; } = null;

        
        public List<VentaCatalogoItemViewModel> ItemsCatalogo { get; set; } = new List<VentaCatalogoItemViewModel>();
    }
}