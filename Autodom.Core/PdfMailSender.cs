using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Autodom.Core
{
    public class PdfMailSender
    {
        private readonly List<string> _emailsToNotify;
        private readonly string _sendgridApiKey;
        private readonly ILogger _logger;

        public PdfMailSender(List<string> emailsToNotify, string sendgridApiKey, ILogger logger)
        {
            _emailsToNotify = emailsToNotify;
            _sendgridApiKey = sendgridApiKey;
            _logger = logger;
        }

        public async Task SendAsync(string emailAddress, BillDto pdf)
        {
            var client = new SendGridClient(_sendgridApiKey);
            var from = new EmailAddress("autodom-bot@outlook.com", "Autodom Bot");
            var subject = "Nowy rachunek w ToMojDom.pl";
            var to = new EmailAddress(emailAddress);
            var plainTextContent = pdf.ToString();
            var htmlContent = pdf.ToString();
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            _logger.LogInformation("Send mail result: {StatusCode} {SendMailResult}", response.StatusCode, await response.Body.ReadAsStringAsync());

            foreach (var email in _emailsToNotify)
            {
                _logger.LogInformation("Sending email to: {0}, about: {1}, attachment: {2}", email, pdf);
            }
        }
    }
}
