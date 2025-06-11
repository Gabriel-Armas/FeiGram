using System.Net;
using System.Net.Mail;

public class EmailService
{
    private readonly string _smtpHost = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _emailFrom = "feigram.supp@gmail.com";
    private readonly string _emailPassword = "opdu llmf jgvq sczv";

    public async Task SendResetPasswordEmail(string toEmail, string resetLink)
    {
        var message = new MailMessage();
        message.From = new MailAddress(_emailFrom, "Feigram Support");
        message.To.Add(toEmail);
        message.Subject = "Recupera tu contraseña en Feigram";
        message.Body = $"Hola~\n\nHaz clic en este enlace mágico para recuperar tu contraseña:\n{resetLink}\n\nSi no pediste esto, ignora este correo!\n\nCon cariño,\nEl equipo de Feigram";
        message.IsBodyHtml = false;

        using (var client = new SmtpClient(_smtpHost, _smtpPort))
        {
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_emailFrom, _emailPassword);
            await client.SendMailAsync(message);
        }
    }
}