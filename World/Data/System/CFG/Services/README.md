# Hot-reloadable server services

JSON configs in this folder are watched by `ConfigService` and reloaded on the game thread within a few seconds of saving (or immediately via `[ConfigReload`).

| File | System | Default | Purpose |
|------|--------|---------|---------|
| `services.json` | Config | on | Poll interval and config log level |
| `world-time.json` | WorldTime | **off** | Calendar mode, seasons, in-game clock |
| `drop-rates.json` | DropRates | **off** | Named drop chances (Christmas motes, champion skull, …) |
| `web-api.json` | WebApi | **off** | Read-only HTTP API for websites/tools |

When a system is **disabled**, the server uses built-in defaults that match the previous hard-coded behaviour. Invalid JSON or validation errors keep the last good config and are logged under `Logs/services/`.

## Commands

- `[ConfigStatus` — list each file’s version, load time and last error
- `[ConfigReload [name]` — force reload (`services`, `world-time`, `drop-rates`, `web-api`)
- `[WorldTime` — show world date, game clock and active seasons

## Web API

Set `"enabled": true` in `web-api.json` (default bind: `http://localhost:8085/`). Endpoints:

- `GET /api/v1` — index
- `GET /api/v1/health` — uptime + config status
- `GET /api/v1/world-time` — calendar + game clock snapshot
- `GET /api/v1/seasons` — season windows and active state
- `GET /api/v1/drop-rates` — rules and effective chances

Optional `api_key` is checked via the `X-Api-Key` header (or `Authorization: Bearer …`). On Windows, prefixes other than `http://localhost:PORT/` need:

```
netsh http add urlacl url=http://+:8085/ user=Everyone
```

Handlers only read immutable snapshots published on the game thread; other systems can register more routes with `WebApiHost.Register(...)`.

## Quick tests

1. Enable world time, set `"calendar_mode": "fixed"` and `"fixed_date": "2026-12-10"` → Christmas season and Category.Christmas vendors activate.
2. Enable drop rates, set `christmas.winter_spirit_mote.base_chance_percent` to `100` → midwinter kills always drop motes.
3. Enable the web API and `curl -H "X-Api-Key: …" http://localhost:8085/api/v1/seasons`.
4. Save broken JSON → last good config stays; `[ConfigStatus` shows the error.
