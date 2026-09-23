# Math Dungeon — Project Context (Techwiz 7, Aptech)

This file is the single source of truth for any AI assistant (Claude Code via Unity MCP) working on this repo. Read it fully before generating or editing anything. **The SRS PDF (`docs/Math_Dungeon_SRS.pdf`) is the higher authority — if this file and the SRS ever disagree, the SRS wins and this file should be corrected.**

## Locked facts — do not re-derive or guess these

- **Engine**: Unity `6000.5.9f1` (Unity 6)
- **Render Pipeline**: URP (Universal Render Pipeline)
- **Language**: C#
- **Platform**: Windows + macOS desktop only. No mobile, no console, no web build. Do not add platform-specific code for anything else unless explicitly asked.
- **Persistence**: Local single-file JSON only. No cloud save, no online accounts, no server calls anywhere in this project. Serialization goes through **Newtonsoft Json.NET** (`com.unity.nuget.newtonsoft-json`), not `JsonUtility` — `JsonUtility` cannot serialize dictionaries, and the SRS save shape requires them. Save file: `Application.persistentDataPath/mathdungeon_save.json`, written atomically with a `.bak` fallback (see `SaveSystem.cs`).
- **AI**: One enemy type, Unity `NavMeshAgent`-based patrol/chase only. No ML-Agents, no custom pathfinding, no multiple enemy types.
- **DOTS/ECS**: Used ONLY for collectible coin entities, as a separate pipeline that does not share runtime with the GameObject/MonoBehaviour enemy AI. Do not convert the whole project to ECS. Do not put player, enemy, or UI logic in ECS.
- **Art assets**: KayKit by Kay Lousberg (itch.io) — free, low-poly, rigged:
  - `KayKit – Dungeon Pack` (itch slug `kaykit-dungeon-pack`, currently v1.1) → environment/props. There is no pack literally named "Dungeon Pack Remastered"; this current pack *is* the remake. Do not use `kaykit-dungeon`, which is titled "(Legacy) KayKit - Dungeon Pack".
  - `KayKit – Character Pack: Adventurers` → player character
  - `KayKit – Character Pack: Skeletons` → the single enemy
  - Do not introduce assets from a different visual style without asking first — consistency matters for the demo video.
- **Version control**: git + GitHub. Remote: `https://github.com/mvntaha/math-dungeon`. The Plastic SCM workspace the project was created with has been removed (`.plastic/`, `ignore.conf`) — do not reintroduce it. Project Settings are set to "Visible Meta Files" + "Force Text" serialization so scenes/prefabs are diffable. `Packages/manifest.json` and `Packages/packages-lock.json` **are tracked** — the Unity template `.gitignore` excluded `/Packages/`, which would stop a fresh clone from resolving URP, AI Navigation and Newtonsoft; that rule is commented out and must stay that way.

## Git workflow — follow this exactly

- `main` — always the last known-working, demo-ready build.
- `dev` — integration branch. Everything merges here first.
- `feature/<milestone-name>` — one branch per milestone, matching the milestone order below (e.g. `feature/save-system`, `feature/enemy-ai`, `feature/dungeon-progression`).
- Never commit directly to `main`. PRs/merges go into `dev`; `dev` is squash-merged into `main` only at a working checkpoint (end of a milestone, verified playable).
- Only one person/session edits a given scene at a time — Unity scenes/prefabs still conflict badly even as text (YAML). Prefer prefab variants over duplicating scenes.
- Git LFS is used for binary assets (FBX, textures, audio from KayKit) — do not commit large binaries directly to git.
- Commit messages: short imperative summary, e.g. `Add ChallengeManager answer validation`, referencing the milestone (`M4: ...`) when useful.

## Gameplay constants (do not change without being asked)

- 3 dungeons total, 4 mandatory math challenges each = **12 challenges total**
- Player starts each dungeon with **3 hearts**
- Incorrect challenge answer → **-1 heart** + show `hintText`, allow retry/retreat
- 0 hearts → Game Over state → offer retry current zone OR return to Main Menu (NOT a hard restart of the whole game)
- Correct answer → coins awarded, `explanationText` optionally shown, challenge marked solved
- Dungeons unlock **linearly** (Dungeon 2 requires Dungeon 1 complete, etc.) — no branching, no dungeon selection screen beyond what's unlocked
- Question types: `multiple_choice`, `numerical_input`, `pattern_match` (exactly these three — see Data Dictionary below)
- Time is tracked for scoring/summary only. **Never implement a countdown, timer pressure, or time-based penalty.** Players may take as long as they want per challenge.

## Explicitly OUT of scope — do not build these even if it seems like a natural extension

- Dynamic/procedural question generation (all 12 challenges are hand-authored, predefined data)
- Multiple enemy types or complex enemy AI
- Inventory management systems beyond the basic collected-item concept
- Achievements/unlockable cosmetics system (not in this SRS — that was the *other* competition SRS, Cyberpunk Detective. Do not import ideas from it.)
- Multiplayer, online leaderboards, or any networking
- Machine learning of any kind

## Data models (from SRS §1.9 Data Dictionary — implement exactly these fields)

```
PlayerProfile: profileId, username, avatarId, createdAt, hearts (0-3), coins,
               unlockedDungeons[], dungeonProgress[DungeonProgress], lastCheckpoint

DungeonProgress: dungeonId (1|2|3), completed (bool), challengesSolved[int],
                 attemptsPerChallenge{challengeId: int}, elapsedTimeSeconds
                 # attemptsPerChallenge is a C# Dictionary<int,int>, serialized by
                 # Newtonsoft as the SRS-specified keyed object, e.g. {"203": 2}.
                 # Never store it as an array of {challengeId, attempts} pairs.

Challenge: challengeId, dungeonId, topic, questionType (enum: multiple_choice |
           numerical_input | pattern_match), promptText, options[]? (nullable,
           multiple_choice only), correctAnswer, hintText, explanationText

PerformanceRecord: sessionId, profileId, correctAnswers, incorrectAttempts,
                    accuracyPercent, coinsEarned, totalScore, completedAt
```

## Folder structure

```
Assets/
  _Project/
    Scripts/
      Core/          # GameManager, SaveSystem, SceneLoader
      Player/        # PlayerController, PlayerInteraction
      Enemy/         # EnemyAI (NavMeshAgent), EnemyDetection
      Challenges/     # ChallengeData, ChallengeManager, ChallengeUI
      Dungeon/        # DungeonManager, GateController, CheckpointSystem
      UI/             # MainMenu, HUD, PauseMenu, SettingsMenu, PerformanceSummary
      ECS/            # Coin collectible entities (DOTS, isolated)
      Data/           # Serializable data classes matching the Data Dictionary above
    ScriptableObjects/ # Challenge data assets (12 of them, 4 per dungeon)
    Prefabs/
    Scenes/
      MainMenu.unity
      Dungeon1.unity
      Dungeon2.unity
      Dungeon3.unity
    Art/              # Imported KayKit packs live here, untouched/unmodified where possible
    Audio/
docs/
  Math_Dungeon_SRS.pdf   # source of truth, see top of this file
```

## Coding conventions

- `GameManager` is a persistent singleton (`DontDestroyOnLoad`), owns save state and scene transitions.
- No magic numbers in gameplay code — reference the constants above via a `GameConstants.cs` static class (e.g. `GameConstants.MaxHearts = 3`, `GameConstants.ChallengesPerDungeon = 4`).
- Challenge content lives in data (ScriptableObjects), never hardcoded as strings inside MonoBehaviours.
- Every script should be small and single-responsibility — this project is judged partly on architecture (see SRS §1.5 Application Architecture Diagram: Presentation / Logic / Persistence layers). Keep that separation.

## Working process for the assistant

1. Before implementing anything, check whether it's covered by this file or the SRS. If a request conflicts with a locked fact or introduces something in the "explicitly out of scope" list, **ask before building it** rather than assuming it's fine.
2. Build in the milestone order below, each on its own `feature/*` branch off `dev`. Do not skip ahead to polish (UI, coins, DOTS) before the core loop (player, challenge, enemy, save) works end-to-end.
3. After finishing each milestone, verify it against the relevant SRS functional requirement number before moving to the next, then merge into `dev`.

## Milestone order

1. Project scaffold + data models
2. Save/Load (JSON)
3. Player controller + interaction
4. Math Challenge system (all 12 questions as data)
5. Enemy AI (NavMesh) + Health/hearts
6. Dungeon progression + gating (linear unlock)
7. UI — Main Menu, HUD, Settings, Pause, Performance Summary
8. Coins/reward + scoring
9. DOTS/ECS collectible coins (isolated, last)
