# Integration Engine

File Watcher Service for D365 Integration Monitoring & Control

## Prerequisites

- Docker Desktop
- Node.js LTS (via nvm)
- Git

## Setup

1. Install dependencies:
   ```bash
   npm install
   ```

2. Copy environment file:
   ```bash
   cp .env.example .env
   ```

3. Start infrastructure:
   ```bash
   docker-compose up -d
   ```

4. Verify database:
   ```bash
   docker-compose ps
   ```

5. Build project:
   ```bash
   npm run build
   ```

## Development

- `npm run dev` - Run with ts-node (no compilation)
- `npm run build` - Compile TypeScript
- `npm start` - Run compiled code

## Architecture

See `docs/superpowers/specs/2026-07-14-initial-setup-design.md` for design details.

## Verification Checklist

Phase 1-5 setup complete when:

- [x] Docker Desktop running
- [x] `docker-compose ps` shows both services "Up"
- [x] `.env` file exists
- [x] Database connects via localhost:5432
- [x] `integration_db` database exists
- [x] `interface_table` exists with test row
- [x] `npm install` completes without errors
- [x] `npm run build` completes without errors
- [x] `dist/index.js` created
- [x] Git branches: main, develop, feature/setup-verification

## Next Steps

- Create GitHub repository and push branches
- Begin component implementation (config providers, adapters)
- Add database migration framework
- Implement OpenTelemetry instrumentation

## Project Structure

```
integration-engine/
├── .env.example              # Environment template
├── .env                       # Local environment (gitignored)
├── .gitignore                 # Git exclusions
├── package.json               # Node dependencies & scripts
├── tsconfig.json              # TypeScript config
├── docker-compose.yml         # Container orchestration
├── docker/
│   └── init.sql              # Database initialization
├── src/
│   └── index.ts              # Application entry point
├── dist/                      # Build output (gitignored)
└── README.md                  # This file
```
