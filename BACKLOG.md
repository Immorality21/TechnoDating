# TechnoDating — Backlog

> Future-feature ideas that aren't on the current build path. Loaded by `CLAUDE.md` for cross-session continuity. Append as ideas come up; promote items into the build when they win a planning round.
>
> Items are grouped by **tier** (rough proxy for "how directly does this advance the vision vs how much does it cost"). Higher-leverage items live nearer the top of each tier.

---

## Tier 1 — Free wins not yet implemented

- [ ] **Headliner ↔ TopArtists cross-reference UI surfacing.** Backend already computes `MatchingArtistsCount` per festival. UI can lean into it harder: a dedicated "festivals matching your top artists" view, push notifications when a top-artist tour gets announced, etc.

## Tier 2 — Trust & verification (the dating-specific differentiator)

- [ ] **Email-forward ticket verification.** Users forward Paylogic/Eventbrite/Ticketmaster/Festicket confirmations to `tickets@technodating.app`. Backend parser flips `AttendanceStatus → Ticketed` *verified* + stamps source. No partnerships needed. Quiet pattern other apps use. **Highest-leverage next move after current scope.**
    - **Privacy model is load-bearing**: extract only `festival name + date + buyer email + order reference`. Original email + any attachment **deleted immediately** after extraction. Show user exactly what was extracted. Explicit copy: *"We only read the festival name and date. Your ticket itself is never stored or shared."* One-tap revoke. If the privacy story isn't bulletproof in the UI, the feature doesn't ship.
    - **Timing**: most NL festivals (Paylogic etc.) only release the actual entry credential ~3–5 days before the event. **That's fine** — we verify the *purchase-confirmation* email (arrives within minutes of buying, months ahead), not the live ticket. Both should work — confirmation OR ticket email, either flips the flag.
    - **Future polish path**: Gmail/Outlook OAuth with read-only filter for known ticketing senders → cleaner mental model + zero attachment handling, but ~5× the integration work and users tend to distrust inbox-OAuth more than a one-off email forward. Email-forward first; OAuth as v2.
- [ ] **Partnership with Paylogic.** They handle Awakenings / Verknipt / Mysteryland / DGTL — most of the techno market in NL. One integration ≈ verified attendance for the bulk of launch-scene festivals. Multi-month effort but a real moat.
- [ ] **`VerifiedAttendance` filter in matches.** Once ticket verification exists, women can filter to verified-only attendees of a specific festival. Direct women's-safety win + women-first marketing line.
- [ ] **Behavioural trust badges.** "Replies thoughtfully", "shows up to dates", etc. — earned silently, not user-facing ratings (too creepy per Breeze's pattern). Surfaces in profile.

## Tier 3 — Vision-aligned product (on-festival, real-life)

- [ ] **On-festival meetup coordinator (SignalR).** When both you and a match are marked attending the same festival, on the day-of unlock a thin real-time "meet at the third bar, 8pm" coordination view. Optional location ping. *This is the elevator-pitch feature.*
- [ ] **Festival cohort chats (opt-in).** Group chat for everyone going to e.g. Awakenings 2026. Turns the app into a pre-event community, not just 1:1 matching. Radiate owns this but isn't a dating app.
- [ ] **Pre-festival nudges.** "Awakenings is in 3 weeks. 12 of your matches are going." Time-bound, behavioural, hits scarcity + forcing-function together.
- [ ] **Post-festival prompt.** After a festival, prompt: "Did you meet anyone you want to follow up with?" Reinforces the IRL → continuing-conversation loop.

## Tier 4 — Subculture / scene-aware matching (where the moat is)

- [ ] **Festival subgenre tagging.** Tag each festival with subgenres (hard techno, melodic, hardstyle, house, D&B). Matching boosts on overlapping festival *clusters*, not just same-festival. From CLAUDE.md: "techno in Berlin ≠ techno in Tilburg."
- [ ] **Listening-behaviour signal (post-Spotify integration).** Festival attendance + Spotify recent-plays → real subculture fingerprint. "You both went from Boris Brejcha to Mind Against in the last 6 months *and* are both going to Verknipt" beats any pure swipe-match heuristic.
- [ ] **Self-identified scene tags.** Let users opt into scene labels ("Berlin techno", "NL hardstyle", "Le Guess Who? indie"). Weighted into matching.

## Tier 5 — Revenue alignment (vision-coherent monetisation)

- [ ] **Pay-per-meetup at the festival.** €2–5 charge when both parties confirm meeting at a festival. Honest-business model echoes Breeze. From CLAUDE.md: *"the honest revenue model is itself a USP in 2026."*
- [ ] **Festival partnership — discounted pair tickets.** "Find a date for DGTL → 20% off if you both buy." Self-funding marketing.
- [ ] **TechnoDating zone at partner festivals.** A physical meet-up tent / branded corner at the festival. Marketing + IRL forcing function in one.

## Tier 6 — Foundational, not feature-shaped

- [ ] **iDIN integration** for verified Dutch identity (real name + age + uniqueness). Foundation for trust badges and women's-safety story.
- [ ] **Live selfie on signup** + before every first date (Breeze-style anti-catfishing).
- [ ] **Photo moderation** (AWS Rekognition / Azure Content Safety / Hive). Wire into `Photo.ModerationStatus` — uploads land as `pending`, async worker scans + flips to `approved` / `rejected`. Block render of non-approved photos in the public DTOs.
- [ ] **Reporting + blocking + mod queue.** Non-negotiable before any real launch.
- [ ] **Push notifications** (FCM + APNs, or OneSignal as a unifier).
- [ ] **Background work runner.** Start with `IHostedService` + `System.Threading.Channels`; promote to Hangfire when we need persistence/retries/dashboard.
- [ ] **Spotify OAuth + listening-history sync.** Token refresh + recently-played + top artists/tracks.

---

## How to use this file

- When an idea surfaces that isn't load-bearing for *this* session's work, add it here with a one-line motivation.
- When a tier-1/tier-2 item gets picked up for build, move it out of this file and into the corresponding `CLAUDE.md` section + add a session-log entry.
- Tier groupings are vibes, not contracts — re-tier freely as priorities shift.
