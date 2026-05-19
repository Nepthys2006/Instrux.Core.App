# Instrux — Build Tracker

## Status Legend

- ✅ Completed
- 🔄 In Progress
- ⬜ Pending

---

## Phase 1: Domain Layer ✅

| Item | Status |
|---|---|
| Enums: AttendanceStatus, AssessmentType, ContentType, Priority, RecurrenceType, EventCategory | ✅ |
| Enums: Subject, SubjectGroup | ✅ |
| Models: Teacher, Class, Student, AttendanceRecord, Assessment, Score, CalendarEvent, TodoItem, ContentItem | ✅ |
| Model: GradingConfig (with static FromSubject weight resolver) | ✅ |
| Class.Subject changed from `string?` to `Subject` enum | ✅ |

## Phase 2: Project Structure ✅

| Item | Status |
|---|---|
| Instrux.Domain (net10.0) | ✅ |
| Instrux.Infrastructure (net10.0) — EF Core + SQL Server LocalDB | ✅ |
| Instrux.Services (net10.0) — DTOs, interfaces, implementations, resolvers, mapper | ✅ |
| Instrux.Application (net10.0-windows) — WPF with DI host | ✅ |
| Solution file with all 4 projects | ✅ |

## Phase 3: Design Documentation ✅

| Item | Status |
|---|---|
| `DESIGN.md` — Full design system with color palette, layout diagrams, component specs | ✅ |
| `AGENTS.md` — This tracker file | ✅ |
| App.xaml — new color palette (Primary Blue / Secondary Teal / Danger Red) | ✅ |

---

## Phase 4: Infrastructure (EF Core) ✅

| Item | Status |
|---|---|
| InstruxDbContext with 10 DbSets | ✅ |
| Entity type configurations (9 + 1 grading reference) | ✅ |
| DatabaseSeeder.cs removed (no seed/demo data) | ✅ |
| InstruxDesignTimeDbContextFactory.cs | ✅ |
| Initial EF migration to LocalDB | ✅ |
| appsettings.json with connection string | ✅ |

## Phase 5: Authentication ✅

| Item | Status |
|---|---|
| AuthenticationWindow.xaml — borderless, centered, dark backdrop | ✅ |
| AuthenticationWindow.xaml.cs — drag-move, close on success | ✅ |
| AuthenticationViewModel.cs — sign-in/sign-up via IAuthenticationService | ✅ |
| AuthenticationView.xaml — email, password, mode toggle, error display | ✅ |
| IAuthenticationService + AuthenticationService | ✅ |

## Phase 6: Services Layer ✅

| Item | Status |
|---|---|
| 18 DTOs in Services/DTOs/ | ✅ |
| 9 service interfaces in Services/Interfaces/ | ✅ |
| 9 service implementations in Services/Implementations/ | ✅ |
| DtoMapper.cs — static manual mapping Domain ↔ DTOs | ✅ |
| GradingSystemResolver.cs — subject → weight lookup | ✅ |

## Phase 7: DI + Startup Flow ✅

| Item | Status |
|---|---|
| App.xaml — remove StartupUri, add merged dictionary for palette | ✅ |
| App.xaml.cs — Host.CreateDefaultBuilder, DI registration, auth → main flow | ✅ |
| DataService.cs — singleton, ObservableCollections, calls service layer | ✅ |
| SessionService.cs — login/logout, current teacher state | ✅ |
| RelayCommandAsync.cs helper | ✅ |

## Phase 8: MainWindow Shell ✅

| Item | Status |
|---|---|
| Sidebar with PrimaryDark bg, nav items, SVG icons | ✅ |
| Content frame (ContentControl bound to CurrentPage) | ✅ |
| Top bar with page title + search placeholder | ✅ |
| Quick Access section (recent classes, upcoming events shortcuts) | ✅ |
| Sign Out button at sidebar bottom (Danger bg) | ✅ |
| User info panel above sign-out | ✅ |

## Phase 9: Dashboard Page ✅

| Item | Status |
|---|---|
| Gradient hero with greeting, date, teacher name | ✅ |
| 4 stat cards: total classes, total students, tasks due today, attendance marked | ✅ |
| Class workspace cards (clickable → navigate to class) | ✅ |
| Upcoming events panel | ✅ |
| Focus tasks panel | ✅ |
| Auto-refresh when data changes in other pages | ✅ |

## Phase 10: Classes Page — Full CRUD + DepEd Grading ✅

| Item | Status |
|---|---|
| Create class with name + subject dropdown (9 DepEd subjects) | ✅ |
| Delete class with confirmation | ✅ |
| Subject→weight display (WW / PT / QA percentages) live on class select | ✅ |
| Student roster: search, add, delete | ✅ |
| **Attendance: circle selector (● Present ○ Late ○ Absent ○ Excused) with auto-save** | ✅ |
| Grades: dynamic columns per assessment, inline edit, weighted DepEd computation | ✅ |
| Grade formula: `(WW_avg × WW%) + (PT_avg × PT%) + (QA_avg × QA%)` | ✅ |
| Content tab: file list, upload button, delete | ✅ |

## Phase 11: Calendar Page — Full CRUD ✅

| Item | Status |
|---|---|
| Month grid (6 rows × 7 cols) with day cells | ✅ |
| Click day → create event | ✅ |
| Right panel: today's agenda + upcoming events list | ✅ |
| Delete event from agenda | ✅ |

## Phase 12: Todo Page — Full CRUD ✅

| Item | Status |
|---|---|
| Quick-add bar with title + priority dropdown | ✅ |
| Segmented filter: All / Today / Upcoming / Completed | ✅ |
| Today column — priority dots, toggle complete, delete | ✅ |
| Upcoming column — due date, toggle complete | ✅ |
| Completed column — strikethrough, toggle back | ✅ |

## Phase 13: Settings Page ✅

| Item | Status |
|---|---|
| Profile form: full name, nickname, email | ✅ |
| Lock/unlock toggle — fields disabled when locked | ✅ |
| Save edits back to SessionService → DB | ✅ |

## Phase 14: Cleanup ✅

| Item | Status |
|---|---|
| Delete MainDashboardView stub (sidebar lives in MainWindow.xaml) | ✅ |
| Remove old in-memory DataService seed code | ✅ |
| Final build + smoke test | ✅ |

---

## Architecture Decisions

| Decision | Choice |
|---|---|
| **Database** | SQL Server LocalDB via EF Core (`(localdb)\MSSQLLocalDB`, database `InstruxDbLocal`) |
| **DI** | Microsoft.Extensions.DependencyInjection via Host.CreateDefaultBuilder |
| **Data access** | DbContext injected directly into service implementations (no Repository) |
| **UI sync** | Singleton DataService holds ObservableCollections — ViewModels bind to it |
| **Mapping** | Manual static DtoMapper.cs (no AutoMapper) |
| **Async** | RelayCommandAsync for all service calls |
| **Dark mode** | Not implemented |
| **Authentication** | Separate AuthenticationWindow (borderless, modal) → opens MainWindow |
| **Navigation** | Manual view swapping via MainDashboardViewModel.CurrentPage |
| **Icons** | SharpVectors.Wpf — SVG files from Resources/ folder |
| **SVG URIs** | Explicit pack URIs: `pack://application:,,,/Resources/icon.svg` |
| **Attendance** | Circle selector — one click per status, auto-saves to DB |
| **Grading** | DepEd Order No. 8, s. 2015 — Subject enum → automatic weight assignment |
| **Assessment map** | Quiz → Written Works, Activity → Performance Tasks, Exam → Quarterly Assessment |
| **Formulas** | `Grade = (WW_avg × WW%) + (PT_avg × PT%) + (QA_avg × QA%)` |

## Color Palette

| Role | Hex | Usage |
|---|---|---|
| PrimaryDark | `#2C5EAD` | Sidebar bg, primary buttons hover, urgent priority |
| PrimaryMid | `#1591DC` | Primary buttons, selected nav, event badges, input focus |
| PrimaryLight | `#4BB8FA` | Scrollbar, secondary highlights, Late attendance |
| PrimarySoft | `#C4E2F5` | Soft buttons, calendar today, search bg |
| SecondaryDark | `#005461` | Deep accents, sidebar overlay |
| SecondaryMid | `#0C7779` | Active states, PT weight display |
| SecondaryLight | `#249E94` | Low priority, success |
| SecondaryPale | `#3BC1A8` | Present attendance circle |
| Danger | `#D32F2F` | Delete, sign-out, errors, Absent circle |
| DangerSoft | `#FFE4E4` | Error bg, Absent bg |
| Ink | `#1A1C1E` | Primary text |
| MutedInk | `#5F6368` | Captions, Excused circle |
| Line | `#DCE3ED` | Borders, dividers |
| Surface | `#F6F8FC` | App background |
| Card | `#FFFFFF` | Card backgrounds |

## Key Files

| File | Purpose |
|---|---|
| `DESIGN.md` | Full design system with color palette, layout diagrams, component specs |
| `AGENTS.md` | This file — build tracker and decisions |
| `InstruxDbContext.cs` | EF Core context with 10 DbSets |
| `DtoMapper.cs` | Static manual mapper: Domain ↔ DTOs |
| `GradingSystemResolver.cs` | Subject → WW/PT/QA weight lookup |
| `DataService.cs` | Singleton — holds ObservableCollections, calls services |
| `SessionService.cs` | Current teacher session, login/logout |
| `MainDashboardViewModel.cs` | Shell navigation hub |

## Commands

```bash
dotnet build                                   # Build entire solution
dotnet run --project Instrux.Application       # Run the app
dotnet ef migrations add <name>                # Create migration
dotnet ef database update                      # Apply to LocalDB
```

## Project Dependency Order

```
Instrux.Domain (no deps)
  ↑
Instrux.Infrastructure (Domain + EF Core NuGet)
  ↑
Instrux.Services (Domain)
  ↑
Instrux.Application (all 3 above + WPF NuGets)
```
