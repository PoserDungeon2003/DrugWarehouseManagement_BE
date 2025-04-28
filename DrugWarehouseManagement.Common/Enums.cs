namespace DrugWarehouseManagement.Common
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

    public enum CustomerStatus
    {
        Active = 1,
        Inactive = 2,
    }
 
    public enum InboundDetailStatus
    {
        Pending = 1,           // Stock-in/stock-out request is pending
        InProgress = 2,        // Stock-in/stock-out is being processed
        Completed = 3,         // Stock-in/stock-out has been completed
        Cancelled = 4          // Stozck-in/stock-out request was cancelled
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
        Completed = 4,         // Stock-in/stock-out has been completed
        Returned = 5,          // Stock-out request was returned
    }
    public enum InboundStatus
    {
        Pending = 1,           // Stock-in/stock-out request is pending
        InProgress = 2,        // Stock-in/stock-out is being processed
        Completed = 3,         // Stock-in/stock-out has been completed
        Cancelled = 4          // Stozck-in/stock-out request was cancelled
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

    public enum LotTransferStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
    }

    public enum CategoriesStatus
    {
        Active = 1,
        Inactive = 2,
    }

    public enum InboundRequestStatus
    {
        WaitingForAccountantApproval = 1,
        WaitingForDirectorApproval = 2,
        InProgress = 3,
        WaitingForSaleAdminApproval = 4,
        Completed = 6,
        Cancelled = 7,
    }

    public enum InboundReportStatus
    {
        Pending = 1,
        Completed = 2,
        Cancelled = 3,
    }

    public enum AssetStatus
    {
        Active = 1,
        Inactive = 2,
    }

    public enum DeviceStatus
    {
        Active = 1,
        Inactive = 2,
    }


    public enum SystemConfigEnum
    {
        ReportId = 100,
        MedicineId = 200,
        MedicalEquipmentId = 300,
        BeautyId = 400,
        MomToolId = 500,
        OtherId = 600,
        SKUId = 700,
    }

    public enum InventoryCheckStatus
    {
        Damaged = 1,
        Excess = 2, // Sai số nhiều hơn số lượng thực tế
        Lost = 3,
        Found = 4
    }

    public enum NotificationType
    {
        AllUsers = 1, // Thông báo cho tất cả
        ByRole = 2, // Thông báo theo role
        ByUser = 3, // Thông báo cho người dùng cụ thể
    }
}
