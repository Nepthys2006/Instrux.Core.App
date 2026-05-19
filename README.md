# Instrux — Teacher Productivity Hub

A WPF desktop application built for teachers to manage attendance, grades, materials, calendar planning, and personal task flow. Designed around the Philippine DepEd Order No. 8, s. 2015 grading system.

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                 Instrux.Application                  │
│  WPF UI (Views, ViewModels, Converters, Resources)  │
│  DataService (ObservableCollections), SessionService │
├─────────────────────────────────────────────────────┤
│                 Instrux.Services                      │
│  DTOs, Service Interfaces, Implementations, Mapper   │
├─────────────────────────────────────────────────────┤
│               Instrux.Infrastructure                  │
│  EF Core DbContext, Entity Configurations, Migrations│
├─────────────────────────────────────────────────────┤
│                  Instrux.Domain                       │
│  Models, Enums (pure C# — no dependencies)           │
└─────────────────────────────────────────────────────┘
```

The application follows a **layered architecture** with strict dependency direction: `Application → Services → Infrastructure → Domain`. Each layer depends only on the layers below it.

### Key Patterns

| Pattern | Implementation |
|---|---|
| **DI** | `Microsoft.Extensions.DependencyInjection` via `Host.CreateDefaultBuilder` |
| **Data access** | DbContext injected directly into service implementations (no Repository pattern) |
| **UI sync** | Singleton `DataService` holds `ObservableCollection<T>` — ViewModels bind to it |
| **Mapping** | Manual static `DtoMapper.cs` (no AutoMapper) |
| **Async** | `RelayCommandAsync` for all service calls |
| **Navigation** | Manual view swapping via `MainDashboardViewModel.CurrentPage` |

---

## Tech Stack

| Layer | Technology | Version |
|---|---|---|
| **Language** | C# | 12.0 |
| **Runtime** | .NET | 10.0 |
| **UI Framework** | WPF (Windows) | — |
| **ORM** | Entity Framework Core | 10.0.8 |
| **Database** | SQL Server LocalDB | — |
| **DI Container** | Microsoft.Extensions.Hosting | 10.0.8 |
| **UI Library** | MaterialDesignThemes | 5.3.2 |
| **SVG Rendering** | SharpVectors.Wpf | 1.8.5 |
| **Configuration** | `appsettings.json` | — |

### NuGet Packages

| Package | Used In |
|---|---|
| `Microsoft.EntityFrameworkCore` | Infrastructure |
| `Microsoft.EntityFrameworkCore.SqlServer` | Infrastructure |
| `Microsoft.EntityFrameworkCore.Tools` | Infrastructure |
| `Microsoft.Extensions.Configuration.Json` | Application |
| `Microsoft.Extensions.Hosting` | Application |
| `MaterialDesignThemes` | Application |
| `SharpVectors.Wpf` | Application |

---

## Database Schema

All entities are mapped to SQL Server LocalDB via EF Core fluent configuration. Enums are stored as strings for readability.

### Tables

| Table | Key Fields | Notes |
|---|---|---|
| **Teachers** | `Id`, `FullName`, `Nickname`, `Email` (unique), `PasswordHash` | Application identity |
| **Classes** | `Id`, `Name`, `Section`, `Subject`, `SchoolYear`, `Semester`, `CoverColor`, `TeacherId` | Indexed on `TeacherId` |
| **Students** | `Id`, `FullName`, `StudentId`, `Email`, `ClassId` | Unique composite index on (`ClassId`, `StudentId`) |
| **AttendanceRecords** | `Id`, `StudentId`, `Date`, `Status`, `Note` | Unique composite index on (`StudentId`, `Date`) |
| **Assessments** | `Id`, `ClassId`, `Name`, `Type`, `MaxScore`, `Weight`, `Date` | Indexed on `ClassId` |
| **Scores** | `Id`, `StudentId`, `AssessmentId`, `Value` | Unique composite index on (`StudentId`, `AssessmentId`) |
| **CalendarEvents** | `Id`, `TeacherId`, `Title`, `Date`, `StartTime`, `EndTime`, `Category`, `LinkedClassId`, `Notes` | Composite index on (`TeacherId`, `Date`) |
| **TodoItems** | `Id`, `TeacherId`, `Title`, `DueDate`, `Priority`, `IsCompleted`, `CompletedAt`, `LinkedClassId`, `IsRecurring`, `Recurrence` | Composite index on (`TeacherId`, `DueDate`) |
| **ContentItems** | `Id`, `ClassId`, `FolderId`, `Title`, `Description`, `Type`, `FilePath`, `UploadedAt`, `IsVisible` | Indexed on `ClassId` |
| **GradingConfigs** | `Id`, `Subject` (unique), `Group`, `WrittenWorksWeight`, `PerformanceTasksWeight`, `QuarterlyAssessmentWeight` | Reference data for DepEd grading |

### Entity Relationships

```
Teacher ──1:N──> Classes ──1:N──> Students ──1:N──> AttendanceRecords
                                    │
Classes ──1:N──> Assessments ──1:N──┼──> Scores
Classes ──1:N──> ContentItems        │
                   └── Students ────┘
Teacher ──1:N──> CalendarEvents
Teacher ──1:N──> TodoItems
```

---

## Project Structure

```
Instrux/
├── Instrux.Domain/                    # Pure domain layer
│   ├── Enums/                         # 8 enums (Subject, AssessmentType, etc.)
│   └── Models/                        # 10 domain models
│
├── Instrux.Infrastructure/            # EF Core data access
│   └── Data/
│       ├── Configurations/            # 10 entity type configurations
│       ├── Migrations/                # EF Core migrations
│       ├── InstruxDbContext.cs        # 10 DbSets
│       └── InstruxDesignTimeDbContextFactory.cs
│
├── Instrux.Services/                  # Business logic layer
│   ├── DTOs/                          # 19 record DTOs
│   ├── Implementations/               # 9 service implementations
│   ├── Interfaces/                    # 9 service interfaces
│   ├── Mapping/                       # DtoMapper.cs (static manual mapper)
│   └── Resolvers/                     # GradingSystemResolver.cs
│
├── Instrux.Application/               # WPF UI layer
│   ├── Converters/                    # 6 value converters
│   ├── Helpers/                       # RelayCommand, RelayCommandAsync, ViewModelBase
│   ├── Resources/                     # SVG icons, logo
│   │   └── MainLogo/                  # Instrux brand logo
│   ├── Services/                      # DataService, SessionService
│   ├── ViewModels/                    # 11 ViewModels
│   ├── Views/                         # 7 UserControls + 2 Windows
│   ├── App.xaml / App.xaml.cs         # DI setup, auth flow
│   ├── AuthenticationWindow.xaml      # Login/register modal
│   ├── MainWindow.xaml                # Shell with sidebar + content frame
│   └── appsettings.json               # Connection string
│
├── Instrux.sln                        # Solution file (4 projects)
├── AGENTS.md                          # Build tracker + architecture decisions
├── DESIGN.md                          # Full design system specification
└── README.md                          # This file
```

---

## Grading System

Implements **DepEd Order No. 8, s. 2015** with automatic weight assignment per subject:

| Assessment Type | Category | Weight Range |
|---|---|---|
| Quiz | Written Works (WW) | 20–40% |
| Activity | Performance Tasks (PT) | 40–60% |
| Exam | Quarterly Assessment (QA) | 20–40% |

**Formula:** `Grade = (WW_avg × WW%) + (PT_avg × PT%) + (QA_avg × QA%)`

### Subject Weight Table

| Subject | WW | PT | QA |
|---|---|---|---|
| Mathematics, Science, English, Filipino, AralingPanlipunan | 20% | 60% | 20% |
| MAPEH, ValuesEducation | 30% | 50% | 20% |
| EdukasyonSaPagpapakatao | 40% | 40% | 20% |

---

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQL Server LocalDB (comes with Visual Studio or SQL Server Express)

### Setup

```bash
# Clone the repo
git clone https://github.com/Nepthys2006/Instrux.Core.App.git
cd Instrux.Core.App

# Build the solution
dotnet build

# Apply EF Core migrations (creates LocalDB database)
dotnet ef database update --project Instrux.Infrastructure

# Run the application
dotnet run --project Instrux.Application
```

### Configuration

Connection string is in `Instrux.Application/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=InstruxDbLocal;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Features

| Feature | Status |
|---|---|
| Teacher authentication (sign-up / sign-in) | ✅ |
| Dashboard with stats, recent classes, events, tasks | ✅ |
| Class CRUD with DepEd subject selection | ✅ |
| Student roster management | ✅ |
| Daily attendance (Present / Late / Absent / Excused) | ✅ |
| Assessment creation (Quiz / Activity / Exam) | ✅ |
| Score entry with inline editing | ✅ |
| Weighted DepEd grade computation | ✅ |
| Assessment deletion | ✅ |
| Class content upload & management | ✅ |
| Calendar (month grid, create/delete events) | ✅ |
| To-Do list (quick-add, filter, priorities) | ✅ |
| Teacher profile editing | ✅ |
| Account deletion (full data wipe) | ✅ |
| Settings profile lock/unlock | ✅ |

---

## Color Palette

| Role | Hex | Usage |
|---|---|---|
| PrimaryDark | `#2C5EAD` | Sidebar bg, primary buttons hover |
| PrimaryMid | `#1591DC` | Primary buttons, selected nav |
| PrimaryLight | `#4BB8FA` | Scrollbar, secondary highlights |
| PrimarySoft | `#C4E2F5` | Soft buttons, calendar today |
| SecondaryDark | `#005461` | Deep accents |
| SecondaryMid | `#0C7779` | Active states |
| SecondaryLight | `#249E94` | Low priority |
| SecondaryPale | `#3BC1A8` | Present attendance |
| Danger | `#D32F2F` | Delete, sign-out, errors |
| Ink | `#1A1C1E` | Primary text |
| MutedInk | `#5F6368` | Captions |
| Line | `#DCE3ED` | Borders |
| Surface | `#F6F8FC` | App background |
| Card | `#FFFFFF` | Card backgrounds |
