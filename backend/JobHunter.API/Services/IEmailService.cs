namespace JobHunter.API.Services;

// 2 ham theo ngu nghia (khong phai 1 ham gui-mail-chung-chung) de
// AuthService khong can biet gi ve URL/HTML/Frontend:BaseUrl - implementation
// (SmtpEmailService) tu lo toan bo phan build noi dung.
public interface IEmailService
{
    Task GuiXacThucEmailAsync(string toEmail, string tokenValue); // UC03
    Task GuiDatLaiMatKhauAsync(string toEmail, string tokenValue); // UC06
}
