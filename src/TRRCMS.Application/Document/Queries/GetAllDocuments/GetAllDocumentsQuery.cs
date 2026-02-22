using MediatR;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Documents.Dtos;

namespace TRRCMS.Application.Documents.Queries.GetAllDocuments;

/// <summary>
/// Query to get all documents with pagination
/// </summary>
public class GetAllDocumentsQuery : PagedQuery, IRequest<PagedResult<DocumentDto>>
{
}
