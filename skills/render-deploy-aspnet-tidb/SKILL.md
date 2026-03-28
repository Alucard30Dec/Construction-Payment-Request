---
name: render-deploy-aspnet-tidb
description: Use this skill when deploying an ASP.NET Core app to Render (Docker) with TiDB/MySQL for a stable public demo URL, including environment setup, deployment flow, and troubleshooting common Render/TiDB errors.
---

# Render Deploy ASP.NET + TiDB

## Overview

Use this skill for end-to-end deployment of ASP.NET Core projects to Render using Docker and an external TiDB (MySQL-compatible) database.
It is best for requests like "deploy demo", "create fixed URL", "Render + TiDB setup", or "fix Render deploy/runtime DB errors".

## Workflow

### 1. Collect required inputs

- Repository URL and default branch
- App runtime/version (for example `net8.0`)
- TiDB host/port/database/user/password
- Desired Render service name (becomes `<service-name>.onrender.com`)

### 2. Prepare project files

Required baseline:
- `Dockerfile` for build + publish + runtime
- `render.yaml` with a web service (`runtime: docker`)
- `Program.cs` handles reverse proxy headers before HTTPS redirect

Proxy requirement (important):
- Configure forwarded headers and call `app.UseForwardedHeaders();` before `UseHttpsRedirection()`.

### 3. Create TiDB connectivity

Use ADO.NET style connection string (not `mysql://...`):

```text
Server=<host>;Port=4000;Database=<db>;User Id=<user>;Password=<password>;SslMode=Required;AllowPublicKeyRetrieval=True;ConnectionTimeout=30;DefaultCommandTimeout=60;
```

Network:
- In TiDB, allow incoming connections from Render.
- Fast test path: temporary allow `0.0.0.0/0`, then tighten later.

### 4. Configure Render env vars

Set these keys in Render service Environment:

- `ASPNETCORE_ENVIRONMENT=Production`
- `DatabaseProvider=MySql`
- `Database__UseSqliteFallbackWhenPrimaryUnavailable=false`
- `ConnectionStrings__DefaultConnection=<full tidb connection string>`
- `SeedSampleData__Enabled=true` (optional)

### 5. Deploy and verify

Deploy steps:
1. Push code to repository.
2. Render Blueprint/Web Service deploy.
3. Wait for `Deploy live`.
4. Verify `https://<service-name>.onrender.com/Account/Login`.

## Troubleshooting Playbook

### Build fails on Render (`dotnet publish`)

- Align SDK in `Dockerfile` with target framework (`net8.0` -> use `mcr.microsoft.com/dotnet/sdk:8.0`).
- Avoid unsupported C# expressions inside EF expression trees.

### `Unable to connect to any of the specified MySQL hosts`

- Check `ConnectionStrings__DefaultConnection` exists and is valid.
- Confirm TiDB network access allows Render.
- Verify host and port are correct.

### DB shown as empty (`database ''`) in logs

- Connection string format is wrong or truncated.
- Re-enter full ADO.NET string with `Database=<db>`.

### `Unsupported collation ... ascii_general_ci` on TiDB

- This usually comes from GUID mapped to `char(36)` with unsupported collation.
- For MySQL/TiDB, map GUID columns to `binary(16)` in EF model configuration.
- If schema was partially created, use a fresh database name and redeploy.

### Redirect loop / auth cookie issues behind proxy

- Ensure forwarded headers are configured.
- Call `UseForwardedHeaders()` before `UseHttpsRedirection()`.

## Output Checklist

When done, provide:
1. Public URL.
2. Current deploy status (`Live`/`Failed`).
3. Confirmed env vars (secret masked).
4. Any remaining risk (free tier sleep, ephemeral disk, quota).
