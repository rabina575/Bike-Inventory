namespace BikeServicing.Data
// Model for Log Details
{
    public class InventoryRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
        public Guid TakenBy { get; set; }                        
        public DateTime DateRequested { get; set; } = DateTime.Now;
        public Guid ApprovedBy { get; set; }
        public DateTime DateApproved { get; set; }
        public RequestStatus Action { get; set; } = RequestStatus.Pending;  }
}
