namespace Multi_Library_Management_Api.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, int libraryId, byte[]? attachment = null, string? attachmentName = null);
        Task SendSystemEmailAsync(string to, string subject, string body);
    }
}
