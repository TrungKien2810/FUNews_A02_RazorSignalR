# FUNews A02 - ASP.NET Core Razor Pages + SignalR

FUNewsManagementSystem built with ASP.NET Core Razor Pages (.NET 8), SignalR, EF Core and SQL Server.
Requirements: [docs/REQUIREMENTS.md](docs/REQUIREMENTS.md).

## Solution structure (`src/TrungKien_SE19C_A02.sln`)

| Project | Role |
|---|---|
| `BusinessObjects` | Entities only (POCO; no EF Core dependency) |
| `DataAccessObjects` | `FUNewsManagementContext` (EF Core, reads the connection string from `appsettings.json`) + DAOs implemented as **Singletons** (`AccountDAO.Instance`, `CategoryDAO.Instance`, ...) |
| `Repositories` | **Repository pattern**: `IXxxRepository` / `XxxRepository` calling the DAOs |
| `TrungKienRazorPages` | Razor Pages web app + SignalR hub. Pages only use repositories (never the DbContext) |

Flow: `Razor Page -> Repository -> DAO -> DbContext`.

## Real-time (SignalR)

- Hub: `Hubs/NewsHub.cs`, mapped at `/newsHub`.
- When a Staff creates / updates / deletes a news article (`Pages/Staff/News/Index.cshtml.cs`) the server broadcasts
  `NewsChanged` to **all** connected clients.
- Clients (`wwwroot/js/site.js`) show a toast and reload the regions marked `data-realtime-url` without reloading the page:
  public news list, staff news table, news history, admin report. An open news details page refreshes itself
  (or shows "this news was deleted"). The navbar badge shows the connection state (Live / Offline).

## Run

1. Create the database: run `database/FUNewsManagement.sql` in SQL Server (creates `FUNewsManagement`).
2. Edit `src/TrungKienRazorPages/appsettings.json` → `ConnectionStrings:DefaultConnection` for your SQL Server.
3. Open `src/TrungKien_SE19C_A02.sln` in Visual Studio and run `TrungKienRazorPages`
   (or `cd src/TrungKienRazorPages && dotnet run`). The Login page is the default page.
   Open the news page in a second browser to watch real-time updates.

If the app starts but every data screen returns HTTP 500 with
*"A network-related or instance-specific error … error: 26"*, the connection string names a SQL
Server instance the app cannot resolve. This bites named instances (`Server=.\SQLEXPRESS`) when
the **SQL Server Browser** service is stopped: `sqlcmd` may still connect over shared memory
while the app cannot. Either start SQL Browser, or address the instance by its TCP endpoint —
for example `Server=tcp:127.0.0.1,1433;Database=FUNewsManagement;User Id=sa;Password=…;TrustServerCertificate=True`.

## Accounts

| Role | Email | Password |
|---|---|---|
| Admin (from `appsettings.json`) | admin@FUNewsManagementSystem.org | @@abc123@@ |
| Staff (role 1) | IsabellaDavid@FUNewsManagement.org | @1 |
| Lecturer (role 2) | EmmaWilliam@FUNewsManagement.org | @1 |

**Why any Staff account can edit any article.** The requirement gives Staff
"Manage news article (includes tags)" with Read / Create / Update / Delete / Search, and ties
authorship only to a separate feature: "View news history created by him/her" (`/Staff/News/History`,
which does filter by the signed-in author). Management is therefore not scoped per author, and the
provided seed data assumes the same thing — all five seeded articles are written by Lecturer
accounts, so scoping management to "own articles" would leave a Staff login with nothing it may
edit.

## UI design system

All presentation lives in `src/TrungKienRazorPages/wwwroot/css/site.css`, in three layers:
a token block, then components built only from those tokens, then per-screen composition.
A screen that reaches for a raw hex or a one-off pixel value has left the system.

- **Colour** — one brand hue (institutional navy `#12305c`), a neutral ramp tinted ~5% toward
  it, and a semantic layer (`--surface`, `--border`, `--text-muted`, …) that screens reference
  instead of raw scale values. One accent (`#8a5a00`, a deep amber) is spent only on the action
  that *creates* something — `New account`, `New article`, `New category`. Everything else is
  navy or a quiet outline. Every text/background pair was measured, not assumed.
- **Type** — Georgia for editorial voice (page titles, article headlines) against Segoe UI for
  UI and body text, on a 1.25 ratio scale, 7 sizes from a 16px base, with tracking adjusted per
  size (negative at display sizes, positive on the 12px all-caps labels). Both faces ship with
  the OS, so there is no webfont download and no swap reflow.
- **Space, radius, elevation** — one spacing scale used at three density tiers; radius tied to
  element size (control 4px, card 8px, sheet 12px); two elevation levels, and one separation
  mechanism per surface (a hairline border *or* a shadow, never both).
- **The signature decision** — the news list gives the newest article display size
  (`.lead-story`) and treats the rest as a quiet list (`.story-list`). A grid of equal cards
  gives the eye nowhere to land; a newsroom makes the first story loudest.
- **Theme** — light only, declared with `color-scheme: light`. A second theme that was never
  designed is worse than one honest theme, and it stops the browser painting native controls
  against a background nobody chose.

### Known cosmetics

The bundled Bootstrap is **5.1.0**, which writes button colours as literals rather than reading
`--bs-btn-*` custom properties (those arrived in 5.2). Overriding the variables alone silently
does nothing, which is why the button rules in `site.css` set the properties directly and say so
in a comment. If Bootstrap is ever upgraded past 5.2, those rules can move back to variables.

## Verification

What was actually run against a live instance, and what was not:

**Verified by running it.** `dotnet build` clean, 0 warnings. Logged in as both Admin and Staff
against SQL Server with the script's seeded data, and checked 9 distinct pages
(`News/Index`, `News/Details/{id}`, `Admin/Accounts`, `Admin/Report`, `Account/Login`,
`Account/AccessDenied`, `Staff/News`, `Staff/Categories`, `Staff/News/History`,
`Staff/Profile`): **no accessibility violations, and no focusable element without a visible
focus indicator**. Also verified: create/delete popups open from the keyboard, an empty submit
returns server-side validation into the open popup (4 field errors, modal stays open, console
clean), the report populated with a real date range, and the news list at 375 / 768 / 1440px.
Console is free of errors and warnings throughout.

**Not verified.** No real screen reader, no cross-browser or real-device testing, no automated
visual-regression baseline, and no load testing. The two "control under 24px" findings are the
skip link (measured while unfocused; it grows to a full panel on `:focus-visible`) and the
footer's inline text link — WCAG 2.5.8 exempts targets in a sentence or block of text.

## Features

- **Anonymous / Lecturer**: view and search active news articles (keyword, category, tag), read details.
- **Admin** (`/Admin/*`): manage accounts (CRUD + search by name/email/role); statistic report by StartDate–EndDate
  on news created date, sorted descending.
- **Staff** (`/Staff/*`): manage categories (CRUD + search; a category used by news cannot be deleted), manage news
  articles including tags (CRUD + search, real-time), manage own profile, view own news history.
- Create / Edit run in Bootstrap popup dialogs (AJAX page handlers, server + client validation); Delete always shows a confirmation popup.
- All inputs are validated with data annotations (`ViewModels/*`).
