namespace RemkofFrontend.ViewModels
{
    public class AdminServicePriceViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Price { get; set; }
        public int ViewPriority { get; set; }
        public bool MarkedForRemoval { get; set; } = false;
    }
}
