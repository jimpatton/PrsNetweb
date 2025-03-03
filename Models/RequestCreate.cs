namespace PrsNetWeb.Models
{
    public class RequestCreate
    {
        public int UserId { get; set; }
        
        public string Description { get; set; }
        public string Justification { get; set; }
        public DateOnly DateNeeded { get; set; }
        public string DeliveryMode { get; set; }



    }
}
