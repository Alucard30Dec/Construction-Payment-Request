namespace ConstructionPayment.Domain.Enums;

public enum PaymentRequestStatus
{
    Draft = 1,
    Submitted = 2,
    PendingDepartmentApproval = 3,
    PendingDirectorApproval = 4,
    PendingAccounting = 5,
    Rejected = 6,
    ReturnedForEdit = 7,
    Paid = 8,
    Cancelled = 9
}
