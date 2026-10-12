# Managed PostgreSQL deployment

## CI/CD workflow

The GitHub Actions workflow at `.github/workflows/ci.yml` runs frontend lint/build and an API build on pushes to `main`, `feature/**`, and `chore/**`, plus pull requests targeting `main`. It does not need production secrets because these jobs only compile and lint.

To connect continuous deployment, connect the same GitHub repository to both hosts and configure their production branch as `main`. In Vercel, set the project root to `frontend/campuseats-web`; in Render, use the API's `backend` root and Docker runtime. Render's service settings support **After CI Checks Pass**, which prevents a deploy when a detected GitHub check fails. Vercel creates a production deployment from the default branch and can use GitHub Actions as a Deployment Check before promoting it. Enable the checks in the Vercel project's deployment settings if you want the CI result to gate production promotion. [Render CI deploys](https://render.com/docs/deploys) and [Vercel Deployment Checks](https://vercel.com/docs/deployment-checks)

After merging or pushing to `main`, confirm the GitHub Actions checks pass, then verify that Render and Vercel each report a successful production deploy for the same commit.

Keep production credentials in the backend host's environment settings. Do not add a real connection string or JWT key to `appsettings.json`, a `.env` file, or source control. The API requires these settings at startup:

| Environment variable | Value |
| --- | --- |
| `ConnectionStrings__Default` | Npgsql keyword format: `Host=<host>;Port=5432;Database=<database>;Username=<user>;Password=<password>;SSL Mode=Require;Channel Binding=Require` |
| `Jwt__Key` | Random secret with at least 32 bytes |
| `Jwt__Issuer` | `CampusEats` |
| `Jwt__Audience` | `CampusEatsUsers` |
| `Cors__FrontendOrigin` | Exact deployed frontend origin, such as `https://campuseats-web.vercel.app` |

In Render, create a Docker Web Service with `backend` as the root directory. Add the variables above under **Environment**, save, and deploy. Keep the connection string and JWT key private. For local development, copy `.env.example` to `.env`, fill in your local values, and run `./run-local.ps1` from PowerShell. The script loads `.env` into the API process; ASP.NET Core does not read `.env` files by itself. The real `.env` file is ignored by Git.

**Npgsql expects the keyword format shown above.** Do not paste Neon’s `postgresql://...` URL directly into `ConnectionStrings__Default`; translate its host, database, username, password, and SSL options to the Npgsql format. The `.env` template uses this format already.

## Apply the database migration

Install or use the .NET 8 `dotnet-ef` tool. After filling in the local `.env`, run this from the `backend` directory. It imports the connection string for the command and applies the migration:

```powershell
.\run-local.ps1 -Migrate
```

Run this against the new managed database before relying on the deployed API. The application no longer calls `EnsureCreated`; it expects the EF migration to have created its tables. If the `dotnet ef` command is unavailable, install the .NET 8 EF CLI tool first.

## Frontend

In the Vercel project settings, set `VITE_API_URL` to the deployed API origin (for example, `https://<your-render-service>.onrender.com`) and redeploy. The API's `Cors__FrontendOrigin` must match the frontend origin exactly. Vite embeds `VITE_*` values at build time; never put database credentials or JWT signing keys in frontend variables.

The current frontend is still the Vite starter screen and does not make API requests yet, so setting `VITE_API_URL` alone will not display menu data. Add API calls using this variable as the frontend is implemented.
