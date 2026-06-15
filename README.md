# The Everyday Odyssey

> **Team DAILY** — Dynamic Adventure in Lived-Out Yesterday  
> Shinhan University · SW-Centered University · Microstone Industry-Academia Project  
> 2026.04.01 – 2026.08.31

---

## Overview

**The Everyday Odyssey** is a 3D campus escape game prototype built with Unity. It
replaces traditional horror antagonists (monsters, killers) with an abstract
concept made tangible: **Academic Stress AI** that patrols corridors, pursues the
player, and scales in difficulty as deadlines approach. The player fights back not
with weapons, but with a **Laptop Skill System** — a set of cooldown-gated
abilities that represent a student's technical resourcefulness under pressure.

### Key Differentiators

| Conventional Escape Games | The Everyday Odyssey |
|---|---|
| Monsters, killers, supernatural entities | Academic Stress AI (visualized real-life pressure) |
| Gore, violence, jump scares | Non-violent, psychological tension |
| Flashlight, hiding, weapons | Laptop skills: Time Rewind, Enemy Freeze, Clone Decoy |
| Pure survival / escape | Resource management, tactical skill play, spatial exploration |

---

## Team

All members affiliated with **SW-Centered University Project Group (SW중심대학사업단)**, Shinhan University.

| Role | Name (EN) | Name (KO) |
|---|---|---|
| Supervising Professor (지도교수) | **Lee Young-beom** | 이영범 |
| Mentor (멘토) | **Seon Sugyun** | 선수균 |
| Team Lead (팀장) | **Wang Jiaxiong** | 왕가웅 |
| Team Member 1 (팀원1) | Kim Wolseong | 김월성 |
| Team Member 2 (팀원2) | Jo Yebong | 조예봉 |
| Team Member 3 (팀원3) | Gyeong Jaseon | 경자선 |
| Team Member 4 (팀원4) | Myo Honam | 묘호남 |
| Team Member 5 (팀원5) | Lee Hee | 이희 |

---

## Core Gameplay

### Controls
- `WASD` — Move
- `Space` — Jump
- `Shift` — Sprint (consumes stamina)
- `V` — Toggle first-person / third-person view
- `Mouse Left` — Fire laptop code projectile
- `E` — Interact with terminals and upload gate

### Game Loop

```
Environment Mapping → Stress Avoidance → Skill Mitigation → Exit Logic
```

1. Explore the 3D campus (classrooms, corridors, plaza).
2. Avoid Academic Stress AI enemies that patrol via **NavMesh** pathfinding.
3. Use **laptop skills** to disrupt, misdirect, or escape pursuers.
4. Hack all five mentor review terminals scattered across the map.
5. Return to the Twin Black Pillars and upload the final defense build.

### Laptop Skill System

| Skill | Technical Implementation | Cooldown | Strategic Use |
|---|---|---|---|
| **Time Rewind** | Circular buffer recording last 5 s of Transform state; replays in reverse. | 45 s | Undo positioning mistakes; escape dead-ends. |
| **Enemy Freeze** | Temporarily suspends the target NavMesh agent's update loop. | 30 s | Buy time to cross exposed corridors or activate terminals. |
| **Clone Decoy** | Instantiates a decoy GameObject with higher AI detection priority at the player's current position. | 20 s | Redirect AI path-planning; create tactical openings. |

### Win / Lose Conditions

- **Win** — Hack all 5 terminals, then stand between the Twin Black Pillars to upload the final build.
- **Lose** — Get caught 3 times by teachers, or let the countdown timer expire (7 minutes).

---

## Development Roadmap (8 Weeks)

| Phase | Weeks | Technical Milestone | Deliverable |
|---|---|---|---|
| **Phase 1** — Foundation | 1–2 | Character controller, mouse-look, campus greybox scene. | Greybox Build |
| **Phase 2** — Core Systems | 3–4 | NavMesh baking, AI patrol/chase state machine, time-limit and fail-state logic. | AI Core Demo |
| **Phase 3** — Skills & UI | 5–6 | Laptop skill manager with cooldown system; HUD (stamina, timer, minimap). | Alpha (Playable) |
| **Phase 4** — Polish | 7–8 | Sound effects, dynamic difficulty scaling, bug fixes, performance optimization. | Final Demo |

---

## Technology Stack

- **Engine:** Unity 2022.3.62f1
- **Language:** C#
- **AI Pathfinding:** Unity NavMesh (A\* runtime)
- **Assets:** Kenney CC0 (Blocky Characters, City Kit Commercial, City Kit Roads, UI Pack, Interface Sounds, Impact Sounds)
- **Version Control:** Git
- **Target Platform:** PC (Windows)

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Editor/
│   │   └── SceneBootstrapper.cs     # One-click scene generation
│   └── Runtime/
│       ├── GameManager.cs           # Singleton game state, UI binding, CN/KO localization
│       ├── PlayerController.cs      # Movement, camera, weapons, interaction
│       ├── PlayerInteractable.cs    # Abstract interactable base class
│       ├── InteractableTerminal.cs  # Hackable review terminal
│       ├── UploadZone.cs            # Win-condition upload gate
│       ├── TeacherAI.cs             # Patrol / chase / stun / defeat state machine
│       ├── CodeProjectile.cs        # Player projectile with code-label display
│       ├── CharacterMotionController.cs
│       ├── SimpleModelAnimator.cs
│       ├── BillboardAdScreen.cs
│       ├── PrototypeAudioBank.cs
│       └── LanguageOption.cs
├── Scenes/Main.unity                # Single scene (regenerate via menu item)
├── Materials/                       # 69 procedural materials
├── ThirdParty/Kenney/               # CC0 assets (characters, city, roads, UI, audio)
├── StreamingAssets/
├── Resources/
└── Imported/
```

### Scene Regeneration

1. Open the project in Unity 2022.3.62f1.
2. Run **Everyday Odyssey → Build Prototype Scene** from the menu bar.
3. The scene, all GameObjects, and UI will be rebuilt procedurally.

---

## Localization

The game supports **Chinese (Simplified)** and **Korean**, switchable at any time
via in-game buttons. All UI text, prompts, and status messages are served through
a `Localize(zh, ko)` helper in `GameManager`.

---

## Repository Contents

| Path | Description |
|---|---|
| `TalkFile.pdf` | Mentor session report #1 (2026-04-23) — game systems primer, design specification, project planning guide |
| `1조 계획서.docx` | Project proposal (Korean) — background research, goals & content, expected outcomes |
| `1组计划书.docx` | Project proposal (Chinese) — same content as above, Simplified Chinese |
| `Assets/` | Full Unity project source code and assets |
| `ProjectSettings/` | Unity project configuration (21 files) |
| `Packages/manifest.json` | Package dependency declaration |
| `README_PROJECT.md` | Original prototype README |

---

## License

Third-party assets under **Kenney CC0** (public domain).  
Project code © 2026 Team DAILY, Shinhan University SW-Centered University Project Group.
