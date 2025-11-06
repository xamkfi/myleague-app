# Development Deployment Checklist

Follow these steps carefully to deploy your development environment without affecting production.

## ✅ Pre-Flight Checks

- [ ] You're in the `terraform` directory
- [ ] File `terraform.tfvars.dev` exists
- [ ] File `deploy-dev.ps1` exists
- [ ] You have your production resources running in `myleague-rg` resource group

## 📋 Step-by-Step Deployment

### Step 1: Get Your Public IP Address

Your IP needs to be whitelisted to access the development database.

```powershell
# Run this command
(Invoke-WebRequest -Uri "https://ifconfig.me").Content
```

**Write down your IP:** ______________________

### Step 2: Verify Development Configuration

Open `terraform.tfvars.dev` and check:

```hcl
resource_group_name = "myleague-dev-rg"    # ✓ Different from production
app_name            = "myleague-dev"       # ✓ Different from production
enable_postgres_public_access = true       # ✓ Public access for dev
enable_jumpbox = false                     # ✓ No jump box for dev
```

**Verify:** These settings will create NEW resources (won't touch production)

### Step 3: Add Your IP to Configuration

Edit `terraform.tfvars.dev` and update line 19:

**Before:**
```hcl
allowed_ip_addresses = []  # Add your IP here when deploying
```

**After:**
```hcl
allowed_ip_addresses = ["YOUR_IP_HERE"]  # Replace with IP from Step 1
```

**Save the file!**

### Step 4: Review What Will Be Created

Run this command to preview changes:

```powershell
terraform plan -var-file="terraform.tfvars.dev"
```

**Look for:**
- ✅ Resource group: `myleague-dev-rg` (NEW, won't touch `myleague-rg`)
- ✅ PostgreSQL: `myleague-dev-postgres-XXXXXX` (NEW)
- ✅ Container Apps: `myleague-dev-backend`, `myleague-dev-frontend` (NEW)
- ✅ No destruction of existing resources (very important!)

**If you see any `-/+` (destroy and recreate) or `-` (destroy) for `myleague-rg` resources, STOP and ask for help!**

### Step 5: Deploy Development Environment

If Step 4 looks good, deploy:

```powershell
terraform apply -var-file="terraform.tfvars.dev"
```

**When prompted:**
- Read the plan summary
- Type `yes` if it looks correct
- Wait ~10-15 minutes for deployment

### Step 6: Get Connection Details

After successful deployment:

```powershell
# Get database connection info
terraform output postgres_server_fqdn
terraform output postgres_admin_user
terraform output postgres_admin_password

# Get application URLs
terraform output frontend_url
terraform output backend_url
```

**Write down these details:**
- Database Host: ______________________
- Database User: ______________________
- Database Password: ______________________
- Frontend URL: ______________________
- Backend URL: ______________________

### Step 7: Verify Both Environments Exist

Check Azure Portal (portal.azure.com):

**Resource Groups:**
- ✅ `myleague-rg` - Production (existing, untouched)
- ✅ `myleague-dev-rg` - Development (new)

**Or via CLI:**
```powershell
az group list --output table | Select-String "myleague"
```

### Step 8: Connect to Development Database (DBeaver)

Open DBeaver and create a new PostgreSQL connection:

1. Click "New Database Connection"
2. Select "PostgreSQL"
3. Enter connection details from Step 6:
   - **Host:** (from `postgres_server_fqdn` output)
   - **Port:** `5432`
   - **Database:** `myleague`
   - **Username:** `myleague_admin`
   - **Password:** (from `postgres_admin_password` output)
4. Click "Test Connection"
5. Should succeed! ✅

**No SSH, no tunneling - direct connection!**

### Step 9: Deploy Docker Images (Optional - if you want to test the apps)

```powershell
# Get ACR name
$ACR_NAME = terraform output -raw container_registry_name

# Deploy backend
cd ..
.\terraform\deploy-backend.ps1 -AcrName $ACR_NAME

# Deploy frontend
.\terraform\deploy-frontend.ps1 -AcrName $ACR_NAME

# Go back to terraform directory
cd terraform
```

### Step 10: Test Development Environment

1. Open frontend URL in browser (from Step 6)
2. Check backend API docs: `<backend-url>/scalar/v1`
3. Connect to database with DBeaver (from Step 8)

## ✅ Success Checklist

After deployment, verify:

- [ ] Production resources still exist in `myleague-rg`
- [ ] Development resources exist in `myleague-dev-rg`
- [ ] Can connect to dev database from DBeaver (no SSH)
- [ ] Cannot connect to prod database from DBeaver (it's private)
- [ ] Frontend URL loads (might show errors if images not deployed yet)

## 🔧 If Something Goes Wrong

### Production resources were modified/deleted

1. **STOP immediately**
2. Check what happened: `terraform state list`
3. If production is gone, restore from backup or redeploy:
   ```powershell
   terraform apply -var-file="terraform.tfvars.prod"
   ```

### Development deployment failed

1. Read the error message carefully
2. Common issues:
   - **IP not whitelisted:** Add your IP to `allowed_ip_addresses` in `terraform.tfvars.dev`
   - **Resource name conflict:** Verify `app_name = "myleague-dev"` in config
   - **Quota exceeded:** Check Azure subscription limits

### Can't connect to database

1. Verify your IP hasn't changed:
   ```powershell
   (Invoke-WebRequest -Uri "https://ifconfig.me").Content
   ```
2. Update `terraform.tfvars.dev` if different
3. Reapply: `terraform apply -var-file="terraform.tfvars.dev"`

## 🗑️ Cleanup (When Done Testing)

To remove development environment (save costs):

```powershell
terraform destroy -var-file="terraform.tfvars.dev"
```

**This only removes dev resources, production stays!**

## 💰 Cost Tracking

After deployment, both environments run:

| Environment | Resource Group | Monthly Cost |
|-------------|----------------|--------------|
| Production | `myleague-rg` | ~€200-260 |
| Development | `myleague-dev-rg` | ~€30-40 |
| **Total** | | **~€230-300** |

To save costs, destroy development when not using it.

## 📞 Need Help?

If anything looks wrong at any step, STOP and ask before proceeding!

