using System.Net;
using System.Net.Mail;
using Email;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Email
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IFluentEmailFactory _fluentEmailFactory;

        public EmailService(ILogger<EmailService> logger, IFluentEmailFactory fluentEmailFactory)
        {
            _logger = logger;
            _fluentEmailFactory = fluentEmailFactory;
        }

        public async Task Send(EmailMessageModel emailMessageModel)
        {
            _logger.LogInformation("Sending email");
            await _fluentEmailFactory.Create().To(emailMessageModel.ToAddress)
                .Subject(emailMessageModel.Subject)
                .Body(emailMessageModel.Body, true) // true means this is an HTML format message
                .SendAsync();
        }
    }

    public interface IEmailService
    {
        /// <summary>
        /// Send an email.
        /// </summary>
        /// <param name="emailMessage">Message object to be sent</param>
        Task Send(EmailMessageModel emailMessage);
    }

    public class EmailMessageModel
    {
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string? Body { get; set; }
        public string? AttachmentPath { get; set; }
        public EmailMessageModel(string toAddress, string subject, string? body = "")
        {
            ToAddress = toAddress;
            Subject = subject;
            Body = body;
        }
    }
}

public class EmailSender : IEmailSender
{
    private readonly IEmailService _emailService;

    public EmailSender(IEmailService emailService)
    {
        _emailService = emailService;
    }
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        EmailMessageModel emailMessage = new(email,
        subject,
        htmlMessage);

        await _emailService.Send(emailMessage);
    }
}

public static class FluentEmailExtensions
{
    public static void AddFluentEmail(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        var emailSettings = configuration.GetSection("EmailSettings");
        var defaultFromEmail = emailSettings["DefaultFromEmail"];
        var host = emailSettings["Host"];
        var port = emailSettings.GetValue<int>("Port");
        var username = emailSettings["Username"];
        var password = emailSettings["Password"];
        bool enableSsl = emailSettings.GetValue<bool>("EnableSsl");
        Console.WriteLine($"Email settings: {defaultFromEmail}, {host}, {port}, {username}, {password}, {enableSsl}");

        services.AddFluentEmail(defaultFromEmail);

        var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = enableSsl,
            Host = host!,
            Port = port,
        };

        services.AddSingleton<ISender>(x => new SmtpSender(client));

        services.AddSingleton<ISender>(x => new SmtpSender(client));
    }
}