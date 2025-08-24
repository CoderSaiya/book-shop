# 📚 Book Shop - Hệ thống Quản lý Cửa hàng Sách

Một ứng dụng web hiện đại cho việc quản lý cửa hàng sách trực tuyến, được xây dựng với kiến trúc Clean Architecture và công nghệ tiên tiến.

## 🌟 Tính năng chính

- ✅ **Quản lý sách**: CRUD operations cho sách, danh mục, tác giả
- 🔐 **Xác thực & Phân quyền**: JWT + Refresh Token flow
- 🌐 **Đa ngôn ngữ**: Hỗ trợ song ngữ với Azure Translator
- 💬 **Chat thông minh**: AI-powered chat classification với XLM-RoBERTa
- 📧 **Gửi email**: Tích hợp email notifications
- 🔄 **Message Queue**: RabbitMQ cho xử lý bất đồng bộ
- 🔗 **OAuth2**: Đăng nhập với Google và GitHub
- 📱 **Responsive UI**: Giao diện thân thiện người dùng

## 📂 Cấu trúc thư mục

```
book-shop/
├─ BookShop/                 # Backend .NET
│  ├─ BookShop.API/          # API (Program.cs, controllers, SignalR hub)
│  ├─ BookShop.Application/  # Use cases, interfaces, DTOs
│  ├─ BookShop.Domain/       # Entities, aggregates
│  ├─ BookShop.Infrastructure/# EF Core, Repositories, ML, Mail, RabbitMQ, ...
│  └─ ...
├─ fe/                       # Frontend TypeScript (UI web) :contentReference[oaicite:2]{index=2}
├─ model_train/              # Notebook/scripts train model & export ONNX :contentReference[oaicite:3]{index=3}
├─ UML/                      # Tài liệu, biểu đồ UML (kiến trúc) :contentReference[oaicite:4]{index=4}
├─ LICENSE (MIT)             # Giấy phép mã nguồn mở MIT :contentReference[oaicite:5]{index=5}
└─ README.md
```

## 🚀 Công nghệ sử dụng

### Backend
- **.NET 9** - Web API Framework
- **Entity Framework Core** - ORM cho database operations
- **SQL Server** - Hệ quản trị cơ sở dữ liệu
- **JWT** - JSON Web Tokens cho authentication
- **RabbitMQ** - Message broker
- **Azure Translator** - Dịch thuật tự động
- **OAuth2** - Google & GitHub authentication

### Frontend
- **Angular** - SPA Framework
- **SCSS** - CSS preprocessor
- **Angular i18n** - Internationalization

### AI/ML
- **Python**
- **ONNX** - Open Neural Network Exchange
- **XLM-RoBERTa-Large** - Mô hình transformer đa ngôn ngữ

## 📋 Yêu cầu hệ thống

- **.NET 9 SDK**
- **Node.js 18+**
- **Angular CLI**
- **SQL Server 2019+**
- **Python 3.8+**
- **RabbitMQ Server**

## 🛠️ Cài đặt và Chạy ứng dụng

### 1. Clone Repository
```bash
git clone https://github.com/CoderSaiya/book-shop.git
cd book-shop
```

### 2. Cấu hình Database
```bash
# Tạo database và chạy migrations
cd backend
dotnet ef database update
```

### 3. Cấu hình Backend
```bash
cd BookShop
# Cấu hình appsettings.json với:
# - Connection string cho SQL Server
# - JWT settings
# - Azure Translator keys
# - RabbitMQ connection
# - Email SMTP settings
# - OAuth2 credentials

dotnet restore
dotnet run
```

### 4. Cấu hình Frontend
```bash
cd fe
npm install
ng serve -o
```

### 5. Chạy AI Model Service
```bash
cd model-train
py -3.12 -m venv .venv
pip install -r requirements.txt
python ml/train_intent_hf.py --data ml/intent_train.aug.jsonl --out models/intent_llm --model xlm-roberta-large 
```

### 6. Khởi động RabbitMQ
```bash
# Windows
rabbitmq-server

# Linux/Mac
sudo systemctl start rabbitmq-server

# Docker
docker pull rabbitmq:3.13.7-management
```

## 🔧 Cấu hình

### Backend Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BookShopDB;Trusted_Connection=true;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key",
    "Issuer": "BookShop",
    "Audience": "BookShop-Users",
    "ExpiryMinutes": 60
  },
  "Translator": {
    "Key": "your-azure-translator-key",
    "Region": "your-region",
    "Endpoint": "https://api.cognitive.microsofttranslator.com"
  },
  "RabbitMqSettings": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "email_queue"
  },
  "MailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-password"
  },
  "Auth": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "GitHub": {
      "ClientId": "your-github-client-id",
      "ClientSecret": "your-github-client-secret"
    }
  }
}
```

## 🔐 Authentication Flow

1. **Đăng nhập truyền thống**: Email/Password với JWT
2. **OAuth2 Flow**: Google & GitHub integration
3. **Refresh Token**: Tự động refresh access token
4. **Role-based Access**: Admin, Client roles

## 🤖 AI Chat Classification

Hệ thống sử dụng model XLM-RoBERTa-Large để phân loại tin nhắn chat của người dùng:

- **Intent Classification**: Xác định ý định của người dùng
- **Multilingual Support**: Hỗ trợ đa ngôn ngữ
- **Real-time Processing**: Xử lý thời gian thực
- **ONNX Optimization**: Tối ưu hóa hiệu suất

## 🌐 Internationalization (i18n)

- **Backend**: Azure Translator cho dịch tự động
- **Frontend**: Angular i18n cho giao diện đa ngôn ngữ
- **Supported Languages**: Tiếng Việt, English (có thể mở rộng)

## 📧 Email & Messaging

- **Email Notifications**: Gửi email xác nhận, thông báo
- **RabbitMQ**: Xử lý message queue bất đồng bộ
- **Event-Driven**: Architecture hướng sự kiện

## 📝 API Documentation

API documentation có thể truy cập tại: `[http://localhost:7130/swagger](https://localhost:7130/swagger/index.html)`

### Endpoints chính:
- `GET /api/books` - Lấy danh sách sách
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/register` - Đăng ký
- `GET /api/auth/google` - Google OAuth
- `GET /api/auth/github` - GitHub OAuth
- `POST /api/chat/classify` - Phân loại chat

## 🤝 Đóng góp

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

## 👥 Tác giả

- **CoderSaiya** - *Initial work* - [CoderSaiya](https://github.com/CoderSaiya)

## 🙏 Lời cảm ơn

- Azure Cognitive Services cho translation
- Hugging Face cho XLM-RoBERTa model
- Angular team cho framework tuyệt vời
- Microsoft cho .NET ecosystem

---

⭐ **Don't forget to star this repo if you found it helpful!**
