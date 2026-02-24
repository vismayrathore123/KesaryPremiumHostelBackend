# 🏨 Kesar Premium — Developer README

> **Hostel Booking Platform** | Indore, Madhya Pradesh, India  
> Tech Stack: .NET Core 8 Web API · SQL Server · HTML/CSS/JS/Bootstrap · JWT · Redis · Serilog

---

## 📌 Table of Contents
1. [Project Overview](#project-overview)
2. [Tech Stack](#tech-stack)
3. [Project Structure](#project-structure)
4. [Getting Started](#getting-started)
5. [Database Setup](#database-setup)
6. [Database Schema Overview](#database-schema-overview)
7. [API Architecture](#api-architecture)
8. [Authentication & Authorization](#authentication--authorization)
9. [Payment Integration](#payment-integration)
10. [Redis Caching](#redis-caching)
11. [Logging (Serilog)](#logging-serilog)
12. [SignalR (Real-time)](#signalr-real-time)
13. [Brochure Generation](#brochure-generation)
14. [Frontend Overview](#frontend-overview)
15. [Admin Panel](#admin-panel)
16. [Deployment Checklist](#deployment-checklist)
17. [Environment Variables](#environment-variables)
18. [Contacts & Support](#contacts--support)

---

## Project Overview

**Kesar Premium** is a full-stack hostel booking website for properties located in Indore, MP, India.

**Hostel Categories:**
- 👦 Boys Hostels
- 👧 Girls Hostels
- 🏘 Independent Rooms (both boys & girls)

**Special Offer:** 30% discount when a user pays 5 months rent at once.

---

## Tech Stack

### Backend
| Component       | Technology                          |
|----------------|-------------------------------------|
| Framework       | .NET Core 8 Web API                 |
| Database        | SQL Server 2019+                    |
| Authentication  | JWT Bearer Tokens + Refresh Tokens  |
| Caching         | Redis (StackExchange.Redis)         |
| Logging         | Serilog (console + file + seq)      |
| Background Jobs | Hangfire (optional)                 |
| Real-time       | SignalR (optional)                  |
| Unit Testing    | xUnit + Moq                         |
| Payments        | Stripe **or** PayU                  |
| PDF Generation  | DinkToPdf / QuestPDF                |

### Frontend
| Component       | Technology                          |
|----------------|-------------------------------------|
| Base            | HTML5, CSS3, JavaScript             |
| Framework       | Bootstrap 5                         |
| jQuery          | 3.x                                 |
| Animations      | AOS.js                              |
| Sliders         | Swiper.js / Owl Carousel            |
| Alerts          | SweetAlert2                         |
| Dropdowns       | Select2                             |
| Image Preview   | Lightbox2                           |
| Fonts           | Google Fonts (Poppins, Inter)       |

---

## Project Structure

```
KesarPremium/
├── KesarPremium.API/              # Main Web API project
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── HostelController.cs
│   │   ├── RoomController.cs
│   │   ├── BookingController.cs
│   │   ├── PaymentController.cs
│   │   ├── EnquiryController.cs
│   │   └── AdminController.cs
│   ├── Middleware/
│   │   └── GlobalExceptionMiddleware.cs
│   ├── Hubs/                      # SignalR Hubs
│   │   └── BookingHub.cs
│   └── Program.cs
│
├── KesarPremium.Core/             # Business logic
│   ├── Entities/                  # EF Core / DB models
│   ├── DTOs/
│   │   ├── Request/
│   │   └── Response/
│   ├── Interfaces/
│   │   ├── IRepository/
│   │   └── IService/
│   └── Constants/
│
├── KesarPremium.Infrastructure/   # Data access
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   ├── Repositories/
│   └── Services/
│
├── KesarPremium.Tests/            # xUnit tests
│   ├── Services/
│   ├── Repositories/
│   └── Controllers/
│
├── Frontend/                      # Static HTML/CSS/JS site
│   ├── index.html
│   ├── hostels.html
│   ├── hostel-detail.html
│   ├── booking.html
│   ├── login.html
│   ├── register.html
│   ├── dashboard.html
│   ├── admin/
│   │   ├── dashboard.html
│   │   ├── hostels.html
│   │   ├── bookings.html
│   │   └── enquiries.html
│   ├── css/
│   ├── js/
│   └── assets/
│
├── Database/
│   └── KesarPremium_DatabaseSchema.sql
│
└── README.md
```

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/)
- SQL Server 2019+ (or SQL Server Express)
- Redis Server (local or via Docker)
- Node.js (optional, for frontend tooling)

### 1. Clone & Restore
```bash
git clone https://github.com/your-org/kesar-premium.git
cd kesar-premium
dotnet restore
```

### 2. Configure Environment
Copy the sample config and update values:
```bash
cp appsettings.example.json appsettings.Development.json
```
Edit `appsettings.Development.json` — see [Environment Variables](#environment-variables).

### 3. Run the API
```bash
cd KesarPremium.API
dotnet run
```
API will be available at: `https://localhost:5001`  
Swagger UI: `https://localhost:5001/swagger`

### 4. Run Tests
```bash
dotnet test
```

---

## Database Setup

### Step 1: Create the database
Run the full schema file in SQL Server Management Studio (SSMS) or via CLI:
```sql
-- In SSMS, open and execute:
Database/KesarPremium_DatabaseSchema.sql
```

Or via CLI:
```bash
sqlcmd -S localhost -E -i Database/KesarPremium_DatabaseSchema.sql
```

### Step 2: Update connection string
In `appsettings.Development.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=KesarPremiumDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Step 3: Seed admin password
The seed script inserts a placeholder hash. Run this in your app startup or manually update:
```sql
UPDATE Users
SET PasswordHash = '<BCRYPT_HASH>'
WHERE Email = 'admin@kesarpremium.com';
```
Generate a bcrypt hash using any online tool or in C# with `BCrypt.Net-Next`.

---

## Database Schema Overview

### Tables (20 total)

| Table               | Purpose                                         |
|--------------------|-------------------------------------------------|
| `Roles`             | User roles (Admin, User)                        |
| `Users`             | Registered users with hashed passwords         |
| `Locations`         | Areas within Indore (Vijay Nagar, Palasia…)    |
| `HostelCategories`  | Boys / Girls / Independent                      |
| `Hostels`           | Hostel master data                              |
| `Facilities`        | Facility master (WiFi, CCTV, Food…)            |
| `HostelFacilities`  | Many-to-many: hostel ↔ facilities              |
| `RoomTypes`         | Single / Double / Triple / Dormitory            |
| `Rooms`             | Individual rooms with bed availability          |
| `HostelImages`      | Images for hostels and rooms                    |
| `PricingPlans`      | Rent plans with discount rules                  |
| `Bookings`          | User bookings with status                       |
| `Payments`          | Payment records (Stripe/PayU)                   |
| `Enquiries`         | User enquiries visible to admin                 |
| `SharedAreas`       | Common areas (kitchen, common room)             |
| `SharedAreaImages`  | Images for shared areas                         |
| `Brochures`         | Downloadable PDF brochures per hostel           |
| `Notifications`     | In-app notifications for users                  |
| `RefreshTokens`     | JWT refresh token storage                       |
| `AuditLogs`         | Admin action audit trail                        |

### Views (5)
| View                   | Description                              |
|-----------------------|------------------------------------------|
| `vw_HostelListing`     | Hostel list with availability & rent     |
| `vw_RoomAvailability`  | Room-level availability                  |
| `vw_BookingSummary`    | Full booking info with payment           |
| `vw_AdminEnquiries`    | Enquiry panel with user contact info     |
| `vw_RevenueDashboard`  | Monthly revenue by hostel                |

### Stored Procedures (8)
| SP                        | Description                                |
|--------------------------|--------------------------------------------|
| `sp_RegisterUser`         | Register user + auto-create enquiry        |
| `sp_CreateBooking`        | Create booking with discount calc          |
| `sp_UpdateBookingStatus`  | Confirm/Reject booking (admin)             |
| `sp_RecordPayment`        | Record payment + auto-confirm booking      |
| `sp_SearchHostels`        | Filtered, paginated hostel search          |
| `sp_GetUserDashboard`     | User bookings + notifications              |
| `sp_GetAdminDashboardStats` | KPI stats for admin                      |
| `sp_GetHostelDetail`      | Full hostel detail with rooms/facilities   |

### Functions (4)
| Function                     | Type          | Description                          |
|-----------------------------|---------------|--------------------------------------|
| `fn_CalculateRent`           | Scalar        | Calc rent with 30% discount for 5mo  |
| `fn_GenerateBookingNumber`   | Scalar        | Generate KP-YYYYNNNNN number         |
| `fn_GetHostelsByCategory`    | Table-Valued  | Filter hostels by category name      |
| `fn_GetAvailableRooms`       | Table-Valued  | Available rooms for a hostel         |

---

## API Architecture

### Layered Architecture
```
Controller → IService → Service → IRepository → Repository → DbContext → SQL Server
```

### Key Design Patterns
- **Repository Pattern** with generic `IRepository<T>`
- **Service Layer** for all business logic
- **DTO pattern** — never expose entity models directly to API consumers
- **Global Exception Middleware** — all unhandled exceptions return a standard `ApiResponse<T>`

### Standard API Response
```json
{
  "success": true,
  "message": "Hostel retrieved successfully.",
  "data": { ... },
  "errors": null
}
```

---

## Authentication & Authorization

- JWT Bearer token issued on login
- Refresh token stored in `RefreshTokens` table
- Token expiry: **Access = 15 minutes**, **Refresh = 7 days**
- Roles: `Admin`, `User`
- Protected routes use `[Authorize(Roles = "Admin")]` or `[Authorize]`

### Token Flow
```
POST /api/auth/login         → returns accessToken + refreshToken
POST /api/auth/refresh-token → returns new accessToken
POST /api/auth/logout        → revokes refreshToken
```

---

## Payment Integration

### Stripe Setup
1. Create Stripe account → get `SecretKey` and `PublishableKey`
2. Add keys to `appsettings.json`
3. Implement `PaymentController.CreatePaymentIntent` endpoint
4. Frontend calls Stripe.js with `PublishableKey`
5. On success, call `sp_RecordPayment` stored procedure

### PayU Setup (Alternative for India)
1. Get Merchant Key & Salt from PayU dashboard
2. Generate hash: `key|txnid|amount|productinfo|firstname|email|||||||||||salt`
3. POST to PayU endpoint
4. Handle success/failure callback in `PaymentController`

> ⚠️ **Never log raw gateway responses containing card data.** Use `[Sensitive]` attribute or mask in Serilog.

---

## Redis Caching

### Cached Keys
| Key Pattern                      | TTL    | Data                     |
|---------------------------------|--------|--------------------------|
| `hostel:list:all`                | 10 min | All active hostels       |
| `hostel:{id}:detail`             | 5 min  | Single hostel detail     |
| `hostel:category:{name}`         | 10 min | Hostels by category      |
| `hostel:location:{locationId}`   | 10 min | Hostels by location      |
| `room:{hostelId}:available`      | 2 min  | Available rooms          |

### Cache Invalidation
- Invalidate `hostel:*` keys whenever admin updates a hostel, room, or pricing.
- Use `IHostelService.InvalidateHostelCache(hostelId)` helper.

### Docker (local dev)
```bash
docker run -d -p 6379:6379 redis:latest
```

---

## Logging (Serilog)

Serilog is configured in `Program.cs`:
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/kesar-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341")   // Optional Seq server
    .Enrich.FromLogContext()
    .CreateLogger();
```

Log levels:
- `Information` — normal operations
- `Warning` — unexpected but handled situations
- `Error` — caught exceptions
- `Fatal` — unrecoverable errors

---

## SignalR (Real-time)

Hub: `BookingHub` at `/hubs/booking`

### Events
| Event Name             | Direction        | Payload                      |
|-----------------------|-----------------|------------------------------|
| `RoomAvailabilityUpdate` | Server → Client | `{ roomId, availableBeds }` |
| `BookingStatusUpdate`   | Server → Client | `{ bookingId, status }`     |
| `NewEnquiry`            | Server → Admin  | `{ enquiryId, userName }`   |

Frontend connection:
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/booking", { accessTokenFactory: () => getToken() })
    .build();
connection.on("RoomAvailabilityUpdate", (data) => { /* update UI */ });
await connection.start();
```

---

## Brochure Generation

Each hostel has a downloadable PDF brochure containing:
- Hostel images
- Facilities list
- Pricing table
- Location map link
- Contact details (📞 12345678)

### Library: QuestPDF
```csharp
// BrochureService.cs
Document.Create(container => {
    container.Page(page => {
        page.Content().Column(col => {
            col.Item().Text(hostel.HostelName).Bold().FontSize(24);
            // ... add images, tables, etc.
        });
    });
}).GeneratePdf(outputPath);
```

Brochure file paths stored in `Brochures` table. Admin can regenerate via admin panel.

---

## Frontend Overview

### Pages
| Page               | File                | Description                          |
|-------------------|---------------------|--------------------------------------|
| Home              | `index.html`        | Hero, featured hostels, features     |
| Hostel Listing    | `hostels.html`      | Filter by category, location, price  |
| Hostel Detail     | `hostel-detail.html`| Images, facilities, rooms, booking   |
| Booking           | `booking.html`      | Seat selection + payment             |
| Login / Register  | `login.html`        | JWT auth forms                       |
| User Dashboard    | `dashboard.html`    | Bookings, notifications              |

### Key JS Files
- `api.js` — Axios/Fetch wrapper for all API calls
- `auth.js` — Login, register, token storage, logout
- `booking.js` — Seat selection, payment flow
- `hostel.js` — Hostel listing, filtering, detail

> Store JWT in `sessionStorage` (not `localStorage`) for better security.

### WhatsApp Chat
```html
<a href="https://wa.me/9112345678?text=Hello%20Kesar%20Premium" target="_blank">
  <i class="fab fa-whatsapp"></i> Chat on WhatsApp
</a>
```

---

## Admin Panel

Admin pages are in `Frontend/admin/`. Protect all admin routes with role check on page load:
```javascript
// admin/dashboard.js
const role = parseJwt(getToken()).role;
if (role !== 'Admin') window.location.href = '/login.html';
```

### Admin Features
- View KPI dashboard (users, bookings, revenue, enquiries)
- CRUD: Hostels, Rooms, Images, Pricing, Facilities
- Manage reservations (approve/reject)
- View & action user enquiries + call follow-up
- Generate/update hostel brochures
- Manage shared area content

---

## Deployment Checklist

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Use HTTPS with valid SSL certificate
- [ ] Hash all passwords with BCrypt (never store plain text)
- [ ] Set JWT secret to 256-bit random string
- [ ] Configure CORS to allow only your frontend domain
- [ ] Enable Redis on production server
- [ ] Configure Serilog to write to file + monitoring (Seq/ELK)
- [ ] Test Stripe/PayU webhook endpoints
- [ ] Run `dotnet test` — all tests passing
- [ ] Enable SQL Server backups
- [ ] Set up Hangfire dashboard (admin only)
- [ ] Test SignalR in production environment

---

## Environment Variables

`appsettings.json` template:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=KesarPremiumDB;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_256_BIT_SECRET_KEY",
    "Issuer": "KesarPremium",
    "Audience": "KesarPremiumUsers",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "PayU": {
    "MerchantKey": "YOUR_MERCHANT_KEY",
    "Salt": "YOUR_SALT",
    "BaseUrl": "https://sandboxsecure.payu.in/_payment"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [{ "Name": "Console" }]
  },
  "Hangfire": {
    "DashboardPath": "/admin/jobs"
  },
  "AdminContact": {
    "Phone": "12345678",
    "WhatsApp": "9112345678",
    "Email": "admin@kesarpremium.com"
  }
}
```

---

## Contacts & Support

| Role              | Contact             |
|------------------|---------------------|
| Admin Phone      | 📞 12345678          |
| Admin Email      | admin@kesarpremium.com |
| WhatsApp Support | 9112345678           |
| Location         | Indore, MP, India   |

---

*Last Updated: February 2026 | Version 1.0*
