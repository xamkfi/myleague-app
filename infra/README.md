# Azure infrastructure

Bicep templates and deploy scripts for MyLeague staging and production. Local Docker setup and app overview: [root README](../README.md).

## Environment Strategy

| Environment | Where | Purpose | Monthly cost |
|-------------|-------|---------|--------------|
| **Local dev** | `docker-compose up` at repo root | Day-to-day development (Postgres, Seq, API, frontend) | $0 |
| **Staging** | Azure (`myleague-staging-rg`) | Pre-release testing, auto-deployed from the `development` branch | ~$26 |
| **Prod** | Azure (`myleague-prod-rg`) | Production, manual deploys gated by approval | ~$27 |

There is deliberately no cloud dev environment - local docker-compose covers it.

**Cost tip:** stop the staging App Service when nobody is testing:

```bash
az webapp stop --name myleague-staging-api --resource-group myleague-staging-rg
az webapp start --name myleague-staging-api --resource-group myleague-staging-rg
```

(The PostgreSQL Flexible Server can also be stopped for up to 7 days: `az postgres flexible-server stop --name myleague-staging-postgres --resource-group myleague-staging-rg`.)

## Folder Structure

```
infra/
├── provision/                        # Infrastructure provisioning (Bicep + scripts)
│   ├── backend.bicep                 # Backend infrastructure template
│   ├── backend.bicepparam            # Shared / default backend params
│   ├── backend.staging.bicepparam    # Backend parameters (staging)
│   ├── backend.prod.bicepparam       # Backend parameters (prod)
│   ├── frontend.bicep                # Frontend infrastructure template (SWA)
│   ├── frontend.bicepparam
│   ├── frontend.staging.bicepparam
│   ├── frontend.prod.bicepparam
│   ├── app-insights-only.bicep       # Standalone App Insights (optional)
│   ├── provision-backend.ps1 / .sh   # Provision backend infra manually
│   ├── provision-frontend.ps1        # Provision frontend infra manually
│   └── modules/
│       ├── app-service-plan.bicep    # App Service Plan
│       ├── app-service.bicep         # App Service (API)
│       ├── application-insights.bicep # App Insights + Log Analytics
│       ├── communication-services.bicep # Azure Communication Services (Email)
│       ├── monitoring-alerts.bicep   # Action group, alerts, uptime test, budget
│       ├── postgresql.bicep          # PostgreSQL Flexible Server
│       ├── static-web-app.bicep      # Static Web App
│       └── storage-account.bicep     # Storage Account (image uploads)
├── deploy/                           # Manual application deployment scripts
│   ├── deploy-backend.ps1
│   └── deploy-frontend.ps1
└── README.md
```

The preferred way to provision and deploy is via GitHub Actions (see below). The PowerShell/bash scripts remain for manual/emergency use.

## Architecture (per environment)

### Backend
- **App Service Plan** (Basic B1, Linux) - hosts the .NET 9 API
  - Keep instance count at **1**: SignalR uses an in-memory timer store, so scaling out requires a Redis/Azure SignalR backplane first
- **App Service** - the MyLeague API (`myleague-{env}-api`)
- **PostgreSQL Flexible Server** (Burstable B1ms) - database
- **Storage Account** - image uploads
- **Azure Communication Services** - email delivery (login codes), Azure-managed domain
- **Application Insights + Log Analytics** - telemetry (30-day retention, 1 GB/day cap)
- **Monitoring & alerting** - see below

### Frontend
- **Azure Static Web App** (Free tier) - hosts the React SPA (`myleague-{env}-web`)

## Monitoring & Alerting

Deployed by [provision/modules/monitoring-alerts.bicep](provision/modules/monitoring-alerts.bicep) whenever the `alertEmail` parameter is set. All alerts email the admin via a shared Action Group.

| Alert | Signal | Threshold | Severity |
|-------|--------|-----------|----------|
| Health check failing | App Service `HealthCheckStatus` | < 100 for 5 min | 1 (critical) |
| Site down (prod only) | Availability test on `/health/ready` from 3 EU regions, every 5 min | 2+ locations failing | 1 (critical) |
| Postgres disk filling | `storage_percent` | > 80% | 1 (critical) |
| HTTP 5xx spike | `Http5xx` | > 10 in 5 min | 2 |
| Server exceptions spike | App Insights `exceptions/server` | > 10 in 15 min | 2 |
| Postgres CPU | `cpu_percent` | > 90% for 15 min | 2 |
| Postgres failed connections | `connections_failed` | > 10 in 15 min | 2 |
| Slow responses | `HttpResponseTime` | avg > 5s for 15 min | 3 |
| Plan CPU / memory | `CpuPercentage` / `MemoryPercentage` | > 85% for 15 min | 3 |
| Failure Anomalies | App Insights Smart Detection (ML-based) | automatic | 3 |
| Cost budget | Resource group spend | 80% and 100% of $35/month | notification |

Additional useful metrics with no extra setup: the App Insights dashboards (Failures, Performance, Live Metrics) and the health check UI at `https://myleague-{env}-api.azurewebsites.net/health-ui`.

Alert costs: metric alert rules ~$0.10/month each, availability test pennies at 5-min frequency, smart detection and budgets free. Total ~$1-2/month.

## CI/CD (GitHub Actions)

| Workflow | Trigger | What it does |
|----------|---------|--------------|
| `backend-ci.yaml` / `frontend-ci.yaml` | Push and PRs to `master` and `development` | Build, lint/tests, Docker startup checks |
| `protect-master.yml` | PRs targeting `master` | Fails unless the source branch is `development` |
| `infra-deploy.yml` | Manual (choose env + component); PRs touching `infra/**` | Provisions Azure resources via Bicep. PRs get template validation + what-if against staging |
| `deploy-backend.yml` | Auto to **staging** after Backend CI on `development`; manual for staging/prod | Builds and zip-deploys the API, health-checks it, then runs smoke tests |
| `deploy-frontend.yml` | Auto to **staging** after Frontend CI on `development`; manual for staging/prod | Builds the SPA with the right `VITE_API_URL`, deploys to SWA, then smoke tests it |

After every backend deploy, a smoke-test job hits the live environment with public read-only requests: liveness/readiness (includes DB health), `GET /api/News`, `GET /api/Clubs`, `GET /api/Divisions` (valid JSON expected), an admin endpoint without a token (must return 401 - proves auth is enforced), and an unknown route (must return 404). The frontend deploy verifies the SPA loads (HTTP 200 with the React root element) both on `/` and on a deep link like `/clubs` (SPA fallback). Any failed check fails the workflow, so a broken staging or prod deploy is visible immediately - and on prod the deploy job's approval gate means the smoke failure emails/notifies right after an intentional release.

All workflows authenticate with **OIDC** (federated credentials) - no publish profiles or long-lived secrets. Prod deploys are gated by required reviewers on the `prod` GitHub environment.

### One-time OIDC setup

1. Create an Entra ID app registration and service principal:

```bash
az ad app create --display-name "myleague-github-actions"
APP_ID=$(az ad app list --display-name "myleague-github-actions" --query "[0].appId" -o tsv)
az ad sp create --id $APP_ID
```

2. Add federated credentials for each GitHub environment (replace `<owner>/<repo>`):

```bash
az ad app federated-credential create --id $APP_ID --parameters '{
  "name": "github-env-staging",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<owner>/<repo>:environment:staging",
  "audiences": ["api://AzureADTokenExchange"]
}'

az ad app federated-credential create --id $APP_ID --parameters '{
  "name": "github-env-prod",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<owner>/<repo>:environment:prod",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

3. Create the resource groups and grant the service principal Contributor on them:

```bash
SUB_ID=$(az account show --query id -o tsv)
az group create --name myleague-staging-rg --location westeurope
az group create --name myleague-prod-rg --location westeurope
az role assignment create --assignee $APP_ID --role Contributor \
  --scope /subscriptions/$SUB_ID/resourceGroups/myleague-staging-rg
az role assignment create --assignee $APP_ID --role Contributor \
  --scope /subscriptions/$SUB_ID/resourceGroups/myleague-prod-rg
```

4. In GitHub, create environments `staging` and `prod` (Settings > Environments). On `prod`, add **required reviewers**. Configure each environment:

**Secrets** (per environment):

| Secret | Value |
|--------|-------|
| `AZURE_CLIENT_ID` | The app registration's application (client) ID |
| `AZURE_TENANT_ID` | Your Entra tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Your subscription ID |
| `POSTGRES_ADMIN_PASSWORD` | PostgreSQL admin password (unique per env) |
| `JWT_SECRET_KEY` | JWT signing key, min 32 chars (unique per env) |

**Variables** (per environment):

| Variable | Value |
|----------|-------|
| `ALERT_EMAIL` | Admin email that receives monitoring alerts |
| `SEED_ADMIN_EMAIL` | Initial admin user email (optional) |
| `VITE_API_URL` | Optional override; defaults to `https://myleague-{env}-api.azurewebsites.net/api` |

The old `AZURE_WEBAPP_PUBLISH_PROFILE` and `AZURE_STATIC_WEB_APP_TOKEN` secrets are no longer used and can be deleted.

### Branch protection (one-time, GitHub Settings)

Workflows cannot stop a direct `git push` to `master`. Set this in the repo: **Settings → Rules → Rulesets** (or **Settings → Branches → Add branch protection rule**) for `master`:

1. **Restrict deletions** and **Block force pushes**
2. **Restrict who can push** — leave the allow-list empty (or only org admins). This is what blocks `git push origin master`.
3. **Require a pull request before merging**
4. **Require status checks to pass** and add at least:
   - `PR must come from development` (from `protect-master.yml`)
   - `Build and Test` from Backend CI and Frontend CI (names as shown in Actions)
5. **Do not allow bypassing the above settings** (uncheck admin bypass if you want even admins to go through a PR)

Release path after that: feature branch → PR into `development` → PR from `development` into `master`. Feature-to-master PRs fail the `protect-master` check.

### First-time provisioning order

1. Run `infra-deploy.yml` with component **backend** - creates API, DB, storage, email, monitoring
2. Run `infra-deploy.yml` with component **frontend** - creates the SWA; note the generated URL from the job summary
3. Add the SWA URL to `allowedOrigins` and `frontendBaseUrl` in `backend.{env}.bicepparam`, merge, and re-run the backend provision (CORS + email links need it)
4. Run `deploy-backend.yml` and `deploy-frontend.yml`

## Manual Provisioning (fallback)

```powershell
cd infra/provision
.\provision-backend.ps1 -Environment staging   # or prod
.\provision-frontend.ps1 -Environment staging
```

The backend script prompts for the PostgreSQL password, JWT secret key, seed admin email, and the monitoring alert email. Linux/macOS: `./provision-backend.sh -e staging`.

Manual app deployment:

```powershell
cd infra/deploy
.\deploy-backend.ps1 -Environment staging
.\deploy-frontend.ps1
```

## Authentication Configuration

The provisioning templates configure the following on the App Service:

| Setting | Source | Description |
|---------|--------|-------------|
| `Jwt__SecretKey` | Secret at deploy time | HMAC-SHA256 signing key (min 32 chars) |
| `Jwt__Issuer` / `Jwt__Audience` | Default: `MyLeague` | JWT token issuer/audience |
| `AzureCommunicationServices__ConnectionString` | Auto from ACS module | ACS connection string |
| `AzureCommunicationServices__SenderAddress` | Auto from ACS domain | Email sender (DoNotReply@...) |
| `Seed__AdminEmail` | Variable at deploy time | Initial admin user email |
| `LoginCode__AutoFillLoginCode` | Manual (default `false`) | When `true`, the `/api/Auth/login` response includes the generated code (skips email). Convenient for internal test environments. **Must stay `false` in any publicly reachable production environment** - it exposes the login code to anyone who can call the endpoint with a known email. |

### Toggling the login-code auto-fill flag in Azure

```bash
# Enable auto-fill (e.g. on staging for easier testing)
az webapp config appsettings set \
  --resource-group myleague-staging-rg \
  --name myleague-staging-api \
  --settings LoginCode__AutoFillLoginCode=true

# Disable it (default; the code is only delivered via email)
az webapp config appsettings set \
  --resource-group myleague-staging-rg \
  --name myleague-staging-api \
  --settings LoginCode__AutoFillLoginCode=false
```

Changing an app setting restarts the App Service automatically.

## Estimated Costs (per environment)

| Resource | SKU | Monthly cost (approx) |
|----------|-----|----------------------|
| App Service Plan | Basic B1 | ~$13 |
| PostgreSQL Flexible Server | Burstable B1ms + 32 GB | ~$12 |
| Static Web App | Free | $0 |
| Storage Account | Standard_LRS | ~$0.02/GB |
| Communication Services | Pay-as-you-go | ~$0 (100 free emails/day) |
| Log Analytics / App Insights | Capped at 1 GB/day | ~$0-2 at this scale |
| Alerts + availability test (prod) | Metric alerts + standard test | ~$1-2 |
| **Total** | | **~$26-28/month** |

Both environments together: **~$55/month**. A budget alert fires at 80% of $35 per resource group, so unexpected growth is flagged before it hurts.

## Troubleshooting

### View App Service logs

```bash
az webapp log tail --resource-group myleague-staging-rg --name myleague-staging-api
```

### Check API health

```bash
curl https://myleague-staging-api.azurewebsites.net/health/ready
```

### Connect to PostgreSQL from your machine

```bash
az postgres flexible-server firewall-rule create \
  --resource-group myleague-staging-rg \
  --name myleague-staging-postgres \
  --rule-name AllowMyIP \
  --start-ip-address <your-ip> \
  --end-ip-address <your-ip>
```

### Run database migrations manually

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=myleague-staging-postgres.postgres.database.azure.com;Database=myleague;Username=myleagueadmin;Password=YourPassword;SSL Mode=Require;Trust Server Certificate=true"

cd src/backend/WebAPI
dotnet ef database update --project ../Infrastructure/Infrastructure.csproj
```

(Note: the API also applies migrations automatically at startup.)

### Check ACS email configuration

```bash
az communication show --name myleague-staging-comm --resource-group myleague-staging-rg
az webapp config appsettings list --name myleague-staging-api --resource-group myleague-staging-rg \
  --query "[?name=='AzureCommunicationServices__SenderAddress']"
```

### Test that alerts work

```bash
# Stop the API; the health check alert (severity 1) should email within ~5-10 minutes
az webapp stop --name myleague-staging-api --resource-group myleague-staging-rg
# ...wait for the email, then:
az webapp start --name myleague-staging-api --resource-group myleague-staging-rg
```

## Clean Up

To delete all resources for an environment:

```bash
az group delete --name myleague-staging-rg --yes --no-wait
```
