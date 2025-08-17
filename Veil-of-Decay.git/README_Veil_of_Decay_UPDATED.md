# Veil of Decay 🎮

> A dark, atmospheric 2D action‑adventure built in Unity (C#). **Level 1 is complete**; the game is actively in development.

![Veil of Decay – Trailer Thumbnail](docs/media/trailer_thumbnail.png)

<p align="center">
  <a href="https://www.linkedin.com/posts/vikrant-vinchurkar-9496862bb_gamedev-indiegame-unity2d-activity-7361487762383912960-CRNu?utm_source=social_share_send&utm_medium=member_desktop_web&rcm=ACoAAEzO7ckBT69hKBaxeTH7JyAMv3giDzyCWEU">▶ Watch the Trailer</a> •
  <a href="https://github.com/Vikrant-kun/Veil-of-Decay">Repo Home</a> •
  <a href="#roadmap">Roadmap</a> •
  <a href="#contributing">Contributing</a>
</p>

---

![status](https://img.shields.io/badge/status-in_development-yellow)
![unity](https://img.shields.io/badge/engine-Unity_2021%2B-blue)
![license](https://img.shields.io/badge/license-MIT-green)
![platforms](https://img.shields.io/badge/platforms-Windows%20%7C%20Linux%20%7C%20MacOS-lightgrey)

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Controls](#controls)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Build & Run](#build--run)
- [Screenshots / GIFs](#screenshots--gifs)
- [Roadmap](#roadmap)
- [Devlog](#devlog)
- [Contributing](#contributing)
- [License](#license)
- [Credits](#credits)
- [Contact](#contact)

## Overview
**Veil of Decay** drops you into a decaying world ruled by the Litch Lord. In **Level 1**, you face the Archimage — a corrupted guardian with evolving attack patterns and a rage mode. The focus is on tight combat, readable telegraphs, and cinematic vibes.

> **Note:** This repo reflects an in‑progress build. Expect frequent updates as Level 2 and beyond go live.

## Features
- 🎭 **Story‑driven intro** with a short narrative sequence
- 🧠 **Custom boss AI** (rage mode, combo chains, attack sequencing)
- ⚔️ **Responsive combat** (light/heavy attacks, dash‑through, hit stop)
- 💥 **VFX & feedback** (particle impacts, screen fade/blur on death, camera shake)
- 💡 **In‑game UI/HUD** (player HP, boss HP, menus)
- 🔁 **Death/respawn system** with Guardian Angel cinematic
- 🚪 **Scene transitions & gate triggers** to progress

## Controls
| Action | Key |
|---|---|
| Move | A / D or Arrow Keys |
| Jump | Space |
| Dash | S (while moving) |
| Heal | Q |

> You can remap keys in `ProjectSettings/Input` (legacy input) or the Input System settings.

## Tech Stack
- **Engine:** Unity (C#)
- **Version Control:** Git + GitHub
- **Art & UI:** Photoshop
- **Audio:** Audacity
- **Packages:** Cinemachine, Input System (optional), URP (optional)

## Project Structure
```
Veil-of-Decay/
├─ Assets/
│  ├─ Scripts/
│  │  ├─ Player/
│  │  ├─ Enemies/Bosses/
│  │  ├─ Systems/ (Respawn, UI, Checkpoints, Gates)
│  │  └─ Utilities/
│  ├─ Art/
│  ├─ Animations/
│  ├─ Audio/
│  ├─ Prefabs/
│  └─ Scenes/
│     ├─ MainMenu.unity
│     ├─ Level1.unity
│     └─ Shared.unity
├─ docs/ (design notes, exports)
│  └─ media/ (screenshots, gifs, trailer thumbnail)
└─ README.md
```

## Getting Started
### Prerequisites
- Unity **2021 LTS** or newer (recommended)
- Git installed

### Clone
```bash
git clone https://github.com/Vikrant-kun/Veil-of-Decay.git
cd Veil-of-Decay
```

### Open in Unity
- Open Unity Hub → Add project → select repo folder.

## Build & Run
1. In Unity: **File → Build Settings**.
2. Add `MainMenu` and `Level1` to **Scenes In Build**.
3. Choose target platform (Windows/Mac/Linux).
4. Click **Build** (or **Build & Run**).

> Prebuilt binaries (when available) will be posted under **Releases**.

## Screenshots / GIFs
Add media to `docs/media/` and reference them here.

| Trailer Still | Boss Fight | HUD |
|---|---|---|
| ![Trailer](docs/media/trailer_still.png) | ![Boss](docs/media/boss.png) | ![HUD](docs/media/hud.png) |

## Roadmap
- [x] Level 1 core loop (intro → explore → boss → exit)
- [x] Boss AI v1 (combo, chase, rage)
- [x] Death/respawn cinematic
- [ ] Level 2 prototype (harder boss, new ability)
- [ ] New environments & hazards
- [ ] Polish pass (SFX, juice, camera work)
- [ ] Steam page prep / Wishlist CTA (TBD)

## Devlog
- **2025‑08‑14:** Level 1 trailer published (LinkedIn). Boss AI tuned; UI polish.
- **2025‑07‑22:** Rage mode & respawn system iterations.
- **2025‑07‑15:** Core combat loop + HUD implemented.

> For detailed changes, see [Commits](../../commits/main).

## Contributing
This is a solo project, but suggestions and bug reports are welcome.
- Open an **Issue** with steps to reproduce.
- For PRs: follow the existing code style and keep changes scoped.

## License
This project is licensed under the **MIT License** — see [`LICENSE`](LICENSE) for details.

## Credits
- Design, Code, Art: **Vikrant Vinchurkar**
- Fonts/Audio/Third‑party assets: See `Assets/ThirdParty/` and `docs/Credits.md`

## Contact
- LinkedIn: https://www.linkedin.com/in/vikrant-vinchurkar-9496862bb/
- GitHub: https://github.com/Vikrant-kun
- Email: vikrantvinchurkar12@gmail.com
