# BetterThinQ

A mobile-first web app for controlling LG ThinQ air conditioners through reusable templates. Save your favourite settings (e.g. *Night*: 20 °C, low fan, cooling), then run them on any connected AC instantly or on a schedule.

Built with .NET 9, Blazor, and the official [LG ThinQ Connect API](https://thinq.developer.lge.com/en/cloud/docs/EMP-Authorization/API-Reference/v2_ThinQ-Connect/).

## Features

- **Templates** — define named AC presets (mode, target temperature, fan speed, swing, etc.) and apply them in one tap.
- **Run now** — push a template to one or more of your connected ACs immediately.
- **Schedule** — run a template at a specific time, on a recurring cron schedule, or as a one-shot delayed action.
- **Mobile-first UI** — designed for thumbs first; works fine on desktop.
- **LG account** — authenticate against ThinQ using a Personal Access Token (PAT) from the LG developer portal. (Full LG OAuth login is on the roadmap.)
- **Self-hosted** — single binary or Docker container; data lives in a local SQLite file.

## Quick start

### Requirements

- .NET 9 SDK
- An LG ThinQ account with at least one supported AC already paired in the LG ThinQ mobile app
- A Personal Access Token from <https://connect-pat.lgthinq.com/>

### Get a ThinQ Personal Access Token

1. Sign in at <https://connect-pat.lgthinq.com/> with your LG account.
2. Click **Create Token**, give it a name, and tick the scopes you want (at minimum the AC read/write scopes).
3. Copy the token; you will paste it on first launch.

Reference: [PAT documentation on the LG ThinQ Developer Site](https://thinq.developer.lge.com/en/cloud/docs/thinq-connect/PAT-en/).

### Run locally

```bash
git clone <repo-url> better-thinq
cd better-thinq
dotnet run --project src/BetterThinq.Web
```

Open <http://localhost:5080>, paste your PAT and pick your region on the setup screen. Your devices will be discovered automatically.

### Run with Docker

```bash
docker compose up -d
```

The compose file mounts `./data` for the SQLite database so your templates and schedules persist across container restarts.

## Configuration

All settings live in `appsettings.json` (or environment variables with the `BETTERTHINQ_` prefix). The PAT and region are entered through the UI on first run and stored in the local database, not in config.

| Key | Default | Notes |
| --- | --- | --- |
| `Server.Port` | `5080` | HTTP port the app listens on |
| `Server.AppPassword` | _(unset)_ | If set, the UI requires this password before showing anything |
| `Database.Path` | `./data/betterthinq.db` | SQLite file location |
| `ThinQ.Region` | `eu` | Default region; overridden by setup wizard |

## How templates work

A template captures a desired AC state, not a sequence of commands. Applying *Night* on two different AC units sends each one the settings it actually supports — unsupported fields (e.g. swing on a model that lacks it) are silently dropped rather than failing the whole apply.

Schedules are owned by templates: one template can have several schedules attached (e.g. *Night* runs at 22:00 on weekdays and at 23:30 on weekends).

## Roadmap

- LG OAuth login (in addition to PAT)
- Sensor readings & room temperature history
- Support for other ThinQ device classes (heat pumps, dehumidifiers, air purifiers)
- Push notifications when a schedule runs or fails
- Home Assistant export

## License

MIT. Not affiliated with or endorsed by LG Electronics. "ThinQ" is a trademark of LG.
