namespace TRRCMS.Application.Users.Dtos
{
    public class AuditLogDto
    {
        public DateTime Timestamp { get; set; }
        public int Action { get; set; }
        public string UserName { get; set; }
        public string Changes { get; set; }
        public string? Reason { get; set; }
    }
}
