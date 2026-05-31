# TechnoDating — Project Context

> Living document of strategic direction, USP exploration, and competitive context. Maintained across sessions. Update as decisions firm up or change.
>
> **Companion file:** [`BACKLOG.md`](BACKLOG.md) — future-feature ideas not on the current build path, grouped by tier. Append new ideas there; promote into `CLAUDE.md` when an item enters active development.

## Elevator pitch (working)

A music + festival-anchored dating app for the Netherlands. Match on **actual music taste and listening behaviour**, then use **verified festival/event attendance** as the forcing function to get people offline and together at shows they're already going to.

Working tagline candidates:
- "Stop swiping. Start dancing. The dating app built around the festivals you're already going to."
- "Match with people who actually like the music you like — and meet them at the next show."
- "Your Spotify and your ticket stubs already know your type. We just help you meet them."

## Tech stack

- **.NET MAUI Blazor Hybrid** — committed. UI is Razor components inside a `BlazorWebView`, not XAML pages. One codebase → iOS / Android / Windows / MacCatalyst.
- **.NET 10** (currently preview SDK `10.0.300-preview.0.26177.108`).
- **ASP.NET Core controllers + MediatR vertical slice** for the backend.
- **ASP.NET Core Identity (`IdentityCore`) + JWT bearer** for auth — phone-OTP-only sign-in, no passwords. Identity used as user store + lockout + security-stamp invalidation; *not* used for cookie/SignInManager pipelines.
- **SignalR** confirmed for real-time (chat, presence, match notifications, on-festival meetup coordination).
- **Monorepo / single solution** (`TechnoDating.slnx`) — Mobile + Api + shared Contracts live in one solution, atomic PRs across the stack.

### Solution layout

```
TechnoDating.slnx
docker-compose.yml             ← Postgres + PostGIS for local dev
dotnet-tools.json              ← local dotnet-ef tool pinned
└── src/
    ├── TechnoDating/                 ← MAUI Blazor Hybrid app
    │   ├── Components/
    │   │   ├── Pages/
    │   │   │   ├── Auth/             ← Login.razor, VerifyOtp.razor, Onboarding.razor
    │   │   │   └── Profile.razor, Home.razor, Festivals.razor, Matches.razor
    │   │   ├── Layout/               ← MainLayout, NavMenu
    │   │   ├── Routes.razor          ← awaits TryRestoreFromStorage before mounting Router
    │   │   ├── RedirectToLogin.razor ← used by <AuthorizeRouteView>'s <NotAuthorized>
    │   │   └── ProfileCompleteGuard.razor ← redirects authed-but-incomplete users to /onboarding
    │   └── Services/                 ← AuthStateService, AuthMessageHandler, AuthenticationStateProvider
    ├── TechnoDating.Api/             ← ASP.NET Core controllers + MediatR (vertical slice)
    │   ├── Application/
    │   │   ├── Auth/                 ← AuthController + Requests/ + Handlers/ + IOtpSender/IOtpService/ITokenService
    │   │   ├── Users/                ← UsersController + Requests/ + Handlers/ + UserMappingExtensions
    │   │   ├── Attendance/           ← AttendanceController + Requests/ + Handlers/ (per-user festival attendance)
    │   │   ├── Festivals/            ← FestivalsController + Requests/ + Handlers/ (list + per-festival attendees)
    │   │   └── Matches/              ← MatchesController + Requests/ + Handlers/
    │   ├── Application/
    │   │   ├── Artists/              ← ArtistsController + Requests/ + Handlers/ (catalog endpoint for the picker)
    │   │   └── …                     ← Auth, Users, Attendance, Festivals, Matches as before
    │   └── Infrastructure/
    │       ├── Entities/             ← User (IdentityUser<Guid>), Festival, Match, OtpChallenge, RefreshToken, UserFestivalAttendance, Artist, UserTopArtist, FestivalHeadlineArtist
    │       ├── TechnoDatingDbContext.cs   ← IdentityDbContext<User, IdentityRole<Guid>, Guid>
    │       ├── Migrations/           ← EF Core migrations
    │       └── Seeding/              ← DatabaseInitializer (IHostedService)
    ├── TechnoDating.Contracts/       ← DTOs shared by Mobile + Api
    └── TechnoDating.Workers/         ← (later) background services
```

Application + Infrastructure both live **inside the Api project** as folders. Don't pre-create empty projects — promote folders to projects only when there's a concrete need (e.g. a Workers process needing its own host).

### Database

- **PostgreSQL 16 + PostGIS** via Docker (`postgis/postgis:16-3.4`). Spin up: `docker compose up -d`.
- **EF Core 10** with `Npgsql.EntityFrameworkCore.PostgreSQL` + `.NetTopologySuite` plugin (for `geography(Point, 4326)` columns and `ST_Distance` translation from LINQ).
- **`DatabaseInitializer`** (IHostedService) runs `MigrateAsync()` then idempotent seed on every startup — drop the volume (`docker compose down -v`) to start fresh.
- Connection string in `appsettings.Development.json` → `ConnectionStrings:TechnoDating`. Production reads the same key from environment variables / Azure Key Vault when it lands.
- `dotnet-ef` is a **local tool** pinned in `dotnet-tools.json`. Run with `dotnet ef migrations add <Name> --project src/TechnoDating.Api --output-dir Infrastructure/Migrations`.

### Entities (current)

- `User` — inherits `IdentityUser<Guid>` (Identity provides `Id`, `UserName`, `PhoneNumber`, `PhoneNumberConfirmed`, `SecurityStamp`, `ConcurrencyStamp`, `LockoutEnd`, `AccessFailedCount`, etc.). Domain fields on top: `DisplayName?`, `DateOfBirth?`, `Gender?`, `Bio?`, `City?`, `Location` (`Point`, SRID 4326), `IsVerified`, `CreatedAt`, `LastActiveAt`. **The four required domain fields are nullable** — they get populated during onboarding, not at OTP-verify time. `IsProfileComplete` is a computed (`[NotMapped]`) property: `DisplayName != null && DateOfBirth != null && Gender != null && City != null`. Top artists are *not* on the user — they live in the `UserTopArtist` link table.
- `Festival` — Id, Name, Date, City, Venue, Location. Headline artists live in the `FestivalHeadlineArtist` link table.
- `Artist` — Id, Name, Slug (unique, canonical lookup key), Genre? ("hard techno" / "melodic techno" / "industrial techno" / "house"), CreatedAt. **Seeded catalog, no user-creation**.
- `UserTopArtist` — link table: Id, UserId (FK→User, cascade), ArtistId (FK→Artist, cascade), Rank (1-based for display order). Unique on `(UserId, ArtistId)`.
- `FestivalHeadlineArtist` — link table: Id, FestivalId (FK→Festival, cascade), ArtistId (FK→Artist, cascade), BillingOrder. Unique on `(FestivalId, ArtistId)`.
- `Match` — Id, UserAId, UserBId, MatchedAt (unique (UserAId, UserBId)) — confirmed mutual matches; not yet wired in (empty).
- `OtpChallenge` — Id, PhoneNumber, CodeHash (PBKDF2 via `IPasswordHasher<OtpChallenge>`), ExpiresAt, AttemptCount, ConsumedAt?, CreatedAt. Indexed on `(PhoneNumber, ExpiresAt)`.
- `RefreshToken` — Id, UserId (FK→User), TokenHash (SHA-256 hex of plaintext), IssuedAt, ExpiresAt, RevokedAt?, ReplacedByTokenId? (rotation chain). Unique index on TokenHash.
- `UserFestivalAttendance` — Id, UserId (FK→User, cascade), FestivalId (FK→Festival, cascade), Status (`AttendanceStatus` enum stored as varchar via `.HasConversion<string>()`), CreatedAt, UpdatedAt. Unique on `(UserId, FestivalId)`; index on `FestivalId`. `AttendanceStatus` lives in `Contracts` and is `Interested | Going | Ticketed` — JSON-serialised as string via `[JsonConverter(typeof(JsonStringEnumConverter<AttendanceStatus>))]`. **"No row" = no status** (no fourth enum value); DELETE the row to revert to that state.
- Plus the six Identity tables: `AspNetUsers`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetUserRoles`, `AspNetRoles`, `AspNetRoleClaims`.

**Not yet modelled** (TODOs as they become relevant): photos, swipe/like events, messages, iDIN verification result store, ticket-verification audit log, artist subgenre hierarchy (currently a single `Genre` string).

### API architecture (vertical slice + MediatR)

- **Controllers** (`[ApiController]`, `[Route("api/[controller]")]`) inject `IMediator` and forward to a request — never contain business logic directly.
- **Requests** are `record`s implementing `IRequest<TResponse>` (MediatR). One file per request, in `Application/[Feature]/Requests/`.
- **Handlers** implement `IRequestHandler<TRequest, TResponse>`. One file per handler, in `Application/[Feature]/Handlers/`.
- **Folder naming rule**: feature folder name = controller name minus the `Controller` suffix (e.g. `FestivalsController` → `Application/Festivals/`).
- **MediatR pinned to `12.5.0`** — the last MIT-licensed version. v13+ is commercial.
- MediatR registration in `Program.cs` scans the API assembly:
  `builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));`

### Authentication (phone OTP + JWT)

- **Primary auth = phone OTP.** No passwords. No email/social login (yet). Industry-standard for dating apps (Tinder/Bumble/Hinge/WhatsApp).
- **Endpoints** (`Application/Auth/AuthController`, no `[Authorize]`):
  - `POST /api/auth/request-otp` → generates 6-digit code, hashes via `PasswordHasher<OtpChallenge>`, fires `IOtpSender`. Enforces a 60s per-phone resend cooldown.
  - `POST /api/auth/verify-otp` → verifies code; on success finds the user by phone or **auto-creates one** with only `PhoneNumber` + `UserName` populated (industry pattern — new users land in an incomplete state and complete onboarding next). Returns `{ accessToken, refreshToken, user }`.
  - `POST /api/auth/refresh` → token rotation (revokes old, sets `ReplacedByTokenId`, issues new pair).
  - `POST /api/auth/logout` → revokes the supplied refresh token.
- **JWT** (`TokenService`): HS256, 15-min access tokens, 60-day refresh tokens. Claims: `sub`, `jti`, `security_stamp`, `phone_number`. `JwtBearer.OnTokenValidated` re-checks `security_stamp` against the DB on every request — bumping the stamp invalidates all outstanding tokens (used for phone-change / logout-everywhere).
- **OTP delivery**: `IOtpSender` abstraction. Dev = `ConsoleOtpSender` (logs `[OTP] {phone}: {code}` via `ILogger`). Twilio/MessageBird is the future prod swap-in — code is unchanged elsewhere.
- **Identity wiring**: `AddIdentityCore<User>()` (NOT `AddIdentity` — we don't want the cookie/SignInManager pipeline). Lockout is on: 5 failed attempts → 15-min lockout. `IPasswordHasher<OtpChallenge>` registered as a singleton (PBKDF2 is overkill for short-lived OTPs but it's already in the box).
- **Config keys**:
  - `Jwt:Issuer`, `Jwt:Audience`, `Jwt:SigningKey`, `Jwt:AccessTokenMinutes`, `Jwt:RefreshTokenDays`. Signing key lives in `appsettings.Development.json` for dev; prod must come from env/Key Vault.
  - `Otp:CodeLength`, `Otp:LifetimeMinutes`, `Otp:MaxAttempts`, `Otp:ResendCooldownSeconds`.
- **Mobile auth state**: `AuthStateService` (singleton) holds the access token in memory and persists the refresh token in MAUI `SecureStorage` (Keychain on iOS, Keystore on Android). `AuthMessageHandler` is a `DelegatingHandler` that attaches `Authorization: Bearer` and on a 401 calls `RefreshAsync()` + retries once. `TechnoDatingAuthenticationStateProvider` bridges `IAuthStateService` → Blazor's `<AuthorizeView>` / `<AuthorizeRouteView>`.
- **Two named HttpClients in `MauiProgram.cs`**:
  - `"auth"` — no message handler. Used by `AuthStateService` for `/api/auth/*` so refresh attempts don't recurse through the auth handler.
  - `"api"` — has `AuthMessageHandler`. Used by every other component.
- **Routing gate**: `Routes.razor` awaits `Auth.TryRestoreFromStorageAsync()` before mounting the `<Router>`. Default route policy is `@attribute [Authorize]` (set per-page on Home/Festivals/Matches/Profile/Onboarding); `Login` and `VerifyOtp` are intentionally public. `<AuthorizeRouteView>` with a `<NotAuthorized>` template → `<RedirectToLogin>` handles the unauth case. `ProfileCompleteGuard` (mounted in `MainLayout`) listens to `NavigationManager.LocationChanged` + `Auth.OnAuthStateChanged` and redirects authenticated-but-incomplete users to `/onboarding` (allowlists `/login`, `/verify-otp`, `/onboarding` to prevent loops).
- **Onboarding contract**: `UserProfileDto` exposes a server-computed `IsProfileComplete` bool. Clients route purely on that — if the required-field list changes, only the server changes.

### Festival attendance + matching signal

- **Attendance is a link table, not a list on the user.** `UserFestivalAttendance` (entity above) carries the relation. Status is the `AttendanceStatus` enum (`Interested | Going | Ticketed`), persisted as a varchar string. No row at all = no status — DELETE the row to clear.
- **Endpoints** (`Application/Attendance/AttendanceController`, `[Authorize]`):
  - `GET /api/attendance` → current user's attendance list (`IReadOnlyList<FestivalAttendanceDto>` ordered by festival date).
  - `PUT /api/attendance/{festivalId}` → upsert with body `{ status }`.
  - `DELETE /api/attendance/{festivalId}` → revert to "no status".
- **Plus a festival-scoped endpoint**: `GET /api/festivals/{id}/attendees` returns the other users marked attending (any status) with PostGIS distance — used by the festival detail page for the "who's going" view.
- **`AttendingCount` semantics**: counts only `Going + Ticketed`, not `Interested`. Interested is soft — useful for matching/discovery, not for "how many people are going."
- **Match ordering boost**: `GetMatchesHandler` orders candidates by `(SharedFestivalCount DESC, Distance ASC)`. Shared count is the intersection of the current user's `Going|Ticketed` festivals with the candidate's. `CommonFestivals` on `MatchProfileDto` carries the festival *names* of that intersection — clients display them directly.
- **`MatchingArtistsCount` on `FestivalDto`** is the intersection of the festival's `HeadlineArtists` with the current user's `TopArtists` (case-insensitive). Computed in-memory in `GetFestivalsHandler` after the festival list comes back — fine at current scale, would move to a per-user materialised view eventually.
- **PostGIS distance gotcha**: distance must be computed **inside** the EF query (translates to `ST_Distance` in PostGIS meters). After `ToListAsync`, NetTopologySuite's `Point.Distance` returns coordinate-system units (degrees for WGS84) — useless. Always project `Distance` into an anonymous type within the `.Select` before materialising.

### Artists + matching catalog

- **Artists are first-class entities, not strings.** Seeded ~30 NL techno-scene artists in `DatabaseInitializer`, tagged with a `Genre` (hard techno / melodic techno / industrial techno / house). No user-create yet — picker UI offers only what's in the catalog.
- **`GET /api/artists`** (`Application/Artists/`, `[Authorize]`) returns the full catalog. Cached client-side for the picker.
- **`ArtistRefDto { Id, Name }`** is the wire-shape used everywhere a user-facing artist reference appears: `FestivalDto.HeadlineArtists`, `UserProfileDto.TopArtists`, `MatchProfileDto.TopArtists`. `ArtistDto { Id, Name, Genre }` is only used by the catalog endpoint (Genre drives the picker grouping).
- **`UpdateProfileDto.TopArtistIds`** (Guid list) is how the client submits picks. `UpdateMeHandler` validates that IDs exist, then `ExecuteDeleteAsync` removes the user's existing `UserTopArtist` rows and re-inserts new ones with `Rank` derived from list order. Atomic replace, never partial.
- **`MatchingArtistsCount`** on `FestivalDto` is now a JOIN-derived count (intersection of festival's `FestivalHeadlineArtist.ArtistId` set with user's `UserTopArtist.ArtistId` set) — not in-memory string matching like before. Future Tier-4 subculture/genre-cluster matching just needs to *use* the existing `Artist.Genre` field.
- **`db.LoadTopArtistsAsync(userId, ct)`** extension method centralises the artist join — used by `GetMeHandler`, `TokenService.IssueAsync`, `GetMatchesHandler`, `GetFestivalAttendeesHandler`. `user.ToProfileDto(topArtists)` takes the artists as a parameter so the mapping stays pure.

### Localization (English + Dutch)

- **All UI copy lives in `.resx` files** at `Resources/Strings.resx` (default English) and `Resources/Strings.nl.resx` (Dutch). Adding a language = drop in `Strings.{code}.resx`, no other code changes. Keys are dot-namespaced by feature (`Auth.Login.Heading`, `Festivals.Col.Date`, `Attendance.Status.Going`).
- **`IStringLocalizer<TechnoDating.Resources.Strings>`** (canonical .NET interface) is wired via `services.AddLocalization()` in `MauiProgram`. The framework's `ResourceManagerStringLocalizer` reads embedded resource `TechnoDating.Resources.Strings.resources` (default) and satellite assemblies (`nl/TechnoDating.resources.dll`) based on `CultureInfo.CurrentUICulture`.
- **Translator-friendly** — `.resx` is the standard import/export format for Crowdin, Lokalise, POEditor, etc. When we hire translators, we hand them `Strings.resx` + new `Strings.{code}.resx` round-trips.
- **`ILanguageService`** (singleton) owns the current language. Reads/writes `tn_language` from `SecureStorage`, applies `CultureInfo.DefaultThreadCurrentCulture` / `…UICulture` (this is what `ResourceManagerStringLocalizer` reads from), and raises `OnLanguageChanged`. Called once from `Routes.razor` at app start, then again whenever the user picks a language on `Profile`.
- **`LocalizedComponentBase`** is the abstract base every localized page inherits from. Injects `IStringLocalizer<Strings>` as `L` and `ILanguageService` as `Language`, subscribes to `OnLanguageChanged` and calls `StateHasChanged`. Pages use `@L["..."]` to look up strings, and language switches re-render every visible page instantly.
- **Date formatting** uses `System.Globalization.CultureInfo.CurrentUICulture` explicitly (`f.Date.ToString("ddd d MMM yyyy", CultureInfo.CurrentUICulture)`) — gives `vrij 4 jul 2026` vs `Fri 4 Jul 2026` automatically.
- **Server stays English-only**: API only returns *error codes* (`"otp_invalid"`, `"refresh_invalid"`) and *data* (artist names, festival names — proper nouns, not localized). The client maps codes to localized text. Clean separation.

### Hot-reload-friendly patterns

Static field initializers don't re-run on Hot Reload — they're a silent trap. Method bodies do re-run. Keep mutable-feeling test data and seed lists **inside handler `Handle` methods**, not as `static readonly` fields. When real data lands, the same handlers move from inline lists to repository/EF Core calls.

## Project owner profile

- Developer (handles technical build independently)
- Solo founder / early phase, currently identifying USPs and differentiation
- Quality-focused — explicitly *not* shipping "slop"
- Based in the Netherlands; product targets NL first

---

## Strategic context

### The Dutch dating app landscape (2026 snapshot)

**International incumbents (dominant by volume):**
- **Tinder** — #1 revenue, default swipe app, perceived casual/hookup in Amsterdam
- **Bumble** — #1 free downloads, women-first messaging, "friendlier" positioning. Already shows shared Spotify top artists → the obvious "match on music" feature is commoditised inside a market leader
- **Hinge** — "Designed to be deleted," prompt-based, urban 25–35
- **Happn** — Location crossings, big cities only
- **Badoo** — Dated/cluttered

**NL-anchored, relationship-focused:**
- **Lexa** — Largest Dutch site, Meetic, older skew, serious positioning
- **Parship / e-Matching** — Personality-test-gated, hoger opgeleiden
- **Inner Circle** — Vetted "ambitious singles," exclusivity + events
- **50plusmatch** — Dominates 50+

**The standout Dutch original — study carefully:**
- **Breeze** (Delft, 2019) — Antithesis of swipe culture: no chat, curated daily profiles, app schedules the date at vetted venues, pay-per-date (~€7.50 incl. first drink), no-show penalty. 400k+ dates done. Expanded BE/DE/UK/FR/US.

**LGBTQ+:** Grindr, HER, Romeo dominate. Breeze positions as inclusive.

### What's broken in the market (opportunity surface)

- Swipe fatigue mainstream — ~79% Gen Z, ~80% millennials report burnout
- Match Group laid off 13%, Bumble 30%, in 2024–25 → category contracting
- Big apps' incentives are misaligned (make money when you stay, not when you leave)
- Trust collapse — AI photos, voice clones, deepfakes, romance scams
- Gen Z "clear-coding" — wants explicit intentions
- Real-life movement growing — run clubs, Thursday Events, pickleball-dating
- Dutch context: directness is cultural, fewer-but-better resonates, Randstad density makes IRL apps viable, big expat/local divide most apps handle poorly

### Differentiation axes (general)

1. **Intent / slow dating** — fewer high-signal profiles, mandatory deeper-than-pics
2. **Anti-chat / push to IRL** — Breeze owns this in NL but room exists (different city tier / demo / activity / scheduling)
3. **Trust & verification** — iDIN works beautifully in NL; women-first marketing
4. **Community / niche** — expats, creatives, sustainability, neurodivergent, parents, sober, religious, hobby tribes

---

## The chosen angle: music + festivals

### Competitive map for this space

**Music-taste dating (exists, mostly small/struggling):**
- **Vinylly** (US) — Spotify-based, "suggest a concert" chat feature, boutique
- **Makromusic** / "Dating for Spotify" — Top-artist matching, bot problems
- **Power of Music (POM)** — 2025, Spotify + Apple Music
- **Bumble Top Artists** — Not a separate app but the obvious music feature is already in a market leader

**Festival/event-based connection:**
- **Radiate** — Biggest player, US festival/rave culture (Insomniac, EDC). Explicitly **not a dating app** — friend/squad framing, event chat groups, ticket marketplace
- **Camp (getcamp.app)** — Pitched as "the festival dating app." Small, early-stage
- **Love Light** — Festival-dating, very small
- **Tinder Festival Mode** — Lives inside Tinder Explore. Mixed reviews — can't verify anyone is actually going

**Key insight:** Nobody has cleanly combined deep music-taste matching with festival/event coordination into one product with a real point of view. None are NL-anchored.

### Why the Netherlands is exceptionally strong for this

- **ADE (Amsterdam Dance Event)** — 500k+ attendees, 1,000+ events in October. Global electronic music industry gathering.
- **Defqon.1** — 100k+ attendees, hardstyle pilgrimage
- **Awakenings, DGTL, Mysteryland, Lowlands, Pinkpop, Down the Rabbit Hole, Draaimolen, Best Kept Secret, Into the Woods, Welcome to the Future, Kingsland** — hundreds of festivals per year
- Randstad density + train network make festival travel trivial
- Subculture diversity unusually high — techno, hardstyle, hardcore, drum & bass, hip-hop, indie, NL-language pop all have real scenes
- Matters because music-taste apps live or die on **subculture clustering** — a hardstyle fan and a Le Guess Who? fan won't date each other regardless of Spotify overlap

### Why previous music-dating attempts haven't broken out

- Music taste alone isn't enough signal — great filter/conversation starter, weak predictor
- Festival mode without verification is noise (Tinder's failure)
- "Play it for me on Spotify" mechanic is fun for a week, then a chore
- Most music-dating apps default to swipe + chat, inheriting the problems they tried to escape

### The proposed USP — three layers

**The strong version: festival-anchored matching with verified attendance**
- Connect ticketing (Ticketmaster, Paylogic, Eventbrite, Festicket — most NL festivals use a small handful) so the app *knows* they actually bought a ticket → kills Tinder Festival Mode's biggest problem
- Match on *who's actually going to the same shows you are* — real, scarce, time-bound signal
- App surfaces: *"5 people you'd vibe with are going to Awakenings on July 4. Want to meet up?"*
- Optional: verified meetup point on festival grounds — *"meet at the third bar near the main stage at 8pm"*

**The deeper version: matching on listening behaviour, not just labels**
- Top-artist matching is weak (Bumble does it). Instead match on *patterns*: discovery in last 90 days, subgenre depth, niche-ness, festival-headliner-listener vs deep-cuts-listener
- *"You both went from Boris Brejcha to Mind Against in the last 6 months"* > *"You both like Charlotte de Witte"*
- Spotify API gives what's needed; the UX of *explaining* the match is where most music apps fail

**The cultural version: scene-aware, not genre-aware**
- Techno in Berlin ≠ techno in Tilburg. Hardstyle Brabant ≠ Limburg. Le Guess Who? indie ≠ Best Kept Secret indie.
- Self-identified scenes weighted into matching — nobody is doing this well

**The honest-business version (incentive alignment)**
- Free to match/chat, paid only when both confirm attending the same event together. Or small fee per "festival buddy" intro.
- Marketing in itself — *"we make money when you actually meet, not when you doomscroll"*
- Breeze-style aligned incentive but for the music/event niche

### Risks and watch-outs

- **Seasonality** — NL festival season heavily May–Sept + ADE in October. Nov–March is dead. Need a winter story: clubs (Marktkanaal, Shelter, Doornroosje, Tivoli, Paradiso), smaller venue shows, ADE-style indoor. Otherwise a 5-month app.
- **Ticketing API access is hard** — Major platforms don't always have open user-facing APIs. May need partnerships, or email parsing of forwarded confirmations (quiet workaround used by some apps).
- **Subculture politics** — hardstyle bro and Le Guess Who? attendee both deserve a great app, but cramming them in dilutes both. **Likely correct to launch in one scene** — probably techno/house in Randstad (ADE/DGTL/Awakenings density).
- **Festival photos as profile pics** — visual identity *and* catfishing/identity problem. Needs thoughtful verification layer.
- **Will be compared to Tinder Festival Mode constantly** — messaging must immediately show why this is not that (verified attendance + deeper taste signal + designed-for-festivals, not bolted-on).

### Foundational truths (apply regardless of feature decisions)

- **Cold start problem** — 200 users is worse than no users. Launch in one city, one demographic, get to liquidity before broadening. Breeze rolled out city by city. Likely launch cities: **Amsterdam, Utrecht, or Delft.**
- **Incentive model is the original sin of the category** — subscriptions want you on the app. Honest revenue model is itself a USP in 2026.
- **Women's experience IS the product** — if it's worse for women they leave, then men leave. Verification, reporting, moderation, behaviour signals.
- **Need a real point of view** — Tinder: "more is better." Breeze: "stop chatting, go meet." If you can't say yours in one sentence, the app will feel generic regardless of polish.

---

## Decisions (locked)

- **Launch scene: techno.** Randstad density + ADE/DGTL/Awakenings/Verknipt makes this the highest-liquidity scene in NL.
- **Launch city: Amsterdam (most likely).** Not 100% set in stone — Utrecht still in play. Decide once we have a clearer view of the matching algo and the first cohort recruitment plan.
- **Mobile framework: .NET MAUI Blazor Hybrid.** Committed.
- **Real-time: SignalR.** Committed.
- **Monorepo (single `.slnx`)** with separate Mobile / Api / Contracts projects.

## Open questions / decisions to make

- [ ] **Ticketing integration approach** — partnerships vs email parsing vs hybrid
- [ ] **Spotify-only vs Spotify + Apple Music** at launch
- [ ] **Revenue model specifics** — pay-per-meetup, freemium with paid intros, partnerships with festivals/venues
- [ ] **Winter strategy** — clubs and smaller venues vs leaning into ADE preparation hype
- [ ] **The "one-sentence point of view"** — final wording

## Product TODOs

All future-feature ideas — profile verification (iDIN, live selfie, ticket attestation, behavioural badges, mod queue), background-work infrastructure, external integrations (Spotify, ticketing providers, push), revenue alignment, on-festival meetup coordinator, festival cohort chats, subculture-aware matching, etc. — live in [`BACKLOG.md`](BACKLOG.md), grouped by tier. Promote items into this file's session log + relevant section when they enter active development.

## Local development notes

- API runs on **`http://localhost:5000`** (HTTP, no dev cert — avoids self-signed cert pain on Android emulator).
- Mobile `HttpClient.BaseAddress` is platform-aware: `http://10.0.2.2:5000` on Android (emulator → host loopback), `http://localhost:5000` elsewhere. See `MauiProgram.cs`.
- **First-time setup**: `docker compose up -d` to start Postgres. The first API run auto-applies migrations and seeds.
- **Daily**: `docker compose up -d` (idempotent — fast no-op if already running) → F5 the multi-startup in VS, or `dotnet run --project src/TechnoDating.Api` + `dotnet build -t:Run -f net10.0-windows10.0.19041.0 src/TechnoDating`.
- **Reset DB**: `docker compose down -v` (drops the volume → next run re-seeds).
- **DB container restart policy**: `unless-stopped` — it'll come back automatically after Docker Desktop / system restarts.
- **Login in dev**: seeded users have phones `+31600000001`–`+31600000004` (Sofie / Daan / Lieke / Maud). Hit `request-otp` with any of them, watch the API console for `[OTP] +31600000001: 123456`, paste into the verify screen. For new-user signup flow, use any unseeded `+31...` number — the user is auto-created and routed to `/onboarding`.
- **Inspect DB**: DBeaver Community installed via winget. Connection: `localhost:5432` / db=`technodating` / user=`technodating` / pw=`dev`. PostGIS `Location` columns show as binary by default — query with `ST_AsText(location)` for readable coordinates.

## Session log

> Append a short entry per session to track how thinking evolves.

- **2026-05-25** — Initial context file created. Strategic research from prior chat captured. Tech stack confirmed as .NET MAUI.
- **2026-05-25** — Locked: techno launch scene, Amsterdam likely launch city, MAUI Blazor Hybrid committed, SignalR for real-time, monorepo solution layout. Restructured repo into `src/` with `TechnoDating` (mobile), `TechnoDating.Api`, `TechnoDating.Contracts`. Scaffolded minimal API with `/api/festivals` + `/api/matches` returning test data; mobile fetches and displays them. Profile verification added as a first-class product TODO (iDIN + live selfie + behavioural signals).
- **2026-05-25** — Refactored API from minimal endpoints to controllers + MediatR vertical slice. Pinned MediatR to **12.5.0** (last MIT release; 13+ is commercial). Adopted feature-folder layout: `Application/[Feature]/[Feature]Controller.cs` + `Requests/` + `Handlers/`. Test data moved inline into handler method bodies so it survives Hot Reload (static field initializers don't re-run).
- **2026-05-25** — Database stack: Postgres + PostGIS via Docker Compose; EF Core 10 + Npgsql + NetTopologySuite. Added `Infrastructure/` folder inside Api with `TechnoDatingDbContext`, `User`/`Festival`/`Match` entities (User has `geography(Point, 4326)` location + `TopArtists text[]`), initial migration, and a `DatabaseInitializer` IHostedService that migrates + seeds on startup. Both handlers now read from the DB; matches are ordered by PostGIS `ST_Distance` from a placeholder Amsterdam-centre point. `dotnet-ef` installed as a local tool.
- **2026-05-27** — Added phone-OTP auth + JWT + user profile + onboarding. Backend: `User` inherits `IdentityUser<Guid>` (required domain fields made nullable), new `OtpChallenge` + `RefreshToken` entities, `IdentityDbContext` base. `Application/Auth/` (controller + handlers + `IOtpSender`/`ConsoleOtpSender`/`IOtpService`/`ITokenService`) and `Application/Users/` (`/me` GET/PUT). `[Authorize]` on Festivals/Matches; `GetMatchesHandler` now uses the current user's `Location` and excludes self. `Program.cs` wires `AddIdentityCore` + `AddJwtBearer` with `OnTokenValidated` re-checking the `security_stamp` claim. Migration rebuilt as a fresh `Initial`. Seeded users got phones `+31600000001`–`04`. Mobile: `AuthStateService` (in-memory access token + `SecureStorage`-backed refresh token), `AuthMessageHandler` (Bearer + refresh-on-401), `TechnoDatingAuthenticationStateProvider`. Named HttpClients `"auth"` / `"api"`. New pages: `Login`, `VerifyOtp`, `Onboarding`, `Profile`. `Routes.razor` awaits token restore; `<AuthorizeRouteView>` + `RedirectToLogin` handle unauth; `ProfileCompleteGuard` redirects authed-but-incomplete users to `/onboarding`. Backend smoke-tested end-to-end (existing user + new user + refresh rotation + lockout). Added `restart: unless-stopped` to the Postgres container; DBeaver Community installed for DB inspection.
- **2026-05-31** — Artists as first-class entities + localization (EN + NL) + Avatar + empty-state polish. Backend: new `Artist` (seeded catalog of 30 NL techno artists, tagged with Genre), `UserTopArtist` and `FestivalHeadlineArtist` link tables. Dropped the old `User.TopArtists` / `Festival.HeadlineArtists` text[] columns. `MatchingArtistsCount` becomes a JOIN-driven count. `ArtistsController` exposes `GET /api/artists` for the picker. `UpdateMeHandler` now takes `TopArtistIds` Guid list, validates existence, replaces via `ExecuteDeleteAsync` + re-insert. New `db.LoadTopArtistsAsync(userId)` extension centralises the join — used by Get/Update Me, Token issue, Matches, FestivalAttendees. `user.ToProfileDto()` now takes artists as a parameter (pure mapping). Migration `AddArtists`. Seeded user/festival → artist relationships re-keyed via slug. Mobile: new `Services/AppStrings.cs` (en + nl dictionaries), `AppStringLocalizer<T>` implementing `IStringLocalizer<T>`, `ILanguageService` singleton (SecureStorage-backed), `LocalizedComponentBase` that pages inherit from (re-renders on language change). New `ArtistPicker` component (fetches catalog once, chips grouped by genre, toggle to select). New `Avatar` component (initials on hue-from-id-hash colored disc). All pages translated to EN + NL + language switcher on Profile. Onboarding gained an optional artist-picker step. Festivals + FestivalDetail + Matches render artists as ArtistRefDto. Empty-state copy + localized error hints across all pages. Both backend + mobile build clean; full smoke test pass.
- **2026-05-30** — Festival attendance + Tier-1 matching signal. New `UserFestivalAttendance` link table with `AttendanceStatus` enum (`Interested | Going | Ticketed`, stored as varchar). `Application/Attendance/` vertical slice (GET/PUT/DELETE `/api/attendance[/{festivalId}]`). `Application/Festivals/` extended with `GET /api/festivals/{id}/attendees` (per-festival match view — the "forcing function" UX). `GetFestivalsHandler` now returns `AttendingCount` (Going + Ticketed), the current user's `MyStatus`, and `MatchingArtistsCount` (intersection of headliners ∩ user's TopArtists). `GetMatchesHandler` orders by `(SharedFestivalCount DESC, Distance ASC)` and populates `CommonFestivals` with the overlap festival names. Seeder gained 12 attendance rows so the matches feed has real shared-festival data out of the box. Mobile: festivals page got an inline `Status` dropdown wired to PUT/DELETE, plus a clickable festival → new `/festivals/{id}` detail page showing the festival + "who's going" list. JSON enum serialisation forced to string via `[JsonConverter(typeof(JsonStringEnumConverter<>))]` on the enum type itself (works both directions without per-call options). Also created `BACKLOG.md` at project root — Tier 2-5 ideas (ticket-email verification, on-festival meetup coordinator, scene-aware matching, pay-per-meetup, etc.) live there, referenced from this file. **PostGIS gotcha learned and documented**: `Point.Distance` is degrees in-memory but `ST_Distance` (meters) when projected inside an EF query — always project distance into the anonymous type before `ToListAsync`.
