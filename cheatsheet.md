# 📘 Instrux — Complete Cheatsheet

## 1. PROJECT BASICS

| Item | Info |
|---|---|
| **App** | Instrux — Teacher Productivity Hub |
| **Purpose** | WPF desktop app for Philippine K-12 teachers to manage classes, attendance, grades (DepEd), calendar, to-dos, content |
| **GitHub** | `https://github.com/Nepthys2006/Instrux.Core.App.git` |
| **Branch** | `main` (9 commits) |
| **Language** | C# 12, .NET 10.0 |
| **DB** | SQL Server LocalDB — `InstruxDbLocal` |
| **Architecture** | 4-layer: Domain → Infrastructure → Services → Application |

---

## 2. 4-LAYER ARCHITECTURE

```
Instrux.Application   → WPF UI: Views, ViewModels, Converters, DataService (reactive hub)
Instrux.Services      → Business logic: 9 interfaces + impls, 19 DTOs, Mapper, Resolvers
Instrux.Infrastructure → Data access: EF Core DbContext, configs, migrations, generic Repository
Instrux.Domain        → Pure models + enums (zero dependencies)
```

Strict downward-only dependencies. Domain has ZERO NuGet packages.

**Instrux.Domain** — The innermost layer with zero external dependencies. Contains only pure C# entity classes and enums that model the real-world concepts (teachers, classes, students, grades) with no knowledge of databases or UI.

**Instrux.Infrastructure** — The data access layer that bridges the domain models to SQL Server LocalDB via Entity Framework Core. Defines the DbContext, entity-to-table mappings via Fluent API, database migrations, and a generic repository pattern for CRUD operations.

**Instrux.Services** — The business logic layer implementing all application features through 9 service interfaces and their implementations. Contains the DepEd grade computation algorithm, attendance upsert logic, account deletion cascade, DTOs for data transfer, and a static mapper for entity↔DTO conversion.

**Instrux.Application** — The outermost WPF presentation layer that users interact with. Implements MVVM pattern with 13 ViewModels, 6 Views, converters, commands, and a central DataService that holds reactive ObservableCollections to keep the UI synchronized with the database.

**Instrux.Tests** — The test project with 52 automated xUnit tests covering all service layers and domain logic. Uses EF Core InMemory provider to create isolated databases per test, ensuring no test dependencies and fast execution (~3 seconds).

---

## 3. TECH STACK

| Library | Version | Used In | Purpose |
|---|---|---|---|
| .NET | 10.0 | All | Runtime |
| EF Core | 10.0.8 | Infrastructure | ORM — SQL Server LocalDB |
| Microsoft.Extensions.Hosting | 10.0.8 | Application | DI container, lifecycle |
| MaterialDesignThemes | 5.3.2 | Application | WPF UI components (cards, buttons, snackbar) |
| SharpVectors.Wpf | 1.8.5 | Application | SVG rendering (icons, logo) |
| xUnit | 2.9.3 | Tests | Testing framework |
| EF Core InMemory | 10.0.8 | Tests | Isolated test databases |
| coverlet | 6.0.4 | Tests | Code coverage (not yet configured) |

---

## 4. DATABASE — 10 TABLES

The database uses SQL Server LocalDB with Entity Framework Core Code First — all schema is defined in C# configuration files and migrations, not raw SQL. Enums are stored as human-readable strings in the database (e.g., `"Present"` not `0`) for direct query readability, and unique composite indexes enforce business rules like one attendance record per student per day.

### Entity Relationships
```
Teacher ──1:N──> Classes ──1:N──> Students ──1:N──> AttendanceRecords
                                     │
Classes ──1:N──> Assessments ──1:N──┼──> Scores
               GradingConfigs        │
Classes ──1:N──> ContentItems        │
Teacher ──1:N──> CalendarEvents (opt linked to Class)
Teacher ──1:N──> TodoItems (opt linked to Class)
```

### Tables Quick Reference

| # | Table | Key Fields | Unique Constraints |
|---|---|---|---|
| 1 | **Teachers** | Id, FullName(160), Nickname(80), Email(180), PasswordHash(256) | Email UNIQUE |
| 2 | **Classes** | Id, Name(120), Section(80), Subject(enum→str), SchoolYear, Semester, CoverColor, TeacherId(FK) | — |
| 3 | **Students** | Id, FullName(160), StudentId(80), Email, ClassId(FK) | (ClassId, StudentId) UNIQUE |
| 4 | **AttendanceRecords** | Id, StudentId(FK), Date, Status(enum→str), Note(240) | (StudentId, Date) UNIQUE |
| 5 | **Assessments** | Id, ClassId(FK), Name(140), Type(enum→str), MaxScore(d8,2), Weight(d5,2), Date | — |
| 6 | **Scores** | Id, StudentId(FK), AssessmentId(FK), Value(d8,2) | (StudentId, AssessmentId) UNIQUE |
| 7 | **CalendarEvents** | Id, TeacherId(FK), Title(160), Date, StartTime, EndTime, Category(enum→str), LinkedClassId, Notes(600) | — |
| 8 | **TodoItems** | Id, TeacherId(FK), Title(180), DueDate, Priority(enum→str), IsCompleted, CompletedAt, IsRecurring, Recurrence | — |
| 9 | **ContentItems** | Id, ClassId(FK), FolderId(self-FK), Title(180), Description(600), Type(enum→str), FilePath(500), UploadedAt, IsVisible | — |
| 10 | **GradingConfigs** | Id, Subject(enum→str), Group(enum→str), WrittenWorksWeight(d5,2), PerformanceTasksWeight(d5,2), QuarterlyAssessmentWeight(d5,2) | Subject UNIQUE |

---

## 5. ALL 8 ENUMS

Enums provide type-safe fixed value sets used across the entire application — from subjects and attendance statuses to priorities and content types. They are stored as strings in the database for readability and used in switch expressions throughout the codebase for clean branching logic.

| Enum | Values | Used In |
|---|---|---|
| **Subject** | English, Filipino, AralingPanlipunan, EdukasyonSaPagpapakatao, Mathematics, Science, TLE, HomeEconomics, MAPEH | Classes, GradingConfigs |
| **SubjectGroup** | LanguagesSocialSciences, MathScience, SkillsArts | GradingConfigs |
| **AssessmentType** | Quiz → WW, Activity → PT, Exam → QA | Assessments, GradeService |
| **AttendanceStatus** | Present, Late, Absent, Excused | AttendanceRecords |
| **EventCategory** | Meeting, ExamDay, Holiday, Reminder, SubmissionDeadline | CalendarEvents |
| **Priority** | Low, Medium, High, Urgent | TodoItems |
| **ContentType** | Pdf, Doc, Ppt, Image, Video, Link | ContentItems |
| **RecurrenceType** | Daily, Weekly, Monthly | TodoItems |

---

## 6. DEPed GRADE COMPUTATION (Core Algorithm)

Implements **DepEd Order No. 8, s. 2015** — the Philippine K-12 grading system for Grades 7-10. Assessment types (Quiz/Activity/Exam) map to three weighted categories (Written Works, Performance Tasks, Quarterly Assessment) with percentages varying by subject group (Languages, Math/Science, or Skills/Arts).

### Weight Table (`GradingConfig.FromSubject()`)

| Subject Group | Subjects | WW (Quiz) | PT (Activity) | QA (Exam) |
|---|---|---|---|---|
| Languages/Social Sciences | English, Filipino, AralingPanlipunan, EdukasyonSaPagpapakatao | **30%** | **50%** | **20%** |
| Math/Science | Mathematics, Science | **40%** | **40%** | **20%** |
| Skills/Arts | TLE, HomeEconomics, MAPEH | **20%** | **60%** | **20%** |

### Formula
```
For each category (Quiz→WW, Activity→PT, Exam→QA):
  percentages = filter(scores WHERE type matches AND value NOT NULL AND maxScore > 0)
               .SELECT(value / maxScore * 100)
  categoryAvg = percentages.AVERAGE()  // 0 if no scores

InitialGrade = (WW_avg × WW_weight) + (PT_avg × PT_weight) + (QA_avg × QA_weight)
```

### Standing Thresholds
| Range | Standing |
|---|---|
| ≥ 90 | **Excellent** |
| ≥ 80 | **On track** |
| ≥ 70 | **Watch** |
| < 70 | **Support** |

### Example (Mathematics — 40/40/20)
```
Quiz 45/50 → WW = 90%
Activity 40/50 → PT = 80%
Exam 35/50 → QA = 70%

InitialGrade = (90×0.40) + (80×0.40) + (70×0.20) = 36 + 32 + 14 = 82% → "On track"
```

---

## 7. ALL 9 SERVICE INTERFACES (The "API")

These interfaces define the application's contract layer — what the system can do, independent of how it's implemented. Each interface is paired with a concrete implementation injected via DI, keeping the business logic swappable and testable with in-memory databases.

| Interface | Key Methods | Purpose |
|---|---|---|
| **IAuthenticationService** | `LoginAsync`, `RegisterAsync` | Teacher identity (plaintext passwords) |
| **IClassService** | `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `DeleteAsync` | Class CRUD + cascade delete |
| **IStudentService** | `GetAllAsync`, `GetByClassAsync`, `CreateAsync`, `DeleteAsync` | Student roster management |
| **IAttendanceService** | `GetAllAsync`, `GetByDateAsync`, `SaveRecordAsync`, `SaveBatchAsync` | Daily attendance upsert |
| **IGradeService** | `GetAssessmentsAsync`, `CreateAssessmentAsync`, `DeleteAssessmentAsync`, `UpdateScoreAsync`, `GetGradeBookAsync` | Gradebook + DepEd computation |
| **ITeacherService** | `GetProfileAsync`, `UpdateProfileAsync`, `DeleteAccountAsync` | Profile + full cascade deletion |
| **ICalendarEventService** | `GetAllAsync`, `GetByMonthAsync`, `GetTodayAsync`, `CreateAsync`, `DeleteAsync` | Calendar events |
| **ITodoService** | `GetAllAsync`, `CreateAsync`, `ToggleAsync`, `DeleteAsync` | To-do items |
| **IContentService** | `GetAllAsync`, `GetByClassAsync`, `CreateAsync`, `DeleteAsync` | Class content/files |

All inject `IRepository`, all singletons, all use `DtoMapper`.

---

## 8. KEY ALGORITHMS & LOGIC

### Attendance Upsert
1. Find existing record by `(StudentId, Date)`
2. If exists → update Status/Note
3. If not → create new
4. Save once

### Score Upsert (same pattern)
1. Find existing by `(StudentId, AssessmentId)`
2. If exists → update Value
3. If not → create new
4. Save once

### Account Deletion Cascade (`TeacherService.DeleteAccountAsync`)
Order (respects FK dependencies): **Scores → Attendance → Students → Assessments → ContentItems → Classes → CalendarEvents → TodoItems → Teacher**
All inside ONE `SaveChangesAsync()` call. Other teachers' data is untouched.

### Attendance Summary (`StudentRosterViewModel`)
```
For each student:
  TotalPresent = count WHERE Status == Present
  TotalLate    = count WHERE Status == Late
  TotalAbsences = count WHERE Status == Absent
  TotalExcused = count WHERE Status == Excused
```

---

## 9. ALL 13 VIEWMODELS

ViewModels are the heart of the MVVM pattern — they hold all UI state and commands, while Views are just passive XAML templates that bind to them. Every ViewModel inherits from `ViewModelBase` (which provides `INotifyPropertyChanged` and `SetProperty<T>`) and uses `RelayCommand`/`RelayCommandAsync` for button bindings with automatic CanExecute gating and error handling.

| ViewModel | Lines | Role |
|---|---|---|
| **ViewModelBase** (abstract) | ~30 | INotifyPropertyChanged, SetProperty<T>, ErrorMessage |
| **AuthenticationViewModel** | 157 | Login/Register form + email regex validation |
| **MainDashboardViewModel** | 116 | Tab navigation shell, recent classes, sign-out |
| **DashboardViewModel** | 72 | Summary stat cards (4 counts) |
| **ClassesViewModel** | 553 | **Largest** — roster, attendance, grades, content tabs |
| **StudentRosterViewModel** | 38 | Per-student attendance summary (P/L/A/E counts) |
| **AttendanceStudentViewModel** | 38 | Single student's daily attendance markers |
| **GradeBookRowViewModel** | 84 | Grade row with cell VMs + WW/PT/QA/Standing |
| **CalendarViewModel** | 213 | Month grid, agenda, event CRUD |
| **CalendarDayViewModel** | 13 | Single day: Date, IsToday, IsSelected |
| **TodoViewModel** | 183 | Task list + Today/Upcoming/Completed filters |
| **SettingsViewModel** | 131 | Profile lock/edit + account deletion |
| **NavigationItemViewModel** | 18 | Sidebar item: Title, Icon, IsSelected |

---

## 10. DI REGISTRATION (App.xaml.cs)

**Singletons:** DbContext, Repository, ALL 9 services, DataService, SessionService, NotificationService
**Transient:** AuthenticationViewModel, MainDashboardViewModel, AuthenticationWindow, MainWindow

### Startup Flow
```
Host.CreateDefaultBuilder()
  → AddDbContext<InstruxDbContext>(UseSqlServer, Singleton)
  → AddSingleton<IRepository, Repository>()
  → AddSingleton<IAuthenticationService, AuthenticationService>()  // + 8 more
  → AddSingleton<DataService>()
  → AddSingleton<SessionService>()
  → AddTransient<MainDashboardViewModel>()
  → dbContext.Database.MigrateAsync()
  → RunAuthenticationFlowAsync()
       ├── Show AuthenticationWindow (modal dialog)
       ├── On success → DataService.InitializeAsync()
       ├── Show MainWindow
       └── On sign-out → loop back to auth
```

---

## 11. DESIGN PATTERNS

| Pattern | Where | Why |
|---|---|---|
| **Layered Architecture** | 4 projects | Separation of concerns, testability |
| **Singleton** | All services, DataService | Single-teacher desktop — one instance |
| **MVVM** | ViewModels + Views | WPF data binding standard |
| **Command** | RelayCommand / RelayCommandAsync | ICommand with CanExecute + error callback |
| **Observer** | INotifyPropertyChanged + ObservableCollection | Auto UI re-render on data change |
| **Static Factory** | `GradingConfig.FromSubject()` | Pure function mapping Subject → weights |
| **Upsert** | Attendance + Score services | Avoid duplicate key exceptions |
| **Mediator** | DataService | Decouples ViewModels from service layer |
| **Generic Repository** | IRepository<T> | Abstraction over EF Core DbSet<T> |

---

## 12. TESTING — 52 Tests (All Passing)

The project uses xUnit with EF Core InMemory provider for fast, isolated integration tests that run in ~3 seconds. Each test class gets a fresh database instance, and the shared `InMemoryDbContextFactory` seeds the required reference data (all 9 GradingConfig records) so tests can focus on business logic without setup boilerplate.

| Test Class | # | What It Tests |
|---|---|---|
| DtoMapperTests | 6 | Entity↔DTO mapping correctness |
| GradingConfigTests | 9 | All 9 subject weight tables |
| AuthenticationServiceTests | 6 | Register, login, duplicate email, case-insensitive |
| GradeServiceTests | 9 | Assessment CRUD, 82% computation, thresholds |
| TeacherServiceTests | 6 | Profile, update, full cascade deletion, isolation |
| ClassServiceTests | 5 | CRUD, cascade delete across 6 entity types |
| StudentServiceTests | 4 | CRUD, cascade delete |
| AttendanceServiceTests | 6 | Create, upsert, date/teacher filtering |

**Infrastructure:** `InMemoryDbContextFactory` — fresh InMemory DB per test, seeds all 9 GradingConfigs.

---

## 13. C# FEATURES USED

The codebase makes extensive use of modern C# features including records for immutable DTOs, switch expressions for clean branching logic, and source-generated regex for compile-time email validation. Nullable reference types are enabled project-wide, and file-scoped namespaces are used consistently across all 100+ source files.

| Feature | Where | Example |
|---|---|---|
| **Records** | All 19 DTOs | `public sealed record TeacherDto(...)` |
| **Switch expressions** | GradingConfig, converters, file type | `subject switch { ... }` |
| **Source-generated Regex** | AuthenticationViewModel | `[GeneratedRegex]` attribute |
| **Nullable reference types** | Every project | `<Nullable>enable</Nullable>` |
| **Collection expressions** | DataService | `ObservableCollection<T> Items { get; } = []` |
| **required + init** | NavigationItemViewModel, GradeBookRowViewModel | `required string Title { get; init; }` |
| **File-scoped namespaces** | Every .cs file | `namespace Instrux.Domain.Models;` |
| **sealed classes** | All services, ViewModels | `sealed class GradeService : IGradeService` |
| **Generic constraint** | Repository | `where T : class` |
| **Expression-bodied members** | Throughout | `public bool HasError => ...` |

---

## 14. KNOWN ISSUES & GAPS

These are documented shortcomings identified during development that were deferred for later iteration. The most critical is plaintext password storage — currently passwords are compared as raw strings in `AuthenticationService.cs` with no hashing, a known security gap that needs bcrypt or Argon2 before any production deployment.

| Issue | Detail |
|---|---|
| **Plaintext passwords** | `PasswordHash` stores raw string — no hashing/bcrypt |
| **Manual DI** | New services require editing App.xaml.cs + DataService constructor |
| **UI MaxLength > DB** | Teacher.Email UI=254 vs DB=180; Assessment.Name UI=200 vs DB=140 |
| **Validation gap** | `Validation.ErrorTemplate` defined but never triggered (no IDataErrorInfo) |
| **No coverage reports** | coverlet installed but not configured |
| **LocalDB file locking** | Must `taskkill` before rebuild during dev |

---

## 15. FOLDER STRUCTURE — FULL MAP

### Layer-by-Layer Descriptions

**Instrux.Domain/Enums/** — Contains 8 enum files that define fixed sets of values used throughout the application (subjects, assessment types, attendance statuses, etc.). These ensure type safety and prevent invalid values from being used in business logic — e.g., a subject can only be one of the 9 DepEd subjects, never a random string.

**Instrux.Domain/Models/** — Contains 10 entity classes (Teacher, Class, Student, Assessment, Score, AttendanceRecord, CalendarEvent, TodoItem, ContentItem, GradingConfig) that represent the core data structures. Each entity has properties matching database columns and relationships but contains no EF Core annotations — all mapping is done separately in the Infrastructure layer.

**Instrux.Infrastructure/Data/** — Contains the `InstruxDbContext` (which exposes 10 DbSet properties for all entities) and the `InstruxDesignTimeDbContextFactory` (which provides a connection string for EF Core CLI commands like `dotnet ef migrations add`). This is where EF Core is configured with the SQL Server provider and where all entity configurations are loaded via `ApplyConfigurationsFromAssembly`.

**Instrux.Infrastructure/Data/Configurations/** — Contains 10 Fluent API configuration files (one per entity) that define table names, column types, max lengths, precision, indexes, and foreign key relationships. Each `IEntityTypeConfiguration<T>` class keeps the mapping details separate from the entity classes themselves, following the principle of persistence ignorance.

**Instrux.Infrastructure/Data/Migrations/** — Contains 3 auto-generated EF Core migration files that track database schema changes. The initial migration creates all 10 tables with their columns, constraints, and indexes; the designer and snapshot files enable EF Core to detect future schema changes for incremental migrations.

**Instrux.Infrastructure/Repositories/** — Contains the generic `IRepository` interface (11 methods) and its `Repository` implementation that wraps EF Core's `DbSet<T>` API. Services depend only on the interface (not EF Core directly), making them testable with any data store and keeping EF Core dependencies contained in the Infrastructure layer.

**Instrux.Services/Interfaces/** — Contains 9 service interfaces that define the application's contract layer (IAuthenticationService, IClassService, IStudentService, etc.). Each interface declares async methods with DTO parameters and return types, keeping the service contracts clean and independent of implementation details.

**Instrux.Services/Implementations/** — Contains 9 concrete service classes that implement the interfaces with actual business logic. Key implementations include: `AuthenticationService` (plaintext password login/register), `GradeService` (DepEd grade computation with WW/PT/QA weighting), `TeacherService` (full account deletion cascade), and `AttendanceService` (upsert pattern for daily records).

**Instrux.Services/DTOs/** — Contains 19 immutable record types that define the shape of data crossing between service and presentation layers. DTOs include input types (CreateClassDto, LoginRequestDto), output types (TeacherDto, GradeBookRowDto), and result types (AuthResultDto) — they exclude sensitive data (e.g., TeacherDto omits PasswordHash) and flatten complex relationships for UI consumption.

**Instrux.Services/Mapping/** — Contains the static `DtoMapper` class with 14 bidirectional conversion methods between domain entities and DTOs. Manually written (no AutoMapper), providing compile-time safety and full control over mapping logic like excluding the password hash from TeacherDto.

**Instrux.Services/Resolvers/** — Contains `GradingSystemResolver`, a thin static wrapper that delegates to `GradingConfig.FromSubject()`. Exists purely for dependency decoupling — services call the resolver instead of the domain model directly, making the grading system swappable without changing service code.

**Instrux.Services/Exceptions/** — Contains `ServiceException`, a custom exception class that carries a `UserFacingMessage` property for clean error display. Service methods throw this instead of raw exceptions, and the DataService catches them to show user-friendly messages in the UI's snackbar notification.

**Instrux.Application/Helpers/** — Contains 3 foundational MVVM building blocks: `ViewModelBase` (abstract class with `INotifyPropertyChanged` and `SetProperty<T>`), `RelayCommand` (ICommand for sync operations), and `RelayCommandAsync` (ICommand for async with re-entrancy guard). Every command in the app uses these — they enable CanExecute gating, error callbacks, and automatic button disabling during async operations.

**Instrux.Application/Converters/** — Contains 7 `IValueConverter` implementations that transform data types for XAML binding (e.g., AttendanceStatus → color brush, bool → Visibility, hex string → SolidColorBrush). These are essential for WPF data binding since XAML can't directly render enums or booleans — they're registered in App.xaml and available app-wide.

**Instrux.Application/Services/** — Contains 3 application-level singleton services: `DataService` (central reactive hub with 8 ObservableCollections, wrapping all service calls), `SessionService` (holds current teacher identity and auth state), and `NotificationService` (wraps MaterialDesign SnackbarMessageQueue for toast notifications). DataService is the most critical — it acts as a mediator between ViewModels and service layer, translating DTOs to domain objects for observable collection binding.

**Instrux.Application/ViewModels/** — Contains 13 ViewModel classes that implement the entire UI logic through bindable properties and commands. The largest is `ClassesViewModel` (553 lines) managing roster, attendance, grades, and content tabs; others handle authentication, dashboard stats, calendar, todos, settings, and navigation — each ViewModel corresponds to either a full page or a reusable component.

**Instrux.Application/Views/** — Contains 6 XAML UserControl files (plus code-behind) that define the visual layout for each page: Dashboard, Classes, Calendar, Todo, Settings, and Authentication. Each View binds to its corresponding ViewModel through data templates defined in App.xaml, keeping the XAML markup free of business logic with most code-behind files containing only `InitializeComponent()`.

**Instrux.Application/Resources/** — Contains 21 SVG icon files used throughout the UI for navigation items, action buttons, and status indicators. Also includes the Instrux logo and a waving hand icon used on the dashboard greeting — all rendered via SharpVectors.Wpf's `SvgViewbox` control.

**Instrux.Application/ root files** — Contains the entry point files: `App.xaml` (global styles, themes, converters, data templates), `App.xaml.cs` (DI container setup, EF migration, authentication loop), `appsettings.json` (database connection string), `MainWindow.xaml/.cs` (sidebar navigation shell), and `AuthenticationWindow.xaml/.cs` (login/register dialog). The startup flow is: build host → migrate DB → show auth window → on success, load data → show main window → loop on sign-out.

**Instrux.Tests/** — Contains 8 xUnit test classes with 52 total tests and a shared `InMemoryDbContextFactory` helper. Tests cover all 9 services and the domain mapper, using EF Core InMemory provider for isolated, fast integration tests that validate CRUD operations, cascade deletes, grade computation (82% benchmark), and teacher data isolation.

```
Instrux/
├── .git/
├── .gitignore
├── Directory.Build.props          # Global: suppress NU1903 warning
├── AGENTS.md                      # AI agent instructions + poster prompt
├── DESIGN.md                      # Design system spec (colors, layout, wireframes)
├── README.md                      # Full project documentation
├── cheatsheet.md                  # ← This file
│
├── Instrux.sln                    # Solution: 5 projects
│
├── Instrux.Domain/                # Pure domain — zero NuGet deps
│   ├── Enums/                     # 8 enum files
│   │   ├── AssessmentType.cs
│   │   ├── AttendanceStatus.cs
│   │   ├── ContentType.cs
│   │   ├── EventCategory.cs
│   │   ├── Priority.cs
│   │   ├── RecurrenceType.cs
│   │   ├── Subject.cs
│   │   └── SubjectGroup.cs
│   └── Models/                    # 10 entity POCOs
│       ├── Assessment.cs
│       ├── AttendanceRecord.cs
│       ├── CalendarEvent.cs
│       ├── Class.cs
│       ├── ContentItem.cs
│       ├── GradingConfig.cs       # Has static FromSubject() factory
│       ├── Score.cs
│       ├── Student.cs
│       ├── Teacher.cs
│       └── TodoItem.cs
│
├── Instrux.Infrastructure/        # EF Core data access layer
│   ├── Data/
│   │   ├── InstruxDbContext.cs    # 10 DbSets + ApplyConfigurationsFromAssembly
│   │   ├── InstruxDesignTimeDbContextFactory.cs  # For CLI migrations
│   │   ├── Configurations/       # 10 Fluent API IEntityTypeConfiguration files
│   │   │   ├── AssessmentConfiguration.cs
│   │   │   ├── AttendanceRecordConfiguration.cs
│   │   │   ├── CalendarEventConfiguration.cs
│   │   │   ├── ClassConfiguration.cs
│   │   │   ├── ContentItemConfiguration.cs
│   │   │   ├── GradingConfigConfiguration.cs
│   │   │   ├── ScoreConfiguration.cs
│   │   │   ├── StudentConfiguration.cs
│   │   │   ├── TeacherConfiguration.cs
│   │   │   └── TodoItemConfiguration.cs
│   │   └── Migrations/           # 3 files — InitialCreate only
│   │       ├── 20260519023932_InitialCreate.cs
│   │       ├── 20260519023932_InitialCreate.Designer.cs
│   │       └── InstruxDbContextModelSnapshot.cs
│   └── Repositories/
│       ├── IRepository.cs         # Generic CRUD interface (11 methods)
│       └── Repository.cs          # EF Core implementation
│
├── Instrux.Services/              # Business logic layer
│   ├── Interfaces/               # 9 service contracts
│   │   ├── IAuthenticationService.cs
│   │   ├── IAttendanceService.cs
│   │   ├── ICalendarEventService.cs
│   │   ├── IClassService.cs
│   │   ├── IContentService.cs
│   │   ├── IGradeService.cs
│   │   ├── IStudentService.cs
│   │   ├── ITeacherService.cs
│   │   └── ITodoService.cs
│   ├── Implementations/          # 9 concrete implementations
│   │   ├── AuthenticationService.cs   # Plaintext login/register
│   │   ├── AttendanceService.cs       # Upsert by StudentId+Date
│   │   ├── CalendarEventService.cs    # Month/today filtering
│   │   ├── ClassService.cs            # Cascade delete
│   │   ├── ContentService.cs          # Teacher-scoped via Class join
│   │   ├── GradeService.cs            # DepEd grade computation
│   │   ├── StudentService.cs          # Cascade: attendance+scores first
│   │   ├── TeacherService.cs          # Full account cascade (9 entities)
│   │   └── TodoService.cs             # Sorted, toggle with CompletedAt
│   ├── DTOs/                    # 19 immutable record types
│   │   ├── AssessmentDto.cs
│   │   ├── AttendanceBatchDto.cs
│   │   ├── AttendanceRecordDto.cs
│   │   ├── AuthResultDto.cs
│   │   ├── CalendarEventDto.cs
│   │   ├── ClassDto.cs
│   │   ├── ContentItemDto.cs
│   │   ├── CreateClassDto.cs
│   │   ├── CreateContentItemDto.cs
│   │   ├── CreateEventDto.cs
│   │   ├── CreateStudentDto.cs
│   │   ├── CreateTodoDto.cs
│   │   ├── GradeBookRowDto.cs
│   │   ├── LoginRequestDto.cs
│   │   ├── RegisterRequestDto.cs
│   │   ├── ScoreDto.cs
│   │   ├── StudentDto.cs
│   │   ├── TeacherDto.cs         # No PasswordHash
│   │   └── TodoItemDto.cs
│   ├── Mapping/
│   │   └── DtoMapper.cs          # Static 14-method mapper
│   ├── Resolvers/
│   │   └── GradingSystemResolver.cs  # Thin wrapper → GradingConfig.FromSubject()
│   └── Exceptions/
│       └── ServiceException.cs   # Custom exception + UserFacingMessage
│
├── Instrux.Application/           # WPF UI (Windows-only)
│   ├── App.xaml                  # Theme, converters, styles, data templates
│   ├── App.xaml.cs               # DI setup, migrations, auth loop
│   ├── appsettings.json          # LocalDB connection string
│   ├── MainWindow.xaml           # Sidebar 292px + ContentControl
│   ├── MainWindow.xaml.cs        # Wires SignOutRequested
│   ├── AuthenticationWindow.xaml # Auth dialog (980x680)
│   ├── AuthenticationWindow.xaml.cs  # DialogResult on success
│   ├── Helpers/
│   │   ├── ViewModelBase.cs      # INotifyPropertyChanged + SetProperty<T>
│   │   ├── RelayCommand.cs       # ICommand (sync)
│   │   └── RelayCommandAsync.cs  # ICommand (async, re-entrancy guard)
│   ├── Converters/               # 7 IValueConverter
│   │   ├── AttendanceStatusToBrushConverter.cs
│   │   ├── BoolToVisibilityConverter.cs
│   │   ├── EventCategoryToBrushConverter.cs
│   │   ├── InverseBoolConverter.cs
│   │   ├── NullToVisibilityConverter.cs
│   │   ├── PriorityToBrushConverter.cs
│   │   └── StringToBrushConverter.cs
│   ├── Services/                 # 3 app-level singletons
│   │   ├── DataService.cs        # 8 ObservableCollections + CRUD wrappers
│   │   ├── NotificationService.cs  # Snackbar toast wrapper
│   │   └── SessionService.cs     # CurrentTeacher + IsAuthenticated
│   ├── ViewModels/               # 13 ViewModels
│   │   ├── AuthenticationViewModel.cs
│   │   ├── MainDashboardViewModel.cs
│   │   ├── DashboardViewModel.cs
│   │   ├── ClassesViewModel.cs   # 553 lines — largest
│   │   ├── StudentRosterViewModel.cs
│   │   ├── AttendanceStudentViewModel.cs
│   │   ├── GradeBookRowViewModel.cs
│   │   ├── CalendarViewModel.cs
│   │   ├── CalendarDayViewModel.cs
│   │   ├── TodoViewModel.cs
│   │   ├── SettingsViewModel.cs
│   │   └── NavigationItemViewModel.cs
│   ├── Views/                    # 6 UserControls
│   │   ├── AuthenticationView.xaml + .cs
│   │   ├── DashboardView.xaml + .cs
│   │   ├── ClassesView.xaml + .cs
│   │   ├── CalendarView.xaml + .cs
│   │   ├── TodoView.xaml + .cs
│   │   └── SettingsView.xaml + .cs
│   └── Resources/               # 21 SVG icons + logo images
│       ├── MainLogo/
│       │   ├── Instrux_Logo.svg
│       │   └── waving_hand_icon.svg
│       ├── calendar-fold.svg
│       ├── chevron-right.svg
│       ├── circle-x.svg
│       ├── ellipsis-vertical.svg
│       ├── eye.svg / eye-closed.svg
│       ├── file-text.svg
│       ├── layout-dashboard.svg
│       ├── library-big.svg
│       ├── menu.svg
│       ├── plus.svg
│       ├── presentation.svg
│       ├── search.svg
│       ├── settings.svg / settings-2.svg
│       ├── square-check-big.svg
│       ├── trash.svg
│       ├── upload.svg
│       └── user.svg
│
├── Instrux.Tests/               # xUnit — 52 tests
│   ├── InMemoryDbContextFactory.cs   # Fresh DB per test + seeding
│   ├── DtoMapperTests.cs             # 6 tests
│   ├── GradingConfigTests.cs         # 9 tests
│   ├── AuthenticationServiceTests.cs # 6 tests
│   ├── GradeServiceTests.cs          # 9 tests
│   ├── TeacherServiceTests.cs        # 6 tests
│   ├── ClassServiceTests.cs          # 5 tests
│   ├── StudentServiceTests.cs        # 4 tests
│   └── AttendanceServiceTests.cs     # 6 tests
│
└── Instrux.Tests.csproj         # xUnit + InMemory + coverlet
```

---

## 16. POSSIBLE PROFESSOR Q&A

**Q: What architecture?** → 4-layer: Domain→Infrastructure→Services→Application. Strict downward deps.

**Q: What design patterns?** → Layered Architecture, MVVM, Singleton, Command, Observer, Static Factory, Upsert, Mediator, Generic Repository.

**Q: How is grade computed?** → Quiz→WW, Activity→PT, Exam→QA. Each averaged as `value/maxScore×100`. Formula: `(WW_avg×WW%) + (PT_avg×PT%) + (QA_avg×QA%)`. Thresholds: ≥90 Excellent, ≥80 On track, ≥70 Watch, <70 Support.

**Q: What are the 3 weight groups?** → Languages/Social Sciences (30/50/20), Math/Science (40/40/20), Skills/Arts (20/60/20).

**Q: How many entities? Enums?** → 10 entities, 8 enums.

**Q: DB provider?** → SQL Server LocalDB. Connection: `Server=(localdb)\MSSQLLocalDB;Database=InstruxDbLocal;Trusted_Connection=True;`

**Q: How is attendance stored?** → One record per student per day. Unique on `(StudentId, Date)`. Upsert pattern.

**Q: Testing framework + count?** → xUnit + EF Core InMemory. 52 tests, all passing, ~3s duration.

**Q: Account deletion?** → Full cascade: Scores→Attendance→Students→Assessments→Content→Classes→Events→Todos→Teacher. Single transaction. Other teachers isolated.

**Q: UI library?** → MaterialDesignThemes 5.3.2 + SharpVectors.Wpf 1.8.5 for SVGs.

**Q: How is data shared between ViewModels?** → DataService singleton with 8 ObservableCollections. All ViewModels reference same instance. CollectionChanged → auto UI re-render.

**Q: DepEd order?** → DepEd Order No. 8, s. 2015.

**Q: 9 subjects?** → English, Filipino, AralingPanlipunan, EdukasyonSaPagpapakatao, Mathematics, Science, TLE, HomeEconomics, MAPEH.

**Q: Security gap?** → Passwords stored as plaintext. No hashing. Deferred (bcrypt/Argon2).

**Q: How many DTOs?** → 19.

**Q: Repository methods?** → 11: GetById, Add, Update, Delete, DeleteRange, SaveChanges, ListAsync, FirstOrDefault, Find, Any, Count, Query.

**Q: How are scores unique?** → Composite unique index on `(StudentId, AssessmentId)` — one score per student per assessment.

**Q: What is GradingConfig?** → Domain model mapping each Subject to its DepEd weight group. Has `FromSubject()` static factory returning correct WW/PT/QA percentages.

**Q: What does DataService do?** → Central mediator holding 8 ObservableCollections. Wraps all service calls with error handling. Keeps in-memory cache in sync with DB. ViewModels bind to its collections.

**Q: How is async handled?** → All service methods return `Task<T>`. `RelayCommandAsync` prevents re-entrancy. `async void` only for event handlers.
