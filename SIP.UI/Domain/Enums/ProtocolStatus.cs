namespace SIP.UI.Domain.Enums;
public enum ProtocolStatus
{
    Open = 0,
    SentForReview = 1,
    Received = 2,
    UnderReview = 3,
    Approved = 4,
    Rejected = 5,
    CorrectionRequested = 6,
    Finalized = 7,
}