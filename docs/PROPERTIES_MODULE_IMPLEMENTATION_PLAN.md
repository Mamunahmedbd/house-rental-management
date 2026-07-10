# Properties Module Implementation Plan

## Document Control

| Item | Details |
| --- | --- |
| Project | House Rental Management System |
| Module | Properties, Houses, and Rooms |
| Application Type | C# Windows Forms Desktop Application |
| Framework | .NET Framework 4.7.2 |
| Architecture | Single-project 3-layer architecture |
| Database | SQL Server Express, `HouseRentalDB` |
| Data Access | ADO.NET with parameterized SQL |
| Target Location | `Forms/Properties`, `BLL/PropertyService.cs`, `DAL/PropertyRepository.cs`, `Models`, `Database` |

## 1. Purpose

The Properties module will manage the physical rental inventory of the system. It will provide a clean workflow for creating and maintaining:

- Properties, such as buildings, hostels, compounds, or apartment groups.
- Houses or units inside a property.
- Rooms inside a house or unit.
- Room rent, type, availability, occupancy, maintenance state, and inactive state.

This module is a core dependency for rental agreements, rent collection, dashboard statistics, occupancy reporting, and maintenance tracking.

## 2. Current Project Analysis

### 2.1 Existing Project Structure

The project currently uses a single WinForms project with organized folders:

```text
housing_rental/
|-- App.config
|-- ApplicationSessionContext.cs
|-- Program.cs
|-- Housing rental.csproj
|-- Assets/
|-- BLL/
|-- DAL/
|-- Database/
|-- Forms/
|-- Models/
|-- Properties/
|-- Reports/
|-- docs/
```

The current structure already follows the intended 3-layer layout:

| Layer | Existing Folder | Current Role |
| --- | --- | --- |
| UI | `Forms` | Login, dashboard, admin user management, placeholders |
| BLL | `BLL` | Authentication, users, dashboard, validation services |
| DAL | `DAL` | SQL Server repositories and helpers |
| Models | `Models` | Entity classes and service result wrapper |
| Database | `Database` | SQL scripts, views, stored procedures, seed data |

### 2.2 Important Existing Files

| File | Relevance to Properties Module |
| --- | --- |
| `Models/Property.cs` | Existing property entity |
| `Models/House.cs` | Existing house or unit entity |
| `Models/Room.cs` | Existing room entity |
| `BLL/PropertyService.cs` | Currently validates room input only |
| `DAL/DbConnectionFactory.cs` | Creates SQL Server connections from `App.config` |
| `DAL/SqlHelper.cs` | Shared parameter and DataTable helper |
| `Forms/Dashboard/FrmDashboard.cs` | Sidebar has a Properties button, currently opens a placeholder |
| `Forms/Common/ModulePlaceholderControl.cs` | Temporary placeholder used for unfinished modules |
| `Database/02_CreateTables.sql` | Defines `Properties`, `Houses`, and `Rooms` tables |
| `Database/03_CreateViews.sql` | Defines `vw_RoomOccupancy` |
| `Database/04_CreateStoredProcedures.sql` | Dashboard and reporting procedures |

### 2.3 Current Implementation Status

| Area | Current Status |
| --- | --- |
| Property model | Exists |
| House model | Exists |
| Room model | Exists |
| Property BLL | Partial, only `ValidateRoom` exists |
| Property DAL repository | Missing |
| Property UI control | Missing |
| Dashboard navigation | Exists, but opens a placeholder |
| SQL tables | Exists |
| SQL room status constraint | Exists |
| Occupancy view | Exists |
| Audit logging repository | Exists |

### 2.4 Naming Note

The root `Properties/` folder is the standard Visual Studio project metadata folder and should not be used for rental property forms. Rental property screens should be placed under:

```text
Forms/Properties/
```

This keeps .NET project resources separate from the business module.

## 3. Target Module Architecture

### 3.1 Module Flow

```text
FrmDashboard
    |
    v
PropertyManagementControl
    |
    v
PropertyService
    |
    v
PropertyRepository
    |
    v
SQL Server: Properties, Houses, Rooms, RentalAgreements
```

### 3.2 Layer Responsibilities

| Layer | Responsibility |
| --- | --- |
| UI | Display property, house, and room lists; collect input; show validation messages |
| BLL | Validate data, enforce business rules, coordinate audit logging |
| DAL | Run SQL queries, map rows to models, handle data persistence |
| Database | Enforce relational integrity, check constraints, indexes, occupancy view |

## 4. Domain Model

### 4.1 Property

Purpose:

Represents the top-level rental asset, such as a building, hostel, compound, or apartment property.

Existing model:

```csharp
public class Property
{
    public int PropertyId { get; set; }
    public string PropertyName { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Required business rules:

- Property name is required.
- Property name should be unique within the same city when possible.
- Inactive properties should not appear as selectable values in new house or room workflows.
- A property with active houses, rooms, or active agreements should be deactivated instead of physically deleted.

### 4.2 House

Purpose:

Represents a house, flat, floor unit, or rental unit under a property.

Existing model:

```csharp
public class House
{
    public int HouseId { get; set; }
    public int PropertyId { get; set; }
    public string HouseName { get; set; }
    public string FloorNo { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Required business rules:

- House name is required.
- A valid active property is required.
- House name should not be duplicated under the same property.
- Inactive houses should not be selectable for new room creation.

### 4.3 Room

Purpose:

Represents the rentable unit assigned to a tenant through a rental agreement.

Existing model:

```csharp
public class Room
{
    public int RoomId { get; set; }
    public int HouseId { get; set; }
    public string RoomNo { get; set; }
    public string RoomType { get; set; }
    public decimal MonthlyRent { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Supported room statuses:

| Status | Meaning | Can Be Used In New Agreement |
| --- | --- | --- |
| Available | Room is ready to rent | Yes |
| Occupied | Room has an active agreement | No |
| Maintenance | Room is temporarily unavailable | No |
| Inactive | Room is removed from active operation | No |

Required business rules:

- Room number is required.
- A valid active house is required.
- Monthly rent must be greater than zero.
- Room number should not be duplicated under the same house.
- Occupied rooms cannot be manually changed to Available while an active agreement exists.
- Rooms with active agreements cannot be deactivated without first terminating or expiring the agreement.

## 5. Database Implementation Plan

### 5.1 Existing Tables

The database script already defines:

```text
Properties
Houses
Rooms
RentalAgreements
```

Existing relationship:

```text
Properties 1 -> many Houses
Houses     1 -> many Rooms
Rooms      1 -> many RentalAgreements
```

Existing database safeguards:

- `FK_Houses_Properties`
- `FK_Rooms_Houses`
- `CK_Rooms_MonthlyRent`
- `CK_Rooms_Status`
- `UX_RentalAgreements_OneActiveRoom`

### 5.2 Recommended Database Improvements

Add uniqueness indexes to prevent duplicate names in the hierarchy:

```sql
CREATE UNIQUE INDEX UX_Properties_PropertyName_City
ON dbo.Properties(PropertyName, City)
WHERE IsActive = 1;

CREATE UNIQUE INDEX UX_Houses_PropertyId_HouseName
ON dbo.Houses(PropertyId, HouseName)
WHERE IsActive = 1;

CREATE UNIQUE INDEX UX_Rooms_HouseId_RoomNo
ON dbo.Rooms(HouseId, RoomNo)
WHERE Status <> 'Inactive';
```

These indexes should be added only after checking existing seed or test data for duplicates.

### 5.3 Recommended View Enhancements

The existing `vw_RoomOccupancy` is correct for occupancy listing. For better UI performance, use it for read-only grid screens because it already joins:

- Property
- House
- Room
- Active agreement
- Tenant

Recommended extra columns for later:

- `p.City`
- `h.FloorNo`
- `r.Description AS RoomDescription`
- `DATEDIFF(DAY, GETDATE(), a.EndDate) AS DaysUntilAgreementEnd`

### 5.4 Physical Delete Policy

Use soft deactivation instead of delete:

| Entity | Deactivation Field |
| --- | --- |
| Property | `IsActive = 0` |
| House | `IsActive = 0` |
| Room | `Status = 'Inactive'` |

Physical deletes should not be used in normal UI workflows because agreement, payment, audit, and reporting history depend on stable records.

## 6. DAL Implementation Plan

Create:

```text
DAL/PropertyRepository.cs
```

### 6.1 Repository Responsibilities

The repository should:

- Use `DbConnectionFactory.CreateConnection()`.
- Use `SqlCommand` and `SqlDataReader` for typed model lists.
- Use parameterized SQL only.
- Return model objects, primitive IDs, or booleans.
- Avoid validation rules that belong in BLL.

### 6.2 Required Methods

Property methods:

| Method | Purpose |
| --- | --- |
| `List<Property> SearchProperties(string searchText, bool includeInactive)` | Load property grid |
| `Property GetPropertyById(int propertyId)` | Load one property for editing |
| `bool PropertyNameExists(string propertyName, string city, int excludedPropertyId)` | Prevent duplicates |
| `int CreateProperty(Property property)` | Insert and return new ID |
| `void UpdateProperty(Property property)` | Update editable fields |
| `void SetPropertyActiveStatus(int propertyId, bool isActive)` | Activate or deactivate |
| `bool PropertyHasActiveRoomsOrAgreements(int propertyId)` | Protect deactivation workflow |

House methods:

| Method | Purpose |
| --- | --- |
| `List<House> SearchHouses(int? propertyId, string searchText, bool includeInactive)` | Load house grid |
| `List<House> GetActiveHousesByPropertyId(int propertyId)` | Populate house ComboBox |
| `House GetHouseById(int houseId)` | Load one house |
| `bool HouseNameExists(int propertyId, string houseName, int excludedHouseId)` | Prevent duplicates |
| `int CreateHouse(House house)` | Insert and return new ID |
| `void UpdateHouse(House house)` | Update editable fields |
| `void SetHouseActiveStatus(int houseId, bool isActive)` | Activate or deactivate |
| `bool HouseHasActiveRoomsOrAgreements(int houseId)` | Protect deactivation workflow |

Room methods:

| Method | Purpose |
| --- | --- |
| `List<Room> SearchRooms(int? propertyId, int? houseId, string status, string searchText)` | Load room grid |
| `DataTable GetRoomOccupancy(int? propertyId, int? houseId, string status, string searchText)` | Load joined occupancy grid |
| `List<Room> GetAvailableRooms()` | Populate agreement form |
| `Room GetRoomById(int roomId)` | Load one room |
| `bool RoomNoExists(int houseId, string roomNo, int excludedRoomId)` | Prevent duplicates |
| `bool RoomHasActiveAgreement(int roomId)` | Protect status changes |
| `int CreateRoom(Room room)` | Insert and return new ID |
| `void UpdateRoom(Room room)` | Update editable fields |
| `void SetRoomStatus(int roomId, string status)` | Change availability state |

### 6.3 Mapping Standards

Use private mapper methods:

```csharp
private static Property MapProperty(SqlDataReader reader)
private static House MapHouse(SqlDataReader reader)
private static Room MapRoom(SqlDataReader reader)
```

Mapping should follow the existing style in `UserRepository.cs`.

### 6.4 Query Standards

Use explicit column lists:

```sql
SELECT
    PropertyId,
    PropertyName,
    Address,
    City,
    Description,
    IsActive,
    CreatedAt
FROM dbo.Properties
WHERE ...
ORDER BY IsActive DESC, PropertyName ASC;
```

Avoid `SELECT *` so grid behavior remains stable when schema changes.

## 7. BLL Implementation Plan

Update:

```text
BLL/PropertyService.cs
```

### 7.1 Service Responsibilities

The service should:

- Validate all property, house, and room input.
- Call `PropertyRepository`.
- Return `ServiceResult` or `ServiceResult<T>`.
- Prevent unsafe status changes.
- Log important actions through `AuditRepository`.
- Keep UI free of business decisions.

### 7.2 Required Validation Methods

| Method | Purpose |
| --- | --- |
| `ValidateProperty(Property property)` | Required fields and formatting |
| `ValidateHouse(House house)` | Required fields and parent property |
| `ValidateRoom(Room room)` | Existing method, expand with status validation |

### 7.3 Required Workflow Methods

Property workflows:

| Method | Purpose |
| --- | --- |
| `SearchProperties(string searchText, bool includeInactive)` | Load property list |
| `CreateProperty(Property property)` | Validate, duplicate check, save, audit |
| `UpdateProperty(Property property)` | Validate, duplicate check, save, audit |
| `SetPropertyActiveStatus(int propertyId, bool isActive)` | Protect and update active status |

House workflows:

| Method | Purpose |
| --- | --- |
| `SearchHouses(int? propertyId, string searchText, bool includeInactive)` | Load house list |
| `GetActiveHousesByPropertyId(int propertyId)` | UI ComboBox data |
| `CreateHouse(House house)` | Validate, duplicate check, save, audit |
| `UpdateHouse(House house)` | Validate, duplicate check, save, audit |
| `SetHouseActiveStatus(int houseId, bool isActive)` | Protect and update active status |

Room workflows:

| Method | Purpose |
| --- | --- |
| `SearchRooms(int? propertyId, int? houseId, string status, string searchText)` | Load room list |
| `GetRoomOccupancy(...)` | Load occupancy grid |
| `GetAvailableRooms()` | Agreement module support |
| `CreateRoom(Room room)` | Validate, duplicate check, save, audit |
| `UpdateRoom(Room room)` | Validate, duplicate check, save, audit |
| `SetRoomStatus(int roomId, string status)` | Enforce active agreement rules |

### 7.4 Business Rule Matrix

| Operation | Rule |
| --- | --- |
| Create property | Name required and not duplicate |
| Update property | Existing ID required and name not duplicate |
| Deactivate property | Block or warn if active rooms or agreements exist |
| Create house | Active property required |
| Update house | House name not duplicate under same property |
| Deactivate house | Block or warn if active rooms or agreements exist |
| Create room | Active house required, rent greater than zero |
| Update room | Cannot duplicate room number under same house |
| Set room Available | Block if active agreement exists |
| Set room Occupied | Should normally be performed by agreement workflow |
| Set room Maintenance | Allow only when no active agreement exists |
| Set room Inactive | Allow only when no active agreement exists |

### 7.5 Audit Logging

Use `AuditRepository.Add` for:

- `CreateProperty`
- `UpdateProperty`
- `ActivateProperty`
- `DeactivateProperty`
- `CreateHouse`
- `UpdateHouse`
- `ActivateHouse`
- `DeactivateHouse`
- `CreateRoom`
- `UpdateRoom`
- `SetRoomStatus`

Recommended audit description format:

```text
Created room '101' under HouseId 3.
Changed room '101' status from Available to Maintenance.
```

Use `CurrentSession.User.UserId` when a user is logged in.

## 8. UI Implementation Plan

### 8.1 Recommended UI Type

Use a `UserControl` instead of separate top-level forms because the dashboard currently loads modules into `pnlContent`.

Create:

```text
Forms/Properties/PropertyManagementControl.cs
Forms/Properties/PropertyManagementControl.Designer.cs
```

Then update `FrmDashboard.BtnProperties_Click` to load the control:

```csharp
NavigateToControl("Properties", new PropertyManagementControl());
```

### 8.2 Main Screen Layout

Recommended layout:

```text
+-------------------------------------------------------------+
| Filters: Search [________] Property [v] House [v] Status [v] |
+-------------------------------------------------------------+
| Tabs: Properties | Houses / Units | Rooms | Occupancy        |
+-------------------------------------------------------------+
| Left or top grid area                                        |
| DataGridView with selected tab data                          |
+-------------------------------------------------------------+
| Right or bottom editor panel                                 |
| Fields, Save, New, Refresh, Activate/Deactivate/Status       |
+-------------------------------------------------------------+
| Status message                                                |
+-------------------------------------------------------------+
```

### 8.3 Tabs

| Tab | Purpose |
| --- | --- |
| Properties | Add, edit, search, activate, deactivate properties |
| Houses / Units | Add and edit houses under selected property |
| Rooms | Add and edit rooms under selected house |
| Occupancy | Read-only joined grid from `vw_RoomOccupancy` |

### 8.4 Property Tab Fields

| Control | Field |
| --- | --- |
| TextBox | Property name |
| TextBox | Address |
| TextBox | City |
| TextBox multiline | Description |
| CheckBox | Is active |

Actions:

- New
- Save
- Refresh
- Activate / Deactivate

### 8.5 House Tab Fields

| Control | Field |
| --- | --- |
| ComboBox | Property |
| TextBox | House name |
| TextBox | Floor number |
| TextBox multiline | Description |
| CheckBox | Is active |

Actions:

- New
- Save
- Refresh
- Activate / Deactivate

### 8.6 Room Tab Fields

| Control | Field |
| --- | --- |
| ComboBox | Property filter |
| ComboBox | House |
| TextBox | Room number |
| ComboBox | Room type |
| NumericUpDown or validated TextBox | Monthly rent |
| ComboBox | Status |
| TextBox multiline | Description |

Room type options:

- Single
- Double
- Master
- Shared
- Studio
- Other

Actions:

- New
- Save
- Refresh
- Set Available
- Set Maintenance
- Set Inactive

### 8.7 Occupancy Tab Columns

Use `vw_RoomOccupancy` and show:

| Column | Source |
| --- | --- |
| Property | `PropertyName` |
| House | `HouseName` |
| Room | `RoomNo` |
| Type | `RoomType` |
| Rent | `MonthlyRent` |
| Room Status | `RoomStatus` |
| Tenant | `TenantName` |
| Agreement No | `AgreementNo` |
| Start Date | `StartDate` |
| End Date | `EndDate` |
| Agreement Status | `AgreementStatus` |

### 8.8 UI Behavior

| Behavior | Requirement |
| --- | --- |
| Grid selection | Loads selected record into editor panel |
| New button | Clears editor and switches to create mode |
| Save button | Creates or updates based on selected ID |
| Search | Filters current tab without reloading whole form |
| Parent selection | Selecting property filters houses; selecting house filters rooms |
| Status messages | Use green for success and red for validation errors |
| Confirmation | Required before deactivation or status changes |
| Admin/Staff access | Staff can manage inventory; Admin can deactivate |

### 8.9 Visual Style

Match the existing dashboard and user management style:

- Font: Segoe UI.
- Background: light neutral surface.
- Main panels: white.
- Primary action: blue.
- Success status: green.
- Error status: red.
- Use `DataGridView` with readable column widths.
- Use consistent spacing and labels.

## 9. Integration Plan

### 9.1 Dashboard Integration

Current dashboard summary already uses:

- Total properties
- Total rooms
- Available rooms
- Occupied rooms

After implementing the Properties module, these values should update after:

- Creating a property.
- Activating or deactivating a property.
- Creating a room.
- Changing room status.
- Creating or ending an agreement.

### 9.2 Agreement Module Integration

The agreement module should use:

```csharp
PropertyService.GetAvailableRooms()
```

Agreement creation should:

- Allow only rooms with `Status = 'Available'`.
- Set selected room to `Occupied` when agreement becomes active.
- Set room back to `Available` when agreement is terminated or expired and no other active agreement exists.

### 9.3 Reports Integration

The occupancy report should use:

```sql
dbo.vw_RoomOccupancy
```

Recommended report filters:

- Property
- House
- Room status
- Occupancy status
- Date range for agreement end date

### 9.4 Maintenance Integration

Maintenance requests can link to:

- `PropertyId`
- `RoomId`

When a room is under maintenance:

- Room status should be changed to `Maintenance`.
- New agreements should not allow that room.

## 10. Implementation Phases

### Phase 1: Repository Foundation

Tasks:

- Create `DAL/PropertyRepository.cs`.
- Add search and CRUD methods for properties.
- Add search and CRUD methods for houses.
- Add search and CRUD methods for rooms.
- Add mapper methods.
- Add duplicate-check methods.
- Add active-agreement protection methods.

Deliverable:

- DAL can read and write `Properties`, `Houses`, and `Rooms` without UI involvement.

### Phase 2: Service Layer

Tasks:

- Expand `BLL/PropertyService.cs`.
- Add validation for properties and houses.
- Expand room validation.
- Add `ServiceResult<T>` workflow methods.
- Add duplicate checks.
- Add audit logging.
- Add room status protection.

Deliverable:

- UI can call service methods and receive friendly success or validation messages.

### Phase 3: UI Control

Tasks:

- Create `Forms/Properties/PropertyManagementControl.cs`.
- Create designer layout with tabs.
- Add BindingSource objects for each grid.
- Add ComboBox loading for active properties and houses.
- Add save, new, refresh, search, and status handlers.
- Add grid selection behavior.

Deliverable:

- Users can manage the full Property -> House -> Room hierarchy from the dashboard.

### Phase 4: Dashboard Navigation

Tasks:

- Update `FrmDashboard.BtnProperties_Click`.
- Replace placeholder module with `PropertyManagementControl`.
- Ensure refresh behavior remains clear.

Deliverable:

- Clicking the sidebar Properties button opens the real module.

### Phase 5: Database Hardening

Tasks:

- Add optional unique indexes after duplicate-data check.
- Consider adding extra columns to `vw_RoomOccupancy`.
- Re-run database scripts on a clean database.

Deliverable:

- Database protects important hierarchy and duplicate rules.

### Phase 6: Testing and Polish

Tasks:

- Test all manual workflows.
- Verify dashboard counts after property and room changes.
- Verify agreement protection logic for occupied rooms.
- Improve DataGridView columns and formatting.
- Add final comments only where logic is not obvious.

Deliverable:

- Professional, stable Properties module ready for demonstration.

## 11. Suggested Development Order

1. Implement `PropertyRepository`.
2. Expand `PropertyService`.
3. Build property tab UI.
4. Build house tab UI.
5. Build room tab UI.
6. Build occupancy tab UI.
7. Replace dashboard placeholder.
8. Add audit logging.
9. Add database hardening indexes.
10. Run manual tests.

This order keeps risk low because each child workflow depends on its parent data.

## 12. Manual Test Plan

### 12.1 Property Tests

| Test | Expected Result |
| --- | --- |
| Create property with valid name | Property is saved and shown in grid |
| Create property without name | Validation message appears |
| Create duplicate property in same city | System blocks duplicate |
| Edit property address | Updated value appears after refresh |
| Deactivate property with no active rooms | Property becomes inactive |
| Deactivate property with active agreement | System blocks or warns according to final rule |

### 12.2 House Tests

| Test | Expected Result |
| --- | --- |
| Create house under active property | House is saved |
| Create house without property | Validation message appears |
| Create duplicate house under same property | System blocks duplicate |
| Create same house name under different property | Allowed |
| Deactivate house with active room agreement | System blocks or warns |

### 12.3 Room Tests

| Test | Expected Result |
| --- | --- |
| Create room under active house | Room is saved as Available |
| Create room with zero rent | Validation message appears |
| Create duplicate room number under same house | System blocks duplicate |
| Set Available room to Maintenance | Status changes |
| Set Maintenance room to Available | Status changes |
| Set Occupied room to Available while active agreement exists | System blocks action |
| Set room to Inactive with active agreement | System blocks action |

### 12.4 Integration Tests

| Test | Expected Result |
| --- | --- |
| Add new room | Dashboard total rooms increases |
| Set room Available | Dashboard available rooms increases |
| Create active agreement | Room becomes Occupied |
| Open occupancy tab | Tenant and agreement show beside occupied room |
| Open agreement form | Only Available rooms are selectable |

## 13. Acceptance Criteria

The Properties module is complete when:

- The dashboard Properties button opens a real management screen.
- Properties can be created, edited, searched, activated, and deactivated.
- Houses can be created, edited, searched, activated, and deactivated under properties.
- Rooms can be created, edited, searched, and moved between allowed statuses.
- Active agreement rules prevent unsafe room status changes.
- Occupancy can be viewed from joined property, house, room, tenant, and agreement data.
- All SQL uses parameters.
- All important validation lives in `PropertyService`.
- The UI does not directly execute SQL.
- Audit logs are created for major create, update, and status-change actions.
- Dashboard counts remain consistent after module operations.

## 14. Quality Checklist

Before marking the module complete:

- `PropertyRepository.cs` follows existing `UserRepository.cs` style.
- `PropertyService.cs` returns `ServiceResult` and `ServiceResult<T>` consistently.
- UI labels, button text, and grid columns are clear.
- No business forms are placed in the root Visual Studio `Properties/` folder.
- No SQL string uses concatenated user input.
- No physical delete exists in normal workflows.
- Room status values match database check constraint exactly.
- ComboBoxes bind to active parent records only.
- Empty grids show friendly status messages.
- Validation messages are understandable for non-technical users.
- The project builds in Visual Studio.

## 15. Future Enhancements

After the core module is stable, consider:

- Room photo or document attachment support.
- Bulk room creation for large properties.
- Import property inventory from Excel.
- Color-coded occupancy map.
- Maintenance calendar by room.
- Rent recommendation history per room.
- Property profitability report using rent income minus expenses.

## 16. Final Recommendation

Implement the Properties module as a dashboard-loaded `UserControl` backed by a dedicated `PropertyService` and `PropertyRepository`. Keep the hierarchy explicit, protect occupied rooms from unsafe status changes, and use `vw_RoomOccupancy` for read-only operational visibility.

This approach fits the current project structure, preserves the existing 3-layer architecture, and creates an industrial-quality foundation for agreements, payments, reports, and maintenance.
