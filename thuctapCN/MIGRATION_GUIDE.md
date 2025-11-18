# Hướng dẫn Migration Database

Sau khi thêm ApplicationUser với các trường mới, bạn cần tạo migration và cập nhật database.

## Các bước thực hiện:

### 1. Tạo Migration mới
```bash
cd thuctapCN
dotnet ef migrations add AddApplicationUserFields
```

### 2. Cập nhật Database
```bash
dotnet ef database update
```

## Lưu ý:

- Migration sẽ thêm các cột mới vào bảng `AspNetUsers`:
  - FullName (nvarchar(100), nullable)
  - EmployeeCode (nvarchar(50), not null) - **UNIQUE INDEX** (khóa chính logic)
  - Address (nvarchar(500), nullable)
  - AvatarPath (nvarchar(500), nullable)
  - DateOfBirth (datetime2, nullable)
  - Gender (nvarchar(10), nullable)
  - Department (nvarchar(100), nullable)
  - Position (nvarchar(100), nullable)
  - CreatedDate (datetime2, not null)
  - UpdatedDate (datetime2, nullable)

- **Lưu ý quan trọng về EmployeeCode:**
  - EmployeeCode là bắt buộc (required) và phải unique
  - EmployeeCode không thể chỉnh sửa sau khi tạo user
  - Index unique: `IX_ApplicationUser_EmployeeCode`

- Dữ liệu hiện có sẽ không bị mất
- Các trường mới sẽ có giá trị NULL cho các user cũ

## Nếu gặp lỗi:

Nếu migration báo lỗi về việc đã có dữ liệu, bạn có thể:
1. Xóa database và tạo lại (chỉ trong môi trường development)
2. Hoặc tạo migration với `--force` flag

