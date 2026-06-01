# TechnoDating — Matching & Messaging

> The core loop the app revolves around: how two users go from *discovery* → *connected* → *talking*. This file is the source of truth for the matching architecture. Update it as the model evolves.
>
> **Companion files:** [`../CLAUDE.md`](../CLAUDE.md) (engineering context), [`PRODUCT.md`](PRODUCT.md) (strategy), [`../BACKLOG.md`](../BACKLOG.md).

## Guiding principle: match *creation* is a swappable policy

We do **not** yet know how matching should fundamentally work. The current model (mutual like) may later be replaced by an algorithm, Breeze-style curation ("the app decides, users don't get a say"), auto-suggestion from shared festival attendance, or a hybrid. The architecture must let us change *how a match is born* without re-architecting the schema, chat, or the matches list.

```
 PRODUCERS (swappable policy)          STABLE SEAM            CONSUMERS (never change)
 ┌─────────────────────────┐                                 ┌──────────────────┐
 │ mutual-like handler      │──┐                          ┌──▶│ Matches list     │
 │ (built now)              │  │                          │   ├──────────────────┤
 ├─────────────────────────┤  │   IMatchmaker            │   │ Chat (SignalR)   │
 │ curated/algorithm job    │  ├──▶ TryCreateMatch ──────┤   ├──────────────────┤
 │ (future, no build)       │  │   (a, b, origin)         │   │ Match notifs     │
 ├─────────────────────────┤  │   idempotent, canonical  │   └──────────────────┘
 │ admin / auto-festival    │──┘                          
 └─────────────────────────┘                              
```

Every path that creates a match calls **one method** — `IMatchmaker.TryCreateMatchAsync`. The "mutual like" rule is just the first *producer* feeding it. Swapping the matching model later = write a new producer + disable the like handler; nothing downstream changes.

### Two rules that protect this flexibility

1. **No match logic in the database.** No triggers, no auto-creating views. The policy lives in C# handlers — testable, observable, swappable.
2. **`Like` is decoupled from `Match`.** `Like` is an append-only *signal* store. Later we can keep it, ignore it, or feed it into a scoring algorithm. Chat hangs off `Match`, never off likes — so conversations work identically no matter how the match was born.

### Stable vs. swappable

| Stable (build once, never re-architect) | Swappable (the policy we're unsure about) |
|---|---|
| `Match` entity + `IMatchmaker` chokepoint | The mutual-like handler (the *only* place "reciprocated like ⇒ match" lives) |
| Chat / messages (hang off `Match`) | Discovery feed ranking |
| Matches list | Future curated / algorithmic match jobs |
| `Like` as a signal store | Whether likes drive matching at all |

## Data model

### `Match` (the stable connection)
- `Id`, `UserAId`, `UserBId`, `Origin` (`MatchOrigin` enum), `Status` (`MatchStatus` enum), `CreatedAt`, `ExpiresAt?`.
- **Canonical pair ordering**: `UserAId = min(userId)`, `UserBId = max(userId)` (Guid compare) so `(A,B)` and `(B,A)` can't both exist. Unique index on `(UserAId, UserBId)`.
- `MatchOrigin`: `MutualLike | Curated | Admin | AutoFestival`. Records *how* each match was made, so a future mixed model coexists in one table and is analyzable.
- `MatchStatus`: `Active | Closed`. (Closed = unmatched/blocked later.)
- `ExpiresAt?` — nullable, **unused for now**. See "Maybe later".

### `Like` (directional signal)
- `Id`, `LikerId`, `LikedId`, `Kind` (`LikeKind` enum: `Like | Pass`), `CreatedAt`.
- Unique on `(LikerId, LikedId)`. Append-only signal; **not** welded to `Match`.
- Passes are **not permanent for now** — they exclude a user from discovery but we are explicitly not treating a pass as a forever-block. Revisit if discovery feels stale.

### `Message` (hangs off `Match`) — Slice 3
- `Id`, `MatchId` (FK→Match, cascade), `SenderId`, `Body`, `SentAt`, `ReadAt?`.

## The chokepoint

`IMatchmaker.TryCreateMatchAsync(Guid userA, Guid userB, MatchOrigin origin, CancellationToken ct)`:
- Canonicalizes the pair (min/max).
- Idempotent: returns the existing `Match` if one already exists for the pair, else creates one.
- Single place that (later) raises the match notification / SignalR event.
- **All producers call this** — mutual-like now, curated/algorithm/admin later.

## Slices

- **Slice 0 — stable seam.** `Match` entity (Origin/Status/ExpiresAt/canonical ordering) + `IMatchmaker` + migration. No UI, no opinion on how matches form.
- **Slice 1 — like signal + mutual-like producer.** `Like` entity; `POST /api/likes { targetUserId, kind }` → record signal; if reciprocal `Like` exists, call `TryCreateMatchAsync(…, MutualLike)`; return `{ matched, matchId? }`. **This handler is the entire current matching policy** — isolated so swapping it is a one-file concern.
- **Slice 2 — matches list + naming cleanup.** `GET /api/matches` → confirmed `Match` rows with the other user's summary + (later) last-message preview. **Rename** today's candidate feed (`GetMatchesHandler`/`/api/matches`) → **discovery** (`/api/discovery`); it's a discovery feed, not matches. Discovery filters out users already liked/passed/matched.
- **Slice 3 — chat over SignalR.** `Message` entity, `ChatHub` (JWT via access-token query string, group-per-match, persist-then-broadcast), `GET /api/matches/{id}/messages` (paginated). Doubles as the SignalR de-risk for Tier-3 (meetup coordinator, cohort chats).
- **Slice 4 — (documented, not built) proof the seam works.** A future `CuratedMatchJob` / `AutoFestivalMatchPolicy` just calls `TryCreateMatchAsync(…, Curated)`.

## Maybe later (deliberately deferred)

- **"X liked you"** — surfacing inbound likes (Tinder's monetization lever). The `Like` table already captures the data; just decide if/when to expose it. **Not now.**
- **Match expiry** — Bumble/Hinge expire unconnected matches. `ExpiresAt?` column exists so adopting it later needs no migration. **Not now.**
- **Permanent pass / block** — passes are non-permanent for now. A real block/report flow is a separate Tier-6 concern.
