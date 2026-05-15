namespace TRRCMS.Application.Reporting.Common;

public sealed record ReportFile(byte[] Content, string FileName, string ContentType);
