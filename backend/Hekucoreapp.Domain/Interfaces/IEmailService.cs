namespace Hekucoreapp.Domain.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string body);
}