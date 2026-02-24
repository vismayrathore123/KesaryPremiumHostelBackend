-- ============================================================
-- KESAR PREMIUM - FULL DATABASE SCHEMA
-- SQL Server 2019+  |  KesarPremiumDB
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'KesarPremiumDB')
    CREATE DATABASE KesarPremiumDB;
GO

USE KesarPremiumDB;
GO

-- ============================================================
-- SECTION 1: TABLES
-- ============================================================

-- 1.1 Roles
CREATE TABLE Roles (
    RoleId    INT IDENTITY(1,1) PRIMARY KEY,
    RoleName  NVARCHAR(50) NOT NULL UNIQUE,  -- 'Admin', 'User'
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- 1.2 Users
CREATE TABLE Users (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(150) NOT NULL,
    Email           NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(500) NOT NULL,
    PhoneNumber     NVARCHAR(20),
    RoleId          INT NOT NULL DEFAULT 2,
    IsActive        BIT DEFAULT 1,
    IsEmailVerified BIT DEFAULT 0,
    ProfilePicture  NVARCHAR(500),
    CreatedAt       DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- 1.3 Locations (areas within Indore)
CREATE TABLE Locations (
    LocationId   INT IDENTITY(1,1) PRIMARY KEY,
    LocationName NVARCHAR(150) NOT NULL,
    Address      NVARCHAR(500),
    City         NVARCHAR(100) DEFAULT 'Indore',
    State        NVARCHAR(100) DEFAULT 'Madhya Pradesh',
    PinCode      NVARCHAR(10),
    Latitude     DECIMAL(9,6),
    Longitude    DECIMAL(9,6),
    IsActive     BIT DEFAULT 1,
    CreatedAt    DATETIME2 DEFAULT GETUTCDATE()
);

-- 1.4 HostelCategories
CREATE TABLE HostelCategories (
    CategoryId   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,  -- 'Boys', 'Girls', 'Independent'
    Description  NVARCHAR(500),
    IsActive     BIT DEFAULT 1
);

-- 1.5 Hostels
CREATE TABLE Hostels (
    HostelId       INT IDENTITY(1,1) PRIMARY KEY,
    HostelName     NVARCHAR(200) NOT NULL,
    LocationId     INT NOT NULL,
    CategoryId     INT NOT NULL,
    Description    NVARCHAR(2000),
    ContactNumber  NVARCHAR(20),
    WhatsAppNumber NVARCHAR(20),
    TotalRooms     INT DEFAULT 0,
    TotalBeds      INT DEFAULT 0,
    IsActive       BIT DEFAULT 1,
    CreatedAt      DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2,
    CONSTRAINT FK_Hostels_Locations   FOREIGN KEY (LocationId)  REFERENCES Locations(LocationId),
    CONSTRAINT FK_Hostels_Categories  FOREIGN KEY (CategoryId)  REFERENCES HostelCategories(CategoryId)
);

-- 1.6 Facilities (master list)
CREATE TABLE Facilities (
    FacilityId   INT IDENTITY(1,1) PRIMARY KEY,
    FacilityName NVARCHAR(150) NOT NULL,
    IconClass    NVARCHAR(100),
    IsActive     BIT DEFAULT 1
);

-- 1.7 HostelFacilities (many-to-many)
CREATE TABLE HostelFacilities (
    HostelFacilityId INT IDENTITY(1,1) PRIMARY KEY,
    HostelId         INT NOT NULL,
    FacilityId       INT NOT NULL,
    CONSTRAINT FK_HF_Hostel    FOREIGN KEY (HostelId)   REFERENCES Hostels(HostelId) ON DELETE CASCADE,
    CONSTRAINT FK_HF_Facility  FOREIGN KEY (FacilityId) REFERENCES Facilities(FacilityId),
    CONSTRAINT UQ_HostelFacility UNIQUE (HostelId, FacilityId)
);

-- 1.8 RoomTypes
CREATE TABLE RoomTypes (
    RoomTypeId  INT IDENTITY(1,1) PRIMARY KEY,
    TypeName    NVARCHAR(100) NOT NULL,  -- 'Single','Double','Triple','Dormitory'
    BedCount    INT NOT NULL DEFAULT 1,
    Description NVARCHAR(500)
);

-- 1.9 Rooms
CREATE TABLE Rooms (
    RoomId        INT IDENTITY(1,1) PRIMARY KEY,
    HostelId      INT NOT NULL,
    RoomTypeId    INT NOT NULL,
    RoomNumber    NVARCHAR(20) NOT NULL,
    FloorNumber   INT DEFAULT 0,
    TotalBeds     INT NOT NULL,
    AvailableBeds INT NOT NULL,
    MonthlyRent   DECIMAL(10,2) NOT NULL,
    IsActive      BIT DEFAULT 1,
    CreatedAt     DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2,
    CONSTRAINT FK_Rooms_Hostels    FOREIGN KEY (HostelId)   REFERENCES Hostels(HostelId) ON DELETE CASCADE,
    CONSTRAINT FK_Rooms_RoomTypes  FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(RoomTypeId)
);

-- 1.10 HostelImages
CREATE TABLE HostelImages (
    ImageId      INT IDENTITY(1,1) PRIMARY KEY,
    HostelId     INT NOT NULL,
    RoomId       INT NULL,           -- NULL = hostel-level image
    ImageUrl     NVARCHAR(500) NOT NULL,
    AltText      NVARCHAR(200),
    IsPrimary    BIT DEFAULT 0,
    DisplayOrder INT DEFAULT 0,
    CreatedAt    DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Images_Hostels FOREIGN KEY (HostelId) REFERENCES Hostels(HostelId) ON DELETE CASCADE,
    CONSTRAINT FK_Images_Rooms   FOREIGN KEY (RoomId)   REFERENCES Rooms(RoomId)
);

-- 1.11 PricingPlans
CREATE TABLE PricingPlans (
    PlanId          INT IDENTITY(1,1) PRIMARY KEY,
    HostelId        INT NOT NULL,
    RoomTypeId      INT NOT NULL,
    PlanName        NVARCHAR(100) NOT NULL,  -- 'Monthly', '5-Month Offer'
    DurationMonths  INT NOT NULL,
    BaseRent        DECIMAL(10,2) NOT NULL,
    DiscountPercent DECIMAL(5,2) DEFAULT 0,
    FinalRent       AS (BaseRent * DurationMonths * (1 - DiscountPercent / 100.0)) PERSISTED,
    IsActive        BIT DEFAULT 1,
    CreatedAt       DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Pricing_Hostels    FOREIGN KEY (HostelId)   REFERENCES Hostels(HostelId) ON DELETE CASCADE,
    CONSTRAINT FK_Pricing_RoomTypes  FOREIGN KEY (RoomTypeId) REFERENCES RoomTypes(RoomTypeId)
);

-- 1.12 Bookings
CREATE TABLE Bookings (
    BookingId     INT IDENTITY(1,1) PRIMARY KEY,
    UserId        INT NOT NULL,
    RoomId        INT NOT NULL,
    PlanId        INT NULL,
    BookingNumber NVARCHAR(50) NOT NULL UNIQUE,    -- KP-20240001
    CheckInDate   DATE NOT NULL,
    CheckOutDate  DATE NULL,
    TotalAmount   DECIMAL(10,2) NOT NULL,
    DiscountAmount DECIMAL(10,2) DEFAULT 0,
    FinalAmount   DECIMAL(10,2) NOT NULL,
    BookingStatus NVARCHAR(30) DEFAULT 'Pending',  -- Pending,Confirmed,Rejected,Cancelled,CheckedOut
    PaymentStatus NVARCHAR(30) DEFAULT 'Unpaid',   -- Unpaid,Paid,Refunded
    AdminNotes    NVARCHAR(1000),
    CreatedAt     DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2,
    CONSTRAINT FK_Bookings_Users  FOREIGN KEY (UserId)  REFERENCES Users(UserId),
    CONSTRAINT FK_Bookings_Rooms  FOREIGN KEY (RoomId)  REFERENCES Rooms(RoomId),
    CONSTRAINT FK_Bookings_Plans  FOREIGN KEY (PlanId)  REFERENCES PricingPlans(PlanId)
);

-- 1.13 Payments
CREATE TABLE Payments (
    PaymentId       INT IDENTITY(1,1) PRIMARY KEY,
    BookingId       INT NOT NULL,
    UserId          INT NOT NULL,
    TransactionId   NVARCHAR(200),          -- Stripe / PayU TX ID
    PaymentGateway  NVARCHAR(50),           -- 'Stripe', 'PayU'
    Amount          DECIMAL(10,2) NOT NULL,
    Currency        NVARCHAR(10) DEFAULT 'INR',
    PaymentStatus   NVARCHAR(30) NOT NULL,  -- Success,Failed,Pending,Refunded
    PaymentMethod   NVARCHAR(100),          -- Card,UPI,NetBanking
    GatewayResponse NVARCHAR(MAX),
    PaidAt          DATETIME2,
    CreatedAt       DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Payments_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId),
    CONSTRAINT FK_Payments_Users    FOREIGN KEY (UserId)    REFERENCES Users(UserId)
);

-- 1.14 Enquiries
CREATE TABLE Enquiries (
    EnquiryId     INT IDENTITY(1,1) PRIMARY KEY,
    UserId        INT NOT NULL,
    HostelId      INT NULL,
    Subject       NVARCHAR(300),
    Message       NVARCHAR(2000),
    EnquiryStatus NVARCHAR(30) DEFAULT 'New',  -- New,InProgress,Resolved,Closed
    AdminNotes    NVARCHAR(1000),
    FollowUpDate  DATE,
    CreatedAt     DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2,
    CONSTRAINT FK_Enquiries_Users   FOREIGN KEY (UserId)   REFERENCES Users(UserId),
    CONSTRAINT FK_Enquiries_Hostels FOREIGN KEY (HostelId) REFERENCES Hostels(HostelId)
);

-- 1.15 SharedAreas
CREATE TABLE SharedAreas (
    SharedAreaId INT IDENTITY(1,1) PRIMARY KEY,
    HostelId     INT NOT NULL,
    AreaName     NVARCHAR(150) NOT NULL,  -- 'Kitchen','Common Room','Terrace'
    Description  NVARCHAR(1000),
    IsActive     BIT DEFAULT 1,
    CreatedAt    DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2,
    CONSTRAINT FK_SharedAreas_Hostels FOREIGN KEY (HostelId) REFERENCES Hostels(HostelId) ON DELETE CASCADE
);

-- 1.16 SharedAreaImages
CREATE TABLE SharedAreaImages (
    ImageId      INT IDENTITY(1,1) PRIMARY KEY,
    SharedAreaId INT NOT NULL,
    ImageUrl     NVARCHAR(500) NOT NULL,
    AltText      NVARCHAR(200),
    DisplayOrder INT DEFAULT 0,
    CONSTRAINT FK_SAImages_SharedAreas FOREIGN KEY (SharedAreaId) REFERENCES SharedAreas(SharedAreaId) ON DELETE CASCADE
);

-- 1.17 Brochures
CREATE TABLE Brochures (
    BrochureId   INT IDENTITY(1,1) PRIMARY KEY,
    HostelId     INT NOT NULL,
    BrochureName NVARCHAR(200) NOT NULL,
    FilePath     NVARCHAR(500) NOT NULL,
    FileSize     BIGINT,
    Version      INT DEFAULT 1,
    IsActive     BIT DEFAULT 1,
    GeneratedAt  DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Brochures_Hostels FOREIGN KEY (HostelId) REFERENCES Hostels(HostelId) ON DELETE CASCADE
);

-- 1.18 Notifications
CREATE TABLE Notifications (
    NotificationId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId           INT NULL,
    Title            NVARCHAR(200) NOT NULL,
    Message          NVARCHAR(1000) NOT NULL,
    NotificationType NVARCHAR(50),  -- BookingConfirmed,PaymentReceived,AdminAlert
    IsRead           BIT DEFAULT 0,
    ReadAt           DATETIME2,
    CreatedAt        DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Notif_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- 1.19 RefreshTokens (JWT)
CREATE TABLE RefreshTokens (
    TokenId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT NOT NULL,
    Token     NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_RT_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- 1.20 AuditLogs
CREATE TABLE AuditLogs (
    LogId     INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT NULL,
    Action    NVARCHAR(100) NOT NULL,
    TableName NVARCHAR(100),
    RecordId  INT,
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    IPAddress NVARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

-- ============================================================
-- SECTION 2: VIEWS
-- ============================================================

-- 2.1 Hostel listing with summary
CREATE OR ALTER VIEW vw_HostelListing AS
SELECT
    h.HostelId,
    h.HostelName,
    h.Description,
    h.ContactNumber,
    h.WhatsAppNumber,
    h.TotalRooms,
    h.TotalBeds,
    h.LocationId,
    h.CategoryId,
    l.LocationName,
    l.Address,
    l.City,
    l.PinCode,
    hc.CategoryName,
    ISNULL(SUM(r.AvailableBeds), 0) AS TotalAvailableBeds,
    MIN(r.MonthlyRent)              AS StartingRentFrom,
    (SELECT TOP 1 ImageUrl
     FROM HostelImages i
     WHERE i.HostelId = h.HostelId AND i.IsPrimary = 1) AS PrimaryImage
FROM Hostels h
INNER JOIN Locations       l  ON h.LocationId  = l.LocationId
INNER JOIN HostelCategories hc ON h.CategoryId = hc.CategoryId
LEFT  JOIN Rooms r ON r.HostelId = h.HostelId AND r.IsActive = 1
WHERE h.IsActive = 1
GROUP BY
    h.HostelId, h.HostelName, h.Description, h.ContactNumber,
    h.WhatsAppNumber, h.TotalRooms, h.TotalBeds, h.LocationId, h.CategoryId,
    l.LocationName, l.Address, l.City, l.PinCode, hc.CategoryName;
GO

-- 2.2 Room availability
CREATE OR ALTER VIEW vw_RoomAvailability AS
SELECT
    r.RoomId,
    r.RoomNumber,
    r.FloorNumber,
    r.TotalBeds,
    r.AvailableBeds,
    r.MonthlyRent,
    h.HostelId,
    h.HostelName,
    hc.CategoryName,
    rt.TypeName AS RoomTypeName,
    l.LocationName,
    CASE WHEN r.AvailableBeds > 0 THEN 'Available' ELSE 'Full' END AS AvailabilityStatus
FROM Rooms r
INNER JOIN Hostels          h  ON r.HostelId   = h.HostelId
INNER JOIN HostelCategories hc ON h.CategoryId = hc.CategoryId
INNER JOIN RoomTypes        rt ON r.RoomTypeId = rt.RoomTypeId
INNER JOIN Locations        l  ON h.LocationId = l.LocationId
WHERE r.IsActive = 1 AND h.IsActive = 1;
GO

-- 2.3 Booking summary (user + room + payment)
CREATE OR ALTER VIEW vw_BookingSummary AS
SELECT
    b.BookingId,
    b.BookingNumber,
    b.BookingStatus,
    b.PaymentStatus,
    b.CheckInDate,
    b.CheckOutDate,
    b.TotalAmount,
    b.DiscountAmount,
    b.FinalAmount,
    b.CreatedAt   AS BookingDate,
    b.AdminNotes,
    u.UserId,
    u.FullName    AS UserName,
    u.Email       AS UserEmail,
    u.PhoneNumber AS UserPhone,
    r.RoomId,
    r.RoomNumber,
    h.HostelId,
    h.HostelName,
    hc.CategoryName,
    l.LocationName,
    p.TransactionId,
    p.PaymentGateway,
    p.Amount      AS PaidAmount,
    p.PaidAt
FROM Bookings b
INNER JOIN Users            u  ON b.UserId    = u.UserId
INNER JOIN Rooms            r  ON b.RoomId    = r.RoomId
INNER JOIN Hostels          h  ON r.HostelId  = h.HostelId
INNER JOIN HostelCategories hc ON h.CategoryId= hc.CategoryId
INNER JOIN Locations        l  ON h.LocationId= l.LocationId
LEFT  JOIN Payments         p  ON p.BookingId = b.BookingId AND p.PaymentStatus = 'Success';
GO

-- 2.4 Admin enquiry panel
CREATE OR ALTER VIEW vw_AdminEnquiries AS
SELECT
    e.EnquiryId,
    e.Subject,
    e.Message,
    e.EnquiryStatus,
    e.AdminNotes,
    e.FollowUpDate,
    e.CreatedAt,
    e.UpdatedAt,
    u.FullName    AS UserName,
    u.Email       AS UserEmail,
    u.PhoneNumber AS UserPhone,
    h.HostelName
FROM Enquiries e
INNER JOIN Users   u ON e.UserId   = u.UserId
LEFT  JOIN Hostels h ON e.HostelId = h.HostelId;
GO

-- 2.5 Revenue dashboard
CREATE OR ALTER VIEW vw_RevenueDashboard AS
SELECT
    YEAR(p.PaidAt)  AS PaymentYear,
    MONTH(p.PaidAt) AS PaymentMonth,
    h.HostelId,
    h.HostelName,
    hc.CategoryName,
    COUNT(p.PaymentId) AS TotalTransactions,
    SUM(p.Amount)      AS TotalRevenue
FROM Payments p
INNER JOIN Bookings          b  ON p.BookingId  = b.BookingId
INNER JOIN Rooms             r  ON b.RoomId     = r.RoomId
INNER JOIN Hostels           h  ON r.HostelId   = h.HostelId
INNER JOIN HostelCategories  hc ON h.CategoryId = hc.CategoryId
WHERE p.PaymentStatus = 'Success'
GROUP BY YEAR(p.PaidAt), MONTH(p.PaidAt), h.HostelId, h.HostelName, hc.CategoryName;
GO

-- ============================================================
-- SECTION 3: FUNCTIONS
-- ============================================================

-- 3.1 Scalar: Calculate rent with optional 30% discount for 5+ months
CREATE OR ALTER FUNCTION fn_CalculateRent (
    @MonthlyRent DECIMAL(10,2),
    @Months      INT
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Discount DECIMAL(5,2) = 0;
    IF @Months >= 5 SET @Discount = 30;
    RETURN @MonthlyRent * @Months * (1 - @Discount / 100.0);
END;
GO

-- 3.2 Scalar: Generate booking number
CREATE OR ALTER FUNCTION fn_GenerateBookingNumber (@BookingId INT)
RETURNS NVARCHAR(50)
AS
BEGIN
    RETURN 'KP-' + FORMAT(GETUTCDATE(), 'yyyy') + RIGHT('00000' + CAST(@BookingId AS NVARCHAR), 5);
END;
GO

-- 3.3 Table-Valued: Get hostels by category
CREATE OR ALTER FUNCTION fn_GetHostelsByCategory (@CategoryName NVARCHAR(100))
RETURNS TABLE
AS
RETURN (
    SELECT h.*, l.LocationName, l.Address, l.PinCode
    FROM Hostels h
    INNER JOIN Locations        l  ON h.LocationId  = l.LocationId
    INNER JOIN HostelCategories hc ON h.CategoryId  = hc.CategoryId
    WHERE hc.CategoryName = @CategoryName AND h.IsActive = 1
);
GO

-- 3.4 Table-Valued: Get available rooms for a hostel
CREATE OR ALTER FUNCTION fn_GetAvailableRooms (@HostelId INT)
RETURNS TABLE
AS
RETURN (
    SELECT r.*, rt.TypeName,
           dbo.fn_CalculateRent(r.MonthlyRent, 1)  AS Rent1Month,
           dbo.fn_CalculateRent(r.MonthlyRent, 5)  AS Rent5Months
    FROM Rooms r
    INNER JOIN RoomTypes rt ON r.RoomTypeId = rt.RoomTypeId
    WHERE r.HostelId = @HostelId
      AND r.IsActive = 1
      AND r.AvailableBeds > 0
);
GO

-- ============================================================
-- SECTION 4: STORED PROCEDURES
-- ============================================================

-- 4.1 Register new user & auto-create admin enquiry
CREATE OR ALTER PROCEDURE sp_RegisterUser
    @FullName     NVARCHAR(150),
    @Email        NVARCHAR(200),
    @PasswordHash NVARCHAR(500),
    @PhoneNumber  NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        RAISERROR('Email already registered.', 16, 1);
        RETURN;
    END

    INSERT INTO Users (FullName, Email, PasswordHash, PhoneNumber, RoleId)
    VALUES (@FullName, @Email, @PasswordHash, @PhoneNumber, 2);

    DECLARE @NewUserId INT = SCOPE_IDENTITY();

    INSERT INTO Enquiries (UserId, Subject, Message, EnquiryStatus)
    VALUES (@NewUserId, 'New User Registration',
            'New user registered: ' + @FullName + ' (' + @Email + '). Phone: ' + ISNULL(@PhoneNumber,'N/A'),
            'New');

    SELECT @NewUserId AS UserId;
END;
GO

-- 4.2 Create booking with discount logic
CREATE OR ALTER PROCEDURE sp_CreateBooking
    @UserId         INT,
    @RoomId         INT,
    @PlanId         INT = NULL,
    @CheckInDate    DATE,
    @DurationMonths INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @AvailableBeds INT;
        SELECT @AvailableBeds = AvailableBeds FROM Rooms WHERE RoomId = @RoomId;

        IF ISNULL(@AvailableBeds, 0) = 0
        BEGIN
            RAISERROR('No beds available in selected room.', 16, 1);
            ROLLBACK; RETURN;
        END

        DECLARE @MonthlyRent   DECIMAL(10,2);
        SELECT @MonthlyRent = MonthlyRent FROM Rooms WHERE RoomId = @RoomId;

        DECLARE @TotalAmount    DECIMAL(10,2) = @MonthlyRent * @DurationMonths;
        DECLARE @DiscountAmount DECIMAL(10,2) = 0;

        IF @DurationMonths >= 5
            SET @DiscountAmount = @TotalAmount * 0.30;

        DECLARE @FinalAmount DECIMAL(10,2) = @TotalAmount - @DiscountAmount;

        INSERT INTO Bookings (UserId, RoomId, PlanId, BookingNumber, CheckInDate, CheckOutDate,
                              TotalAmount, DiscountAmount, FinalAmount, BookingStatus, PaymentStatus)
        VALUES (@UserId, @RoomId, @PlanId, 'KP-TEMP',
                @CheckInDate, DATEADD(MONTH, @DurationMonths, @CheckInDate),
                @TotalAmount, @DiscountAmount, @FinalAmount, 'Pending', 'Unpaid');

        DECLARE @BookingId INT = SCOPE_IDENTITY();

        UPDATE Bookings
        SET BookingNumber = dbo.fn_GenerateBookingNumber(@BookingId)
        WHERE BookingId = @BookingId;

        UPDATE Rooms SET AvailableBeds = AvailableBeds - 1, UpdatedAt = GETUTCDATE()
        WHERE RoomId = @RoomId;

        INSERT INTO Notifications (UserId, Title, Message, NotificationType)
        VALUES (@UserId, 'Booking Created',
                'Your booking ' + dbo.fn_GenerateBookingNumber(@BookingId) + ' has been created. Awaiting payment.',
                'BookingPending');

        COMMIT;
        SELECT @BookingId AS BookingId,
               dbo.fn_GenerateBookingNumber(@BookingId) AS BookingNumber,
               @FinalAmount AS FinalAmount,
               @DiscountAmount AS DiscountAmount;
    END TRY
    BEGIN CATCH
        ROLLBACK; THROW;
    END CATCH
END;
GO

-- 4.3 Admin: Confirm or Reject booking
CREATE OR ALTER PROCEDURE sp_UpdateBookingStatus
    @BookingId  INT,
    @Status     NVARCHAR(30),
    @AdminNotes NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Bookings
    SET BookingStatus = @Status,
        AdminNotes    = @AdminNotes,
        UpdatedAt     = GETUTCDATE()
    WHERE BookingId = @BookingId;

    IF @Status IN ('Rejected', 'Cancelled')
    BEGIN
        DECLARE @RoomId INT;
        SELECT @RoomId = RoomId FROM Bookings WHERE BookingId = @BookingId;
        UPDATE Rooms SET AvailableBeds = AvailableBeds + 1, UpdatedAt = GETUTCDATE()
        WHERE RoomId = @RoomId;
    END

    DECLARE @UserId INT;
    SELECT @UserId = UserId FROM Bookings WHERE BookingId = @BookingId;

    INSERT INTO Notifications (UserId, Title, Message, NotificationType)
    VALUES (@UserId,
            'Booking ' + @Status,
            'Your booking status has been updated to: ' + @Status + ISNULL('. Notes: ' + @AdminNotes, ''),
            'BookingUpdate');
END;
GO

-- 4.4 Record payment (Stripe/PayU callback)
CREATE OR ALTER PROCEDURE sp_RecordPayment
    @BookingId       INT,
    @TransactionId   NVARCHAR(200),
    @PaymentGateway  NVARCHAR(50),
    @Amount          DECIMAL(10,2),
    @PaymentStatus   NVARCHAR(30),
    @PaymentMethod   NVARCHAR(100),
    @GatewayResponse NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UserId INT;
    SELECT @UserId = UserId FROM Bookings WHERE BookingId = @BookingId;

    INSERT INTO Payments (BookingId, UserId, TransactionId, PaymentGateway, Amount,
                          PaymentStatus, PaymentMethod, GatewayResponse, PaidAt)
    VALUES (@BookingId, @UserId, @TransactionId, @PaymentGateway, @Amount, @PaymentStatus,
            @PaymentMethod, @GatewayResponse,
            CASE WHEN @PaymentStatus = 'Success' THEN GETUTCDATE() ELSE NULL END);

    IF @PaymentStatus = 'Success'
    BEGIN
        UPDATE Bookings
        SET PaymentStatus = 'Paid', BookingStatus = 'Confirmed', UpdatedAt = GETUTCDATE()
        WHERE BookingId = @BookingId;

        INSERT INTO Notifications (UserId, Title, Message, NotificationType)
        VALUES (@UserId, 'Payment Successful',
                'Payment of INR ' + CAST(@Amount AS NVARCHAR) + ' received. Booking confirmed.',
                'PaymentReceived');
    END
END;
GO

-- 4.5 Search hostels
CREATE OR ALTER PROCEDURE sp_SearchHostels
    @SearchTerm NVARCHAR(200) = NULL,
    @CategoryId INT = NULL,
    @LocationId INT = NULL,
    @PageNumber INT = 1,
    @PageSize   INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT hl.*
    FROM vw_HostelListing hl
    WHERE
        (@SearchTerm IS NULL OR hl.HostelName LIKE '%' + @SearchTerm + '%' OR hl.LocationName LIKE '%' + @SearchTerm + '%')
    AND (@CategoryId IS NULL OR hl.CategoryId  = @CategoryId)
    AND (@LocationId IS NULL OR hl.LocationId  = @LocationId)
    ORDER BY hl.HostelId
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 4.6 Get user dashboard
CREATE OR ALTER PROCEDURE sp_GetUserDashboard
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_BookingSummary
    WHERE UserId = @UserId
    ORDER BY BookingDate DESC;

    SELECT * FROM Notifications
    WHERE UserId = @UserId AND IsRead = 0
    ORDER BY CreatedAt DESC;
END;
GO

-- 4.7 Admin dashboard stats
CREATE OR ALTER PROCEDURE sp_GetAdminDashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*)           FROM Users    WHERE RoleId = 2)                              AS TotalUsers,
        (SELECT COUNT(*)           FROM Hostels  WHERE IsActive = 1)                            AS TotalHostels,
        (SELECT COUNT(*)           FROM Bookings WHERE BookingStatus = 'Confirmed')              AS ActiveBookings,
        (SELECT COUNT(*)           FROM Bookings WHERE BookingStatus = 'Pending')               AS PendingBookings,
        (SELECT ISNULL(SUM(Amount),0) FROM Payments WHERE PaymentStatus = 'Success')            AS TotalRevenue,
        (SELECT COUNT(*)           FROM Enquiries WHERE EnquiryStatus = 'New')                  AS NewEnquiries,
        (SELECT ISNULL(SUM(AvailableBeds),0) FROM Rooms WHERE IsActive = 1)                     AS TotalAvailableBeds;
END;
GO

-- 4.8 Get hostel detail with facilities and images
CREATE OR ALTER PROCEDURE sp_GetHostelDetail
    @HostelId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Hostel info
    SELECT * FROM vw_HostelListing WHERE HostelId = @HostelId;

    -- Facilities
    SELECT f.FacilityName, f.IconClass
    FROM HostelFacilities hf
    INNER JOIN Facilities f ON hf.FacilityId = f.FacilityId
    WHERE hf.HostelId = @HostelId;

    -- Images
    SELECT ImageUrl, AltText, IsPrimary, DisplayOrder, RoomId
    FROM HostelImages
    WHERE HostelId = @HostelId
    ORDER BY IsPrimary DESC, DisplayOrder;

    -- Rooms
    SELECT * FROM fn_GetAvailableRooms(@HostelId);

    -- Shared Areas
    SELECT sa.*, sai.ImageUrl
    FROM SharedAreas sa
    LEFT JOIN SharedAreaImages sai ON sa.SharedAreaId = sai.SharedAreaId
    WHERE sa.HostelId = @HostelId AND sa.IsActive = 1;
END;
GO

-- ============================================================
-- SECTION 5: INDEXES
-- ============================================================

CREATE NONCLUSTERED INDEX IX_Bookings_UserId    ON Bookings(UserId);
CREATE NONCLUSTERED INDEX IX_Bookings_RoomId    ON Bookings(RoomId);
CREATE NONCLUSTERED INDEX IX_Bookings_Status    ON Bookings(BookingStatus);
CREATE NONCLUSTERED INDEX IX_Rooms_HostelId     ON Rooms(HostelId);
CREATE NONCLUSTERED INDEX IX_Payments_BookingId ON Payments(BookingId);
CREATE NONCLUSTERED INDEX IX_Enquiries_UserId   ON Enquiries(UserId);
CREATE NONCLUSTERED INDEX IX_Users_Email        ON Users(Email);
CREATE NONCLUSTERED INDEX IX_Hostels_LocationId ON Hostels(LocationId);
CREATE NONCLUSTERED INDEX IX_Hostels_CategoryId ON Hostels(CategoryId);
GO

-- ============================================================
-- SECTION 6: SEED DATA
-- ============================================================

INSERT INTO Roles (RoleName) VALUES ('Admin'), ('User');

INSERT INTO HostelCategories (CategoryName, Description) VALUES
('Boys',        'Hostel exclusively for male residents'),
('Girls',       'Hostel exclusively for female residents'),
('Independent', 'Private rooms open to both boys and girls');

INSERT INTO Facilities (FacilityName, IconClass) VALUES
('Room Cleaning',   'fas fa-broom'),
('Toilet Cleaning', 'fas fa-toilet'),
('Electricity',     'fas fa-bolt'),
('Security',        'fas fa-shield-alt'),
('RO Water',        'fas fa-tint'),
('Vehicle Parking', 'fas fa-parking'),
('CCTV Cameras',    'fas fa-video'),
('WiFi',            'fas fa-wifi'),
('Breakfast',       'fas fa-coffee'),
('Lunch',           'fas fa-utensils'),
('Snacks',          'fas fa-cookie'),
('Dinner',          'fas fa-hamburger');

INSERT INTO RoomTypes (TypeName, BedCount, Description) VALUES
('Single',    1, 'Single occupancy room'),
('Double',    2, 'Double sharing room'),
('Triple',    3, 'Triple sharing room'),
('Dormitory', 6, 'Dormitory with 6 beds');

INSERT INTO Locations (LocationName, Address, City, PinCode) VALUES
('Vijay Nagar', 'Vijay Nagar, Indore',  'Indore', '452010'),
('Palasia',     'Palasia, Indore',       'Indore', '452001'),
('Bhawarkua',   'Bhawarkua, Indore',     'Indore', '452001'),
('Scheme 54',   'Scheme 54, Indore',     'Indore', '452010'),
('AB Road',     'AB Road, Indore',       'Indore', '452008');

-- Default Admin (hash password properly in application)
INSERT INTO Users (FullName, Email, PasswordHash, PhoneNumber, RoleId, IsActive, IsEmailVerified)
VALUES ('Kesar Admin', 'admin@kesarpremium.com', 'BCRYPT_HASH_HERE', '12345678', 1, 1, 1);

-- Pricing plans (5-month = 30% discount)
-- Populate after adding Hostels & RoomTypes
-- INSERT INTO PricingPlans (HostelId, RoomTypeId, PlanName, DurationMonths, BaseRent, DiscountPercent)
-- VALUES (1, 1, 'Monthly',       1, 5000.00, 0),
--        (1, 1, '5-Month Offer', 5, 5000.00, 30);

PRINT 'KesarPremiumDB schema created successfully.';
GO
