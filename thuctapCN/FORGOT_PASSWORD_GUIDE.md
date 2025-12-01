# Hướng dẫn Sử dụng Chức năng Quên Mật Khẩu

## 📌 Tổng quan

Chức năng "Quên mật khẩu" đã được tích hợp hoàn chỉnh vào hệ thống. Trong môi trường **Development**, liên kết đặt lại mật khẩu sẽ được hiển thị trong console/logs thay vì gửi email thật.

---

## 🔐 Cách sử dụng Quên Mật Khẩu

### Bước 1: Truy cập trang đăng nhập
- Mở trình duyệt và truy cập: `http://localhost:5169`
- Bạn sẽ thấy trang đăng nhập

### Bước 2: Click vào "Quên mật khẩu?"
- Tìm link **"Quên mật khẩu?"** bên dưới ô mật khẩu
- Click vào link này

### Bước 3: Nhập email
- Nhập email của tài khoản bạn muốn reset mật khẩu
  - Ví dụ: `thanhtai@gmail.com`
- Click nút **"Gửi liên kết đặt lại mật khẩu"**

### Bước 4: Lấy link reset từ Console
- Quay lại terminal/console nơi đang chạy ứng dụng
- Tìm phần log có dạng:

```
=================================================
📧 EMAIL ĐÃ GỬI (Development Mode)
To: thanhtai@gmail.com
Subject: Reset Password
Message: Please reset your password by <a href='http://localhost:5169/Identity/Account/ResetPassword?code=...'>clicking here</a>.
=================================================
```

### Bước 5: Copy link và mở trong trình duyệt
- Copy toàn bộ URL trong thẻ `<a href='...'>`
- Ví dụ: `http://localhost:5169/Identity/Account/ResetPassword?code=Q2ZESjh...`
- Paste vào trình duyệt và truy cập

### Bước 6: Đặt mật khẩu mới
- Nhập email của bạn: `thanhtai@gmail.com`
- Nhập mật khẩu mới (phải đáp ứng yêu cầu):
  - Ít nhất 6 ký tự
  - Có chữ hoa (A-Z)
  - Có chữ thường (a-z)
  - Có số (0-9)
- Nhập lại mật khẩu để xác nhận
- Click **"Đặt lại mật khẩu"**

### Bước 7: Đăng nhập với mật khẩu mới
- Sau khi thấy thông báo thành công, click **"Đăng nhập ngay"**
- Đăng nhập bằng email và mật khẩu mới

---

## ⚙️ Cấu hình kỹ thuật

### Email Sender Service
Hệ thống sử dụng `ConsoleEmailSender` cho development:
- Không gửi email thật
- Log thông tin email ra console
- Tiết kiệm chi phí và dễ debug

**File:** `Services/ConsoleEmailSender.cs`

### Các thay đổi đã thực hiện:

#### 1. Tạo Console Email Sender
```csharp
// Services/ConsoleEmailSender.cs
public class ConsoleEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Log ra console thay vì gửi email thật
        Console.WriteLine($"📧 EMAIL ĐÃ GỬI");
        Console.WriteLine($"To: {email}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Message: {htmlMessage}");
        return Task.CompletedTask;
    }
}
```

#### 2. Đăng ký Service trong Program.cs
```csharp
// Program.cs
builder.Services.AddTransient<IEmailSender, ConsoleEmailSender>();
```

#### 3. Sửa ForgotPassword.cshtml.cs
- Đổi từ `UserManager<IdentityUser>` sang `UserManager<ApplicationUser>`
- Bỏ qua kiểm tra `EmailConfirmed` trong development
- Sử dụng `IEmailSender` đã đăng ký

#### 4. Sửa ResetPassword.cshtml.cs
- Đổi từ `UserManager<IdentityUser>` sang `UserManager<ApplicationUser>`

#### 5. Cập nhật giao diện
- ✅ `ForgotPassword.cshtml` - Tiếng Việt, thiết kế đẹp
- ✅ `ForgotPasswordConfirmation.cshtml` - Tiếng Việt, có icon
- ✅ `ResetPassword.cshtml` - Tiếng Việt, hướng dẫn rõ ràng
- ✅ `ResetPasswordConfirmation.cshtml` - Tiếng Việt, thông báo thành công

---

## 🎯 Production: Sử dụng Email Service thật

Khi deploy lên production, bạn cần:

### Option 1: SendGrid (Khuyên dùng)
1. Đăng ký tài khoản SendGrid (Free tier: 100 emails/day)
2. Lấy API Key
3. Cài package:
```bash
dotnet add package SendGrid
```

4. Tạo `SendGridEmailSender.cs`:
```csharp
using SendGrid;
using SendGrid.Helpers.Mail;

public class SendGridEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SendGridEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var client = new SendGridClient(apiKey);
        
        var from = new EmailAddress("noreply@thuctapcn.com", "Thực Tập CN");
        var to = new EmailAddress(email);
        
        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlMessage);
        await client.SendEmailAsync(msg);
    }
}
```

5. Cập nhật `appsettings.json`:
```json
{
  "SendGrid": {
    "ApiKey": "SG.your-api-key-here"
  }
}
```

6. Sửa `Program.cs`:
```csharp
if (app.Environment.IsDevelopment())
{
    builder.Services.AddTransient<IEmailSender, ConsoleEmailSender>();
}
else
{
    builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();
}
```

### Option 2: SMTP Gmail
```csharp
public class SmtpEmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        using var client = new SmtpClient("smtp.gmail.com", 587);
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password");
        
        var message = new MailMessage
        {
            From = new MailAddress("your-email@gmail.com"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        message.To.Add(email);
        
        await client.SendMailAsync(message);
    }
}
```

---

## ❗ Troubleshooting

### Lỗi: "A code must be supplied for password reset"
**Nguyên nhân:** Không có code trong URL hoặc code không hợp lệ

**Giải pháp:**
- Kiểm tra lại URL có đầy đủ `?code=...` không
- Code phải được copy đầy đủ từ console log
- Code có thể rất dài (200-300 ký tự)

### Lỗi: "Invalid login attempt" sau khi reset
**Nguyên nhân:** Mật khẩu mới không đúng format

**Giải pháp:**
- Đảm bảo mật khẩu có ít nhất 6 ký tự
- Phải có chữ hoa, chữ thường và số
- Ví dụ: `Password123`

### Không thấy log email trong console
**Nguyên nhân:** Console bị clear hoặc không scroll xuống

**Giải pháp:**
- Scroll terminal xuống dưới cùng
- Tìm dòng bắt đầu bằng `=================================================`
- Hoặc search "EMAIL ĐÃ GỬI"

---

## 📝 Lưu ý

1. **Security**: Link reset password chỉ sử dụng được 1 lần
2. **Expiration**: Token có thời hạn (mặc định: 1 ngày)
3. **Development Mode**: Không bật email confirmation cho dễ test
4. **Production Mode**: Nên bật `RequireConfirmedAccount = true` và email confirmation

---

## ✅ Checklist triển khai

- [x] Tạo ConsoleEmailSender service
- [x] Đăng ký IEmailSender trong Program.cs
- [x] Sửa ForgotPassword.cshtml.cs
- [x] Sửa ResetPassword.cshtml.cs
- [x] Cập nhật giao diện tiếng Việt
- [x] Test chức năng trong development
- [ ] Cấu hình email service thật cho production
- [ ] Test email service thật
- [ ] Bật email confirmation trong production

---

## 📞 Support

Nếu gặp vấn đề, hãy kiểm tra:
1. Console logs có lỗi gì không
2. Database có user với email đó không
3. Link reset password có đầy đủ code không
4. Mật khẩu mới có đúng format không

**Tài khoản test mặc định:**
- Email: `admin@thuctapcn.com` / Password: `Admin@123`
- Email: `management@thuctapcn.com` / Password: `Management@123`

