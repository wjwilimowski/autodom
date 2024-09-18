﻿using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Autodom.Core
{
    public class MailSender
    {
        private readonly string _sendgridApiKey;
        private readonly ILogger _logger;

        public MailSender(string sendgridApiKey, ILogger logger)
        {
            _sendgridApiKey = sendgridApiKey;
            _logger = logger;
        }

        public async Task SendAsync(string emailAddress, string subject, string message)
        {
            var client = new SendGridClient(_sendgridApiKey);
            var from = new EmailAddress("autodom-bot@outlook.com", "Autodom Bot");
            var to = new EmailAddress(emailAddress);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            _logger.LogInformation("Send mail result: {StatusCode} {SendMailResult}", response.StatusCode, await response.Body.ReadAsStringAsync());
        }
    }
}
