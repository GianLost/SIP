using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Interfaces.Default;
using SIP.API.Domain.DTOs.Sectors;
using SIP.API.Domain.DTOs.Sectors.Pagination;
using SIP.API.Domain.DTOs.Sectors.Responses;

namespace SIP.API.Domain.Interfaces.Sectors;

/// <summary>
/// Defines a specialized entity manager for sector-related operations,
/// extending the generic <see cref="IEntityManager{TEntity, TCreateDTO, TUpdateDTO, TListItemDTO, TDefaultResponse, TResponseDTO}"/>.
/// </summary>
/// <remarks>
/// This interface provides CRUD, pagination, and data retrieval operations
/// specific to <see cref="Sector"/> entities.  
/// It enables separation of concerns by isolating domain-specific logic
/// from the generic data access layer.
/// </remarks>
public interface ISector : IEntityManager<Sector, SectorCreateDTO, SectorUpdateDTO, SectorListItemDTO, SectorDefaultResponseDTO, SectorResponseDTO> { }