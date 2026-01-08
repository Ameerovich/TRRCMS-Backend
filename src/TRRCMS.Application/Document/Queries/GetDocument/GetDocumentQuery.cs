using MediatR;
using TRRCMS.Application.Documents.Dtos;

namespace TRRCMS.Application.Documents.Queries.GetDocument;

/// <summary>
/// Query to get document by ID
/// </summary>
public class GetDocumentQuery : IRequest<DocumentDto?>
{
    public Guid Id { get; }

    public GetDocumentQuery(Guid id)
    {
        Id = id;
    }
}
