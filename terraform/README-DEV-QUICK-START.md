# Development Quick Start (The Easy Way!)

**For development environments - much simpler than production!**

## Why Development Mode?

✅ **Direct database access** - Connect from DBeaver without SSH/jump box  
✅ **Simple setup** - No VNet complexity  
✅ **Cheaper** - ~€30-40/month (vs ~€200-260 for production)  
✅ **Scale to zero** - Save money when idle  
✅ **Fast iteration** - Change database schema easily  

## One-Command Deployment

```powershell
cd terraform
.\deploy-dev.ps1
```

That's it! The script will:
1. Auto-detect your IP address
2. Update `terraform.tfvars.dev`
3. Deploy the development environment
4. Show you all connection details

## Manual Deployment (3 Steps)

### Step 1: Get Your IP

```powershell
# Windows PowerShell
(Invoke-WebRequest -Uri "https://ifconfig.me").Content

# Or visit: https://ifconfig.me in your browser
```

### Step 2: Edit terraform.tfvars.dev

Open `terraform/terraform.tfvars.dev` and update:

```hcl
allowed_ip_addresses = ["123.45.67.89"]  # Replace with YOUR IP
```

### Step 3: Deploy

```bash
cd terraform
terraform init  # First time only
terraform apply -var-file="terraform.tfvars.dev"
```

Type `yes` when prompted.

## Connect to Database (DBeaver)

Get connection details:

```bash
terraform output postgres_server_fqdn
terraform output postgres_admin_password
```

Create new PostgreSQL connection in DBeaver:
- **Host:** (from output above)
- **Port:** `5432`
- **Database:** `myleague`
- **Username:** `myleague_admin`
- **Password:** (from output above)
- **SSL Mode:** `require`

**No SSH, no jump box, no tunneling - just connect!** 🎉

## Deploy Backend & Frontend

```powershell
cd terraform
$ACR_NAME = terraform output -raw container_registry_name

# Deploy backend
.\deploy-backend.ps1 -AcrName $ACR_NAME

# Deploy frontend
.\deploy-frontend.ps1 -AcrName $ACR_NAME
```

## Get Application URLs

```bash
terraform output frontend_url   # Your app
terraform output backend_url    # API docs
```

## What Gets Deployed?

| Resource | Configuration | Monthly Cost |
|----------|--------------|--------------|
| PostgreSQL | Burstable Basic, Public access | ~€15 |
| Container Apps | Scale to zero (0-2 replicas) | ~€5-10 |
| Container Registry | Basic | ~€4 |
| Application Insights | Basic monitoring | ~€5 |
| VNet & Networking | Minimal | ~€2 |
| Jump Box | **Disabled** | €0 |
| **Total** | | **~€31-36** |

## Key Differences from Production

| Feature | Development (You!) | Production (5000 users) |
|---------|-------------------|------------------------|
| Database | Public (your IP only) | Private (VNet) |
| Jump Box | Not needed | Required (~€13/month) |
| Scale | 0-2 replicas | 2-10 replicas |
| Resources | Small (0.25 CPU) | Large (1.0 CPU) |
| Cost | ~€30-40/month | ~€200-260/month |
| DBeaver | Direct connection | SSH tunnel required |

## If Your IP Changes

Your home internet IP might change. When it does:

```powershell
cd terraform

# Option 1: Run the script again (easiest)
.\deploy-dev.ps1

# Option 2: Manual update
# Get new IP: (Invoke-WebRequest -Uri "https://ifconfig.me").Content
# Edit terraform.tfvars.dev: allowed_ip_addresses = ["NEW_IP"]
terraform apply -var-file="terraform.tfvars.dev"
```

## Cost Saving Tips

1. **Apps scale to zero** automatically when idle (already configured)
2. **Destroy when not using** for long periods:
   ```bash
   terraform destroy -var-file="terraform.tfvars.dev"
   ```
   Rebuilds in ~10 minutes when needed.

3. **Use Burstable PostgreSQL** (already configured - cheapest option)

## Switching to Production Later

When you're ready for production (5000 users):

```bash
# Deploy production environment
terraform apply -var-file="terraform.tfvars.prod"
```

See [DEV-VS-PROD-DEPLOYMENT.md](./DEV-VS-PROD-DEPLOYMENT.md) for migration guide.

## Troubleshooting

### "Can't connect to database from DBeaver"

1. Check your IP is correct:
   ```powershell
   (Invoke-WebRequest -Uri "https://ifconfig.me").Content
   ```

2. Update if needed:
   ```bash
   # Edit terraform.tfvars.dev with new IP
   terraform apply -var-file="terraform.tfvars.dev"
   ```

3. Verify SSL mode is set to `require` in DBeaver

### "Container Apps show as unhealthy"

Apps scale to zero when idle. They'll start automatically on first request (may take 30-60 seconds).

### "Want to test production setup without production costs"

Edit `terraform.tfvars.dev`:
```hcl
enable_postgres_public_access = false  # Test private mode
enable_jumpbox = true                   # Test jump box
```

This lets you test production features in dev environment.

## Documentation

- **This guide** - Development quick start (you are here!)
- [DEV-VS-PROD-DEPLOYMENT.md](./DEV-VS-PROD-DEPLOYMENT.md) - Detailed comparison
- [JUMPBOX-GUIDE.md](./JUMPBOX-GUIDE.md) - Jump box for production
- [README.md](./README.md) - Full deployment guide
- [ACCESS-POSTGRES.md](./ACCESS-POSTGRES.md) - All database access methods

## Summary

**Development is MUCH simpler than production:**

```powershell
# The entire process:
cd terraform
.\deploy-dev.ps1          # Deploy (auto-detects your IP)
# Type 'yes' when prompted

# Get database details
terraform output postgres_server_fqdn
terraform output postgres_admin_password

# Connect DBeaver - no SSH, no complexity!
# Host: <from output>, Port: 5432, DB: myleague
# Username: myleague_admin, Password: <from output>
```

**That's it! No jump boxes, no VPN, no SSH tunnels - just simple Azure deployment!** 🚀

---

**Cost:** ~€30-40/month  
**Setup Time:** ~10 minutes  
**Complexity:** ⭐⭐ (vs ⭐⭐⭐⭐ for production)

