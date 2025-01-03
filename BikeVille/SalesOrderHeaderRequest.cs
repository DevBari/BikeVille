namespace BikeVille
{
    using BikeVille.Entity;
    public class SalesOrderHeaderRequest
    {
        public int CustomerId { get; set; }
        public string shipMethod { get; set; }
        public string SalesOrderNumber { get; set; }
        public byte RevisionNumber { get; set; }
        public byte Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public bool OnlineOrderFlag { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal Freight { get; set; }
        public decimal TotalDue { get; set; }
        public int? BillToAddressId { get; set; }
        public int? ShipToAddressId { get; set; }
        public string? CreditCardApprovalCode { get; set; }
        public string Comment { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public string? AccountNumber { get; set; }
        public List<SalesOrderDetailRequest> SalesOrderDetails { get; set; }
    }

    public class SalesOrderDetailRequest
    {
        public int ProductId { get; set; }
        public int OrderQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceDiscount { get; set; }
        public decimal LineTotal { get; set; }
    }

}
