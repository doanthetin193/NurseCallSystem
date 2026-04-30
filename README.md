# 🏥 Hospital Nurse Call System — Hệ Thống Gọi Y Tá Khẩn Cấp

> **Đồ án môn Lập Trình Mạng** — Ứng dụng mạng LAN mô phỏng hệ thống gọi y tá tại bệnh viện, tích hợp đầy đủ 10 chương kiến thức lập trình mạng.

---

## 📋 Mục lục

- [Bài toán thực tế](#-bài-toán-thực-tế)
- [Kiến trúc hệ thống](#-kiến-trúc-hệ-thống)
- [Tính năng](#-tính-năng)
- [Kiến thức mạng sử dụng](#-kiến-thức-mạng-sử-dụng-8-chương)
- [Công nghệ](#-công-nghệ)
- [Cấu trúc thư mục](#-cấu-trúc-thư-mục)
- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
- [Cách clone và chạy](#-cách-clone-và-chạy)
- [Hướng dẫn sử dụng](#-hướng-dẫn-sử-dụng)
- [Giao thức mạng](#-giao-thức-mạng-chi-tiết)

---

## 🏥 Bài toán thực tế

Tại các bệnh viện chuẩn quốc tế (Vinmec, FV Hospital...), mỗi giường bệnh đều có một thiết bị đầu giường cho phép bệnh nhân gọi y tá trong trường hợp khẩn cấp. Hệ thống sẽ:

- Thông báo **chính xác phòng và giường** nào đang cần giúp đỡ — y tá không phải chạy vòng quanh tòa nhà để tìm
- Phân loại mức độ **khẩn cấp** (cấp cứu / thay dịch truyền / hỗ trợ vệ sinh)
- Cho phép **báo động toàn viện** khi có sự cố nghiêm trọng (Code Blue / sơ tán)
- **Giám sát tự động** kết nối thiết bị đầu giường — phát hiện thiết bị mất mạng ngay lập tức

---

## 🏛️ Kiến trúc hệ thống

```
┌─────────────────────────────────────────────┐
│         MẠNG NỘI BỘ LAN (192.168.x.x)       │
│                                             │
│  ┌──────────────────┐    ┌───────────────┐  │
│  │   NurseServer    │    │ PatientClient  │  │
│  │  (Quầy Y Tá)    │◄──►│ (Đầu Giường)  │  │
│  │                  │    │               │  │
│  │ • Quản lý 125+   │    │ • 3 nút bấm   │  │
│  │   giường bệnh    │    │ • Auto kết    │  │
│  │ • Nhận cảnh báo  │    │   nối server  │  │
│  │ • Theo dõi ICMP  │    │               │  │
│  └──────────────────┘    └───────────────┘  │
│                                             │
│  Giao thức sử dụng:                         │
│  • UDP  :50000 — Auto-Discovery & Rooms     │
│  • TCP  :50001 — Gửi cảnh báo khẩn cấp     │
│  • UDP Multicast 239.0.0.1:50002 — CodeBlue │
│  • ICMP         — Health Check giường bệnh  │
└─────────────────────────────────────────────┘
```

> **Mô hình Client–Server:** 1 NurseServer (quầy trực y tá) + N PatientClient (mỗi instance = 1 giường bệnh)

---

## ✨ Tính năng

### Phía NurseServer (Quầy Y Tá)
| Tính năng | Mô tả |
|-----------|-------|
| 🟢 Bảng theo dõi giường | Hiển thị real-time trạng thái tất cả giường bệnh, lọc theo tầng / trạng thái |
| 🔴 Nhận cảnh báo khẩn cấp | Phát âm thanh + đổi màu ô giường ngay khi nhận tín hiệu TCP |
| 📡 Báo động toàn viện | Gửi multicast CODE_BLUE đến tất cả thiết bị đầu giường cùng lúc |
| ✅ Xác nhận xử lý | Chuột phải → "Đã đến giường xử lý xong" → gửi RESOLVED về client |
| 📊 Lịch sử & Thống kê | Xem lịch sử cuộc gọi, thống kê KPI: thời gian phản hồi trung bình, phòng gọi nhiều nhất |
| ⚙️ Quản lý giường | Thêm giường tịnh tiến tự động hoặc nhập tay (kèm giới hạn số tầng) |
| 🔒 Reset phiên | Tự động reset DB về Offline khi khởi động server — không hiển thị trạng thái giả từ phiên cũ |
| 🗑️ Xóa giường | Xóa vĩnh viễn giường khỏi hệ thống qua menu chuột phải |

### Phía PatientClient (Đầu Giường)
| Tính năng | Mô tả |
|-----------|-------|
| 🔍 Auto-Discovery | Tự động tìm và kết nối Server qua UDP broadcast, retry mỗi 4 giây |
| 📋 Chọn phòng/giường | Dropdown chỉ hiển thị phòng và giường **thực tế có trong DB** (không hardcode) |
| 🔴 Cấp cứu mở rộng | Gửi tín hiệu TCP loại CẤP CỨU |
| 🟡 Cần thay dịch | Gửi tín hiệu TCP loại THAY DỊCH TRUYỀN |
| 🟢 Hỗ trợ vệ sinh | Gửi tín hiệu TCP loại HỖ TRỢ VỆ SINH |
| 🚨 Nhận Code Blue | Màn hình chớp đỏ + cảnh báo khi server phát báo động toàn viện |
| 🔔 Nhận RESOLVED | Tự động mở lại nút bấm khi y tá xác nhận đã xử lý |
| 🚫 Chống trùng giường | Server từ chối (`BED_TAKEN`) nếu giường đã có client khác kết nối |

---

## 📚 Kiến thức mạng sử dụng (10 Chương)

| Chương | Chủ đề | Hiện thực trong project |
|--------|--------|------------------------|
| **1** | Giới thiệu lập trình mạng | Mô hình Client–Server, địa chỉ IP, Port, giao thức TCP/UDP |
| **2** | Luồng dữ liệu vào ra | `NetworkStream`, `StreamReader`, `StreamWriter { AutoFlush=true }` bọc TCP |
| **3** | Socket hướng kết nối (TCP) | `TcpListener` / `TcpClient` nhận 3 loại cảnh báo + gói OFFLINE, phản hồi `ACK_ALERT` |
| **4** | Socket không hướng kết nối (UDP) | Broadcast `NURSE_CALL_DISCOVERY` tự động tìm Server; `GET_ACTIVE_ROOMS` tải danh sách phòng |
| **5** | Lớp Helper C# | `TcpClient`, `TcpListener`, `UdpClient` (Helper) + raw `Socket` cho Multicast |
| **6** | Lập trình bất đồng bộ | `async/await` xuyên suốt: `AcceptTcpClientAsync`, `ReceiveAsync`, `SendPingAsync`, APM `BeginReceive/EndReceive` |
| **7** | Thread trong ứng dụng mạng | `new Thread()` với `IsBackground`, `Name` cho ICMP loop; `lock(_dbLock)` bảo vệ write DB; `InvokeRequired/Invoke` đồng bộ UI thread |
| **8** | IP Multicasting | `CODE_BLUE_EVACUATE` → `239.0.0.1:50002`; replay Code Blue cho client kết nối muộn; `RESOLVED` multicast |
| **9** | ICMP và ứng dụng | `Ping`, `SendPingAsync`, `PingReply`, `IPStatus.Success` — health check mỗi 5 giây |
| **10** | Bảo mật lập trình mạng | **AES-128-CBC** mã hóa toàn bộ UDP/TCP (10.3); **HMAC-SHA256** xác thực TCP Alert (10.4) |

### Chi tiết Chương 10 — Bảo mật

| Kỹ thuật | Method | Mục đích | Mục 10.x |
|---------|--------|---------|----------|
| AES-128-CBC | `Encrypt / Decrypt` | Bảo vệ nội dung UDP/Multicast | 10.3 Confidentiality |
| AES-128-CBC + Base64 | `EncryptToBase64 / DecryptFromBase64` | Bảo vệ nội dung TCP | 10.3 |
| HMAC-SHA256 | `AttachHmac / VerifyAndStripHmac` | Toàn vẹn + xác thực nguồn gửi TCP Alert | 10.4 |
| Reject giả mạo | `HandleTcpClientAsync` → `if (data == null)` | Server từ chối gói bị tamper, ghi log cảnh báo | 10.1, 10.4 |

---

## 🛠️ Công nghệ

| Thành phần | Chi tiết |
|------------|----------|
| Ngôn ngữ | C# |
| Framework | .NET Framework 4.7.2 |
| UI | Windows Forms (WinForms) |
| Database | SQLite (`System.Data.SQLite.Core` v1.0.118) |
| IDE khuyến nghị | Visual Studio 2019 / 2022 |
| Giao thức mạng | TCP, UDP Broadcast, UDP Multicast, ICMP |

---

## 📁 Cấu trúc thư mục

```
NurseCallSystem/
│
├── NurseCallSystem.slnx          # Solution file
│
├── NurseServer/                  # Project: Máy chủ quầy Y Tá
│   ├── NurseServer.csproj
│   ├── Program.cs
│   ├── Form1.cs                  # UI chính: bảng giường, bộ lọc, lịch sử
│   ├── Form1.Designer.cs
│   ├── App.config
│   │
│   ├── Net/
│   │   ├── ServerNetworkManager.cs   # Toàn bộ logic mạng phía Server
    │   └── NetworkCrypto.cs          # Mã hóa AES-128-CBC + HMAC-SHA256
│   │
│   ├── Data/
│   │   └── DatabaseHelper.cs         # Tất cả thao tác SQLite
│   │
│   └── Models/
│       ├── PatientBed.cs             # Model giường bệnh
│       └── CallLog.cs                # Model lịch sử cuộc gọi
│
└── PatientClient/                # Project: Thiết bị đầu giường bệnh nhân
    ├── PatientClient.csproj
    ├── Program.cs
    ├── Form1.cs                  # UI: 3 nút bấm + setup phòng/giường
    ├── Form1.Designer.cs
    ├── App.config
    │
    └── Net/
        ├── ClientNetworkManager.cs   # Toàn bộ logic mạng phía Client
        └── NetworkCrypto.cs          # Mã hóa AES-128-CBC + HMAC-SHA256 (bản Client)
```

---

## 💻 Yêu cầu hệ thống

- **OS:** Windows 10 / 11
- **IDE:** Visual Studio 2019+ (có cài workload `.NET desktop development`)
- **.NET Framework:** 4.7.2 (thường đã có sẵn trên Windows 10+)
- **NuGet:** Tự động restore khi build (cần có kết nối Internet lần đầu)
- **Mạng:** Các máy phải cùng mạng LAN; tắt firewall hoặc cho phép ứng dụng qua firewall

---

## 🚀 Cách clone và chạy

### Bước 1 — Clone repository

```bash
git clone <url-repository>
cd NurseCallSystem
```

Hoặc tải về file ZIP và giải nén.

### Bước 2 — Mở solution trong Visual Studio

```
File → Open → Project/Solution → chọn NurseCallSystem.slnx
```

### Bước 3 — Restore NuGet packages

Visual Studio tự động restore khi build. Nếu không tự động:

```
Tools → NuGet Package Manager → Package Manager Console
→ Update-Package -reinstall
```

Hoặc chuột phải vào Solution → **Restore NuGet Packages**.

### Bước 4 — Cấu hình Startup Projects

```
Chuột phải vào Solution → Set Startup Projects...
→ Chọn "Multiple startup projects"
→ NurseServer:    Action = Start
→ PatientClient:  Action = Start
→ OK
```

### Bước 5 — Build và chạy

Nhấn **F5** hoặc **Ctrl+F5**. Hai cửa sổ sẽ khởi động đồng thời.

---

## 📖 Hướng dẫn sử dụng

### Khởi động

1. Hai form xuất hiện: **NurseServer** (quầy y tá) và **PatientClient** (đầu giường)
2. Trên **NurseServer**: nhấn **"Khởi động Server (Auto-Discovery)"**
   - Server tự động reset DB về Offline, sẵn sàng nhận kết nối
3. Trên **PatientClient**: dropdown phòng/giường tự động load sau vài giây
   - Chọn phòng → chọn giường → nhấn **"Lưu & Bắt đầu Kết nối"**
   - Client tự broadcast tìm server, kết nối tự động

### Gửi cảnh báo

- Nhấn **🔴 CẤP CỨU MỞ RỘNG** — Server nhận, phát âm thanh, ô giường chuyển đỏ
- Nhấn **🟡 CẦN THAY DỊCH** — ô giường chuyển cam
- Nhấn **🟢 HỖ TRỢ VỆ SINH** — ô giường chuyển xanh lá

### Xử lý cảnh báo (phía Server)

- Chuột phải vào ô giường đang báo động → **"Đã đến giường xử lý xong"**
- Client nhận tín hiệu RESOLVED → nút bấm tự mở lại

### Báo động toàn viện (Code Blue)

- Nhấn **"BÁO ĐỘNG TOÀN VIỆN"** trên Server
- Toàn bộ màn hình PatientClient chớp đỏ + cảnh báo đồng loạt

### Chạy nhiều máy trong LAN

- Chạy **NurseServer** trên 1 máy (máy chủ)
- Chạy **PatientClient** trên các máy khác trong cùng mạng LAN
- Client tự tìm Server qua UDP Broadcast — **không cần nhập IP thủ công**

> ⚠️ **Lưu ý firewall:** Mở port **50000 (UDP)**, **50001 (TCP)**, **50002 (UDP)** trên máy chạy NurseServer. Hoặc tắt tạm Windows Defender Firewall cho mạng Private khi demo.

---

## 🔌 Giao thức mạng chi tiết

| Port | Giao thức | Hướng | Nội dung (sau giải mã AES) |
|------|-----------|-------|----------------------------|
| 50000 | UDP Broadcast | Client → Server | `NURSE_CALL_DISCOVERY\|room\|bed\|mac` |
| 50000 | UDP Broadcast | Client → Server | `GET_ACTIVE_ROOMS` |
| 50000 | UDP Unicast | Server → Client | `SERVER_ACK` / `BED_TAKEN` / `BED_NOT_FOUND` |
| 50000 | UDP Unicast | Server → Client | `ROOMS\|phong:giuong~phong:giuong...` |
| 50001 | TCP | Client → Server | `ALERT\|{type}\|{room}\|{bed}\|HMAC:{hex}` (AES+Base64) |
| 50001 | TCP | Server → Client | `ACK_ALERT` (AES+Base64) |
| 50002 | UDP Multicast | Server → All Clients | `CODE_BLUE_EVACUATE` |
| 50002 | UDP Multicast | Server → All Clients | `RESOLVED\|{room}\|{bed}` |

**Địa chỉ Multicast:** `239.0.0.1` (dải địa chỉ private multicast)  
**Mã hóa UDP/Multicast:** AES-128-CBC (key + IV 16 bytes cố định)  
**Mã hóa TCP:** AES-128-CBC → Base64 (tương thích `StreamWriter.WriteLine`)  
**Xác thực TCP Alert:** HMAC-SHA256 gắn vào payload trước khi mã hóa — Server reject gói nếu HMAC không khớp

---

## 🗄️ Cơ sở dữ liệu

File SQLite được tạo tự động tại: `NurseServer/bin/Debug/NurseCallSystem.sqlite`

### Bảng `PatientBeds`

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| RoomName | TEXT | Tên phòng (VD: `Phòng 101`) |
| BedName | TEXT | Tên giường (VD: `Giường A`) |
| MacAddress | TEXT | MAC giả để định danh client |
| IpAddress | TEXT | IP hiện tại của client (rỗng = Offline) |
| Status | TEXT | `Offline` / `Normal` / `CẤP CỨU` / `THAY DỊCH TRUYỀN` / `HỖ TRỢ VỆ SINH` |
| LastSeen | DATETIME | Lần cuối giường hoạt động |

### Bảng `CallLogs`

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| Id | INTEGER | Khóa chính tự tăng |
| PatientBedName | TEXT | Định danh giường (`Phòng 101 - Giường A`) |
| CallType | TEXT | Loại cảnh báo |
| RequestTime | DATETIME | Thời điểm bệnh nhân gọi |
| ResolvedTime | DATETIME | Thời điểm y tá xử lý xong (NULL nếu chưa xử lý) |

Mặc định hệ thống seed sẵn **125 giường** (5 tầng × 5 phòng × 5 giường A–E).

---

*Đồ án môn Lập Trình Mạng — .NET Framework 4.7.2 — Windows Forms — 10 Chương kiến thức*
