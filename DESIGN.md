# Instrux — Design System

## 1. Color Palette

### Primary (Blue)

| Token | Hex | RGB | Usage |
|---|---|---|---|
| `PrimaryDark` | `#2C5EAD` | `44,94,173` | Sidebar background, button hover, Urgent priority indicator, WW weight number |
| `PrimaryMid` | `#1591DC` | `21,145,220` | Primary button fill, selected nav item bg, event badge, input focus border, QA weight number |
| `PrimaryLight` | `#4BB8FA` | `75,184,250` | Scrollbar thumb, secondary highlights, Late attendance circle |
| `PrimarySoft` | `#C4E2F5` | `196,226,245` | Soft button bg, calendar today cell highlight, search bar bg |

### Secondary (Teal)

| Token | Hex | RGB | Usage |
|---|---|---|---|
| `SecondaryDark` | `#005461` | `0,84,97` | Sidebar bottom overlay stripe |
| `SecondaryMid` | `#0C7779` | `12,119,121` | Button active state, PT weight number |
| `SecondaryLight` | `#249E94` | `36,158,148` | Low priority dot, success toast bg |
| `SecondaryPale` | `#3BC1A8` | `59,193,168` | Present attendance filled circle |

### Danger

| Token | Hex | RGB | Usage |
|---|---|---|---|
| `Danger` | `#D32F2F` | `211,47,47` | Delete buttons, Sign Out, error text, Absent circle |
| `DangerSoft` | `#FFE4E4` | `255,228,228` | Error toast background, Absent row bg |

### Neutral

| Token | Hex | Usage |
|---|---|---|
| `Surface` | `#F6F8FC` | Main application background |
| `Card` | `#FFFFFF` | Card, panel, modal surfaces |
| `SurfaceMuted` | `#EDF1F7` | Hover backdrop, muted section bg |
| `Ink` | `#1A1C1E` | Primary body text |
| `MutedInk` | `#5F6368` | Captions, labels, secondary text, Excused circle outline |
| `Line` | `#DCE3ED` | Borders, dividers, separator lines |

---

## 2. Typography

- **Font family:** `Segoe UI Variable Text, Segoe UI`
- **Scale:**

| Style | Size | Weight | Color |
|---|---|---|---|
| Page Title | 34pt | SemiBold | Ink |
| Section Title | 20pt | SemiBold | Ink |
| Card Title | 17pt | SemiBold | Ink |
| Body | 15pt | Regular | Ink |
| Caption | 13pt | Regular | MutedInk |
| Stat Number | 34pt | SemiBold | Ink |
| Weight Percentage | 28pt | SemiBold | (per subject group color) |

---

## 3. Layout

### Window Structure (MainWindow.xaml)

```
┌─[Sidebar, 282px]────┬────────────────────────────────┐
│ PrimaryDark bg       │  White bg                     │
│                      │                               │
│ ┌────────────────┐   │  [Page Title]      [🔍 Search]│
│ │ Logo (128px)   │   ├───────────────────────────────┤
│ │ Instrux        │   │                               │
│ │ caption        │   │                               │
│ └────────────────┘   │   Content Area                │
│                      │   (ContentControl             │
│  ○ Dashboard         │    bound to CurrentPage)      │
│  ○ Classes           │                               │
│  ● Calendar          │                               │
│  ○ Todo              │                               │
│  ○ Settings          │                               │
│                      │                               │
│  ─── Quick Access ── │                               │
│  › Recent class      │                               │
│  › Recent class      │                               │
│                      │                               │
│  ┌────────────────┐  │                               │
│  │ Teacher Name   │  │                               │
│  │ [Sign Out]     │  │                               │
│  └────────────────┘  │                               │
└──────────────────────┴───────────────────────────────┘
```

### Sidebar Layout

| Zone | Height | Content |
|---|---|---|
| Brand | Auto | 128px logo, "Instrux" title, caption text |
| Navigation | * (stretch) | 5 items: Dashboard, Classes, Calendar, Todos, Settings |
| Quick Access | Auto | 2-4 shortcut items (recent class, upcoming event) |
| User Footer | Auto | Teacher name + email + Sign Out button (Danger bg) |

### Navigation Item States

| State | Background | Icon | Text |
|---|---|---|---|
| Default | Transparent | White @ 60% | White @ 70% |
| Hover | White @ 8% opacity | White | White |
| Selected | PrimaryMid @ 20% overlay | White | White, SemiBold |

---

## 4. Component Specs

### Buttons

| Style | Background | Text | Corner | Padding | Border |
|---|---|---|---|---|---|
| Primary | PrimaryMid | White | 14px | 18,10 | None |
| Soft | PrimarySoft | PrimaryDark | 14px | 18,10 | None |
| Danger | Danger | White | 14px | 18,10 | None |
| Circle (status) | Depends on selected | — | 50% (square) | 0 | 2px status color |

### Cards

| Property | Value |
|---|---|
| Background | White |
| Corner radius | 24px |
| Border | 1px Line |
| Shadow | DropShadow: blur 20, opacity 5%, depth 8px, 270° |
| Padding | 22px |
| Margin bottom | 16px |

### Inputs

| Property | Value |
|---|---|
| Background | White |
| Border | 1px Line |
| Focus border | PrimaryMid |
| Padding | 14px horizontal, 10px vertical |
| Font size | 14pt |

---

## 5. Attendance Circle Selector

```
 ┌──────────────────────────────────────────┐
 │  Ari Tan                                 │
 │                                          │
 │  ● Present   ○ Late   ○ Absent  ○ Excused│
 │  (#3BC1A8)   (#4BB8FA) (#D32F2F) (#5F6368)│
 │                                          │
 │  (second row)                            │
 │  ○ Present   ● Late   ○ Absent  ○ Excused│
 └──────────────────────────────────────────┘
```

### Behavior

| State | Circle Fill | Action on Click |
|---|---|---|
| Unselected | White (transparent) | Fills with status color, saves to DB |
| Selected | Status color | Already selected — no change |

- **One click = set + auto-save. No save button.**
- All 4 statuses visible at all times so teacher can see available options.
- Clicking a filled circle does nothing (no toggle — prevents accidental unset).

---

## 6. Subject + Grading Weight Display

When a class is selected, the detail header shows:

```
┌────────────────────────────────────────────────────────────┐
│ Mathematics                                    [❖ Section] │
│                                                             │
│ Subject: [ Mathematics ▼ ]                                  │
│                                                             │
│  ┌──────────────────┬──────────────────┬──────────────────┐  │
│  │  Written Works   │ Performance Tasks│ Quarterly Assess │  │
│  │      40%         │      40%         │      20%         │  │
│  │  PrimaryDark     │  SecondaryMid    │  PrimaryMid      │  │
│  └──────────────────┴──────────────────┴──────────────────┘  │
└────────────────────────────────────────────────────────────┘
```

The weights update **immediately** every time the Subject dropdown changes.

---

## 7. Grades Tab

### Dynamic Columns

| Student | Quiz 1 (WW) | Quiz 2 (WW) | Activity 1 (PT) | Exam 1 (QA) | WW Avg | PT Avg | QA Avg | **Initial Grade** |
|---|---|---|---|---|---|---|---|---|
| Ari Tan | 18/20 | 15/20 | 42/50 | 76/80 | 82.5% | 84% | 95% | **86.15** |
| Mila Reyes | 16/20 | 17/20 | 38/50 | 70/80 | 82.5% | 76% | 87.5% | **80.85** |

- Columns auto-generate per assessment
- **Initial Grade column** is read-only, computed as: `(WW_avg × WW%) + (PT_avg × PT%) + (QA_avg × QA%)`
- Grades are color-coded: ≥90 green, ≥80 blue, ≥70 orange, <70 red

### Assessment Type Mapping

| AssessmentType | DepEd Category |
|---|---|
| Quiz | Written Works |
| Activity | Performance Tasks |
| Exam | Quarterly Assessment |

---

## 8. Subject Enum (Grades 7-10, DepEd)

| # | Subject | Group | WW | PT | QA |
|---|---|---|---|---|---|
| 1 | English | Languages & Social Sciences | 30% | 50% | 20% |
| 2 | Filipino | Languages & Social Sciences | 30% | 50% | 20% |
| 3 | Araling Panlipunan | Languages & Social Sciences | 30% | 50% | 20% |
| 4 | Edukasyon sa Pagpapakatao | Languages & Social Sciences | 30% | 50% | 20% |
| 5 | Mathematics | Math & Science | 40% | 40% | 20% |
| 6 | Science | Math & Science | 40% | 40% | 20% |
| 7 | TLE | Skills & Arts | 20% | 60% | 20% |
| 8 | Home Economics | Skills & Arts | 20% | 60% | 20% |
| 9 | MAPEH | Skills & Arts | 20% | 60% | 20% |

---

## 9. Page Layouts

### Dashboard

```
┌─────────────────────────────────────────────────────────────┐
│  Gradient hero (PrimaryDark → PrimaryMid → SecondaryDark)    │
│  "Good day, Elena"  "Tuesday, May 19"                       │
│  [summary text]                                  [Logo]     │
├─────────┬─────────┬─────────┬──────────┬────────────────────┤
│  ▐◼      │  ▐◼      │  ▐◼      │  ▐◼       │                    │
│  4       │  16     │  2      │  0       │  Upcoming          │
│  Classes │ Students│ Due Tod │ Attend.  │  • STEM 9 Lab      │
├─────────┴─────────┴─────────┴──────────┤  • Math Exam        │
│  Class Workspaces                      ├────────────────────┤
│  ┌────────┐  ┌────────┐               │  Focus Tasks        │
│  │STEM 9  │  │Eng 10  │               │  ▎Prepare trays     │
│  │Science │  │Lit.    │               │  ▎Grade drafts      │
│  └────────┘  └────────┘               │                     │
└───────────────────────────────────────┴─────────────────────┘
```

### Classes Detail

```
┌─[320px]───────────────┬──────────────────────────────────────┐
│  Classes               │  [Math 8]            [❖ Nova]       │
│                        │                                      │
│  [______Add class__]   │  Subject: [Mathematics          ▼]  │
│                        │                                      │
│  ┌──────────────────┐  │  ┌──────────────────┬──────────────┐ │
│  │ ▎ STEM 9         │  │  │  WW: 40%  PT: 40%│ QA: 20%      │ │
│  │   Integrated Sci │  │  └──────────────────┴──────────────┘ │
│  ├──────────────────┤  │                                      │
│  │ ▎ English 10     │  │  ┌─Tab: Roster│Attend.│Grades│Cont.┐│ │
│  ├──────────────────┤  │  │                                   ││
│  │ ▎ Advisory       │  │  │  [Search...]     [+ Add Student] ││
│  │ ▎ Math 8   ←sel  │  │  │                                   ││
│  └──────────────────┘  │  │  Name    │ Student ID │ Email     ││
│                        │  │  Ari Tan │ STU-202600 │ ari.t...  ││
│                        │  │  Mila... │ STU-202601 │ mila.r... ││
│                        │  └───────────────────────────────────┘│
└────────────────────────┴──────────────────────────────────────┘
```

### Classes → Attendance Tab

```
┌──────────────────────────────────────────────────────────────┐
│  Date: [2026-05-19 ▼]                                        │
│                                                              │
│  ┌──────────────────────────────────────────────────────────┐│
│  │  Ari Tan                                                 ││
│  │  ● Present   ○ Late   ○ Absent   ○ Excused              ││
│  ├──────────────────────────────────────────────────────────┤│
│  │  Mila Reyes                                              ││
│  │  ○ Present   ○ Late   ● Absent   ○ Excused              ││
│  └──────────────────────────────────────────────────────────┘│
│  (scrollable, one row per student)                           │
└──────────────────────────────────────────────────────────────┘
```

### Calendar

```
┌────────────────────────────────┬────────────────────────────┐
│  ‹  June 2026  ›               │  Agenda                    │
│                                │                            │
│  Su  Mo  Tu  We  Th  Fr  Sa   │  Today's Events            │
│ ┌──┐┌──┐┌──┐┌──┐┌──┐┌──┐┌──┐│  ● STEM 9 Lab Demo 9-10AM │
│ │  ││  ││  ││  ││  ││  ││ 1││  ● Grade submission         │
│ ├──┤├──┤├──┤├──┤├──┤├──┤├──┤│                            │
│ │ 2││ 3││ 4││ 5││ 6││ 7││ 8││  Upcoming                  │
│ │  ││  ││  ││  ││  ││  ││  ││  ● Math 8 Exam (Jun 2)     │
│ ├──┤├──┤├──┤├──┤├──┤├──┤├──┤│  ● Parent conf. (Jun 7)    │
│ │ 9││10││11││12││13││14││15││                            │
│ └──┘└──┘└──┘└──┘└──┘└──┘└──┘│                            │
│ ...6 rows                     │                            │
└────────────────────────────────┴────────────────────────────┘
```

### Todo

```
┌──────────────────────────────────────────────────────────────┐
│  [Add a new task...          ]  [Medium ▼]  [+ Add]         │
├──────────────────────────────────────────────────────────────┤
│  ┌─All───┬──Today──┬──Upcoming──┬──Completed──┐             │
│  │       │         │            │              │             │
│  │ ■Lab  │ ■Lab    │ ○Grade     │ ✓Send email  │             │
│  │ ■Grade│ ■Send   │ ○Export    │ ✓Review      │             │
│  │ ■Send │   email │   results  │              │             │
│  │       │         │            │              │             │
│  └───────┴─────────┴────────────┴──────────────┘             │
└──────────────────────────────────────────────────────────────┘
```

### Settings

```
┌────────────────────┬────────────────────────────────────────┐
│  [Logo] Teacher    │  🔒 Lock / unlock                      │
│  Profile           │                                        │
│                    │  Full name                             │
│  Keep your         │  [Teacher Name                  ]      │
│  workspace clean.  │                                        │
│                    │  Nickname                              │
│                    │  [Teacher                        ]      │
│                    │                                        │
│                    │  Email                                 │
│                    │  [teacher@email.com              ]      │
│                    │                                        │
│                    │  [Save profile]                        │
├────────────────────┴────────────────────────────────────────┤
│  Clean Architecture     │  Instrux                          │
│  Domain models stay in  │  Teacher-first productivity hub   │
│  Instrux.Domain.        │  for attendance, grades, ...      │
└─────────────────────────────────────────────────────────────┘
```

---

## 10. Responsive Behavior

| Property | Value |
|---|---|
| Minimum window | 1040 × 680 |
| Sidebar | Fixed 282px (never collapses) |
| Card grids | WrapPanel — auto-flow to next row |
| Calendar | UniformGrid maintains square cells |
| Scrolling | Content area scrolls vertically per-page |
| Window startup | CenterScreen |
