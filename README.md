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
   docker compose up -d
   ```

4. Verify database:
   ```bash
   docker compose ps
   ```

5. Build project:
   ```bash
   npm run build
   ```

## Development

- `npm run dev` - Run with ts-node (no compilation)
- `npm run build` - Compile TypeScript
- `npm start` - Run compiled code


## Next Steps

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
