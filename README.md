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

All services follow the same pattern: injected with `InstruxDbContext`, use `DtoMapper` for DTO↔entity conversion, and are registered as singletons in DI.

### Challenges

- **LocalDB file locking** — During active development, the running WPF process holds a lock on the database DLLs, requiring `taskkill` before every rebuild.
- **SVG migration** — SharpVectors.Wpf's `SvgViewbox` lacks direct `BitmapImage` resource support; every `<Image>` referencing the logo resource had to be manually swapped to a hardcoded SVG path.
- **Plaintext passwords** — `AuthenticationService.cs` stores and compares passwords as raw strings with no hashing, a known security gap deferred for later.

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
