# Azure infrastructure

Bicep templates, provisioning scripts, and deploy scripts for the MyLeague Azure environments. The GitHub Actions workflows in [.github/workflows](../.github/workflows) are the normal way to provision and deploy. The scripts are for manual or emergency use.

Local development does not use Azure. See [.claude/skills/run-local/SKILL.md](../.claude/skills/run-local/SKILL.md).

## Environments

| Environment | GitHub environment | Resource group | Prefix | Regions | Deployed from |
|-------------|--------------------|----------------|--------|---------|---------------|
| Staging | `staging` | `myleague-staging-rg` | `myleague-staging` | West Europe | `development`, automatically, no approval |
| Prod | `prod` | `myleague-prod-rg` | `myleague-prod` | West Europe | `master`, manual run only, after one approval |
| MAHL prod | `mahl-prod` | `mahl-prod-rg` | `mahl-prod` | Sweden Central (backend), East US 2 (Static Web App) | `master`, automatically on merge, after one approval |

MAHL prod is a separate production instance in its own Azure subscription. Its subscription rejects new resources in West Europe, so the backend runs in Sweden Central. Static Web Apps have no other European region, so the frontend is in East US 2. Static files are served from the global edge either way.

There is no cloud dev environment. The templates still accept `dev` as `environmentName`, but nothing uses it.

## Folder structure

```
infra/
├── provision/
│   ├── backend.bicep                  backend resources (entry point)
│   ├── backend.staging.bicepparam
│   ├── backend.prod.bicepparam
│   ├── backend.mahl-prod.bicepparam
│   ├── frontend.bicep                 Static Web App (entry point)
│   ├── frontend.staging.bicepparam
│   ├── frontend.prod.bicepparam
│   ├── frontend.mahl-prod.bicepparam
│   ├── app-insights-only.bicep        optional: Log Analytics + App Insights only
│   ├── provision-backend.ps1 / .sh    manual backend provisioning (staging, prod)
│   ├── provision-frontend.ps1         manual frontend provisioning (staging, prod)
│   └── modules/
│       ├── app-service-plan.bicep
│       ├── app-service.bicep
│       ├── postgresql.bicep
│       ├── storage-account.bicep
│       ├── communication-services.bicep
│       ├── application-insights.bicep
│       ├── monitoring-alerts.bicep
│       └── static-web-app.bicep
├── deploy/
│   ├── deploy-backend.ps1             manual API build + zip deploy
│   └── deploy-frontend.ps1            manual SPA build + SWA deploy
└── README.md
```

## Resources per environment

Names use `{prefix}` from the table above (`{baseName}-{environmentName}`).

| Resource | Type | Name | Key settings |
|----------|------|------|--------------|
| App Service Plan | `Microsoft.Web/serverfarms` | `{prefix}-plan` | Linux, 1 instance. Basic B1 on staging, Basic B2 (2 cores, 3.5 GB) on prod and MAHL prod |
| App Service (API) | `Microsoft.Web/sites` | `{prefix}-api` | .NET 10, Always On, HTTPS only, TLS 1.2, HTTP/2, FTPS off, health check `/health/ready`, run from package |
| PostgreSQL Flexible Server | `Microsoft.DBforPostgreSQL/flexibleServers` | `{prefix}-postgres` | PostgreSQL 16, Burstable `Standard_B1ms`, 32 GB, database `myleague`, admin user `myleagueadmin`, no HA, no geo-redundant backup, firewall rule `AllowAzureServices` |
| Storage account | `Microsoft.Storage/storageAccounts` | `myleaguestagingstorage`, `myleagueprodstorage`, `mahlprodstorage` | StorageV2, `Standard_LRS`, container `images` with public blob read, 7-day blob soft delete |
| Communication Services | `Microsoft.Communication/communicationServices` | `{prefix}-comm` | Data location Europe, linked to the email domain |
| Email service | `Microsoft.Communication/emailServices` | `{prefix}-email` | Azure-managed domain, sender `DoNotReply` (display name `MyLeague`) |
| Log Analytics workspace | `Microsoft.OperationalInsights/workspaces` | `{prefix}-logs` | 30-day retention, 1 GB/day cap |
| Application Insights | `Microsoft.Insights/components` | `{prefix}-ai` | Workspace-based |
| Monitoring | action group, metric alerts, budget | `{prefix}-alerts-ag`, `{prefix}-alert-*`, `{prefix}-budget` | Only when `alertEmail` is set. See [Monitoring](#monitoring-and-alerts) |
| Availability test | `Microsoft.Insights/webtests` | `{prefix}-availability` | Prod and MAHL prod only (`enableAvailabilityTest`) |
| Static Web App | `Microsoft.Web/staticSites` | `{prefix}-web` | Free tier |

PostgreSQL backup retention is 7 days on staging and 21 days on prod and MAHL prod.

### Single instance only

Keep the App Service Plan at one instance. Match timer state is held in the API's memory, and SignalR has no backplane. A second instance would split timer state and SignalR groups between instances. Scaling out needs a Redis or Azure SignalR backplane first. The templates do not set an instance count, so Azure uses the default of 1.

## App Service configuration

`modules/app-service.bicep` writes these settings. Values come from template outputs or from secrets at deploy time.

| Setting | Source |
|---------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Staging` for staging, `Production` for prod and MAHL prod |
| `WEBSITE_RUN_FROM_PACKAGE` | `1` |
| `Jwt__SecretKey` | `JWT_SECRET_KEY` secret |
| `Jwt__Issuer`, `Jwt__Audience` | `MyLeague` |
| `AzureCommunicationServices__ConnectionString` | Communication Services module |
| `AzureCommunicationServices__SenderAddress` | `DoNotReply@<azure-managed-domain>` |
| `AzureStorage__ContainerName` | `images` |
| `Seed__AdminEmail` | `SEED_ADMIN_EMAIL` secret (optional). Creates the first admin user at startup |
| `Frontend__BaseUrl` | The Static Web App URL |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Application Insights module |
| `LoginCode__AutoFillLoginCode` | `false`, set only when `ASPNETCORE_ENVIRONMENT` is `Production` |
| Connection string `DefaultConnection` | PostgreSQL, `SSL Mode=VerifyFull` |
| Connection string `AzureBlobStorage` | Storage account key |

CORS is set on the App Service itself (`siteConfig.cors`, credentials allowed) from the `allowedOrigins` parameter. The workflows override `allowedOrigins` and `frontendBaseUrl` with the live Static Web App URL:

| Environment | `allowedOrigins` at deploy time |
|-------------|--------------------------------|
| Staging | `http://localhost:5173` and the staging SWA URL |
| Prod | The prod SWA URL |
| MAHL prod | The MAHL SWA URL and the custom domains in `CUSTOM_DOMAIN_ORIGINS` in `release-mahl-production.yml` |

When you add a custom domain to the MAHL Static Web App, add it to `CUSTOM_DOMAIN_ORIGINS` and to `allowedOrigins` in `backend.mahl-prod.bicepparam`. The override replaces the whole list, so a domain missing from the workflow loses CORS on the next release.

The API applies EF Core migrations for all four DbContexts at startup.

### Login code auto-fill

> **Warning:** Never set `LoginCode__AutoFillLoginCode` (`LoginCode:AutoFillLoginCode`) to `true` on any Azure environment, staging included. When it is `true`, `POST /api/Auth/login` returns the login code in the response, so anyone who knows an admin email can sign in as that admin.

`appsettings.json` defaults it to `false`. Only `appsettings.Development.json` turns it on, for local development. Bicep forces `false` on Production App Services. Staging has no override and uses the `false` default.

To check the current value:

```bash
az webapp config appsettings list \
  --resource-group myleague-staging-rg \
  --name myleague-staging-api \
  --query "[?name=='LoginCode__AutoFillLoginCode']"
```

## Monitoring and alerts

[modules/monitoring-alerts.bicep](provision/modules/monitoring-alerts.bicep) is deployed when `alertEmail` is set. Every alert emails the address in `ALERT_EMAIL` through one action group.

| Alert | Signal | Threshold | Severity |
|-------|--------|-----------|----------|
| Health check failing | App Service `HealthCheckStatus` | avg < 100 over 5 min | 1 |
| Site down (prod and MAHL prod) | Availability test on `/health/ready` every 5 min from Amsterdam, Dublin, and Paris | 2 or more locations failing | 1 |
| PostgreSQL storage | `storage_percent` | avg > 80% over 30 min | 1 |
| HTTP 5xx | `Http5xx` | > 10 in 5 min | 2 |
| Server exceptions | App Insights `exceptions/server` | > 10 in 15 min | 2 |
| PostgreSQL CPU | `cpu_percent` | avg > 90% over 15 min | 2 |
| PostgreSQL CPU credits | `cpu_credits_remaining` (Burstable throttles to baseline at 0) | avg < 30 over 30 min | 2 |
| PostgreSQL failed connections | `connections_failed` | > 10 in 15 min | 2 |
| PostgreSQL memory | `memory_percent` | avg > 90% over 15 min | 3 |
| Slow responses | `HttpResponseTime` | avg > 5 s over 15 min | 3 |
| Plan CPU / memory | `CpuPercentage` / `MemoryPercentage` | avg > 85% over 15 min | 3 |
| Failure anomalies | App Insights smart detection | automatic | 3 |
| Cost budget | Resource group spend | Actual 80% and 100%, forecasted 100% of `monthlyBudgetAmount` (35 USD staging, 50 USD prod and MAHL prod) | notification |

`budgetStartDate` is `2026-09-01T00:00:00Z` in every backend parameter file. Azure rejects changes to the start date of an existing budget, so do not edit it.

Health endpoints on each API: `/health/live`, `/health/ready` (includes the database), `/health`, and `/health-ui` (redirects to `/health-test.html`).

## CI/CD workflows

| Workflow | Trigger | What it does |
|----------|---------|--------------|
| [backend-ci.yaml](../.github/workflows/backend-ci.yaml) | Push and PR to `master` / `development` that touch backend, backend tests, or Compose files; manual | Build, tests, startup checks |
| [frontend-ci.yaml](../.github/workflows/frontend-ci.yaml) | Push and PR to `master` / `development` that touch `src/frontend/**` or Compose files; manual | Lint, type check, build, Docker checks |
| [protect-master.yml](../.github/workflows/protect-master.yml) | PR into `master` | Fails unless the source branch is `development` |
| [infra-deploy.yml](../.github/workflows/infra-deploy.yml) | PR touching `infra/**`; push of `infra/**` to `development`; manual | See below |
| [deploy-backend.yml](../.github/workflows/deploy-backend.yml) | Backend CI succeeds on `development`; called by `infra-deploy.yml`; manual | Publishes and zip-deploys the API, waits for `/health/ready`, runs API smoke tests |
| [deploy-frontend.yml](../.github/workflows/deploy-frontend.yml) | Frontend CI succeeds on `development`; called by `infra-deploy.yml`; manual | Builds the SPA with `VITE_API_URL`, uploads `dist/` to the Static Web App, runs SPA smoke tests |
| [release-production.yml](../.github/workflows/release-production.yml) | Manual from `master` only | After `prod` approval: provisions frontend then backend, deploys API and SPA, runs smoke tests |
| [release-mahl-production.yml](../.github/workflows/release-mahl-production.yml) | Push to `master`; manual from `master` | Same as above for MAHL prod, after `mahl-prod` approval |

`infra-deploy.yml` in detail:

- **Pull request**: compiles every `.bicep` and `.bicepparam` file. For PRs from this repo (not forks) it also runs `what-if` for the backend against staging.
- **Push to `development`**: provisions staging (frontend and backend), then calls `deploy-backend.yml` and `deploy-frontend.yml` for staging.
- **Manual**: choose `staging` or `prod` and `backend`, `frontend`, or `both`. `prod` is accepted only from `master`, and it provisions only. It does not deploy the apps.
- It does not cover MAHL prod. Use `release-mahl-production.yml`.

`deploy-backend.yml` and `deploy-frontend.yml` accept `staging` or `prod` only. A `prod` run fails unless it starts from `master`. Prefer the release workflow for prod.

### Release path

1. Merge a feature branch into `development`. Staging is deployed with no approval.
2. Check staging.
3. Open a PR from `development` into `master` and merge it.
4. `Release MAHL Production` starts. Open **Review deployments** and approve.
5. XAMK prod is not released automatically. To update it, run `Release Production` manually on `master` and approve the `prod` deployment.

To replay a MAHL production release without a new merge, run its workflow manually on `master`.

### Smoke tests

After each API deploy the workflows call, without a token:

| Request | Expected |
|---------|----------|
| `GET /health/live` | 200 |
| `GET /health/ready` | 200 |
| `GET /api/News?page=1&pageSize=1` | 200, valid JSON |
| `GET /api/Clubs` | 200, valid JSON |
| `GET /api/Divisions` | 200, valid JSON |
| `GET /api/Users` | 401 |
| `GET /api/DoesNotExist` | 404 |

After each SPA deploy they request `/` and `/clubs` and expect HTTP 200 with `id="root"` in the body. Any failure fails the workflow.

## GitHub secrets and variables

All workflows sign in to Azure with OIDC (federated credentials). The Static Web App deployment token is fetched at run time with `az staticwebapp secrets list`. No publish profile or SWA token is stored in GitHub.

Set these on each GitHub environment (`staging`, `prod`, `mahl-prod`):

| Secret | Used for |
|--------|----------|
| `AZURE_CLIENT_ID` | OIDC app registration (client ID) |
| `AZURE_TENANT_ID` | Entra tenant |
| `AZURE_SUBSCRIPTION_ID` | Target subscription |
| `POSTGRES_ADMIN_PASSWORD` | `postgresAdminPassword`. Use a different value per environment |
| `JWT_SECRET_KEY` | `jwtSecretKey`, at least 32 characters. Use a different value per environment |
| `ALERT_EMAIL` | `alertEmail`. Empty means no alerts are deployed |
| `SEED_ADMIN_EMAIL` | `seedAdminEmail` (optional) |

| Variable | Used for |
|----------|----------|
| `VITE_API_URL` | Optional. Default: `https://{prefix}-api.azurewebsites.net/api` |
| `ALERT_EMAIL`, `SEED_ADMIN_EMAIL` | Fallback, used only when the secret of the same name is empty |

Environment protection:

- `prod` and `mahl-prod`: add required reviewers.
- `staging`: no required reviewers. Otherwise the automatic provision and deploy runs wait for a person.

### One-time OIDC setup

Do this once for the XAMK subscription (staging and prod), and once in the MAHL subscription for `mahl-prod`.

1. Create an app registration and service principal:

   ```bash
   az ad app create --display-name "myleague-github-actions"
   APP_ID=$(az ad app list --display-name "myleague-github-actions" --query "[0].appId" -o tsv)
   az ad sp create --id $APP_ID
   ```

2. Add a federated credential for each GitHub environment the app serves (`staging`, `prod`, or `mahl-prod`). Replace `<owner>/<repo>` and `<env>`:

   ```bash
   az ad app federated-credential create --id $APP_ID --parameters '{
     "name": "github-env-<env>",
     "issuer": "https://token.actions.githubusercontent.com",
     "subject": "repo:<owner>/<repo>:environment:<env>",
     "audiences": ["api://AzureADTokenExchange"]
   }'
   ```

3. Create the resource groups and grant the service principal Contributor on each one. The workflows can create a missing group, but only if the principal has rights at subscription scope.

   ```bash
   SUB_ID=$(az account show --query id -o tsv)
   az group create --name myleague-staging-rg --location westeurope
   az role assignment create --assignee $APP_ID --role Contributor \
     --scope /subscriptions/$SUB_ID/resourceGroups/myleague-staging-rg
   ```

   Repeat for `myleague-prod-rg`, and for `mahl-prod-rg` in the MAHL subscription.

4. In GitHub (**Settings > Environments**), create `staging`, `prod`, and `mahl-prod` and add the secrets above.

### Branch protection

A workflow cannot block a direct `git push` to `master`. Set a ruleset or branch protection rule on `master`:

1. Block force pushes and deletions.
2. Restrict who can push.
3. Require a pull request before merging.
4. Require status checks: `PR must come from development` (protect-master) and `Build and Test` (Backend CI and Frontend CI).
5. Do not allow bypass, if admins should also go through a PR.

### First-time provisioning

1. Push to `development`, or run `infra-deploy.yml` for `staging` / `both`. Staging is provisioned and the apps are deployed.
2. Check the staging UI and the smoke test results.
3. Merge `development` into `master` and approve the `mahl-prod` run. For XAMK prod, run `release-production.yml` manually on `master` and approve it.
4. Optional: commit the new Static Web App hostname to `allowedOrigins` and `frontendBaseUrl` in `backend.prod.bicepparam`. The release workflow already passes the live URL, so CORS works without this.

## Running it manually

### From GitHub

```bash
# Provision staging (both components); this also deploys the apps to staging
gh workflow run infra-deploy.yml --ref development -f environment=staging -f component=both

# Redeploy only the API or only the SPA to staging (deploys the ref you pass)
gh workflow run deploy-backend.yml --ref development -f environment=staging
gh workflow run deploy-frontend.yml --ref development -f environment=staging

# Replay a production release (needs approval)
gh workflow run release-production.yml --ref master
gh workflow run release-mahl-production.yml --ref master
```

### With the scripts

The scripts support `staging` and `prod` only. For MAHL prod, use the release workflow or call `az deployment group create` with `backend.mahl-prod.bicepparam` and `frontend.mahl-prod.bicepparam`.

```powershell
cd infra/provision
.\provision-frontend.ps1 -Environment staging
.\provision-backend.ps1 -Environment staging    # prompts for secrets
```

On Linux or macOS: `./provision-backend.sh -e staging`.

`provision-backend` prompts for the PostgreSQL password, JWT key, seed admin email, and alert email. It does not override `allowedOrigins` or `frontendBaseUrl`, so it uses the values in the parameter file. `backend.prod.bicepparam` has an empty `allowedOrigins`. Running the script against prod therefore removes CORS for the live site until the next release. Use the release workflow for prod.

```powershell
cd infra/deploy
.\deploy-backend.ps1 -Environment staging
.\deploy-frontend.ps1
```

- `deploy-backend.ps1` defaults to `-Environment dev`, which has no resource group. Always pass `-Environment`. It can also run EF migrations, but the API runs them at startup anyway.
- `deploy-frontend.ps1` asks for the API URL and the Static Web App. It overwrites `src/frontend/.env.production`, which is a committed file. Revert it afterwards with `git checkout src/frontend/.env.production`. It uses the `swa` CLI and installs it globally if it is missing.

### Optional: App Insights only

`app-insights-only.bicep` creates only `{prefix}-logs` and `{prefix}-ai`. After you deploy it, set `APPLICATIONINSIGHTS_CONNECTION_STRING` on the App Service from its `connectionString` output. `backend.bicep` already includes App Insights, so you need this only for an environment that was provisioned without it.

## Estimated cost

Rough monthly estimate per environment at list prices. Check the Azure pricing calculator before you rely on it.

| Resource | SKU | Approx. per month |
|----------|-----|-------------------|
| App Service Plan | Basic B1 (staging) | ~13 USD |
| App Service Plan | Basic B2 (prod, MAHL prod) | ~26 USD |
| PostgreSQL Flexible Server | Burstable B1ms, 32 GB | ~12 USD |
| Static Web App | Free | 0 |
| Storage account | Standard_LRS | cents |
| Communication Services email | Pay as you go | near 0 at this volume |
| Log Analytics / App Insights | 1 GB/day cap | 0 to 2 USD |
| Alerts and availability test | | 1 to 2 USD |

To save money on staging, stop the API and the database when nobody is testing. Azure starts a stopped Flexible Server again after 7 days.

```bash
az webapp stop --name myleague-staging-api --resource-group myleague-staging-rg
az postgres flexible-server stop --name myleague-staging-postgres --resource-group myleague-staging-rg

az postgres flexible-server start --name myleague-staging-postgres --resource-group myleague-staging-rg
az webapp start --name myleague-staging-api --resource-group myleague-staging-rg
```

## Troubleshooting

Stream API logs:

```bash
az webapp log tail --resource-group myleague-staging-rg --name myleague-staging-api
```

Check API health:

```bash
curl https://myleague-staging-api.azurewebsites.net/health/ready
```

Allow your IP on PostgreSQL:

```bash
az postgres flexible-server firewall-rule create \
  --resource-group myleague-staging-rg \
  --name myleague-staging-postgres \
  --rule-name AllowMyIP \
  --start-ip-address <your-ip> \
  --end-ip-address <your-ip>
```

Apply migrations by hand. The API normally does this at startup. Run from `src/backend/Infrastructure`, once per context (`CommonDbContext`, `FloorballDbContext`, `FootballDbContext`, `HockeyDbContext`):

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=myleague-staging-postgres.postgres.database.azure.com;Database=myleague;Username=myleagueadmin;Password=<password>;SSL Mode=VerifyFull"
dotnet ef database update --context CommonDbContext --startup-project ../WebAPI/WebAPI.csproj
```

Check the email sender:

```bash
az communication show --name myleague-staging-comm --resource-group myleague-staging-rg
az webapp config appsettings list --name myleague-staging-api --resource-group myleague-staging-rg \
  --query "[?name=='AzureCommunicationServices__SenderAddress']"
```

Test that alerts fire. Stop the API. The health check alert should email within about 5 to 10 minutes. Then start it again.

```bash
az webapp stop --name myleague-staging-api --resource-group myleague-staging-rg
az webapp start --name myleague-staging-api --resource-group myleague-staging-rg
```

## Delete an environment

```bash
az group delete --name myleague-staging-rg --yes --no-wait
```

This deletes the database and stored images with it.
