using System.Net;
using System.Text;
using JobbRadar.Models;
using JobbRadar.Settings;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace JobbRadar.Services;

public class EmailSender(EmailSettings settings)
{
    public async Task SendAsync(IReadOnlyList<JobAd> ads, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("JobbRadar", settings.User));
        message.To.Add(MailboxAddress.Parse(settings.To));
        message.Subject = $"JobbRadar: {ads.Count} nya .NET-jobb";
        message.Body = new BodyBuilder { HtmlBody = BuildHtml(ads) }.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.StartTls, ct);
        await smtp.AuthenticateAsync(settings.User, settings.Password, ct);
        await smtp.SendAsync(message, ct);
        await smtp.DisconnectAsync(true, ct);
    }

    public static string BuildHtml(IReadOnlyList<JobAd> ads)
    {
        var html = new StringBuilder();
        html.Append($"<h2>{ads.Count} nya .NET-jobb</h2><ul>");

        foreach (var ad in ads)
        {
            var deadline = ad.ApplicationDeadline?.ToString("yyyy-MM-dd") ?? "okänd";
            html.Append("<li style=\"margin-bottom:12px\">")
                .Append($"<a href=\"{WebUtility.HtmlEncode(ad.Url)}\"><b>{WebUtility.HtmlEncode(ad.Headline)}</b></a><br>")
                .Append($"{WebUtility.HtmlEncode(ad.Employer)} – {WebUtility.HtmlEncode(ad.Municipality ?? "")}<br>")
                .Append($"Sista ansökningsdag: {deadline}")
                .Append("</li>");
        }

        return html.Append("</ul>").ToString();
    }
}