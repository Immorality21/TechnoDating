# TechnoDating — Product & Strategy

> Strategic direction, USP exploration, and competitive context. Maintained across sessions; update as decisions firm up or change.
>
> **Companion files:** [`../CLAUDE.md`](../CLAUDE.md) (engineering context + conventions), [`../BACKLOG.md`](../BACKLOG.md) (future-feature ideas by tier), [`SESSIONLOG.md`](SESSIONLOG.md) (per-session history).

## Elevator pitch (working)

A music + festival-anchored dating app for the Netherlands. Match on **actual music taste and listening behaviour**, then use **verified festival/event attendance** as the forcing function to get people offline and together at shows they're already going to.

Working tagline candidates:
- "Stop swiping. Start dancing. The dating app built around the festivals you're already going to."
- "Match with people who actually like the music you like — and meet them at the next show."
- "Your Spotify and your ticket stubs already know your type. We just help you meet them."

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

All future-feature ideas — profile verification (iDIN, live selfie, ticket attestation, behavioural badges, mod queue), background-work infrastructure, external integrations (Spotify, ticketing providers, push), revenue alignment, on-festival meetup coordinator, festival cohort chats, subculture-aware matching, etc. — live in [`../BACKLOG.md`](../BACKLOG.md), grouped by tier. Promote items into [`SESSIONLOG.md`](SESSIONLOG.md) + the relevant `CLAUDE.md`/`PRODUCT.md` section when they enter active development.
