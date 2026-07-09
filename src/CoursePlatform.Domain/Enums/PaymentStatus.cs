namespace CoursePlatform.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Expired = 3,
    Refunded = 4,
    Chargeback = 5
}
