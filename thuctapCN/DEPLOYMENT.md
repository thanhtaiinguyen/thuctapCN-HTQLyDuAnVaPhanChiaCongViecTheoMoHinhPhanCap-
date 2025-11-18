# Hướng dẫn Deploy lên Hosting

## 1. Chuẩn bị trước khi Deploy

### 1.1. Cập nhật Connection String
- Mở file `appsettings.Production.json`
- Thay thế connection string với thông tin database thực tế của hosting:
```json
{
  "ConnectionStrings": {
    "thuctapCNContextConnection": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=True;"
  }
}
```

### 1.2. Chạy Migration
Trước khi deploy, chạy migration để tạo database:
```bash
cd thuctapCN
dotnet ef migrations add AddApplicationUserFields
dotnet ef database update
```

Hoặc trên hosting, chạy migration tự động khi ứng dụng khởi động (đã được cấu hình trong Program.cs).

## 2. Deploy lên Windows Server (IIS)

### 2.1. Yêu cầu
- Windows Server với IIS đã cài đặt
- .NET 8.0 Hosting Bundle
- SQL Server hoặc SQL Server Express

### 2.2. Các bước

1. **Cài đặt .NET 8.0 Hosting Bundle**
   - Tải từ: https://dotnet.microsoft.com/download/dotnet/8.0
   - Cài đặt và restart IIS

2. **Publish ứng dụng**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

3. **Copy files lên server**
   - Copy thư mục `publish` lên server
   - Đặt trong thư mục `C:\inetpub\wwwroot\thuctapCN` (hoặc thư mục bạn chọn)

4. **Cấu hình IIS**
   - Mở IIS Manager
   - Tạo Application Pool mới:
     - .NET CLR Version: No Managed Code
     - Managed Pipeline Mode: Integrated
   - Tạo Website mới:
     - Physical Path: `C:\inetpub\wwwroot\thuctapCN`
     - Binding: Port 80 (HTTP) hoặc 443 (HTTPS)
     - Application Pool: Chọn pool vừa tạo

5. **Cấu hình Connection String**
   - Tạo file `appsettings.Production.json` trên server
   - Hoặc cấu hình trong IIS → Configuration Editor → connectionStrings

6. **Cấu hình quyền**
   - Cấp quyền cho Application Pool Identity:
     - Read/Write cho thư mục `wwwroot/uploads`
     - Read cho toàn bộ ứng dụng

## 3. Deploy lên Linux Server

### 3.1. Yêu cầu
- Linux Server (Ubuntu 20.04+ hoặc tương đương)
- .NET 8.0 Runtime
- Nginx hoặc Apache
- SQL Server hoặc PostgreSQL

### 3.2. Các bước

1. **Cài đặt .NET 8.0 Runtime**
   ```bash
   wget https://dot.net/v1/dotnet-install.sh
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 8.0
   ```

2. **Publish ứng dụng**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

3. **Copy files lên server**
   ```bash
   scp -r ./publish user@server:/var/www/thuctapCN
   ```

4. **Cấu hình Systemd Service**
   Tạo file `/etc/systemd/system/thuctapCN.service`:
   ```ini
   [Unit]
   Description=Thuc Tap CN Web App
   
   [Service]
   WorkingDirectory=/var/www/thuctapCN
   ExecStart=/usr/bin/dotnet /var/www/thuctapCN/thuctapCN.dll
   Restart=always
   RestartSec=10
   SyslogIdentifier=thuctapCN
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=ASPNETCORE_URLS=http://localhost:5000
   
   [Install]
   WantedBy=multi-user.target
   ```

5. **Khởi động service**
   ```bash
   sudo systemctl enable thuctapCN
   sudo systemctl start thuctapCN
   ```

6. **Cấu hình Nginx**
   Tạo file `/etc/nginx/sites-available/thuctapCN`:
   ```nginx
   server {
       listen 80;
       server_name your-domain.com;
       
       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
           proxy_cache_bypass $http_upgrade;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }
   ```
   
   Enable site:
   ```bash
   sudo ln -s /etc/nginx/sites-available/thuctapCN /etc/nginx/sites-enabled/
   sudo nginx -t
   sudo systemctl reload nginx
   ```

## 4. Deploy với Docker

### 4.1. Build và chạy
```bash
docker-compose up -d
```

### 4.2. Chạy Migration
```bash
docker-compose exec web dotnet ef database update
```

## 5. Cấu hình Environment Variables

Thay vì hardcode trong appsettings, nên sử dụng Environment Variables:

```bash
# Linux
export ConnectionStrings__thuctapCNContextConnection="Server=..."

# Windows
setx ConnectionStrings__thuctapCNContextConnection "Server=..."
```

## 6. Bảo mật

### 6.1. HTTPS
- Cài đặt SSL certificate (Let's Encrypt miễn phí)
- Cấu hình redirect HTTP → HTTPS

### 6.2. Firewall
- Chỉ mở port 80, 443
- Đóng port 5000 (chỉ localhost)

### 6.3. Database
- Sử dụng strong password
- Không expose database port ra ngoài
- Sử dụng connection string với encryption

## 7. Monitoring và Logging

### 7.1. Logs
- Logs được lưu trong: `/var/log/thuctapCN/` (Linux)
- Hoặc Event Viewer (Windows)

### 7.2. Health Check
- Endpoint: `http://your-domain.com/health`
- Sử dụng để monitor ứng dụng

## 8. Backup

### 8.1. Database
- Backup định kỳ database
- Lưu trữ ở nơi an toàn

### 8.2. Uploads
- Backup thư mục `wwwroot/uploads`
- Có thể sync với cloud storage (Azure Blob, AWS S3)

## 9. Troubleshooting

### Lỗi thường gặp:

1. **500 Internal Server Error**
   - Kiểm tra logs
   - Kiểm tra connection string
   - Kiểm tra quyền thư mục

2. **Database connection failed**
   - Kiểm tra firewall
   - Kiểm tra connection string
   - Kiểm tra SQL Server đang chạy

3. **Upload không hoạt động**
   - Kiểm tra quyền thư mục `uploads`
   - Kiểm tra disk space

## 10. Checklist trước khi Deploy

- [ ] Đã cập nhật connection string
- [ ] Đã chạy migration
- [ ] Đã test trên môi trường staging
- [ ] Đã cấu hình HTTPS
- [ ] Đã backup database
- [ ] Đã cấu hình firewall
- [ ] Đã test upload file
- [ ] Đã test đăng nhập/đăng xuất
- [ ] Đã kiểm tra logs

