# FairwayQuest

FairwayQuest is a prototype command-line golf experience written in C#/.NET. It focuses on lightweight round simulation so gameplay rules, scoring models, and shot physics can be iterated on before a potential GUI build.

## Getting Started

```bash
dotnet restore
dotnet build
dotnet test
```

Run the CLI round driver (prompts are interactive):

```bash
dotnet run --project src/FairwayQuest.Cli -- --seed 123 --fast
```

### CLI Options

* `--seed <int>` – provide a deterministic random seed so the same inputs reproduce the same round (default `90210`).
* `--fast` – trims some narration while keeping all essential prompts and outcomes.
* `--stableford-allowance <int>` – override the Stableford allowance percentage (default `95`, set `100` to disable the allowance).

## Rules & Handicaps

*Stroke Index (SI)* ranks hole difficulty. A lower SI means a tougher hole, so handicap strokes are applied to those holes first.

**Playing Handicap**

* 18 holes: use the entered handicap index (HI18) as-is.
* 9 holes: use half of HI18, rounded to the nearest whole number with .5 rounding away from zero.
* Stableford rounds apply the allowance percentage (95% by default) to the 9- or 18-hole playing handicap using the same rounding rule. The result is always clamped between 0 and 54.

When you start a Stableford round the CLI prints each player's HI18, the intermediate playing handicap for the chosen hole count, the allowance used, and the resulting effective handicap. It also lists the per-hole stroke allocation in hole order so you can see where shots are received.

**Example (previous regression)**

Player HI18 `22`, 9-hole Stableford with the default 95% allowance:

* HI18 = 22 → 9-hole playing handicap = 11 → Stableford 95% allowance → effective handicap = 10.
* On the SI-1 par 4, two strokes are allocated. A gross 3 therefore becomes a net 1 (3 − 2, clamped to a minimum of 1) for an albatross worth 5 points—not the 6 that previously appeared.

FairwayQuest intentionally simplifies golf math and physics for a faster-paced experience. Feel free to tweak the course data, allowance percentage, or other parameters to suit your group.

### Gameplay Flow

1. Choose 1–4 players, enter their names and handicaps (0–54).
2. Select 9 or 18 holes on the default course and pick *stroke* or *Stableford* scoring.
3. Play hole-by-hole, selecting clubs when prompted. Enter `?` at any turn to see the club reference.
4. After each shot an outcome line shows carry, lie, and distance to the pin. Putting resolves into 1–3 putts depending on distance.
5. A final scoreboard summarises gross/net totals (stroke) or Stableford points and per-hole breakdowns.

### Club Codes

| Code | Club            | Typical Range |
|------|-----------------|---------------|
| D    | Driver          | 230–280 yds   |
| 3w   | 3 Wood          | 210–240 yds   |
| 5w   | 5 Wood          | 195–215 yds   |
| 3i   | 3 Iron          | 180–200 yds   |
| 4i   | 4 Iron          | 170–190 yds   |
| 5i   | 5 Iron          | 160–180 yds   |
| 6i   | 6 Iron          | 150–170 yds   |
| 7i   | 7 Iron          | 140–160 yds   |
| 8i   | 8 Iron          | 130–150 yds   |
| 9i   | 9 Iron          | 120–140 yds   |
| pw   | Pitching Wedge  | 95–115 yds    |
| sw   | Sand Wedge      | 70–95 yds     |
| lw   | Lob Wedge       | 40–70 yds     |
| p    | Putter (green only) | — |

### Simplified Physics Notes

* Non-putter shots randomise around each club’s carry window and apply lie modifiers (rough −10%, bunker −15%).
* Mishits (≈7%) and duffs (≈2%) drastically reduce carry for unpredictable moments.
* Landing lie probabilities favour the fairway; shorter approaches can hold the green.
* Putting resolves probabilistically within distance bands (tap-ins, makeable range, long lag putts).

### Next Steps

* Swap in data-driven course definitions (JSON) and support multiple layouts.
* Expand shot physics with wind, elevation, and spin models.
* Build a GUI layer once the simulation stabilises.
