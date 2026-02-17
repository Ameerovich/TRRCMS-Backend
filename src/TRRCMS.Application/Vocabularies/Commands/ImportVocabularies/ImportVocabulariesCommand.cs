using MediatR;
using TRRCMS.Application.Vocabularies.Queries.ExportVocabularies;

namespace TRRCMS.Application.Vocabularies.Commands.ImportVocabularies;

/// <summary>
/// Import vocabularies from a snapshot (disaster recovery or environment sync).
/// For each vocabulary:
/// - If it doesn't exist → create new vocabulary
/// - If it exists → create new version with imported values
/// </summary>
public class ImportVocabulariesCommand : IRequest<ImportVocabulariesResult>
{
    public List<VocabularyExportDto> Vocabularies { get; set; } = new();
}

public class ImportVocabulariesResult
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Messages { get; set; } = new();
}
