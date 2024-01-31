namespace WomenBeautyBoutique.Model
{
    public class TransactionItem
    {
        public string Id { get; set; }
        public int Status { get; set; }
        public int TotalAmount { get; set; }
        public int Discount { get; set; }

        public List<InventoryItem> productItems { get; set; }
    }
}
