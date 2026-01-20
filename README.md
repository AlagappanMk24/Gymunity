# 🏋️ Gymunity - A Comprehensive Fitness Management Platform

<div align="center">

![Gymunity](https://img.shields.io/badge/Platform-Fitness%20Management-FF6B6B?style=for-the-badge)
![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Active-success?style=for-the-badge)

**Gymunity** is a cutting-edge, full-stack fitness management and training platform designed to connect trainers and clients in a seamless, integrated ecosystem.

[Features](#-features) • [Architecture](#-architecture) • [Getting Started](#-getting-started) • [API Documentation](#-api-documentation) • [Database Schema](#-database-schema)

</div>

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#-features)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Core Modules](#-core-modules)
- [API Documentation](#-api-documentation)
- [Database Schema](#-database-schema)
- [Key Workflows](#-key-workflows)
- [Contributing](#-contributing)

---

## Overview

Gymunity is an enterprise-grade fitness platform that revolutionizes how trainers and clients interact. Built with clean architecture principles and modern .NET technologies, the platform provides:

- **Complete Training Management**: Create, manage, and deliver personalized workout programs
- **Subscription-Based Model**: Multiple payment gateways for flexible subscription handling
- **Real-Time Communication**: Instant messaging and notifications using SignalR
- **Advanced Analytics**: Comprehensive admin dashboards and performance tracking
- **Multi-Role Support**: Dedicated experiences for Clients, Trainers, and Administrators

---

## 🎯 Features

### 👥 Client Features
- **Program Discovery**: Browse and filter trainer programs
- **Subscription Management**: Subscribe to training packages with multiple payment options
- **Workout Tracking**: Log daily workouts and track progress
- **Body Stats Monitoring**: Record and visualize body measurements and goals
- **Trainer Connection**: Find, review, and communicate with trainers
- **Payment Management**: View payment history and manage subscriptions
- **Real-Time Notifications**: Get instant updates on program changes and messages
- **Profile Management**: Maintain personalized fitness profile with goals and preferences

### 👨‍🏫 Trainer Features
- **Program Creation**: Design comprehensive multi-week training programs
- **Exercise Library Management**: Curate a personal exercise library
- **Client Management**: Track and manage subscribed clients
- **Package Management**: Create and price training packages
- **Performance Analytics**: Monitor earnings, subscriptions, and client progress
- **Direct Communication**: Chat with clients in real-time
- **Review Management**: Monitor and respond to client reviews
- **Payment Tracking**: View earnings and payment history

### 🛡️ Administrator Features
- **Comprehensive Dashboard**: Real-time system analytics and KPIs
- **User Management**: Create, edit, and manage all user accounts
- **Program Oversight**: Monitor and manage all training programs
- **Payment Processing**: Track all transactions and payment statuses
- **Subscription Management**: Handle and monitor all subscriptions
- **Review Moderation**: Approve, reject, and manage platform reviews
- **Analytics & Reports**: Advanced insights on platform metrics
- **User Notifications**: Send announcements to specific user groups

### 💰 Payment Integration
- **Stripe Integration**: Secure payment processing with Stripe
- **PayPal Support**: Alternative payment gateway for flexibility
- **Paymob Gateway**: Regional payment processing support
- **Webhook Security**: Secured webhook handlers for payment confirmations
- **Transaction History**: Complete audit trail of all payments
- **Refund Management**: Handle refunds and payment disputes

### 🔌 Real-Time Features
- **WebSocket Communication**: SignalR-based real-time messaging
- **Live Notifications**: Instant alerts for important events
- **Chat System**: Direct trainer-client messaging with thread support
- **Admin Notifications**: Real-time alerts for admins on critical events

---

## 🏗️ Architecture

The solution follows a **Clean Architecture** pattern with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│        Presentation Layer               │
│  (APIs + Admin.MVC - Razor Pages)      │
└────────────┬────────────────────────────┘
             │
┌────────────▼────────────────────────────┐
│      Application Layer                  │
│  (Services, DTOs, Specifications)       │
└────────────┬────────────────────────────┘
             │
┌────────────▼────────────────────────────┐
│        Domain Layer                     │
│  (Entities, Interfaces, Contracts)      │
└────────────┬────────────────────────────┘
             │
┌────────────▼────────────────────────────┐
│     Infrastructure Layer                │
│  (Data Access, External Services)       │
└─────────────────────────────────────────┘
```

### Design Patterns Used
- **Repository Pattern**: Data access abstraction
- **Unit of Work Pattern**: Transaction management
- **Specification Pattern**: Complex query encapsulation
- **Dependency Injection**: Loose coupling and testability
- **Service Locator Pattern**: Dynamic service resolution
- **Observer Pattern**: Event-driven notifications

---

## 🛠️ Tech Stack

### Backend
- **Framework**: .NET 9.0
- **API**: ASP.NET Core REST API with Swagger/OpenAPI
- **Web Interface**: ASP.NET Core Razor Pages
- **Database**: SQL Server with Entity Framework Core 9.0
- **Authentication**: JWT Bearer Tokens
- **Authorization**: Role-based access control (RBAC)
- **Real-Time**: SignalR WebSockets
- **Mapping**: AutoMapper for DTO transformations
- **Validation**: FluentValidation, DataAnnotations

### External Services
- **Payment Processing**: Stripe, PayPal, Paymob
- **Email**: SMTP email service
- **File Storage**: Cloud file upload service
- **Authentication**: Google OAuth integration
- **Real-Time Communication**: WebSockets via SignalR

### Development Tools
- **ORM**: Entity Framework Core (Code-First approach)
- **Migrations**: EF Core Database Migrations
- **API Documentation**: Swagger UI / OpenAPI
- **Logging**: Microsoft.Extensions.Logging
- **Caching**: In-Memory Cache

---

## 📁 Project Structure

### **Gymunity.Domain** 
Core business logic and domain models
```
├── Models/
│   ├── Identity/          (AppUser, roles)
│   ├── Client/            (ClientProfile, WorkoutLog, BodyStatLog)
│   ├── Trainer/           (TrainerProfile, TrainerReview, Package)
│   ├── ProgramAggregate/  (Program, ProgramWeek, ProgramDay, Exercise)
│   ├── Messaging/         (Message, MessageThread)
│   ├── Enums/             (UserRole, SubscriptionStatus, PaymentMethod, etc.)
│   └── [Other Models]     (Subscription, Payment, Notification)
├── RepositoryContracts/   (IRepository, IProgramRepository, etc.)
├── Specification/         (ISpecification, BaseSpecification)
└── IUnitOfWork.cs         (Transaction management)
```

### **Gymunity.Infrastructure**
Data persistence and external service implementation
```
├── _Data/
│   ├── AppDbContext.cs           (Entity Framework context)
│   ├── AppContextSeed.cs         (Database seeding)
│   ├── Configurations/           (Entity configurations)
│   ├── Migrations/               (Database migrations)
│   └── DbContextMigrationAndSeeding.cs
├── Repositories/                 (Repository implementations)
│   ├── ClientRepositories/
│   ├── ProgramRepository.cs
│   ├── PackageRepository.cs
│   └── [Other repositories]
├── ExternalServices/             (Third-party integrations)
│   ├── StripePaymentService.cs
│   ├── PayPalService.cs
│   ├── EmailService.cs
│   ├── FileUploadService.cs
│   ├── GoogleAuthService.cs
│   ├── SignalRConnectionManager.cs
│   ├── WebhookService.cs
│   └── [Other services]
├── Specification/                (Specification pattern implementation)
│   └── SpecificationEvaluator.cs
├── UnitOfWork.cs                 (Unit of Work implementation)
└── Dependancy Injection/          (DI configuration)
```

### **Gymunity.Application**
Business logic, orchestration, and data transfer objects
```
├── Services/
│   ├── ClientServices/           (Client-specific operations)
│   ├── Admin/                    (Admin services)
│   ├── ReviewTrainerService.cs
│   ├── ReviewClientService.cs
│   ├── PaymentService.cs
│   ├── SubscriptionService.cs
│   ├── NotificationService.cs
│   ├── ChatService.cs
│   └── [Other services]
├── DTOs/                         (Data Transfer Objects)
│   ├── User/                     (Auth, subscription, payment DTOs)
│   ├── Account/                  (Login, register, password reset)
│   ├── Client/                   (Client-specific DTOs)
│   ├── Trainer/                  (Trainer-specific DTOs)
│   ├── Program/                  (Program management DTOs)
│   ├── Messaging/                (Chat and messaging DTOs)
│   ├── Package/                  (Package management DTOs)
│   └── [Other DTOs]
├── Specifications/               (Query specifications)
│   ├── ClientSpecification/
│   ├── Admin/
│   ├── Trainer/
│   ├── Payment/
│   ├── Subscriptions/
│   └── Chat/
├── Contracts/                    (Service interfaces)
│   ├── ExternalServices/         (IEmailService, IFileUploadService, etc.)
│   ├── Services/                 (INotificationService, IChatService, etc.)
│   └── [Other contracts]
├── Configuration/                (App settings, PayPal, Stripe configs)
├── Mapping/                      (AutoMapper profiles and resolvers)
├── Validators/                   (FluentValidation validators)
├── Validation/                   (Custom validation logic)
├── Common/                       (ServiceResult, common utilities)
└── Dependancy Injection/          (Service registration)
```

### **Gymunity.APIs**
RESTful API endpoints and real-time hubs
```
├── Areas/
│   ├── Client/                   (Client API endpoints)
│   │   ├── ClientProfileController.cs
│   │   ├── ClientProgramsController.cs
│   │   ├── ClientTrainersController.cs
│   │   ├── WorkoutLogController.cs
│   │   ├── BodyStateLogController.cs
│   │   ├── OnboardingController.cs
│   │   ├── ReviewClientController.cs
│   │   ├── SubscriptionsController.cs
│   │   └── PaymentsController.cs
│   └── Trainer/                  (Trainer API endpoints)
│       ├── TrainerProfileController.cs
│       ├── ProgramsController.cs
│       ├── PackagesController.cs
│       ├── ReviewTrainerController.cs
│       ├── ExerciseLibraryController.cs
│       ├── DayExercisesController.cs
│       ├── DaysController.cs
│       ├── WeeksController.cs
│       └── ClientsController.cs
├── Controllers/
│   ├── AccountController.cs      (Authentication & authorization)
│   ├── ChatController.cs         (Chat management)
│   ├── NotificationsController.cs (Notifications)
│   ├── GuestReviewController.cs  (Public reviews)
│   ├── ChatBotController.cs      (AI assistance)
│   ├── HomeClientController.cs   (Public home endpoint)
│   ├── WebhooksController.cs     (Payment webhooks)
│   ├── ErrorsController.cs       (Error handling)
│   └── BaseApiController.cs      (Base controller)
├── Hubs/                         (SignalR real-time hubs)
│   ├── ChatHub.cs                (Chat messaging)
│   ├── NotificationHub.cs        (Client notifications)
│   └── AdminNotificationHub.cs   (Admin notifications)
├── Services/
│   ├── AdminNotificationService.cs
│   ├── AdminUserResolverService.cs
│   └── IAdminNotificationService.cs
├── Middlewares/
│   ├── ExceptionMiddleware.cs    (Global error handling)
│   └── WebhookSecurityMiddleware.cs (Webhook security)
├── Extensions/
│   └── ApiInvalidModelStateConfiguration.cs
├── Responses/
│   ├── ApiResponse.cs            (Standard API response)
│   └── Errors/                   (Error response classes)
└── Program.cs                    (App configuration & startup)
```

### **Gymunity.Admin.MVC**
Admin dashboard built with Razor Pages
```
├── Controllers/
│   ├── DashboardController.cs    (Analytics dashboard)
│   ├── UsersController.cs        (User management)
│   ├── ClientsController.cs      (Client management)
│   ├── TrainersController.cs     (Trainer management)
│   ├── ProgramsController.cs     (Program management)
│   ├── SubscriptionsController.cs (Subscription management)
│   ├── PaymentsController.cs     (Payment tracking)
│   ├── ReviewsController.cs      (Review moderation)
│   ├── NotificationsController.cs (Notification management)
│   ├── ChatsController.cs        (Chat management)
│   ├── AnalyticsController.cs    (Advanced analytics)
│   ├── AuthController.cs         (Admin login)
│   └── BaseAdminController.cs    (Base controller)
├── ViewModels/
│   ├── Dashboard/                (Dashboard models)
│   ├── Users/                    (User models)
│   ├── Clients/                  (Client models)
│   ├── Trainers/                 (Trainer models)
│   ├── Programs/                 (Program models)
│   ├── Subscriptions/            (Subscription models)
│   ├── Payments/                 (Payment models)
│   ├── Reviews/                  (Review models)
│   ├── Notifications/            (Notification models)
│   ├── Analytics/                (Analytics models)
│   └── Chat/                     (Chat models)
├── Services/
│   ├── DashboardStatisticsService.cs
│   ├── AnalyticsService.cs
│   ├── AdminNotificationService.cs
│   ├── AdminUserResolverService.cs
│   ├── TrainerNotificationService.cs
│   ├── PaymentNotificationService.cs
│   ├── SubscriptionNotificationService.cs
│   ├── AccountNotificationService.cs
│   ├── UserNotificationService.cs
│   └── [Other services]
├── Hubs/
│   └── AdminNotificationHub.cs   (Real-time notifications)
├── Views/
│   ├── Dashboard/
│   ├── Users/
│   ├── Clients/
│   ├── Trainers/
│   ├── Programs/
│   ├── Subscriptions/
│   ├── Payments/
│   ├── Reviews/
│   ├── Notifications/
│   ├── Analytics/
│   ├── Chats/
│   ├── Auth/
│   └── Shared/                   (Layout & partials)
├── wwwroot/                      (Static files, libraries)
├── Models/
│   └── ErrorViewModel.cs
└── Program.cs                    (App configuration)
```

---

## 🚀 Getting Started

### Prerequisites
- **.NET 9.0 SDK** or later
- **SQL Server** 2019 or later
- **Visual Studio 2022** or VS Code with C# extension
- **Git** for version control

### Installation

1. **Clone the Repository**
   ```bash
   git clone [https://github.com/AlagappanMk24/Gymunity].git
   cd Gymunity
   ```

2. **Configure Database Connection**
   - Update `appsettings.json` in `Gymunity.APIs` project:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=GymunityDB;Integrated Security=true;"
     }
   }
   ```

3. **Configure External Services**
   - Add your Stripe, PayPal, and Paymob credentials to `appsettings.json`:
   ```json
   {
     "Stripe": {
       "SecretKey": "your_stripe_secret",
       "PublishableKey": "your_stripe_publishable"
     },
     "PayPal": {
       "ClientId": "your_paypal_client_id",
       "ClientSecret": "your_paypal_client_secret"
     }
   }
   ```

4. **Setup JWT Authentication**
   - Add JWT settings to `appsettings.json`:
   ```json
   {
     "JWT": {
       "ValidIssuer": "your_issuer",
       "ValidAudience": "your_audience",
       "AuthKey": "your_secret_key_min_32_chars"
     }
   }
   ```

5. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

6. **Apply Database Migrations**
   ```bash
   cd Gymunity.Infrastructure
   dotnet ef database update
   ```

7. **Seed Database** (Optional)
   - The system automatically seeds sample data on first run

8. **Run the APIs**
   ```bash
   cd Gymunity.APIs
   dotnet run
   ```
   - API will be available at `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`

9. **Run the Admin Dashboard**
   ```bash
   cd Gymunity.Admin.MVC
   dotnet run
   ```
   - Admin will be available at `https://localhost:5002`

---

## 📚 Core Modules

### 1. **Authentication & Authorization**
- JWT-based token authentication
- Role-based access control (RBAC)
- Google OAuth integration
- Secure password reset workflow
- Email verification

### 2. **Program Management**
- Create multi-week training programs
- Structure programs with weeks, days, and exercises
- Support for different program types
- Public and private program visibility
- Program cloning and customization

### 3. **Subscription System**
- Package-based subscription model
- Multiple payment gateway support (Stripe, PayPal, Paymob)
- Annual and monthly subscription options
- Subscription lifecycle management
- Automatic renewal handling

### 4. **Payment Processing**
- Secure payment gateway integration
- Webhook handling for payment confirmations
- Transaction history and audit trail
- Refund management
- Currency support (EGP, USD)

### 5. **Messaging System**
- Real-time trainer-client communication
- Message threading for conversation organization
- Unread message tracking
- Secure message delivery via SignalR

### 6. **Fitness Tracking**
- Workout log creation and management
- Body stat tracking (weight, measurements)
- Progress analytics and visualization
- Client goal management

### 7. **Review System**
- Trainer reviews from clients
- Client reviews from trainers
- Guest reviews (public feedback)
- Admin review moderation

### 8. **Notification System**
- Real-time push notifications
- Event-driven architecture
- Admin broadcast notifications
- Email notifications for important events

### 9. **Analytics & Reporting**
- Comprehensive admin dashboard
- Payment analytics and reporting
- Trainer earnings tracking
- Client activity monitoring
- System-wide KPI tracking

---

## 🔌 API Documentation

### Base URL
```
https://localhost:5001/api
```

### Authentication
All requests require JWT Bearer token in the Authorization header:
```
Authorization: Bearer <your_jwt_token>
```

### Main Endpoints

#### **Authentication**
```
POST   /Account/Register           - Register new user
POST   /Account/Login              - Login user
POST   /Account/GoogleAuth         - Google OAuth login
POST   /Account/RefreshToken       - Refresh JWT token
POST   /Account/ForgotPassword     - Request password reset
POST   /Account/ResetPassword      - Reset password
```

#### **Client Profile**
```
GET    /Client/Profile             - Get client profile
PUT    /Client/Profile             - Update client profile
GET    /Client/Trainers            - Discover trainers
GET    /Client/Programs            - Browse programs
GET    /Client/Subscriptions       - Get active subscriptions
```

#### **Trainer Profile**
```
GET    /Trainer/Profile            - Get trainer profile
PUT    /Trainer/Profile            - Update trainer profile
POST   /Trainer/Programs           - Create program
PUT    /Trainer/Programs/{id}      - Update program
GET    /Trainer/Clients            - Get subscribed clients
POST   /Trainer/Packages           - Create package
```

#### **Programs**
```
GET    /Programs                   - Get all programs (paginated)
GET    /Programs/{id}              - Get program details
POST   /Programs                   - Create program
PUT    /Programs/{id}              - Update program
DELETE /Programs/{id}              - Delete program
```

#### **Subscriptions**
```
GET    /Subscriptions              - Get client subscriptions
POST   /Subscriptions              - Subscribe to package
PUT    /Subscriptions/{id}/Cancel  - Cancel subscription
GET    /Subscriptions/{id}         - Get subscription details
```

#### **Payments**
```
POST   /Payments/Initiate          - Initiate payment
GET    /Payments/History           - Get payment history
POST   /Payments/Webhooks/Stripe   - Stripe webhook
POST   /Payments/Webhooks/PayPal   - PayPal webhook
```

#### **Chat**
```
GET    /Chat/Threads              - Get chat threads
POST   /Chat/Threads              - Create thread
POST   /Chat/Messages             - Send message
GET    /Chat/Messages/{threadId}  - Get thread messages
```

#### **Workouts**
```
POST   /Client/WorkoutLog         - Log workout
GET    /Client/WorkoutLog         - Get workout logs
PUT    /Client/WorkoutLog/{id}    - Update workout log
```

#### **Fitness Tracking**
```
POST   /Client/BodyStats          - Record body stats
GET    /Client/BodyStats          - Get body stat history
```

#### **Reviews**
```
POST   /Reviews/Trainer           - Review trainer
POST   /Reviews/Client            - Review client
GET    /Reviews/Trainer/{id}      - Get trainer reviews
```

### SignalR Hubs

#### **ChatHub** - Real-time messaging
```
wss://localhost:5001/hubs/chat

Methods:
- SendMessage(threadId, content)
- ReceiveMessage(sender, content, timestamp)
```

#### **NotificationHub** - Client notifications
```
wss://localhost:5001/hubs/notifications

Methods:
- ReceiveNotification(message, type)
- MarkAsRead(notificationId)
```

#### **AdminNotificationHub** - Admin alerts
```
wss://localhost:5001/hubs/admin-notifications

Methods:
- ReceiveAlert(message, severity)
- PaymentAlert(details)
- SubscriptionAlert(details)
```

---

## 📊 Database Schema

### Core Entities

#### **Users & Profiles**
- `AspNetUsers` - Identity users with roles
- `TrainerProfiles` - Trainer-specific information
- `ClientProfiles` - Client-specific information and goals

#### **Programs & Training**
- `Programs` - Training programs created by trainers
- `ProgramWeeks` - Weekly structures within programs
- `ProgramDays` - Daily workout routines
- `ProgramDayExercises` - Specific exercises in workouts
- `Exercises` - Exercise definitions

#### **Subscriptions & Payments**
- `Packages` - Trainer packages for subscription
- `PackagePrograms` - Programs included in packages
- `Subscriptions` - Client subscriptions to packages
- `Payments` - Payment transactions

#### **Client Tracking**
- `WorkoutLogs` - Workout completion records
- `BodyStatLogs` - Body measurements over time

#### **Communication**
- `MessageThreads` - Conversation threads
- `Messages` - Individual messages in threads
- `Notifications` - System notifications

#### **Reviews & Ratings**
- `TrainerReviews` - Reviews of trainers
- `ClientReviews` - Reviews from clients (if applicable)
- `AdminReviews` - Admin moderation data

---

## 🔄 Key Workflows

### 1. **Client Registration & Onboarding**
```
Register → Email Verification → Complete Profile → Set Goals → Ready to Browse
```

### 2. **Program Subscription**
```
Browse Programs → Select Package → Initiate Payment → Payment Confirmation → 
Activate Subscription → Access Program & Trainer
```

### 3. **Payment Processing**
```
Client Initiates Payment → Choose Gateway → Complete Transaction → 
Webhook Verification → Update Subscription Status → Notification
```

### 4. **Trainer-Client Communication**
```
Client Initiates Chat → Create Thread → Real-time Messaging (SignalR) → 
Message History Maintained
```

### 5. **Workout Logging**
```
Client Logs Workout → Record Sets/Reps → Update Body Stats → 
Generate Progress Report → Share with Trainer
```

### 6. **Review & Moderation**
```
Client Submits Review → Admin Review Queue → Approve/Reject → 
Display on Platform → Trainer Notification
```

---

## 🔐 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Role-Based Access Control**: Fine-grained permission system
- **Password Hashing**: Secure password storage with salting
- **Webhook Verification**: Secure payment webhook validation
- **HTTPS Enforcement**: All communications encrypted
- **CORS Security**: Controlled cross-origin requests
- **SQL Injection Protection**: Parameterized queries via EF Core
- **XSS Prevention**: Input validation and output encoding
- **Rate Limiting**: Configurable API rate limiting

---

## 📈 Performance Considerations

- **Database Indexing**: Optimized queries with proper indexes
- **Caching Strategy**: In-memory caching for frequently accessed data
- **Pagination**: Large result sets are paginated
- **Specification Pattern**: Efficient query composition
- **Async/Await**: Non-blocking I/O operations
- **Connection Pooling**: Optimized database connections

---

## 🐛 Error Handling

The application implements comprehensive error handling:

- **Custom Exception Middleware**: Centralized error processing
- **Validation Error Responses**: Detailed field-level validation errors
- **HTTP Status Codes**: Proper HTTP status code responses
- **Error Logging**: Centralized logging of all errors
- **User-Friendly Messages**: Clear error descriptions for clients

---

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines
- Follow C# naming conventions (PascalCase for public members)
- Use async/await for I/O operations
- Write meaningful commit messages
- Add unit tests for new features
- Document complex logic with comments

---

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 📧 Support & Contact

For issues, questions, or suggestions:
- **GitHub Issues**: [Create an issue](https://github.com/AlagappanMk24/Gymunity/issues)
- **Email**: alagappanmk98@gmail.com

---

## 🎉 Acknowledgments

- Built with ❤️ using .NET 9.0
- Clean Architecture principles
- Clean Code practices
- SOLID principles
- Repository and Unit of Work patterns

---

<div align="center">

**Made with ❤️ by the Gymunity Team**

[⬆ Back to top](#-gymunity---a-comprehensive-fitness-management-platform)

</div>
