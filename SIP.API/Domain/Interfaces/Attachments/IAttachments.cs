using SIP.API.Domain.Entities.Attachments;

namespace SIP.API.Domain.Interfaces.Attachments;

public interface IAttachments
{
    Task<Attachment> CreateAsync(Attachment attachment);
    Task<Attachment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Attachment>> GetAllAsync();
    Task<Attachment?> UpdateAsync(Attachment attachment);
    Task<bool> DeleteAsync(Guid id);
}