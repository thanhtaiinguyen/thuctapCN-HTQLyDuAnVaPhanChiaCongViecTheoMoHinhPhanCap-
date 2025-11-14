# Thông tin tài khoản hệ thống

## Tài khoản mặc định

Hệ thống đã được cấu hình với các tài khoản và roles sau:

### 1. Tài khoản Admin
- **Email:** admin@thuctapcn.com
- **Mật khẩu:** Admin@123
- **Role:** Admin
- **Quyền:** Toàn quyền quản trị hệ thống

### 2. Tài khoản Management
- **Email:** management@thuctapcn.com
- **Mật khẩu:** Management@123
- **Role:** Management
- **Quyền:** Quản lý và điều hành

## Roles trong hệ thống

1. **Admin** - Quản trị viên hệ thống
2. **Management** - Quản lý

## Lưu ý bảo mật

⚠️ **QUAN TRỌNG:** Đây là các tài khoản mặc định cho môi trường phát triển. 
Trong môi trường production, vui lòng:
- Đổi mật khẩu ngay sau khi triển khai
- Xóa hoặc vô hiệu hóa các tài khoản này nếu không cần thiết
- Sử dụng mật khẩu mạnh và bảo mật

## Cách sử dụng

1. Chạy ứng dụng
2. Truy cập trang đăng nhập
3. Sử dụng một trong các tài khoản trên để đăng nhập

## Phân chia công việc

### Quy trình tạo và phân công công việc

**Bước 1: Truy cập form tạo công việc**
- Từ giao diện chi tiết dự án, Manager chọn tab **"Công việc"**
- Chọn vào mục **"Thêm công việc mới"**
- Hệ thống sẽ hiển thị form tạo công việc mới

**Bước 2: Nhập thông tin công việc**

Manager cần nhập các thông tin sau:
- **Tên công việc** (bắt buộc)
- **Mô tả** (tùy chọn)
- **Deadline** (bắt buộc)
- **Mức độ ưu tiên** (bắt buộc)
- **Thành viên được giao** (bắt buộc)

> **Lưu ý:** Hệ thống sẽ tự động kiểm tra định dạng dữ liệu (ngày, tháng, ký tự, độ dài,...)

### Các trường hợp xử lý lỗi

#### ❌ Thiếu thông tin bắt buộc
**Điều kiện:** Manager chưa nhập đủ thông tin bắt buộc (ví dụ: tên công việc, deadline, người được giao)

**Thông báo:** 
```
Vui lòng nhập đầy đủ thông tin bắt buộc.
```

#### ❌ Ngày deadline không hợp lệ
**Điều kiện:** Ngày deadline sớm hơn ngày hiện tại hoặc ngoài phạm vi dự án

**Thông báo:** 
```
Ngày hoàn thành phải nằm trong thời gian dự án.
```

#### ❌ Không chọn thành viên
**Điều kiện:** Không chọn thành viên nào để giao việc

**Thông báo:** 
```
Vui lòng chọn ít nhất một thành viên để giao việc.
```

#### ❌ Lỗi hệ thống
**Điều kiện:** Hệ thống lỗi khi lưu dữ liệu

**Thông báo:** 
```
Không thể lưu công việc. Vui lòng thử lại sau.
```

### Hoàn tất phân công

**Bước 3: Lưu công việc**
- Manager nhấn nút **"Lưu"** hoặc **"Phân công"**
- Hệ thống sẽ:
  - Lưu công việc vào cơ sở dữ liệu
  - Liên kết với dự án hiện tại
  - Gắn công việc cho các thành viên được chọn

**Bước 4: Xác nhận thành công**
- Hệ thống hiển thị thông báo: **"Phân chia công việc thành công"**
- Công việc mới sẽ xuất hiện trong:
  - ✅ Danh sách công việc của dự án
  - ✅ Danh sách công việc của các member được giao

### Lưu đồ quy trình

```
[Bắt đầu]
    ↓
[Chọn tab "Công việc"]
    ↓
[Chọn "Thêm công việc mới"]
    ↓
[Hiển thị form]
    ↓
[Nhập thông tin] → [Kiểm tra validation]
    ↓                      ↓
    ↓                   [Có lỗi?] → [Hiển thị lỗi] → [Quay lại nhập]
    ↓                      ↓
[Nhấn "Lưu/Phân công"]  [Không lỗi]
    ↓                      ↓
[Lưu vào database]        ↓
    ↓                      ↓
[Thành công?] → [Không] → [Hiển thị lỗi hệ thống]
    ↓
  [Có]
    ↓
[Hiển thị thông báo thành công]
    ↓
[Cập nhật danh sách công việc]
    ↓
[Kết thúc]
```

## Theo dõi và cập nhật tiến độ công việc

### Xem danh sách công việc

**Vai trò:** Manager và Member

**Cách truy cập:**
- **Manager:** Từ giao diện chi tiết dự án → Tab "Công việc" → Xem toàn bộ công việc của dự án
- **Member:** Từ Dashboard cá nhân → "Công việc của tôi" → Xem công việc được giao cho mình

**Thông tin hiển thị:**
- Tên công việc
- Mức độ ưu tiên (Cao/Trung bình/Thấp)
- Trạng thái (Chưa bắt đầu/Đang thực hiện/Hoàn thành/Tạm dừng)
- Người được giao
- Deadline
- Tiến độ (%)

### Cập nhật trạng thái công việc

**Bước 1: Truy cập chi tiết công việc**
- Member chọn vào công việc cần cập nhật
- Hệ thống hiển thị form chi tiết công việc

**Bước 2: Cập nhật thông tin**

Member có thể cập nhật:
- **Trạng thái công việc** (bắt buộc)
  - Chưa bắt đầu
  - Đang thực hiện
  - Hoàn thành
  - Tạm dừng
- **Tiến độ (%)** (tùy chọn)
- **Ghi chú/Báo cáo** (tùy chọn)
- **File đính kèm** (tùy chọn)

### Các trường hợp xử lý

#### ✅ Cập nhật thành công
**Điều kiện:** Dữ liệu hợp lệ

**Kết quả:**
- Hệ thống lưu thông tin cập nhật
- Thông báo: **"Cập nhật trạng thái công việc thành công"**
- Gửi thông báo cho Manager
- Cập nhật dashboard của Manager

#### ⚠️ Tiến độ không khớp với trạng thái
**Điều kiện:** Trạng thái "Hoàn thành" nhưng tiến độ < 100%

**Thông báo:** 
```
Trạng thái "Hoàn thành" yêu cầu tiến độ 100%. Vui lòng kiểm tra lại.
```

#### ❌ File đính kèm quá lớn
**Điều kiện:** File > 10MB

**Thông báo:** 
```
Kích thước file không được vượt quá 10MB.
```

#### ❌ Định dạng file không hợp lệ
**Điều kiện:** File không thuộc danh sách cho phép (.pdf, .doc, .docx, .xls, .xlsx, .zip, .rar, .jpg, .png)

**Thông báo:** 
```
Định dạng file không được hỗ trợ. Vui lòng upload file: PDF, DOC, DOCX, XLS, XLSX, ZIP, RAR, JPG, PNG.
```

---

## Quản lý thành viên dự án

### Thêm thành viên vào dự án

**Vai trò:** Manager

**Bước 1: Truy cập quản lý thành viên**
- Từ giao diện chi tiết dự án → Tab "Thành viên"
- Chọn **"Thêm thành viên mới"**

**Bước 2: Chọn thành viên**
- Hệ thống hiển thị danh sách users chưa tham gia dự án
- Manager chọn một hoặc nhiều thành viên
- Chọn vai trò cho từng thành viên:
  - **Member** - Thành viên thực hiện
  - **Viewer** - Chỉ xem

**Bước 3: Xác nhận**
- Nhấn **"Thêm vào dự án"**
- Hệ thống kiểm tra và lưu thông tin

### Các trường hợp xử lý

#### ✅ Thêm thành công
**Kết quả:**
- Thông báo: **"Đã thêm thành viên vào dự án thành công"**
- Thành viên mới xuất hiện trong danh sách
- Gửi email/thông báo cho thành viên mới

#### ❌ Chưa chọn thành viên
**Thông báo:** 
```
Vui lòng chọn ít nhất một thành viên.
```

#### ❌ Thành viên đã tồn tại
**Điều kiện:** Thành viên đã có trong dự án

**Thông báo:** 
```
Thành viên này đã tham gia dự án.
```

#### ❌ Vượt quá số lượng thành viên tối đa
**Điều kiện:** Dự án đã đạt giới hạn thành viên (nếu có)

**Thông báo:** 
```
Đã đạt số lượng thành viên tối đa cho dự án này.
```

### Xóa thành viên khỏi dự án

**Vai trò:** Manager

**Quy trình:**
1. Từ danh sách thành viên → Chọn **"Xóa"** bên cạnh tên thành viên
2. Hệ thống hiển thị xác nhận: **"Bạn có chắc muốn xóa thành viên này khỏi dự án?"**
3. Manager xác nhận

#### ⚠️ Cảnh báo khi xóa thành viên có công việc
**Điều kiện:** Thành viên đang có công việc chưa hoàn thành

**Thông báo:** 
```
Thành viên này đang có [X] công việc chưa hoàn thành. Vui lòng phân công lại trước khi xóa.
```

**Tùy chọn:**
- **Hủy** - Không xóa
- **Xem công việc** - Xem danh sách công việc cần phân công lại
- **Xóa và gỡ công việc** - Xóa thành viên và gỡ khỏi tất cả công việc

---

## Báo cáo và thống kê dự án

### Dashboard tổng quan dự án

**Vai trò:** Manager

**Thông tin hiển thị:**

#### 📊 Thống kê chung
- Tổng số công việc
- Công việc hoàn thành / Tổng công việc (%)
- Công việc đang thực hiện
- Công việc quá hạn
- Số thành viên

#### 📈 Biểu đồ tiến độ
- **Biểu đồ tròn:** Phân bố trạng thái công việc
- **Biểu đồ cột:** Số lượng công việc theo mức độ ưu tiên
- **Biểu đồ đường:** Tiến độ theo thời gian

#### 👥 Hiệu suất thành viên
- Danh sách thành viên
- Số công việc được giao
- Số công việc hoàn thành
- Tỷ lệ hoàn thành (%)
- Số công việc quá hạn

### Xuất báo cáo

**Bước 1: Chọn loại báo cáo**
- **Báo cáo tổng quan dự án**
- **Báo cáo chi tiết công việc**
- **Báo cáo hiệu suất thành viên**
- **Báo cáo công việc quá hạn**

**Bước 2: Chọn khoảng thời gian**
- Từ ngày: ___/___/___
- Đến ngày: ___/___/___

**Bước 3: Chọn định dạng xuất**
- PDF
- Excel (XLSX)
- CSV

**Bước 4: Xuất báo cáo**
- Nhấn **"Xuất báo cáo"**
- Hệ thống tạo file và tự động tải xuống

#### ❌ Không có dữ liệu trong khoảng thời gian
**Thông báo:** 
```
Không có dữ liệu trong khoảng thời gian đã chọn.
```

#### ❌ Lỗi khi tạo báo cáo
**Thông báo:** 
```
Không thể tạo báo cáo. Vui lòng thử lại sau.
```

---

## Thông báo hệ thống

### Các loại thông báo

#### 🔔 Thông báo cho Member
- Được giao công việc mới
- Công việc sắp đến hạn (trước 2 ngày)
- Công việc quá hạn
- Manager yêu cầu cập nhật tiến độ
- Bị xóa khỏi dự án
- Công việc bị hủy

#### 🔔 Thông báo cho Manager
- Thành viên cập nhật trạng thái công việc
- Công việc được hoàn thành
- Công việc quá hạn
- Thành viên báo cáo vấn đề
- Dự án sắp đến deadline

### Cài đặt thông báo

**Tùy chọn:**
- ✅ Nhận thông báo qua email
- ✅ Nhận thông báo trong hệ thống
- ✅ Nhận thông báo công việc sắp đến hạn
- ✅ Nhận thông báo công việc quá hạn
- ✅ Tóm tắt hàng ngày (Daily digest)

---

## Tìm kiếm và lọc

### Tìm kiếm công việc

**Tiêu chí tìm kiếm:**
- Tên công việc
- Mô tả
- Người được giao
- Ngày tạo
- Deadline

### Lọc công việc

**Bộ lọc:**
- **Trạng thái:** Tất cả / Chưa bắt đầu / Đang thực hiện / Hoàn thành / Tạm dừng
- **Mức độ ưu tiên:** Tất cả / Cao / Trung bình / Thấp
- **Người được giao:** Tất cả thành viên / [Chọn thành viên cụ thể]
- **Thời gian:** Tất cả / Hôm nay / Tuần này / Tháng này / Quá hạn
- **Tiến độ:** < 25% / 25-50% / 50-75% / 75-99% / 100%

**Sắp xếp theo:**
- Ngày tạo (Mới nhất/Cũ nhất)
- Deadline (Gần nhất/Xa nhất)
- Mức độ ưu tiên (Cao → Thấp / Thấp → Cao)
- Tiến độ (Tăng dần/Giảm dần)

---

## Quy trình quản lý dự án hoàn chỉnh

```
[Tạo dự án mới (Manager)]
    ↓
[Thêm thông tin dự án: tên, mô tả, thời gian, ngân sách]
    ↓
[Thêm thành viên vào dự án]
    ↓
[Phân chia công việc cho thành viên]
    ↓
┌───────────────────────────────────────┐
│  Vòng lặp thực hiện dự án             │
│                                        │
│  [Member nhận công việc]              │
│           ↓                            │
│  [Member thực hiện & cập nhật tiến độ]│
│           ↓                            │
│  [Manager theo dõi dashboard]         │
│           ↓                            │
│  [Manager kiểm tra tiến độ]           │
│           ↓                            │
│  ┌─────[Công việc hoàn thành?]        │
│  │ NO ← ← ← ← ← ← ← ← ← ← ← ↑        │
│  │                                     │
│  YES                                   │
│  ↓                                     │
│  [Đánh dấu công việc hoàn thành]      │
│           ↓                            │
│  ┌─────[Còn công việc khác?]          │
│  │ YES → → → → → (quay lại đầu vòng)  │
│  │                                     │
│  NO                                    │
└──│────────────────────────────────────┘
   ↓
[Xuất báo cáo tổng kết dự án]
    ↓
[Đánh giá hiệu suất thành viên]
    ↓
[Đóng dự án]
    ↓
[Kết thúc]
```

