using MediatR;
using TRRCMS.Application.Documents.Dtos;

namespace TRRCMS.Application.Documents.Queries.GetAllDocuments;

/// <summary>
/// Query to get all documents
/// </summary>
public class GetAllDocumentsQuery : IRequest<IEnumerable<DocumentDto>>
{
}
