namespace Autodom.Core
{
    public class PdfMailSender
    {
        private readonly List<string> _emailsToNotify;

        public PdfMailSender(List<string> emailsToNotify)
        {
            _emailsToNotify = emailsToNotify;
        }

        public async Task SendAsync(MonthlyPdf pdf, byte[] attachmentBytes)
        {
            foreach (var email in _emailsToNotify)
            {
                Console.WriteLine($"Sending email to: {email}, about: {pdf}, attachment: {attachmentBytes.Length}");
            }
        }
    }
}
