# Instrux — Teacher Productivity Hub

A WPF desktop application built for teachers to manage attendance, grades, materials, calendar planning, and personal task flow. Designed around the Philippine DepEd Order No. 8, s. 2015 grading system.

---

## 3. Requirements Analysis

### Functional Requirements

The system must support the following capabilities for authenticated teachers:

| ID | Requirement | Description |
|---|---|---|
| FR-01 | Teacher Authentication | Register a new account and log in with email/password credentials |
| FR-02 | Class Management | Create, view, and delete classes with DepEd subject assignment |
| FR-03 | Student Roster | Add students to classes with name, student ID, and email; delete students |
| FR-04 | Daily Attendance | Record attendance per student per date as Present, Late, Absent, or Excused with auto-save on click |
| FR-05 | Assessment Creation | Create quizzes, activities, and exams per class with name, max score, and date |
| FR-06 | Score Entry | Record and update individual student scores per assessment with inline editing |
| FR-07 | Grade Computation | Compute weighted initial grades per DepEd Order No. 8, s. 2015 with standing thresholds |
| FR-08 | Content Management | Upload and organize class materials (PDF, DOC, PPT, images, videos, links) |
| FR-09 | Calendar Management | Create, view, and delete calendar events with category tagging |
| FR-10 | To-Do List | Create, toggle completion, filter, and delete personal tasks with priority levels |
| FR-11 | Profile Management | View and update teacher profile (name, nickname, email) |
| FR-12 | Account Deletion | Permanently delete account and all associated data with full cascade |
| FR-13 | Teacher Data Isolation | Each teacher's data (classes, students, grades, events, todos) is isolated from others |

### Non-Functional Requirements

| ID | Requirement | Target / Constraint |
|---|---|---|
| NFR-01 | Platform | Windows desktop only (WPF/.NET 10.0-windows) |
| NFR-02 | Database | SQL Server LocalDB — single-user, locally installed |
| NFR-03 | Performance | Grade computation completes in <50ms per class; UI remains responsive during service calls via async/await |
| NFR-04 | Scalability | Single-teacher desktop application; no multi-tenancy or horizontal scaling |
| NFR-05 | Security (known gap) | Passwords stored and compared as plaintext (bcrypt/Argon2 deferred) |
| NFR-06 | Concurrency | Single-user; no concurrent write conflicts |
| NFR-07 | Offline Capability | Fully offline — no network or internet dependency |
| NFR-08 | Testability | Service layer fully testable via EF Core InMemory provider; 52 automated tests |
| NFR-09 | UI Responsiveness | Minimum window 1040×680; all DB operations on background threads via async commands |
| NFR-10 | Maintainability | 4-layer architecture with strict downward-only dependencies; manual DI registration |

### Use Case Diagram

```
┌──────────────────────────────────────────────────────┐
│                    Instrux System                     │
│                                                       │
│  ┌───────────────────────────────────────────────┐   │
│  │                                               │   │
│  │  ┌──────────────┐     ┌──────────────────┐   │   │
│  │  │ Authenticate  │     │  Manage Classes  │   │   │
│  │  │ (Login/Reg.)  │     │  (CRUD)          │   │   │
│  │  └──────┬───────┘     └───────┬──────────┘   │   │
│  │         │                     │               │   │
│  │  ┌──────▼───────┐     ┌──────▼──────────┐   │   │
│  │  │   Manage      │     │   Manage        │   │   │
│  │  │   Students    │     │   Attendance    │   │   │
│  │  └──────┬───────┘     └──────┬──────────┘   │   │
│  │         │                     │               │   │
│  │  ┌──────▼───────┐     ┌──────▼──────────┐   │   │
│  │  │   Manage      │     │   Manage        │   │   │
│  │  │   Assessments │     │   Scores/Grades │   │   │
│  │  └──────┬───────┘     └──────┬──────────┘   │   │
│  │         │                     │               │   │
│  │  ┌──────▼───────┐     ┌──────▼──────────┐   │   │
│  │  │   Manage      │     │   Manage        │   │   │
│  │  │   Content     │     │   Calendar      │   │   │
│  │  └──────┬───────┘     └──────┬──────────┘   │   │
│  │         │                     │               │   │
│  │  ┌──────▼───────┐     ┌──────▼──────────┐   │   │
│  │  │   Manage      │     │   Manage        │   │   │
│  │  │   To-Do List  │     │   Profile       │   │   │
│  │  └──────┬───────┘     └──────┬──────────┘   │   │
│  │         │                     │               │   │
│  │         └──────┬─────────────┘               │   │
│  │                │                              │   │
│  │         ┌──────▼──────────┐                  │   │
│  │         │   Delete        │                  │   │
│  │         │   Account       │                  │   │
│  │         └─────────────────┘                  │   │
│  │                                               │   │
│  └───────────────────────────────────────────────┘   │
│                                                       │
│              Actor: Teacher (Primary User)            │
└──────────────────────────────────────────────────────┘
```

---

## 4. System Architecture

### Architectural Pattern

**4-Layer Architecture** with strict downward-only dependencies:

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

Each layer depends only on the layers below it. No layer has circular dependencies. The Domain layer has zero external package dependencies.

### Technology Stack

| Component | Technology | Version |
|---|---|---|
| **Language** | C# | 12.0 |
| **Runtime** | .NET | 10.0 |
| **UI Framework** | WPF (Windows) | — |
| **ORM** | Entity Framework Core | 10.0.8 |
| **Database** | SQL Server LocalDB | — |
| **DI Container** | Microsoft.Extensions.Hosting | 10.0.8 |
| **UI Component Library** | MaterialDesignThemes | 5.3.2 |
| **SVG Rendering** | SharpVectors.Wpf | 1.8.5 |
| **Configuration** | `appsettings.json` | — |

**NuGet Packages:**

| Package | Version | Used In |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 10.0.8 | Infrastructure |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.8 | Infrastructure |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.8 | Tests |
| `Microsoft.Extensions.Hosting` | 10.0.8 | Application |
| `Microsoft.Extensions.Configuration.Json` | 10.0.8 | Application |
| `MaterialDesignThemes` | 5.3.2 | Application |
| `SharpVectors.Wpf` | 1.8.5 | Application |
| `xUnit` | 2.9.3 | Tests |
| `coverlet.collector` | 6.0.4 | Tests |

### System Components

The system comprises four major modules:

| Module | Project | Responsibility |
|---|---|---|
| **Domain Models** | `Instrux.Domain` | Pure C# entities and enums — zero external dependencies |
| **Data Access** | `Instrux.Infrastructure` | EF Core DbContext, fluent configurations, SQL Server LocalDB migrations |
| **Business Logic** | `Instrux.Services` | 9 service interfaces + implementations, DTOs, manual mapping, DepEd grade computation |
| **Presentation** | `Instrux.Application` | WPF UI with MVVM: 12 ViewModels, 6 Views, 2 Windows, singleton DataService + SessionService |

**Communication flow:**

```
User Input → View (XAML) → ViewModel (RelayCommand) → DataService → Service → DbContext → LocalDB
                                                                                              │
User Output ← View (XAML) ← ViewModel (Binding) ← DataService ← ObservableCollection ←───────┘
```

All service calls are async. ViewModels use `RelayCommandAsync` which disables the button during execution. `DataService` wraps service responses and updates its `ObservableCollection<T>` collections, triggering automatic UI updates via WPF data binding.

---

## 5. Detailed Design

### Design Patterns

The following design patterns were explicitly chosen to solve specific architectural problems:

| Pattern | Location | Rationale |
|---|---|---|
| **Layered Architecture** | Overall solution | Separates concerns into 4 strict layers (Domain → Infrastructure → Services → Application). Enables testability — services can be tested with InMemory DB without UI dependencies. |
| **Singleton** | All services, `DataService`, `SessionService` (registered via `AddSingleton<>`) | Single teacher desktop app — exactly one instance of each service and data hub needed. `DataService` holds all `ObservableCollection<T>` state shared across ViewModels. |
| **MVVM** | `ViewModelBase` → 12 ViewModels → 6 Views | WPF's native data binding pattern. ViewModels expose `INotifyPropertyChanged` properties; XAML Views bind declaratively. No code-behind logic. |
| **Command** | `RelayCommand` / `RelayCommandAsync` | Encapsulates user actions as `ICommand` objects. `RelayCommandAsync` tracks execution state and disables buttons during async DB operations to prevent double-submits. |
| **Observer** | `INotifyPropertyChanged` + `ObservableCollection.CollectionChanged` | ViewModels subscribe to `DataService` collections. When a service call mutates data, the collection fires change events and the UI re-renders automatically. |
| **Static Factory** | `GradingConfig.FromSubject()` | Maps each `Subject` enum value to the correct weight group (LanguagesSocialSciences, MathScience, SkillsArts) with the appropriate WW/PT/QA percentages. Pure function — no state needed. |
| **Upsert** | `AttendanceService.SaveRecordAsync()`, `GradeService.UpdateScoreAsync()` | Avoids duplicate key exceptions: queries for existing `(StudentId, Date)` or `(StudentId, AssessmentId)` pair; creates new entity if not found, updates existing if found. |
| **Mediator** | `DataService` | Central hub decoupling ViewModels from service layer. ViewModels call `DataService.AddClassAsync(...)`, never `IClassService` directly. DataService coordinates service calls and collection updates. |

### Class Diagrams

**Service Layer Interfaces and Implementations:**

```
┌─────────────────────────────────────────────────────────┐
│                  IService Interfaces                     │
├─────────────────────────────────────────────────────────┤
│  IAuthenticationService  ←── AuthenticationService      │
│  IClassService           ←── ClassService               │
│  IStudentService         ←── StudentService             │
│  IAttendanceService      ←── AttendanceService          │
│  IGradeService           ←── GradeService               │
│  ITeacherService         ←── TeacherService             │
│  ICalendarEventService   ←── CalendarEventService       │
│  ITodoService            ←── TodoService                │
│  IContentService         ←── ContentService             │
│                                                          │
│  All implement: sealed class                             │
│  All inject: IRepository                                 │
│  All registered as: Singleton                            │
└──────────────────────────────────────────────────────────┘
```

**Domain Entity Relationships:**

```
Teacher (1) ──────────── (N) Class       Teacher (1) ─── (N) CalendarEvent
  │                               │         Teacher (1) ─── (N) TodoItem
  │                               │
  │                        Class (1) ─── (N) Assessment
  │                               │         │
  │                               │    Assessment (1) ─── (N) Score
  │                               │
  │                        Class (1) ─── (N) Student
  │                                         │
  │                                    Student (1) ─── (N) AttendanceRecord
  │                                         │
  │                                    Student (1) ─── (N) Score
  │
  │                        Class (1) ─── (N) ContentItem
```

**ViewModel Hierarchy:**

```
ViewModelBase (abstract)
├── AuthenticationViewModel
├── MainDashboardViewModel
│   ├── DashboardViewModel
│   ├── ClassesViewModel
│   │   ├── StudentRosterViewModel
│   │   ├── AttendanceStudentViewModel
│   │   └── GradeBookRowViewModel
│   ├── CalendarViewModel
│   │   └── CalendarDayViewModel
│   ├── TodoViewModel
│   └── SettingsViewModel
└── NavigationItemViewModel
```

**App Startup / DI Wiring:**

```
Host.CreateDefaultBuilder()
  └── services.AddSingleton<InstruxDbContext>()
  └── services.AddSingleton<IRepository, Repository>()
  └── services.AddSingleton<IAuthenticationService, AuthenticationService>()
  └── services.AddSingleton<IClassService, ClassService>()
  └── services.AddSingleton<IStudentService, StudentService>()
  └── services.AddSingleton<IAttendanceService, AttendanceService>()
  └── services.AddSingleton<IGradeService, GradeService>()
  └── services.AddSingleton<ITeacherService, TeacherService>()
  └── services.AddSingleton<ICalendarEventService, CalendarEventService>()
  └── services.AddSingleton<ITodoService, TodoService>()
  └── services.AddSingleton<IContentService, ContentService>()
  └── services.AddSingleton<SessionService>()
  └── services.AddSingleton<DataService>()
  └── services.AddTransient<MainDashboardViewModel>()
  └── services.AddTransient<...>()   (all 12 ViewModels as transient)
  └── dbContext.Database.MigrateAsync()
  └── RunAuthenticationFlowAsync()
       ├── Show AuthenticationWindow (modal)
       ├── On success → DataService.InitializeAsync()
       ├── Show MainWindow (modal)
       └── On sign-out → loop back to AuthenticationWindow
```

### Database Schema

All entities are mapped to SQL Server LocalDB via EF Core fluent configuration. Enums are stored as strings for readability.

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

| Subject Group | Subjects | WW | PT | QA |
|---|---|---|---|---|
| Languages / Social Sciences | English, Filipino, AralingPanlipunan, EdukasyonSaPagpapakatao | 30% | 50% | 20% |
| Math / Science | Mathematics, Science | 40% | 40% | 20% |
| Skills / Arts | TLE, HomeEconomics, MAPEH | 20% | 60% | 20% |

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

---

## 6. Implementation Details

### Key Algorithms

**DepEd Grade Computation** — `GradingConfig.cs:14` maps each `Subject` to one of three weight groups per DepEd Order No. 8, s. 2015:

| Group | Subjects | WW | PT | QA |
|---|---|---|---|---|
| Languages / Social Sciences | English, Filipino, AralingPanlipunan, EdukasyonSaPagpapakatao | 30% | 50% | 20% |
| Math / Science | Mathematics, Science | 40% | 40% | 20% |
| Skills / Arts | TLE, HomeEconomics, MAPEH | 20% | 60% | 20% |

The `GradeService.cs:84-98` algorithm computes category averages as:

```
For each category (Quiz → WW, Activity → PT, Exam → QA):
  percentages = filter(scores where type matches AND value is not null AND maxScore > 0)
               .select(value / maxScore * 100)
  categoryAvg = percentages.average()  // 0 if no scores

InitialGrade = (WW_avg × WW_weight) + (PT_avg × PT_weight) + (QA_avg × QA_weight)
```

Standing thresholds: `≥90` Excellent, `≥80` On track, `≥70` Watch, else Support.

**Attendance Summary** — `StudentRosterViewModel.cs` pre-computes per-student counts by filtering the global `Attendance` collection by `StudentId` and counting by `AttendanceStatus` (Present / Late / Absent / Excused).

**Account Deletion Cascade** — `TeacherService.cs:41-89` deletes all teacher-associated data in dependency-safe order: scores → attendance → students → assessments → content → classes → events → todos → teacher, all within a single `SaveChangesAsync` call.

### APIs

This is a WPF desktop application — no REST or web APIs. The service contract layer comprises 9 interfaces:

| Interface | Key Methods | Purpose |
|---|---|---|
| `IAuthenticationService` | `LoginAsync`, `RegisterAsync` | Teacher identity |
| `IClassService` | `GetAllAsync`, `CreateAsync`, `DeleteAsync` | Class CRUD |
| `IStudentService` | `GetAllAsync`, `CreateAsync`, `DeleteAsync` | Student roster |
| `IAttendanceService` | `GetAllAsync`, `SaveRecordAsync` | Daily attendance |
| `IGradeService` | `GetAssessmentsAsync`, `CreateAssessmentAsync`, `DeleteAssessmentAsync`, `UpdateScoreAsync`, `GetGradeBookAsync` | Assessment & score management |
| `ITeacherService` | `GetProfileAsync`, `UpdateProfileAsync`, `DeleteAccountAsync` | Teacher profile & account |
| `ICalendarEventService` | `GetAllAsync`, `CreateAsync`, `DeleteAsync` | Calendar events |
| `ITodoService` | `GetAllAsync`, `CreateAsync`, `ToggleAsync`, `DeleteAsync` | To-Do items |
| `IContentService` | `GetAllAsync`, `CreateAsync`, `DeleteAsync` | Class content/files |

All services follow the same pattern: injected with `IRepository` (abstraction over `InstruxDbContext`), use `DtoMapper` for DTO↔entity conversion, and are registered as singletons in DI.

### Challenges

- **LocalDB file locking** — During active development, the running WPF process holds a lock on the database DLLs, requiring `taskkill` before every rebuild.
- **SVG migration** — SharpVectors.Wpf's `SvgViewbox` lacks direct `BitmapImage` resource support; every `<Image>` referencing the logo resource had to be manually swapped to a hardcoded SVG path.
- **Plaintext passwords** — `AuthenticationService.cs` stores and compares passwords as raw strings with no hashing, a known security gap deferred for later.

---

## Input Validation

### MaxLength Constraints

UI-side limits applied via `MaxLength` on 19 text input fields across 5 views:

| View | Field | MaxLength |
|---|---|---|
| AuthenticationView | FullName | 100 |
| AuthenticationView | Nickname | 50 |
| AuthenticationView | Email | 254 |
| AuthenticationView | Password | 128 |
| ClassesView | NewClassName | 100 |
| ClassesView | NewClassSection | 50 |
| ClassesView | ClassSearch | 100 |
| ClassesView | NewStudentName | 100 |
| ClassesView | NewAssessmentName | 200 |
| ClassesView | NewAssessmentMaxScore | 6 |
| ClassesView | GradeCellValue | 6 |
| CalendarView | NewEventTitle | 200 |
| CalendarView | StartTimeText | 5 |
| CalendarView | EventNotes | 500 |
| TodoView | NewTaskTitle | 200 |
| TodoView | TodoSearch | 100 |
| SettingsView | FullName | 100 |
| SettingsView | Nickname | 50 |
| SettingsView | Email | 254 |

Database-side limits defined via EF Core fluent configuration on 29 entity properties. Notable mismatches where UI allows more than the DB column:

- `Teacher.Email`: UI=254, DB=180
- `Assessment.Name`: UI=200, DB=140

### Tooltips

22 tooltips across 4 views guide user input. Examples: `"Example: Grade 8 Mathematics"` (NewClassName), `"HH:mm format"` (StartTimeText), `"At least 6 characters"` (Password). SettingsView has no tooltips.

### InputScope

2 fields set `InputScope="Number"` — `NewAssessmentMaxScore` and grade cell value — restricting the touch/on-screen keyboard to numeric input on Windows.

### Regex Validation

| Pattern | Location | Purpose |
|---|---|---|
| `^[^@\s]+@[^@\s]+\.[^@\s]+$` | `AuthenticationViewModel.cs:20-21` | Email format check on submit |
| `^([01]\d\|2[0-3]):[0-5]\d$` | `CalendarViewModel.cs:124` | 24-hour HH:mm time format check |

### Numeric Clamping

| Rule | Location | Range |
|---|---|---|
| `NewAssessmentMaxScore` | `ClassesViewModel.cs:183` | Clamped to 1–1000 |
| Grade cell score | `ClassesViewModel.cs:497` | Clamped to 0–current assessment `MaxScore` |

### Password Minimum

`AuthenticationViewModel.cs:124-127` — email regex validated before submit; password checked for length ≥6 characters.

### Input Trimming

Whitespace trimmed at 11 locations across 3 services and 4 ViewModels before data reaches the database.

### Validation Wiring Gap

The `Validation.ErrorTemplate` (2px red border defined in `App.xaml`) is never triggered — no `IDataErrorInfo` implementation or `ValidationRule` class exists in any ViewModel. All validation runs either in property setters, `CanExecute` gating, or at submit time.

---

## Error Handling

### Architecture

Errors are caught at three layers:

```
Service Layer ──► DataService ──► ViewModel (RelayCommand onError) ──► UI Notification
      │                │                          │
      │          ServiceException                  │
      │         (UserFacingMessage)                │
      │                                            │
Global Exception Handlers (last resort) ──────► MessageBox
```

### Try-Catch Coverage (33+ blocks)

| Location | Count | Scope |
|---|---|---|
| `App.xaml.cs` | 3 | Host startup, shutdown, auth flow initialization |
| `RelayCommand.cs` / `RelayCommandAsync.cs` | 2 | Wraps every command's `Execute` delegate |
| ViewModels (7 files) | 12 | `async void` event handlers (DeleteStudent, MarkAttendance, DeleteAssessment, etc.) |
| `DataService.cs` | 16 | Every service call wrapped individually |
| Code-behind files | 3 | AuthenticationView, AuthenticationWindow password/close handlers |

### Error Callback Pattern

`RelayCommand` and `RelayCommandAsync` accept an `Action<Exception>? onError` parameter. 25 command bindings pass an error callback. The standard pattern across most ViewModels:

```csharp
new RelayCommandAsync(
    execute: async () => await _dataService.AddClassAsync(...),
    canExecute: () => !string.IsNullOrWhiteSpace(NewClassName),
    onError: ex => _notifications.ShowError(UnwrapMessage(ex)))
```

### UnwrapMessage Helper

Defined in 4 ViewModels to convert exceptions to user-facing messages:

```csharp
private static string UnwrapMessage(Exception ex) =>
    ex is ServiceException se ? se.UserFacingMessage
                              : "Something went wrong. Please try again.";
```

`ServiceException` carries a `UserFacingMessage` property for clean error surfacing.

### Global Exception Handlers

Registered in `App.xaml.cs`:

| Handler | Thread | App Termination |
|---|---|---|
| `AppDomain.CurrentDomain.UnhandledException` | Non-UI/any | Yes — last resort catch |
| `DispatcherUnhandledException` | UI thread | No — `args.Handled = true` |

Both display a `MessageBox` with the error text.

### ErrorMessage / HasError Properties

Defined in `ViewModelBase.cs:12-24`. Used by `AuthenticationViewModel` for email/password validation errors and `CalendarViewModel` for time format errors. Consumed in `AuthenticationView.xaml` via a conditional error border bound to `HasError`.

### CanExecute Gating (Pre-Submit)

9 commands use `CanExecute` to prevent submission with invalid input:

| ViewModel | Command | Condition |
|---|---|---|
| `AuthenticationViewModel` | `SubmitCommand` | Email + password non-empty; sign-up also requires full name |
| `ClassesViewModel` | `CreateClassCommand` | `NewClassName` non-empty |
| `ClassesViewModel` | `AddStudentCommand` | Class selected + name non-empty |
| `ClassesViewModel` | `AddAssessmentCommand` | Class selected + name non-empty + max score > 0 |
| `ClassesViewModel` | `UploadContentCommand` | Class selected |
| `CalendarViewModel` | `AddEventCommand` | Title non-empty |
| `TodoViewModel` | `AddTaskCommand` | Title non-empty |
| `SettingsViewModel` | `SaveCommand` | Editing + all fields non-empty |

### Service Layer

No service implementation contains try-catch blocks. Exceptions propagate to `DataService` which wraps them, rethrowing `ServiceException` with a user-facing message. This keeps the service layer clean and testable.

---

## 7. Verification & Testing

### Testing Strategy

**Automated unit + integration tests** via `xUnit` with EF Core InMemory provider. 8 test classes covering all service layers and domain logic:

| Test Class | Type | Tests | Scope |
|---|---|---|---|
| `DtoMapperTests` | Unit | 6 | DTO↔Entity mapping correctness |
| `GradingConfigTests` | Unit | 9 | Subject weight tables for all 9 subjects |
| `AuthenticationServiceTests` | Integration | 6 | Register, login, duplicate email, case insensitivity |
| `GradeServiceTests` | Integration | 9 | Assessment CRUD, grade computation, score upsert, standing thresholds |
| `TeacherServiceTests` | Integration | 6 | Profile CRUD, full account deletion cascade |
| `ClassServiceTests` | Integration | 5 | Class CRUD, cascade delete across 6 entity types |
| `StudentServiceTests` | Integration | 4 | Student CRUD, cascade delete |
| `AttendanceServiceTests` | Integration | 6 | Attendance CRUD, date filtering, teacher scoping |

**Total: 52 tests — 52 passed, 0 failed, 0 skipped (Duration: ~3s)**

### Critical Automated Test Cases

| Scenario | Expected Outcome | Status |
|---|---|---|
| Grade computation: Quiz 45/50, Activity 40/50, Exam 35/50 (Mathematics) | WW=90%, PT=80%, QA=70%; InitialGrade = `(90×0.40)+(80×0.40)+(70×0.20) = 82%` → "On track" | ✅ |
| Assessment deletion with existing scores | Assessment and all scores removed from DB | ✅ |
| Account deletion | All 9 entity tables empty for that teacher; other teachers' data untouched | ✅ |
| Attendance upsert (create then update) | Status changes from Present → Late correctly | ✅ |
| Login case insensitivity | Email "JANE@TEST.COM" matches "jane@test.com" | ✅ |
| Duplicate email registration | Returns failure, no duplicate teacher created | ✅ |
| Standing threshold boundary | Perfect scores → "Excellent"; zero scores → "Support" | ✅ |
| Account deletion isolation | Other teacher's classes and students remain intact | ✅ |

### Code Coverage

Coverage tooling (`coverlet`) is available but not yet configured for reporting. All service assembly methods (`Instrux.Services`, `Instrux.Domain`) are exercised through the integration test suite. Targeting full coverage of `GradeService`, `AuthenticationService`, `TeacherService`, `DtoMapper`, and `GradingConfig`.

---

## 8. Conclusion & Future Work

### Reflection

The 4-layer architecture (Domain → Infrastructure → Services → Application) held up well during implementation. The `DataService` singleton with `ObservableCollection<T>` bindings provided a clean reactive UI layer without a formal state management library. However, the absence of a dedicated validation layer and error boundary around service calls means unhandled exceptions (e.g., DB connection failure) bubble up as unhandled WPF crashes.

### Lessons Learned

- **Tests caught real issues** — The test suite revealed that the Mathematics group grade computation (40/40/20) produces 82% for the benchmark case, not 81% as initially calculated for the 30/50/20 group. The weight tables must be verified per subject group.
- **Password security was deferred too long** — Storing passwords in plaintext is unacceptable for any production-adjacent release.
- **Manual DI registration is fragile** — Each new service requires touching DI registration in `App.xaml.cs` and adding a constructor parameter to `DataService`. A convention-based registration or source generator would be more maintainable.

### Future Enhancements

| Feature | Priority | Effort |
|---|---|---|
| Password hashing (bcrypt/Argon2) | Critical | Small |
| Coverlet code coverage reporting | High | Small |
| Grade PDF / CSV export | Medium | Small |
| Excel import for student rosters | Medium | Small |
| Dark mode theme toggle | Medium | Medium |
| Class timetable / scheduling view | Low | Large |
| Parent/guardian portal read-only view | Low | Large |
| Multi-term grade averaging (quarterly → final) | Low | Medium |

---

## 9. References

| Library / Framework | Version | Purpose |
|---|---|---|
| `.NET` | 10.0 | Runtime and base class library |
| `Entity Framework Core` | 10.0.8 | ORM — SQL Server LocalDB |
| `Microsoft.Extensions.Hosting` | 10.0.8 | DI container, app lifecycle |
| `Microsoft.Extensions.Configuration.Json` | 10.0.8 | `appsettings.json` configuration |
| `MaterialDesignThemes` | 5.3.2 | WPF UI component library (cards, buttons, inputs, colors) |
| `SharpVectors.Wpf` | 1.8.5 | SVG rendering for icons and logo |
| `xUnit` | — | Unit/integration test framework |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.8 | In-memory database provider for tests |
| DepEd Order No. 8, s. 2015 | — | Philippine K–12 grading system specification |

No academic papers were directly referenced. All design decisions were driven by the framework documentation (Microsoft Learn, MaterialDesignInXaml docs, SharpVectors GitHub wiki) and the DepEd order linked above.
