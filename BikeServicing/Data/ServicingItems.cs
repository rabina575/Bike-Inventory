namespace BikeServicing.Data
// Model for servicing item details.
{
    public class ServicingItems
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int Quantity { get; set; }
        public float UnitPrice { get; set; }
        public Guid AddedBy { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
