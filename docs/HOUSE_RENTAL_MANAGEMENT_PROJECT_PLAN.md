# House Rental Management System - Project Architecture and Development Plan

## 1. Project Overview

### 1.1 Project Title

**House Rental Management System**

### 1.2 Project Type

University lab project desktop application.

### 1.3 Project Goal

The goal of this project is to build a professional Windows Forms application that helps an Admin or Staff user manage rental properties, houses, rooms, tenants, rental agreements, monthly rent payments, reports, and core administrative settings.

The project should demonstrate clean software engineering practices using a traditional 3-layer architecture:

| Layer | Responsibility |
| --- | --- |
| UI Layer | Windows Forms screens, user interaction, validation display |
| BLL Layer | Business rules, workflow decisions, role checks, calculations |
| DAL Layer | SQL Server communication using ADO.NET |

### 1.4 Selected Technology Stack

| Area | Technology |
| --- | --- |
| Language | C# |
| UI | Windows Forms |
| Framework | .NET Framework 4.7.2 |
| Database | Microsoft SQL Server 2025 Express |
| Data Access | ADO.NET |
| Architecture | 3-Layer Architecture |
| Reporting | Microsoft ReportViewer with RDLC reports |
| IDE | Visual Studio Community 2022 |
| Version Control | Git |
| Database Tool | SQL Server Management Studio |

### 1.5 Existing Environment

The development machine already has:

- Visual Studio Community 2022
- .NET desktop development workload
- Data storage and processing workload
- SQL Server 2025 Express
- SQL Server Management Studio
- SQL Server instance: `.\SQLEXPRESS`
- Database name: `HouseRentalDB`

Recommended connection string:

```csharp
string connectionString =
    @"Server=.\SQLEXPRESS;Database=HouseRentalDB;Trusted_Connection=True;TrustServerCertificate=True;";
```

Recommended `App.config` connection string:

```xml
<connectionStrings>
  <add name="HouseRentalDB"
       connectionString="Server=.\SQLEXPRESS;Database=HouseRentalDB;Trusted_Connection=True;TrustServerCertificate=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

## 2. Current Project Analysis

### 2.1 Current Workspace Structure

Current project files:

```text
housing_rental/
|-- App.config
|-- Form1.cs
|-- Form1.Designer.cs
|-- Housing rental.csproj
|-- Housing rental.slnx
|-- Program.cs
|-- Properties/
|   |-- AssemblyInfo.cs
|   |-- Resources.Designer.cs
|   |-- Resources.resx
|   |-- Settings.Designer.cs
|   |-- Settings.settings
|-- bin/
|-- obj/
```

### 2.2 Current Code State

The project is currently a fresh Windows Forms application:

- The main entry point is `Program.cs`.
- The application opens `Form1`.
- `Form1` is still the default starter form.
- `App.config` currently contains only the .NET Framework runtime configuration.
- No database connection string has been added yet.
- No UI modules, BLL classes, DAL classes, entity models, reports, or database scripts exist yet.
- The workspace is not currently initialized as a Git repository.

### 2.3 What This Means

This is a good starting point because the project has no unnecessary complexity yet. The next step should be to create a clean structure before adding forms and code. This will make the final lab submission easier to explain, maintain, demonstrate, and extend.

## 3. Recommended Solution Architecture

### 3.1 Architecture Style

Use a 3-layer architecture:

```text
Windows Forms UI
      |
      v
Business Logic Layer
      |
      v
Data Access Layer
      |
      v
SQL Server Database
```

### 3.2 Layer Responsibilities

| Layer | Project/Folder | Responsibility |
| --- | --- | --- |
| UI | `HousingRental.UI` | Forms, controls, navigation, display validation messages |
| BLL | `HousingRental.BLL` | Business rules, calculations, workflow, authorization decisions |
| DAL | `HousingRental.DAL` | ADO.NET database operations |
| Models | `HousingRental.Entities` or `Models` folder | Data transfer classes shared between layers |
| Reports | `Reports` folder | RDLC files and report data sources |
| Database | `Database` folder | SQL scripts for tables, seed data, views, stored procedures |

### 3.3 Recommended Project Approach

For a university lab project, there are two acceptable approaches.

#### Option A: Professional Multi-Project Solution

Recommended for best architecture marks.

```text
HouseRentalManagement.sln
|-- HousingRental.UI/
|   |-- Forms/
|   |-- UserControls/
|   |-- Reports/
|   |-- App.config
|   |-- Program.cs
|
|-- HousingRental.BLL/
|   |-- Services/
|   |-- Validators/
|   |-- Helpers/
|
|-- HousingRental.DAL/
|   |-- Data/
|   |-- Repositories/
|   |-- Helpers/
|
|-- HousingRental.Entities/
|   |-- Models/
|   |-- DTOs/
|   |-- Enums/
|
|-- Database/
|   |-- 01_CreateDatabase.sql
|   |-- 02_CreateTables.sql
|   |-- 03_CreateViews.sql
|   |-- 04_CreateStoredProcedures.sql
|   |-- 05_SeedData.sql
|
|-- docs/
|   |-- HOUSE_RENTAL_MANAGEMENT_PROJECT_PLAN.md
```

#### Option B: Simpler Single-Project Folder Structure

Acceptable if the instructor expects one WinForms project.

```text
Housing rental/
|-- BLL/
|   |-- AuthService.cs
|   |-- DashboardService.cs
|   |-- PropertyService.cs
|   |-- TenantService.cs
|   |-- AgreementService.cs
|   |-- RentPaymentService.cs
|   |-- ReportService.cs
|
|-- DAL/
|   |-- DbConnectionFactory.cs
|   |-- SqlHelper.cs
|   |-- UserRepository.cs
|   |-- PropertyRepository.cs
|   |-- TenantRepository.cs
|   |-- AgreementRepository.cs
|   |-- RentPaymentRepository.cs
|   |-- ReportRepository.cs
|
|-- Models/
|   |-- User.cs
|   |-- Role.cs
|   |-- Property.cs
|   |-- House.cs
|   |-- Room.cs
|   |-- Tenant.cs
|   |-- RentalAgreement.cs
|   |-- RentPayment.cs
|   |-- DashboardSummary.cs
|
|-- Forms/
|   |-- Auth/
|   |   |-- FrmLogin.cs
|   |-- Dashboard/
|   |   |-- FrmDashboard.cs
|   |-- Properties/
|   |   |-- FrmPropertyList.cs
|   |   |-- FrmPropertyEntry.cs
|   |   |-- FrmHouseList.cs
|   |   |-- FrmRoomList.cs
|   |-- Tenants/
|   |   |-- FrmTenantList.cs
|   |   |-- FrmTenantEntry.cs
|   |-- Agreements/
|   |   |-- FrmAgreementList.cs
|   |   |-- FrmAgreementEntry.cs
|   |-- Payments/
|   |   |-- FrmRentCollection.cs
|   |   |-- FrmPaymentHistory.cs
|   |-- Reports/
|   |   |-- FrmReports.cs
|   |-- Users/
|   |   |-- FrmUserManagement.cs
|
|-- Reports/
|   |-- RentCollectionReport.rdlc
|   |-- TenantReport.rdlc
|   |-- OccupancyReport.rdlc
|
|-- Database/
|   |-- 01_CreateTables.sql
|   |-- 02_SeedData.sql
|
|-- docs/
|-- App.config
|-- Program.cs
```

### 3.4 Recommended Choice

Use **Option A** if you want the project to look more professional and clearly demonstrate 3-layer architecture.

Use **Option B** if you want the project to remain simple for a lab submission while still keeping the code clean.

Because this is a university lab project, the best balanced recommendation is:

**Start with Option B if the deadline is short. Use Option A if you have enough time to prepare a stronger final submission.**

## 4. Core System Modules

### 4.1 Authentication and Role-Based Access

Purpose:

Allow only authorized users to access the system.

Roles:

| Role | Permissions |
| --- | --- |
| Admin | Full access to users, settings, all modules, reports, delete actions |
| Staff | Manage tenants, agreements, rent collection, view reports, limited settings |

Main features:

- Login screen
- Password validation
- Role-based menu visibility
- Active/inactive user status
- Last login date tracking
- Logout

Recommended forms:

- `FrmLogin`
- `FrmUserManagement`
- `FrmChangePassword`

Recommended BLL classes:

- `AuthService`
- `UserService`

Recommended DAL classes:

- `UserRepository`
- `RoleRepository`

### 4.2 Dashboard

Purpose:

Give a quick summary of rental business status.

Dashboard cards:

- Total properties
- Total houses
- Total rooms
- Available rooms
- Occupied rooms
- Total tenants
- Active agreements
- Monthly expected rent
- Monthly collected rent
- Monthly due rent
- Overdue payments

Recommended visuals:

- Summary cards
- Recent payments grid
- Expiring agreements grid
- Room occupancy chart
- Monthly collection chart

Recommended classes:

- `DashboardService`
- `DashboardRepository`
- `DashboardSummary`

### 4.3 Property, House, and Room Management

Purpose:

Manage the physical rental structure.

Hierarchy:

```text
Property
  |-- House
      |-- Room
```

Property examples:

- Building
- Apartment property
- Hostel property
- Residential compound

House examples:

- House A
- Flat 101
- Unit 2

Room examples:

- Room 101
- Master room
- Single room
- Shared room

Main features:

- Add, edit, delete, search properties
- Add houses under properties
- Add rooms under houses
- Track room rent amount
- Track room availability
- Track occupancy status

Room statuses:

| Status | Meaning |
| --- | --- |
| Available | Room can be rented |
| Occupied | Room has an active tenant agreement |
| Maintenance | Room is temporarily unavailable |
| Inactive | Room is not used anymore |

Recommended forms:

- `FrmPropertyList`
- `FrmPropertyEntry`
- `FrmHouseList`
- `FrmHouseEntry`
- `FrmRoomList`
- `FrmRoomEntry`

### 4.4 Tenant Management

Purpose:

Maintain tenant records.

Main features:

- Add tenant
- Edit tenant
- Search tenant
- View tenant details
- Store contact information
- Store identity information
- Store emergency contact
- View agreement and payment history

Recommended tenant fields:

- Tenant ID
- Full name
- Phone
- Email
- National ID or student ID
- Address
- Emergency contact name
- Emergency contact phone
- Status
- Created date

Recommended forms:

- `FrmTenantList`
- `FrmTenantEntry`
- `FrmTenantDetails`

### 4.5 Rental Agreement Management

Purpose:

Create and manage rental contracts between tenants and rooms.

Main features:

- Create agreement
- Select tenant
- Select available room
- Set start date and end date
- Set monthly rent
- Set security deposit
- Track agreement status
- Renew agreement
- Terminate agreement
- Prevent assigning an occupied room to another active agreement

Agreement statuses:

| Status | Meaning |
| --- | --- |
| Draft | Agreement created but not active |
| Active | Tenant currently rents the room |
| Expired | Agreement end date passed |
| Terminated | Agreement ended manually |
| Cancelled | Agreement cancelled before activation |

Business rules:

- A tenant can have only one active agreement unless the business rule allows multiple rooms.
- A room can have only one active agreement at a time.
- Agreement end date must be after start date.
- Monthly rent must be greater than zero.
- Room status should become `Occupied` when agreement becomes active.
- Room status should become `Available` when an agreement ends and no other active agreement exists.

Recommended forms:

- `FrmAgreementList`
- `FrmAgreementEntry`
- `FrmAgreementDetails`

### 4.6 Monthly Rent Collection

Purpose:

Record and track monthly rent payments.

Main features:

- Generate monthly rent due records
- Collect full payment
- Collect partial payment
- Track due amount
- Track payment method
- Print receipt
- View payment history
- Search payment by tenant, room, month, or date

Payment methods:

- Cash
- Bank transfer
- Mobile payment
- Card

Payment statuses:

| Status | Meaning |
| --- | --- |
| Pending | Payment not received |
| Partial | Some amount received |
| Paid | Full amount received |
| Overdue | Payment date passed |
| Cancelled | Payment record cancelled |

Business rules:

- Paid amount cannot be negative.
- Paid amount cannot exceed payable amount unless advance payment is supported.
- Payment month must belong to an active agreement period.
- Receipt number should be unique.
- Each payment must be connected to an agreement.

Recommended forms:

- `FrmRentCollection`
- `FrmPaymentHistory`
- `FrmPaymentReceipt`

### 4.7 Reports

Purpose:

Provide printable and exportable summaries for project demonstration and management.

Recommended reports:

| Report | Description |
| --- | --- |
| Tenant List Report | List of all active/inactive tenants |
| Property Occupancy Report | Available and occupied rooms by property |
| Rent Collection Report | Payments collected between date ranges |
| Monthly Due Report | Tenants with unpaid or partial rent |
| Agreement Report | Active, expired, and terminated agreements |
| Income Summary Report | Monthly rental income summary |

Reporting technology:

- Microsoft ReportViewer
- RDLC report files
- Data source from BLL or DAL result sets

Recommended forms:

- `FrmReports`
- `FrmReportViewer`

### 4.8 Maintenance and Expenses

Purpose:

Add professional value by tracking maintenance issues and property expenses.

This module is optional but recommended for a stronger project.

Main features:

- Add maintenance request
- Link maintenance to property, house, or room
- Track repair cost
- Track request status
- Record utility or maintenance expenses

Maintenance statuses:

- Open
- In Progress
- Completed
- Cancelled

Recommended forms:

- `FrmMaintenanceList`
- `FrmMaintenanceEntry`
- `FrmExpenseList`
- `FrmExpenseEntry`

### 4.9 Settings and Administration

Purpose:

Manage system-level configuration.

Main features:

- User management
- Role management
- Change password
- Application settings
- Backup database instruction screen
- Audit log viewer

Recommended settings:

- Organization/lab project name
- Default currency
- Receipt footer text
- Rent due day

## 5. Database Architecture

### 5.1 Database Name

```sql
HouseRentalDB
```

### 5.2 Main Tables

Recommended tables:

| Table | Purpose |
| --- | --- |
| `Roles` | Stores user roles such as Admin and Staff |
| `Users` | Stores login users |
| `Properties` | Stores rental property/building information |
| `Houses` | Stores houses/flats/units under a property |
| `Rooms` | Stores rooms under a house |
| `Tenants` | Stores tenant information |
| `RentalAgreements` | Stores tenant-room rental contracts |
| `RentPayments` | Stores monthly rent payment records |
| `MaintenanceRequests` | Stores property/room maintenance records |
| `Expenses` | Stores rental-related expenses |
| `AuditLogs` | Stores important user activity logs |
| `AppSettings` | Stores configurable application settings |

### 5.3 Suggested Entity Relationships

```text
Roles 1 ----- * Users

Properties 1 ----- * Houses
Houses     1 ----- * Rooms

Tenants 1 ----- * RentalAgreements
Rooms   1 ----- * RentalAgreements

RentalAgreements 1 ----- * RentPayments

Users 1 ----- * RentPayments
Users 1 ----- * AuditLogs

Properties 1 ----- * MaintenanceRequests
Rooms      1 ----- * MaintenanceRequests
Properties 1 ----- * Expenses
```

### 5.4 Recommended Table Design

#### Roles

| Column | Type | Notes |
| --- | --- | --- |
| RoleId | int identity primary key | Unique role ID |
| RoleName | nvarchar(50) | Admin, Staff |
| Description | nvarchar(200) | Optional |
| IsActive | bit | Active status |

#### Users

| Column | Type | Notes |
| --- | --- | --- |
| UserId | int identity primary key | Unique user ID |
| RoleId | int foreign key | Links to Roles |
| FullName | nvarchar(100) | User display name |
| Username | nvarchar(50) unique | Login username |
| PasswordHash | nvarchar(255) | Hashed password |
| PasswordSalt | nvarchar(255) | Salt value if used |
| Phone | nvarchar(30) | Optional |
| Email | nvarchar(100) | Optional |
| IsActive | bit | Login allowed or not |
| LastLoginAt | datetime null | Last successful login |
| CreatedAt | datetime | Record creation date |

#### Properties

| Column | Type | Notes |
| --- | --- | --- |
| PropertyId | int identity primary key | Unique property ID |
| PropertyName | nvarchar(100) | Building or property name |
| Address | nvarchar(250) | Property address |
| City | nvarchar(80) | City |
| Description | nvarchar(300) | Optional |
| IsActive | bit | Active status |
| CreatedAt | datetime | Record creation date |

#### Houses

| Column | Type | Notes |
| --- | --- | --- |
| HouseId | int identity primary key | Unique house/unit ID |
| PropertyId | int foreign key | Links to Properties |
| HouseName | nvarchar(100) | House, unit, or flat name |
| FloorNo | nvarchar(20) | Optional |
| Description | nvarchar(300) | Optional |
| IsActive | bit | Active status |
| CreatedAt | datetime | Record creation date |

#### Rooms

| Column | Type | Notes |
| --- | --- | --- |
| RoomId | int identity primary key | Unique room ID |
| HouseId | int foreign key | Links to Houses |
| RoomNo | nvarchar(50) | Room number/name |
| RoomType | nvarchar(50) | Single, Double, Master, Shared |
| MonthlyRent | decimal(18,2) | Standard rent |
| Status | nvarchar(30) | Available, Occupied, Maintenance, Inactive |
| Description | nvarchar(300) | Optional |
| CreatedAt | datetime | Record creation date |

#### Tenants

| Column | Type | Notes |
| --- | --- | --- |
| TenantId | int identity primary key | Unique tenant ID |
| FullName | nvarchar(120) | Tenant name |
| Phone | nvarchar(30) | Required |
| Email | nvarchar(100) | Optional |
| NationalId | nvarchar(80) | National ID, passport, or student ID |
| Address | nvarchar(250) | Permanent address |
| EmergencyContactName | nvarchar(100) | Optional |
| EmergencyContactPhone | nvarchar(30) | Optional |
| Status | nvarchar(30) | Active, Inactive, Blacklisted |
| CreatedAt | datetime | Record creation date |

#### RentalAgreements

| Column | Type | Notes |
| --- | --- | --- |
| AgreementId | int identity primary key | Unique agreement ID |
| AgreementNo | nvarchar(50) unique | Human-friendly contract number |
| TenantId | int foreign key | Links to Tenants |
| RoomId | int foreign key | Links to Rooms |
| StartDate | date | Agreement start |
| EndDate | date | Agreement end |
| MonthlyRent | decimal(18,2) | Agreed monthly rent |
| SecurityDeposit | decimal(18,2) | Deposit amount |
| Status | nvarchar(30) | Draft, Active, Expired, Terminated, Cancelled |
| Notes | nvarchar(500) | Optional |
| CreatedByUserId | int foreign key | User who created record |
| CreatedAt | datetime | Record creation date |

#### RentPayments

| Column | Type | Notes |
| --- | --- | --- |
| PaymentId | int identity primary key | Unique payment ID |
| ReceiptNo | nvarchar(50) unique | Unique receipt number |
| AgreementId | int foreign key | Links to RentalAgreements |
| PaymentMonth | int | Month number 1-12 |
| PaymentYear | int | Payment year |
| DueAmount | decimal(18,2) | Monthly due amount |
| PaidAmount | decimal(18,2) | Amount paid |
| BalanceAmount | decimal(18,2) | Due minus paid |
| PaymentDate | date | Date payment received |
| PaymentMethod | nvarchar(50) | Cash, Bank Transfer, etc. |
| Status | nvarchar(30) | Pending, Partial, Paid, Overdue, Cancelled |
| CollectedByUserId | int foreign key | User who collected payment |
| Remarks | nvarchar(300) | Optional |
| CreatedAt | datetime | Record creation date |

#### MaintenanceRequests

| Column | Type | Notes |
| --- | --- | --- |
| MaintenanceId | int identity primary key | Unique maintenance ID |
| PropertyId | int foreign key null | Related property |
| RoomId | int foreign key null | Related room |
| Title | nvarchar(150) | Issue title |
| Description | nvarchar(500) | Details |
| Cost | decimal(18,2) | Repair cost |
| Status | nvarchar(30) | Open, In Progress, Completed, Cancelled |
| RequestedAt | datetime | Request date |
| CompletedAt | datetime null | Completion date |

#### Expenses

| Column | Type | Notes |
| --- | --- | --- |
| ExpenseId | int identity primary key | Unique expense ID |
| PropertyId | int foreign key null | Related property |
| ExpenseDate | date | Expense date |
| Category | nvarchar(80) | Utility, Repair, Service, Other |
| Amount | decimal(18,2) | Expense amount |
| Description | nvarchar(300) | Optional |
| CreatedByUserId | int foreign key | User who recorded expense |
| CreatedAt | datetime | Record creation date |

#### AuditLogs

| Column | Type | Notes |
| --- | --- | --- |
| AuditLogId | int identity primary key | Unique log ID |
| UserId | int foreign key null | User who performed action |
| ActionName | nvarchar(100) | Login, Insert, Update, Delete |
| TableName | nvarchar(100) | Related table |
| RecordId | nvarchar(50) | Related record ID |
| Description | nvarchar(500) | Activity details |
| CreatedAt | datetime | Activity date |

#### AppSettings

| Column | Type | Notes |
| --- | --- | --- |
| SettingId | int identity primary key | Unique setting ID |
| SettingKey | nvarchar(100) unique | Setting name |
| SettingValue | nvarchar(300) | Setting value |
| Description | nvarchar(300) | Optional |

### 5.5 Recommended Views

| View | Purpose |
| --- | --- |
| `vw_RoomOccupancy` | Show property, house, room, and current occupancy |
| `vw_ActiveAgreements` | Show all active rental agreements with tenant and room details |
| `vw_RentCollectionSummary` | Show rent payment summary by month |
| `vw_TenantBalances` | Show tenants with pending balances |

### 5.6 Recommended Stored Procedures

Stored procedures are optional for a university ADO.NET project, but they make reporting and complex actions cleaner.

| Procedure | Purpose |
| --- | --- |
| `sp_GetDashboardSummary` | Load dashboard card data |
| `sp_GetRentCollectionReport` | Load report data by date range |
| `sp_GetTenantPaymentHistory` | Load one tenant's payment history |
| `sp_GenerateMonthlyRentDue` | Generate pending monthly rent records |

## 6. Business Logic Layer Design

### 6.1 BLL Purpose

The BLL should protect the system from invalid business operations. UI forms should not directly decide important rules, and DAL classes should not contain business decisions.

### 6.2 Recommended BLL Classes

| Class | Responsibility |
| --- | --- |
| `AuthService` | Login, password verification, current user session |
| `UserService` | User creation, status updates, role checks |
| `DashboardService` | Dashboard summary calculations |
| `PropertyService` | Property, house, room workflows |
| `TenantService` | Tenant validation and tenant workflows |
| `AgreementService` | Agreement creation, activation, renewal, termination |
| `RentPaymentService` | Rent due generation, payment collection, balance calculation |
| `ReportService` | Report filtering and report data preparation |
| `MaintenanceService` | Maintenance workflow |
| `ExpenseService` | Expense validation and saving |
| `AuditService` | Log important actions |

### 6.3 BLL Validation Examples

Tenant validation:

- Full name is required.
- Phone number is required.
- Duplicate phone or National ID should be avoided when possible.

Room validation:

- Room number is required.
- Monthly rent must be greater than zero.
- Room must belong to a valid house.

Agreement validation:

- Tenant must exist.
- Room must exist.
- Room must be available.
- Start date must be before end date.
- Monthly rent must be greater than zero.

Payment validation:

- Agreement must be active.
- Payment amount must be valid.
- Payment month and year must be valid.
- Duplicate full payment for the same month should be prevented.

## 7. Data Access Layer Design

### 7.1 DAL Purpose

The DAL should be responsible only for database work:

- Open SQL Server connection
- Run parameterized queries
- Run commands inside transactions where required
- Convert database rows into model objects
- Return data to BLL

### 7.2 Recommended DAL Helper Classes

| Class | Responsibility |
| --- | --- |
| `DbConnectionFactory` | Reads connection string and creates `SqlConnection` |
| `SqlHelper` | Common helper methods for commands and parameters |
| `BaseRepository` | Shared repository utilities |

### 7.3 Connection Factory Example

```csharp
using System.Configuration;
using System.Data.SqlClient;

namespace Housing_rental.DAL
{
    public static class DbConnectionFactory
    {
        public static SqlConnection CreateConnection()
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["HouseRentalDB"].ConnectionString;

            return new SqlConnection(connectionString);
        }
    }
}
```

### 7.4 ADO.NET Rules

Use these rules consistently:

- Always use `using` blocks for `SqlConnection`, `SqlCommand`, and `SqlDataReader`.
- Always use parameterized queries.
- Never concatenate user input into SQL strings.
- Use transactions for operations that update multiple tables.
- Keep SQL query text organized and readable.
- Return meaningful success/failure results to BLL.

Example:

```csharp
using (SqlConnection connection = DbConnectionFactory.CreateConnection())
using (SqlCommand command = new SqlCommand(sql, connection))
{
    command.Parameters.AddWithValue("@FullName", tenant.FullName);
    command.Parameters.AddWithValue("@Phone", tenant.Phone);

    connection.Open();
    command.ExecuteNonQuery();
}
```

## 8. UI Architecture and Forms

### 8.1 UI Design Goal

The UI should look clean, modern, and easy to use while still being realistic for a WinForms university project.

### 8.2 Main UI Flow

```text
Application Start
      |
      v
Login Form
      |
      v
Dashboard Form
      |
      +-- Property Management
      +-- Tenant Management
      +-- Agreements
      +-- Rent Collection
      +-- Reports
      +-- Users and Settings
```

### 8.3 Recommended Main Forms

| Form | Purpose |
| --- | --- |
| `FrmLogin` | User authentication |
| `FrmDashboard` | Main dashboard and navigation |
| `FrmPropertyList` | Property list and search |
| `FrmPropertyEntry` | Add/edit property |
| `FrmHouseList` | House/unit list |
| `FrmHouseEntry` | Add/edit house |
| `FrmRoomList` | Room list and status |
| `FrmRoomEntry` | Add/edit room |
| `FrmTenantList` | Tenant list and search |
| `FrmTenantEntry` | Add/edit tenant |
| `FrmAgreementList` | Agreement list |
| `FrmAgreementEntry` | Add/edit agreement |
| `FrmRentCollection` | Collect monthly rent |
| `FrmPaymentHistory` | View payment history |
| `FrmReports` | Select and filter reports |
| `FrmReportViewer` | Display RDLC reports |
| `FrmUserManagement` | Admin user management |
| `FrmSettings` | Application settings |

### 8.4 UI Design Guidelines

- Use a fixed main navigation menu on the left or top.
- Use consistent colors, fonts, button styles, and spacing.
- Use `DataGridView` for list screens.
- Use search boxes and filter controls for large data lists.
- Use separate entry forms or panels for add/edit operations.
- Use confirmation dialogs before delete or terminate actions.
- Show friendly validation messages.
- Use role-based visibility for Admin-only screens.
- Use clear form titles and action buttons.

### 8.5 Suggested Navigation Layout

```text
+----------------------------------------------------------+
| House Rental Management System                           |
+-------------------+--------------------------------------+
| Dashboard         | Summary cards and charts             |
| Properties        |                                      |
| Tenants           | Main content area                    |
| Agreements        |                                      |
| Rent Collection   |                                      |
| Reports           |                                      |
| Users             |                                      |
| Settings          |                                      |
+-------------------+--------------------------------------+
```

## 9. Reporting Plan

### 9.1 RDLC Report Structure

Recommended report files:

```text
Reports/
|-- TenantListReport.rdlc
|-- PropertyOccupancyReport.rdlc
|-- RentCollectionReport.rdlc
|-- MonthlyDueReport.rdlc
|-- AgreementReport.rdlc
|-- IncomeSummaryReport.rdlc
```

### 9.2 Report Data Flow

```text
Report Form
   |
   v
ReportService
   |
   v
ReportRepository
   |
   v
SQL Server view/procedure/query
   |
   v
DataTable or List<ReportDto>
   |
   v
ReportViewer
```

### 9.3 Recommended Report Filters

- Date from
- Date to
- Property
- House
- Room status
- Tenant
- Payment status
- Agreement status

## 10. Security Plan

### 10.1 Authentication

- Use username and password login.
- Do not store plain text passwords.
- Store password hash, and optionally password salt.
- For a lab project, a simple SHA-256 password hash helper is acceptable.
- For professional systems, use stronger password hashing such as PBKDF2, bcrypt, or Argon2.

### 10.2 Authorization

Role-based access should be applied in both UI and BLL:

- UI hides unauthorized menu items.
- BLL rejects unauthorized actions.

### 10.3 Suggested Role Permissions

| Feature | Admin | Staff |
| --- | --- | --- |
| Dashboard | Yes | Yes |
| Properties | Yes | View/Edit |
| Tenants | Yes | Yes |
| Agreements | Yes | Yes |
| Rent Collection | Yes | Yes |
| Reports | Yes | View |
| User Management | Yes | No |
| Settings | Yes | No |
| Delete Records | Yes | Limited/No |

### 10.4 Audit Logging

Log important actions:

- Login success
- Login failed
- Add tenant
- Update tenant
- Create agreement
- Terminate agreement
- Collect rent
- Delete or deactivate records

## 11. Naming Conventions

### 11.1 Project and Namespace Names

Recommended final naming:

| Item | Recommended Name |
| --- | --- |
| Solution | `HouseRentalManagement` |
| UI Project | `HousingRental.UI` |
| BLL Project | `HousingRental.BLL` |
| DAL Project | `HousingRental.DAL` |
| Models Project | `HousingRental.Entities` |
| Root Namespace | `HousingRental` |

If you keep the current project, consider gradually renaming:

- `Housing rental` to `HouseRentalManagement`
- `Housing_rental` namespace to `HousingRental`
- `Form1` to `FrmLogin` or `FrmDashboard`

### 11.2 Class Naming

| Type | Naming Pattern | Example |
| --- | --- | --- |
| Forms | `FrmModuleAction` | `FrmTenantEntry` |
| Services | `ModuleService` | `TenantService` |
| Repositories | `ModuleRepository` | `TenantRepository` |
| Models | Singular noun | `Tenant` |
| DTOs | `NameDto` | `DashboardSummaryDto` |
| Reports | `NameReport.rdlc` | `RentCollectionReport.rdlc` |

### 11.3 Database Naming

| Object | Naming Pattern | Example |
| --- | --- | --- |
| Tables | Plural nouns | `Tenants` |
| Primary keys | Singular table name + `Id` | `TenantId` |
| Foreign keys | Referenced key name | `RoomId` |
| Views | `vw_Description` | `vw_RoomOccupancy` |
| Stored procedures | `sp_ActionName` | `sp_GetDashboardSummary` |

## 12. Error Handling and Validation

### 12.1 UI Validation

Use UI validation for simple immediate checks:

- Required fields
- Numeric amount fields
- Date range selection
- ComboBox selection

### 12.2 BLL Validation

Use BLL validation for business rules:

- Duplicate room assignment
- Agreement date logic
- Payment balance logic
- Role permission rules

### 12.3 DAL Error Handling

DAL should:

- Catch SQL exceptions only when it can add useful context.
- Let BLL or UI display user-friendly messages.
- Avoid hiding exceptions silently.

Recommended user message examples:

- "Tenant name is required."
- "This room is already occupied."
- "Payment amount cannot be greater than the due amount."
- "Unable to connect to the database. Please check SQL Server."

## 13. Version Control Plan

### 13.1 Git Recommendation

Initialize Git before development:

```powershell
git init
git add .
git commit -m "Initial WinForms project setup"
```

### 13.2 Recommended `.gitignore`

Use a Visual Studio `.gitignore` that excludes:

- `bin/`
- `obj/`
- `.vs/`
- User-specific Visual Studio files
- Build artifacts

### 13.3 Suggested Commit Flow

Good commit examples:

```text
Add project architecture plan
Add database schema scripts
Add login form and authentication service
Add property and room management module
Add tenant management module
Add rental agreement module
Add rent collection module
Add RDLC reports
Polish dashboard UI
```

## 14. Development Phases

### Phase 1: Project Foundation

Tasks:

- Initialize Git.
- Add `.gitignore`.
- Add project documentation.
- Add database connection string to `App.config`.
- Decide whether to use single-project or multi-project architecture.
- Rename `Form1` to the first real form.
- Create folders or class library projects for UI, BLL, DAL, and Models.

Deliverables:

- Clean project structure.
- Working application startup.
- Database connection helper.

### Phase 2: Database Setup

Tasks:

- Create database tables.
- Add foreign keys.
- Add constraints.
- Add seed data for roles and default admin user.
- Add basic views for dashboard and reports.

Deliverables:

- SQL scripts in `Database` folder.
- Database can be recreated from scripts.
- Default login account works.

### Phase 3: Authentication and Main UI

Tasks:

- Build login form.
- Implement `AuthService`.
- Implement `UserRepository`.
- Add password hashing.
- Add dashboard shell.
- Add navigation menu.
- Apply role-based menu visibility.

Deliverables:

- Admin and Staff login.
- Main dashboard opens after login.
- Unauthorized features are hidden.

### Phase 4: Property, House, and Room Module

Tasks:

- Build model classes.
- Build repositories.
- Build services.
- Build forms.
- Add add/edit/search/delete or deactivate actions.
- Add room availability status logic.

Deliverables:

- Manage property hierarchy.
- Room list shows correct status.

### Phase 5: Tenant Module

Tasks:

- Build tenant model.
- Build tenant repository.
- Build tenant service.
- Build tenant list and entry forms.
- Add search and validation.

Deliverables:

- Tenant records can be managed.
- Tenant detail view can show related agreements and payments.

### Phase 6: Rental Agreement Module

Tasks:

- Build agreement model.
- Build agreement repository.
- Build agreement service.
- Build agreement forms.
- Enforce active room rule.
- Update room status when agreement becomes active or ends.

Deliverables:

- Agreements can be created, renewed, expired, and terminated.
- Room occupancy is controlled by agreement status.

### Phase 7: Rent Collection Module

Tasks:

- Build payment model.
- Build payment repository.
- Build payment service.
- Build rent collection form.
- Add receipt number generation.
- Add payment history.
- Add balance calculation.

Deliverables:

- Monthly payments can be recorded.
- Paid, partial, due, and overdue statuses work.
- Receipt data can be printed or displayed.

### Phase 8: Reports

Tasks:

- Add ReportViewer package/reference.
- Create RDLC files.
- Create report DTOs.
- Create report data queries.
- Add report filters.

Deliverables:

- Tenant report.
- Rent collection report.
- Occupancy report.
- Monthly due report.
- Agreement report.

### Phase 9: Polish and Final Submission

Tasks:

- Improve UI layout.
- Add icons and consistent colors.
- Add validation messages.
- Test all workflows.
- Prepare screenshots.
- Prepare project report.
- Prepare database backup or SQL scripts.

Deliverables:

- Final runnable application.
- Database scripts.
- Documentation.
- Lab presentation/demo materials.

## 15. Suggested Minimum Viable Project Scope

If time is limited, complete these modules first:

1. Login with Admin and Staff roles
2. Dashboard summary cards
3. Property, house, and room management
4. Tenant management
5. Rental agreement management
6. Monthly rent collection
7. Rent collection report

Optional after MVP:

- Maintenance
- Expenses
- Audit logs
- Advanced charts
- Backup/restore helper screen

## 16. Recommended Final Submission Package

For university submission, prepare:

```text
HouseRentalManagement_Submission/
|-- SourceCode/
|-- Database/
|   |-- CreateDatabase.sql
|   |-- CreateTables.sql
|   |-- SeedData.sql
|-- Documentation/
|   |-- ProjectArchitecture.md
|   |-- UserManual.pdf or UserManual.docx
|   |-- Screenshots/
|-- README.md
```

README should include:

- Project title
- Technology stack
- Setup instructions
- Database setup instructions
- Default login credentials
- Feature list
- Screenshots
- Author/student information

## 17. Testing Plan

### 17.1 Manual Test Cases

| Test Case | Expected Result |
| --- | --- |
| Login with valid Admin | Dashboard opens with all menus |
| Login with valid Staff | Dashboard opens with limited menus |
| Login with wrong password | Error message shown |
| Add property | Property appears in list |
| Add house under property | House appears under selected property |
| Add room under house | Room appears with Available status |
| Create agreement for available room | Agreement becomes active and room becomes Occupied |
| Try to rent occupied room | System blocks action |
| Collect full rent | Payment status becomes Paid |
| Collect partial rent | Payment status becomes Partial |
| View rent report | Report displays filtered records |

### 17.2 Important Edge Cases

- SQL Server not running
- Duplicate username
- Duplicate receipt number
- Agreement end date before start date
- Payment amount greater than due amount
- Deleting a room with active agreement
- Staff trying to access Admin-only form

## 18. Quality Checklist

Before final submission:

- The project opens in Visual Studio 2022.
- The solution builds without errors.
- SQL scripts run successfully in SSMS.
- Connection string points to `.\SQLEXPRESS`.
- Default admin login works.
- Forms have meaningful names.
- No default `Form1` title remains.
- UI is consistent and readable.
- DAL uses parameterized SQL queries.
- BLL contains business validation.
- Reports load without errors.
- Git repository excludes `bin`, `obj`, and `.vs`.
- Documentation explains the architecture clearly.

## 19. Recommended Immediate Next Steps

1. Add a Visual Studio `.gitignore`.
2. Initialize Git.
3. Add the connection string to `App.config`.
4. Rename `Form1` to `FrmLogin`.
5. Create `Models`, `DAL`, `BLL`, `Forms`, `Reports`, and `Database` folders.
6. Create SQL scripts for tables and seed data.
7. Implement login first.
8. Build the dashboard shell second.
9. Build modules in this order: properties, tenants, agreements, payments, reports.

## 20. Final Architecture Summary

The House Rental Management System should be built as a clean C# WinForms application using SQL Server and ADO.NET. The UI layer should only manage screens and user interaction. The BLL should contain business rules such as room availability, agreement validation, payment calculation, and role authorization. The DAL should handle all SQL Server operations using parameterized ADO.NET commands. SQL Server should store users, roles, properties, houses, rooms, tenants, agreements, rent payments, maintenance, expenses, audit logs, and settings.

This plan gives the project a professional structure while staying realistic for a university lab submission.
