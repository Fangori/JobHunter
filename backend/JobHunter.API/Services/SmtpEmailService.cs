using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace JobHunter.API.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public SmtpEmailService(IConfiguration config)
    {
        _config = config;
    }

    // Dung IsNullOrWhiteSpace thay vi "??" - "??" chi bat duoc null (bien
    // moi truong CHUA set), khong bat duoc chuoi rong (bien DA set nhung
    // rong, vd docker-compose truyen Frontend__BaseUrl: ${FRONTEND_BASE_URL}
    // ma .env chua co dong do) -> link trong email bi thieu domain, bug
    // that da gap 2026-08-12.
    private string FrontendBaseUrl
    {
        get
        {
            var value = _config["Frontend:BaseUrl"];
            return string.IsNullOrWhiteSpace(value) ? "http://localhost:5173" : value;
        }
    }

    public Task GuiXacThucEmailAsync(string toEmail, string tokenValue)
    {
        var url = $"{FrontendBaseUrl}/verify-email?token={tokenValue}";
        var html = BuildHtml(
            "Xác thực email JobHunter",
            "Cảm ơn bạn đã đăng ký JobHunter. Bấm nút bên dưới để xác thực địa chỉ email và kích hoạt tài khoản (liên kết có hiệu lực trong thời gian giới hạn).",
            "Xác thực email", url);
        return GuiAsync(toEmail, "Xác thực email JobHunter", html);
    }

    public Task GuiDatLaiMatKhauAsync(string toEmail, string tokenValue)
    {
        var url = $"{FrontendBaseUrl}/reset-password?token={tokenValue}";
        var html = BuildHtml(
            "Đặt lại mật khẩu JobHunter",
            "Bạn (hoặc ai đó) vừa yêu cầu đặt lại mật khẩu cho tài khoản này. Bấm nút bên dưới để đặt mật khẩu mới. Nếu không phải bạn, có thể bỏ qua email này.",
            "Đặt lại mật khẩu", url);
        return GuiAsync(toEmail, "Đặt lại mật khẩu JobHunter", html);
    }

    private static string BuildHtml(string tieuDe, string noiDung, string chuNut, string url) => $"""
        <div style="font-family: Arial, Helvetica, sans-serif; max-width: 480px; margin: 0 auto;">
          <div style="background: #3949c6; color: #ffffff; padding: 20px 24px; border-radius: 8px 8px 0 0; font-size: 20px; font-weight: 700;">
            JobHunter
          </div>
          <div style="border: 1px solid #e2e4ea; border-top: none; padding: 24px; border-radius: 0 0 8px 8px;">
            <h2 style="margin: 0 0 12px; color: #1a2140;">{tieuDe}</h2>
            <p style="color: #1f2430; line-height: 1.5;">{noiDung}</p>
            <p style="text-align: center; margin: 24px 0;">
              <a href="{url}" style="background: #3949c6; color: #ffffff; text-decoration: none; padding: 12px 24px; border-radius: 8px; font-weight: 600; display: inline-block;">{chuNut}</a>
            </p>
            <p style="color: #6b7280; font-size: 13px;">Nếu nút không hoạt động, dán liên kết sau vào trình duyệt:<br />{url}</p>
          </div>
        </div>
        """;

    // Chua cau hinh Smtp:Host/Username/Password (dev/test) -> fallback log
    // console y het hanh vi mock cu, KHONG throw - khong pha luong dang
    // ky/quen mat khau, khong can sua gi o cac test hien co.
    private async Task GuiAsync(string toEmail, string subject, string html)
    {
        var host = _config["Smtp:Host"];
        var username = _config["Smtp:Username"];
        var password = _config["Smtp:Password"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine($"[EMAIL MOCK - SMTP chua cau hinh] Gui toi {toEmail}: {subject}\n{html}");
            return;
        }

        try
        {
            var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
            var fromEmailRaw = _config["Smtp:FromEmail"];
            var fromEmail = string.IsNullOrWhiteSpace(fromEmailRaw) ? username : fromEmailRaw;
            var fromNameRaw = _config["Smtp:FromName"];
            var fromName = string.IsNullOrWhiteSpace(fromNameRaw) ? "JobHunter" : fromNameRaw;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = html };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Gui that bai (SMTP sap, sai mat khau, mang loi...) KHONG duoc
            // lam hong luong nghiep vu chinh - tai khoan van phai tao/token
            // van phai luu thanh cong du mail khong gui duoc. Chi log lai.
            Console.WriteLine($"[EMAIL LOI] Gui toi {toEmail} that bai: {ex.Message}");
        }
    }
}
