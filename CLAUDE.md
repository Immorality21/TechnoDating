# TechnoDating — Engineering Context

> Tech stack, architecture, conventions, and local-dev notes. This is the always-loaded file — keep it lean. Strategy, history, and ideas live in companion docs:
>
> - [`docs/PRODUCT.md`](docs/PRODUCT.md) — elevator pitch, USP, competitive landscape, locked decisions, open questions. **Read when working on product direction.**
> - [`docs/SESSIONLOG.md`](docs/SESSIONLOG.md) — per-session history of how the build evolved. **Append a short entry each session; read when you need the backstory of a decision.**
> - [`BACKLOG.md`](BACKLOG.md) — future-feature ideas not on the current build path, grouped by tier.

## One-liner

A music + festival-anchored dating app for the Netherlands. Match on actual music taste and listening behaviour, then use verified festival/event attendance as the forcing function to get people offline. Launch scene: techno. Launch city: Amsterdam (likely). (Full pitch & strategy → [`docs/PRODUCT.md`](docs/PRODUCT.md).)

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
    │   │   ├── Matches/              ← MatchesController + Requests/ + Handlers/
    │   │   ├── Artists/              ← ArtistsController + Requests/ + Handlers/ (catalog endpoint for the picker)
    │   │   ├── Photos/               ← PhotosController + Requests/ + Handlers/ + PhotoMappingExtensions (upload/delete/set-primary)
    │   │   └── Storage/              ← IBlobStorage + S3BlobStorage + StorageOptions (S3-compatible; MinIO local, R2 prod)
    │   └── Infrastructure/
    │       ├── Entities/             ← User (IdentityUser<Guid>), Festival, Match, OtpChallenge, RefreshToken, UserFestivalAttendance, Artist, UserTopArtist, FestivalHeadlineArtist, Photo
    │       ├── TechnoDatingDbContext.cs   ← IdentityDbContext<User, IdentityRole<Guid>, Guid>
    │       ├── Migrations/           ← EF Core migrations
    │       └── Seeding/              ← DatabaseInitializer + BlobStorageInitializer (IHostedServices)
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

- `User` — inherits `IdentityUser<Guid>` (Identity provides `Id`, `UserName`, `PhoneNumber`, `PhoneNumberConfirmed`, `SecurityStamp`, `ConcurrencyStamp`, `LockoutEnd`, `AccessFailedCount`, etc.). Domain fields on top: `DisplayName?`, `DateOfBirth?`, `Gender?`, `Bio?`, `City?`, `Location` (`Point`, SRID 4326), `IsVerified`, `CreatedAt`, `LastActiveAt`, **`PrimaryPhotoId?` (FK→Photo with `OnDelete: SetNull`)**. **The four required domain fields are nullable** — they get populated during onboarding, not at OTP-verify time. `IsProfileComplete` is a computed (`[NotMapped]`) property: `DisplayName != null && DateOfBirth != null && Gender != null && City != null`. Top artists are *not* on the user — they live in the `UserTopArtist` link table.
- `Festival` — Id, Name, Date, City, Venue, Location. Headline artists live in the `FestivalHeadlineArtist` link table.
- `Artist` — Id, Name, Slug (unique, canonical lookup key), Genre? ("hard techno" / "melodic techno" / "industrial techno" / "house"), CreatedAt. **Seeded catalog, no user-creation**.
- `UserTopArtist` — link table: Id, UserId (FK→User, cascade), ArtistId (FK→Artist, cascade), Rank (1-based for display order). Unique on `(UserId, ArtistId)`.
- `FestivalHeadlineArtist` — link table: Id, FestivalId (FK→Festival, cascade), ArtistId (FK→Artist, cascade), BillingOrder. Unique on `(FestivalId, ArtistId)`.
- `Match` — Id, UserAId, UserBId, `Origin` (`MatchOrigin` enum, varchar), `Status` (`MatchStatus` enum, varchar), CreatedAt, `ExpiresAt?` (nullable, unused for now). Pair stored **canonically** (`UserAId < UserBId`), unique on `(UserAId, UserBId)`. Confirmed connections; created **only** via `IMatchmaker.TryCreateMatchAsync` (the single chokepoint — see `docs/MATCHING.md`).
- `Like` — Id, LikerId, LikedId, `Kind` (`LikeKind` enum: `Like | Pass`, varchar), CreatedAt. Unique on `(LikerId, LikedId)`; index on `LikedId`. Append-only **directional signal**, deliberately decoupled from `Match` so the matching policy can change without a migration.
- `OtpChallenge` — Id, PhoneNumber, CodeHash (PBKDF2 via `IPasswordHasher<OtpChallenge>`), ExpiresAt, AttemptCount, ConsumedAt?, CreatedAt. Indexed on `(PhoneNumber, ExpiresAt)`.
- `RefreshToken` — Id, UserId (FK→User), TokenHash (SHA-256 hex of plaintext), IssuedAt, ExpiresAt, RevokedAt?, ReplacedByTokenId? (rotation chain). Unique index on TokenHash.
- `UserFestivalAttendance` — Id, UserId (FK→User, cascade), FestivalId (FK→Festival, cascade), Status (`AttendanceStatus` enum stored as varchar via `.HasConversion<string>()`), CreatedAt, UpdatedAt. Unique on `(UserId, FestivalId)`; index on `FestivalId`. `AttendanceStatus` lives in `Contracts` and is `Interested | Going | Ticketed` — JSON-serialised as string via `[JsonConverter(typeof(JsonStringEnumConverter<AttendanceStatus>))]`. **"No row" = no status** (no fourth enum value); DELETE the row to revert to that state.
- `Photo` — Id, UserId (FK→User, cascade), Ordinal, Width, Height, StorageKey (e.g. `users/{userId}/photos/{photoId}` — variants live at `{key}/thumb.webp|card.webp|full.webp`), ContentType, ModerationStatus (`approved` by default; full mod queue is Tier-6 BACKLOG), UploadedAt. Unique on `(UserId, Ordinal)`. **No `IsPrimary` column** — the primary photo is referenced from `User.PrimaryPhotoId` (Option C: single source of truth, atomic UPDATE on switch, no partial-unique-index hack). Up to 6 photos per user (enforced in `UploadPhotoHandler`).
- Plus the six Identity tables: `AspNetUsers`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetUserRoles`, `AspNetRoles`, `AspNetRoleClaims`.

**Not yet modelled** (TODOs as they become relevant): messages (Slice 3 — chat), iDIN verification result store, ticket-verification audit log, artist subgenre hierarchy (currently a single `Genre` string).

### Matching & messaging — see [`docs/MATCHING.md`](docs/MATCHING.md)

The core loop. **Match creation is a swappable policy**: every match is created through the single `IMatchmaker.TryCreateMatchAsync(a, b, origin)` chokepoint (idempotent, canonical pair). The "mutual like ⇒ match" rule lives entirely in `SubmitLikeHandler` and is the only swappable piece — `Match`, the matches list, and (later) chat never depend on it. Endpoints:
- `POST /api/likes` (`Application/Likes/`) → record a `Like`/`Pass` signal; returns `{ matched, matchId? }`.
- `GET /api/matches` (`Application/Matches/`) → **confirmed** mutual matches (`MatchDto`).
- `GET /api/discovery` (`Application/Discovery/`) → the **candidate feed** (`MatchProfileDto`), ranked by shared festivals + distance, excluding anyone already liked/passed/matched. (This is the feed formerly served by `/api/matches`.)

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

### Photos / blob storage

- **S3-compatible storage everywhere.** `IBlobStorage` is the abstraction (`Application/Storage/`), `S3BlobStorage` is the only implementation, talking to whatever `Storage:Endpoint` config points at. Same code path local + staging + prod.
- **Local dev = MinIO** running as a sidecar container in `docker-compose.yml` (console at `localhost:9001`, S3 API at `localhost:9000`). Volume-backed, restarts with the rest. `appsettings.Development.json` Storage section points at it with the dev keys.
- **Staging / prod = Cloudflare R2.** Same `IAmazonS3` client, just different `Storage:Endpoint` (`https://<accountId>.r2.cloudflarestorage.com`), bucket (`technodating-photos-staging` / `…-prod`), and credentials. Production secrets come from env vars / Key Vault, never `appsettings*.json`. Zero egress fees on R2 — important for a photo-heavy read pattern.
- **Bucket bootstrap** is `BlobStorageInitializer` (`IHostedService` alongside `DatabaseInitializer`). Calls `EnsureBucketExistsAsync` on startup; safe to call repeatedly.
- **Photos are private. URLs are signed.** Every read goes through `IBlobStorage.GetSignedUrl(key)` which returns a 15-minute pre-signed URL. Nothing in the bucket is publicly readable. URLs expire — clients re-fetch profile/match data when they come back to the screen. Configured via `Storage:SignedUrlMinutes`.
- **Upload pipeline** (`Application/Photos/UploadPhotoHandler`): server receives `IFormFile` (multipart, capped at 10 MB, content-type whitelist), `ImageSharp` decodes once, then clones into 3 variants:
  - `thumb.webp` — 96×96 center-crop (nav / avatar disc replacement)
  - `card.webp` — 480×720 center-crop portrait (match cards, gallery tiles)
  - `full.webp` — max 1080×1620 with aspect preserved (profile viewer)
  All three stored under `users/{userId}/photos/{photoId}/{variant}.webp`. Format is WebP (smaller than JPEG, universal support now).
- **Endpoints** (`Application/Photos/PhotosController`, `[Authorize]`, route `/api/users/me/photos`):
  - `POST /api/users/me/photos` (multipart `file=...`) → returns `PhotoDto` (with signed URLs). If this is the user's first photo, also sets `User.PrimaryPhotoId` to it.
  - `DELETE /api/users/me/photos/{photoId}` → clears `User.PrimaryPhotoId` (if it pointed at this photo) before removing row + blobs, then promotes the next photo (lowest Ordinal) to primary.
  - `PUT /api/users/me/photos/{photoId}/primary` → single `UPDATE Users SET PrimaryPhotoId = …`. No two-phase dance; the FK guarantees uniqueness.
- **`UserProfileDto.Photos`** is `IReadOnlyList<PhotoDto>` ordered by `Ordinal`; each `PhotoDto.IsPrimary` is computed at mapping time by comparing the photo's Id to `User.PrimaryPhotoId`. **`PrimaryPhotoUrl`** is the *card-size* signed URL for that photo (or null). **`MatchProfileDto.PrimaryPhotoUrl`** mirrors that — same card-size URL for the candidate user, populated by a single bulk query in `GetMatchesHandler` / `GetFestivalAttendeesHandler` (`db.LoadPrimaryPhotoCardUrlsAsync(...)` joins `Users.PrimaryPhotoId → Photos.StorageKey`).
- **Mobile**: `<PhotoGallery>` component on Profile (Blazor `<InputFile>` → multipart POST, plus per-photo set-primary / delete actions). `<Avatar>` was extended with an optional `PhotoUrl` parameter — renders the card image when present, falls back to the hue-from-hash initials disc otherwise. Used on Matches + FestivalDetail cards.
- **Config keys**:
  - `Storage:Endpoint`, `Storage:Region`, `Storage:Bucket`, `Storage:AccessKey`, `Storage:SecretKey`, `Storage:ForcePathStyle` (true for both MinIO and R2), `Storage:SignedUrlMinutes` (15).

### Hot-reload-friendly patterns

Static field initializers don't re-run on Hot Reload — they're a silent trap. Method bodies do re-run. Keep mutable-feeling test data and seed lists **inside handler `Handle` methods**, not as `static readonly` fields. When real data lands, the same handlers move from inline lists to repository/EF Core calls.

## Testing

- **`tests/TechnoDating.Api.Tests`** — xUnit, in the solution. Run with `dotnet test tests/TechnoDating.Api.Tests` (avoid `dotnet test` on the whole `.slnx` — it pulls in the MAUI project, which needs the workload).
- Uses the **EF Core InMemory** provider (`TestDb.NewContext()`) for fast, isolated logic tests — no Postgres needed. Caveat: InMemory does **not** enforce unique indexes and does not run PostGIS, so don't rely on DB constraints or spatial queries in unit tests; assert on handler/service logic instead.
- Write tests alongside new application logic going forward (handlers, services). Current coverage: `Matchmaker` (canonical pair, idempotency, self-match guard) and `SubmitLikeHandler` (one-way vs reciprocal like, pass, unknown target, re-like).

## Local development notes

- API runs on **`http://localhost:5000`** (HTTP, no dev cert — avoids self-signed cert pain on Android emulator).
- Mobile `HttpClient.BaseAddress` is platform-aware: `http://10.0.2.2:5000` on Android (emulator → host loopback), `http://localhost:5000` elsewhere. See `MauiProgram.cs`.
- **First-time setup**: `docker compose up -d` to start Postgres. The first API run auto-applies migrations and seeds.
- **Daily**: `docker compose up -d` (idempotent — fast no-op if already running) → F5 the multi-startup in VS, or `dotnet run --project src/TechnoDating.Api` + `dotnet build -t:Run -f net10.0-windows10.0.19041.0 src/TechnoDating`.
- **Reset DB**: `docker compose down -v` (drops the volume → next run re-seeds).
- **DB container restart policy**: `unless-stopped` — it'll come back automatically after Docker Desktop / system restarts.
- **Login in dev**: seeded users have phones `+31600000001`–`+31600000004` (Sofie / Daan / Lieke / Maud). Hit `request-otp` with any of them, watch the API console for `[OTP] +31600000001: 123456`, paste into the verify screen. For new-user signup flow, use any unseeded `+31...` number — the user is auto-created and routed to `/onboarding`.
- **Inspect DB**: DBeaver Community installed via winget. Connection: `localhost:5432` / db=`technodating` / user=`technodating` / pw=`dev`. PostGIS `Location` columns show as binary by default — query with `ST_AsText(location)` for readable coordinates.
- **Inspect blobs (MinIO)**: open `http://localhost:9001` for the MinIO console. Login `technodating` / `dev-only-secret`. Bucket `technodating-photos` auto-created on first API run.
