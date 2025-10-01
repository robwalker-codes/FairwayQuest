# FairwayQuest

FairwayQuest is a .NET 8 console experience for walking through golf rounds with accurate handicapping, stroke allocation, and Stableford scoring. The app now loads courses from JSON so you can swap layouts without code changes, experiment with different tees, and validate scoring scenarios end-to-end.

## Getting Started

```bash
dotnet restore
dotnet build
dotnet test
```

Run the interactive CLI:

```bash
dotnet run --project src/FairwayQuest.Cli
```

The CLI will prompt for the course, tee, hole selection (18/front/back 9), scoring format, and each player's strokes per hole.

## Rules & Scoring

### Effective Playing Handicap (EPH)

* Start from the player’s declared handicap index (integer).
* Apply a 95% allowance when playing Stableford.
* Halve the value for 9-hole rounds (apply before rounding).
* Round down (floor) to the nearest integer at the end.

Mathematically:

```
base = handicap
if format == Stableford: base *= 0.95
if holesToPlay == 9: base /= 2.0
EPH = floor(base)
```

### Stroke Allocation

* Determine which holes are being played (18, front 9, or back 9).
* Rank those holes by stroke index for the selected set (ranks always become 1..holesPlayed).
* Give one stroke on each hole whose ranked SI is ≤ EPH.
* If EPH exceeds the number of holes, loop again so holes with SI ≤ (EPH − holesPlayed) receive a second stroke, and so on.

Example: 9-hole round with EPH = 12 → all holes receive one stroke; the three lowest stroke indexes receive a second stroke.

### Stableford Points (Men’s Stroke Play Scale)

| Relative to Par | Points |
|-----------------|--------|
| −4 or better    | 6      |
| −3              | 5      |
| −2              | 4      |
| −1              | 3      |
| 0               | 2      |
| +1              | 1      |
| ≥ +2            | 0      |

The scoring engine caps awards at 6 points so extreme net scores never exceed the extended albatross (“hole-out”) bonus.

## Courses via JSON

Course definitions live under `courses/` as `*.course.json` files. The loader discovers files at runtime and validates:

* 18 sequential holes numbered 1–18.
* Men’s stroke indexes cover 1–18 exactly once.
* Every hole includes a par and at least one tee yardage.
* `defaultTee` exists in the `teesMeta` section.

### Schema Example

```json
{
  "name": "Sample Links",
  "location": "Sample City",
  "defaultTee": "blue",
  "teesMeta": {
    "blue": {"ratingMen": 71.4, "slopeMen": 129}
  },
  "holes": [
    {
      "number": 1,
      "par": 4,
      "strokeIndexMen": 10,
      "strokeIndexWomen": 12,
      "yards": {"blue": 355}
    }
  ]
}
```

Add new layouts by dropping files that follow this shape into `courses/`. They are picked up automatically.

### Seeded Courses

* **St Andrews – Old Course** (pars, yardages, and men’s/women’s stroke indexes from the official scorecard). Source: [Wikipedia – Old Course at St Andrews](https://en.wikipedia.org/wiki/Old_Course_at_St_Andrews).
* **Pebble Beach Golf Links** (pars, yardages, and handicap allocations from the AT&T Pebble Beach Pro-Am scorecard). Source: [PGA TOUR Media Guide – Pebble Beach](https://pgatourmedia.pgatourhq.com/).

When you select front or back nine play, FairwayQuest re-ranks the stroke indexes for just those holes before allocating handicap strokes.

## Testing

Run the full suite:

```bash
dotnet test
```

Key coverage includes:

* Effective Playing Handicap rounding (18- and 9-hole Stableford scenarios).
* Stroke allocation for single- and multi-pass distributions.
* Stableford scoring regression that now awards 6 points for a net −4 on a par 5 with two strokes received.
* JSON course loading and stroke-index re-ranking for front/back nine play.

These automated tests guard the Stableford regression and validate the JSON-backed course system end-to-end.
