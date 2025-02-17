namespace DrugWarehouseManagement.Common.Enums
{
    public enum AccountStatus
    {
        Active = 1,
        Inactive = 2,
        Deleted = 3,
    }
    public enum ProviderStatus
    {
        Active = 1,
        Inactive = 2,
        Deleted = 3,
    }
    public enum RejectReason
    {
        Damaged = 1,
        Expired = 2,
        Missing = 3,
        Other = 4,
    }

    public enum SupplierStatus
    {
        Active = 1,           // Supplier is active and available for transactions
        Inactive = 2,         // Supplier is temporarily unavailable
        Blacklisted = 3,      // Supplier is blacklisted
        PendingApproval = 4,  // Supplier is awaiting approval
        Approved = 5,         // Supplier has been approved
        Rejected = 6,         // Supplier's application was rejected
    }
    public enum InboundDetailStatus
    {
        Pending = 1,            // Item is awaiting processing or receipt
        Received = 2,           // Item has been received in the warehouse
        Rejected = 3,           // Item was rejected after inspection
        Accepted = 4,           // Item was returned after being received
    }
    public enum WarehouseStatus
    {
        Active = 1,
        Inactive = 2,
    }

    public enum OutboundStatus
    {
        Pending = 1,           // Stock-in/stock-out request is pending
        InProgress = 2,        // Stock-in/stock-out is being processed
        Cancelled = 3,         // Stozck-in/stock-out request was cancelled    
        AcceptedFromWarehouse = 4,          // Stock-out request was accepted
        AcceptedFromCustomer = 5,          // Stock-out request was accepted
        RejectedFromWarehouse = 6,          // Stock-out request was rejected    
        RejectedFromCustomer = 7,          // Stock-out request was returned
        PartiallyAccepted = 8,
        Packed = 9,
    }
    public enum InboundStatus
    {
        Pending = 1,           // Stock-in/stock-out request is pending
        InProgress = 2,        // Stock-in/stock-out is being processed
        Completed = 3,         // Stock-in/stock-out has been completed
        Cancelled = 4,         // Stozck-in/stock-out request was cancelled
        PartiallyAccepted = 5, // Stock-in request was partially accepted
        AcceptedFromSupllier = 6,
        AcceptedFromWarehouse = 7,
        RejectedFromSupllier = 8,
        RejectedFromWarehouse = 9,

    }



    public enum ContainerLoadStatus
    {
        InStock = 1,           // Item is available in inventory
        OutOfStock = 2,        // Item is not available
        Damaged = 3,           // Item is damaged
        Expired = 4,           // Item has expired
        Missing = 5,           // Item is missing

    }

    public enum OrderStatus
    {
        Pending = 1,           // Order is pending approval or processing
        Approved = 2,          // Order has been approved
        Rejected = 3,          // Order was rejected
        Shipped = 4,           // Order has been shipped
        Delivered = 5,         // Order has been delivered
        Cancelled = 6,         // Order has been cancelled
        Returned = 7,          // Order was returned
    }

    public enum ProductStatus
    {
        Active = 1,
        Inactive = 2,

    }

    public enum TwoFactorAuthenticatorSetupStatus
    {
        NotStarted = 1,
        Pending = 2,
        Completed = 3,
    }

}
