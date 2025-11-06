# Accessing Azure PostgreSQL Database with DBeaver

This guide shows you how to connect to your Azure PostgreSQL database using DBeaver, for both **Development** (public access) and **Production** (private access via jump box) environments.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Development Environment (Public Access)](#development-environment-public-access)
3. [Production Environment (Private Access via Jump Box)](#production-environment-private-access-via-jump-box)
4. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

1. **DBeaver Community Edition** (Free)
   - Download: https://dbeaver.io/download/
   - Install the PostgreSQL driver when prompted

2. **Your Public IP Address** (for development)
   - Get it: `curl https://ifconfig.me` or visit https://ifconfig.me

### Required Information

You'll need the following database connection details:

- **Server Hostname**
- **Port** (usually 5432)
- **Database Name**
- **Username**
- **Password**

Get these from Terraform outputs:

```powershell
cd terraform
terraform output postgres_server_fqdn
terraform output postgres_database_name
terraform output postgres_admin_user
terraform output postgres_admin_password
```

---

## Development Environment (Public Access)

Development databases are configured with **public access** and firewall rules, allowing direct connection from your local machine.

### Step 1: Get Your Public IP Address

```powershell
# Windows PowerShell
$myIp = (Invoke-WebRequest -Uri "https://ifconfig.me" -UseBasicParsing).Content
Write-Host "Your IP: $myIp"
```

**Important:** If your IP changes, you'll need to update the firewall rules in Azure.

### Step 2: Verify Your IP is Allowed

```powershell
cd terraform
terraform output postgres_server_name

# Check firewall rules
az postgres flexible-server firewall-rule list \
  --resource-group myleague-dev-rg \
  --name <postgres-server-name> \
  --output table
```

If your IP is not in the list, add it:

```powershell
az postgres flexible-server firewall-rule create \
  --resource-group myleague-dev-rg \
  --name <postgres-server-name> \
  --rule-name "MyIP" \
  --start-ip-address <YOUR_IP> \
  --end-ip-address <YOUR_IP>
```

Or update Terraform and reapply:

```powershell
# Edit terraform.tfvars.dev
notepad terraform.tfvars.dev
# Update: allowed_ip_addresses = ["YOUR_NEW_IP"]

terraform apply -var-file="terraform.tfvars.dev"
```

### Step 3: Get Database Connection Details

```powershell
cd terraform

# Get connection details
$serverFqdn = terraform output -raw postgres_server_fqdn
$databaseName = terraform output -raw postgres_database_name
$username = terraform output -raw postgres_admin_user
$password = terraform output -raw postgres_admin_password

Write-Host "Server: $serverFqdn"
Write-Host "Database: $databaseName"
Write-Host "Username: $username"
Write-Host "Password: $password"
```

### Step 4: Configure DBeaver Connection

1. **Open DBeaver**

2. **Create New Connection:**
   - Click **"New Database Connection"** (plug icon) or `Ctrl+Shift+N`
   - Select **PostgreSQL**

3. **Configure Connection Settings:**

   **Main Tab:**
   - **Host:** `<postgres-server-fqdn>` (e.g., `myleague-postgres-t2dns6.postgres.database.azure.com`)
   - **Port:** `5432`
   - **Database:** `<database-name>` (e.g., `myleague`)
   - **Username:** `<admin-username>` (e.g., `myleague_admin`)
   - **Password:** `<admin-password>` (from Terraform output)

   **SSL Tab:**
   - **Use SSL:** ✅ **Check this box**
   - **SSL Mode:** Select **"require"** or **"verify-ca"**
   - **SSL Factory:** Leave default

   **Driver Properties Tab (Optional):**
   - Click **"Add Property"**
   - Add: `sslmode` = `require`

4. **Test Connection:**
   - Click **"Test Connection"**
   - If prompted to download PostgreSQL driver, click **"Download"**
   - You should see: **"Connected"** ✅

5. **Save and Connect:**
   - Click **"Finish"**
   - The connection will appear in your database navigator

### Step 5: Verify Connection

1. **Expand the connection** in the database navigator
2. **Expand "Databases"** → **"myleague"**
3. **Expand "Schemas"** → **"public"**
4. **Expand "Tables"** to see your database tables

You should now be able to:
- Browse tables
- Run SQL queries
- View and edit data
- Export/import data

---

## Production Environment (Private Access via Jump Box)

Production databases are **private** (VNet-only) for security. You need to connect through a **jump box VM**.

### Step 1: Get Jump Box Connection Details

```powershell
cd terraform

# Get jump box details
terraform output jumpbox_public_ip
terraform output jumpbox_ssh_command
terraform output jumpbox_admin_username
terraform output jumpbox_admin_password
```

### Step 2: Connect to Jump Box

**Option A: SSH (Recommended if you have SSH key)**

```powershell
# If you configured SSH key in Terraform
ssh azureuser@<jumpbox-ip>
```

**Option B: Azure Bastion (If configured)**

1. Go to Azure Portal
2. Navigate to the jump box VM
3. Click **"Connect"** → **"Bastion"**
4. Enter username and password

**Option C: RDP (Windows)**

```powershell
# Get RDP file
az vm show --name <vm-name> --resource-group myleague-rg --query "id" -o tsv | ForEach-Object {
    az network bastion rdp --name <bastion-name> --resource-group myleague-rg --target-resource-id $_ --output file --file-path connect.rdp
}
```

### Step 3: Install DBeaver on Jump Box (If Needed)

Once connected to the jump box:

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install -y wget
wget -O - https://dbeaver.io/debs/dbeaver.gpg.key | sudo apt-key add -
echo "deb https://dbeaver.io/debs/dbeaver-ce /" | sudo tee /etc/apt/sources.list.d/dbeaver.list
sudo apt update
sudo apt install -y dbeaver-ce

# Or download and install manually
wget https://dbeaver.io/files/dbeaver-ce_latest_amd64.deb
sudo dpkg -i dbeaver-ce_latest_amd64.deb
```

**Alternative:** Use Azure Data Studio (lighter weight):

```bash
wget https://azuredatastudio-update.azurewebsites.net/latest/linux-deb-x64/stable -O azure-data-studio.deb
sudo dpkg -i azure-data-studio.deb
```

### Step 4: Configure DBeaver on Jump Box

1. **Open DBeaver** on the jump box

2. **Create New Connection:**
   - Select **PostgreSQL**

3. **Configure Connection:**
   - **Host:** `<postgres-server-fqdn>` (e.g., `myleague-postgres-om63tv.postgres.database.azure.com`)
   - **Port:** `5432`
   - **Database:** `myleague`
   - **Username:** `myleague_admin`
   - **Password:** `<password-from-terraform-output>`
   - **SSL:** ✅ Enabled, Mode: `require`

4. **Test and Connect**

### Step 5: Port Forwarding (Alternative: Use DBeaver Locally)

Instead of installing DBeaver on the jump box, you can use **SSH port forwarding** to connect from your local DBeaver:

#### Windows (PowerShell):

```powershell
# Create SSH tunnel
ssh -L 5432:<postgres-server-fqdn>:5432 azureuser@<jumpbox-ip>

# Keep this terminal open!
```

#### Windows (PuTTY):

1. Open PuTTY
2. **Session:**
   - Host: `<jumpbox-ip>`
   - Port: `22`
3. **Connection → SSH → Tunnels:**
   - Source port: `5432`
   - Destination: `<postgres-server-fqdn>:5432`
   - Click **"Add"**
4. **Connection → Data:**
   - Auto-login username: `azureuser`
5. Click **"Open"** and enter password

#### Then Configure DBeaver Locally:

1. **Host:** `localhost` (not the PostgreSQL server!)
2. **Port:** `5432`
3. **Database:** `myleague`
4. **Username:** `myleague_admin`
5. **Password:** `<password>`
6. **SSL:** ✅ Enabled

The connection will be forwarded through the SSH tunnel to the database.

---

## Troubleshooting

### Connection Refused / Timeout

**Development:**
- ✅ Verify your IP is in firewall rules
- ✅ Check firewall rules: `az postgres flexible-server firewall-rule list --resource-group <rg> --name <server>`
- ✅ Ensure SSL is enabled in DBeaver
- ✅ Verify you're using the correct database name (not `postgres`, but `myleague`)

**Production:**
- ✅ Verify jump box is running: `az vm show --name <vm-name> --resource-group <rg> --query "powerState"`
- ✅ Check VNet connectivity from jump box
- ✅ Verify database is in the same VNet as jump box
- ✅ Test connection from jump box: `psql -h <db-fqdn> -U <username> -d <database>`

### Authentication Failed

**Error:** `password authentication failed`

**Solutions:**
1. **Verify password:**
   ```powershell
   terraform output postgres_admin_password
   ```

2. **Check username:**
   - Development: Usually `myleague_admin`
   - Make sure you're not using `postgres` (default database) as the database name

3. **Reset password (if needed):**
   ```powershell
   az postgres flexible-server update \
     --resource-group <rg> \
     --name <server-name> \
     --admin-password <new-password>
   ```

### SSL Connection Error

**Error:** `SSL connection required`

**Solution:**
- ✅ Enable SSL in DBeaver connection settings
- ✅ Set SSL Mode to `require` or `verify-ca`
- ✅ Some Azure PostgreSQL instances require SSL

### Database Not Found

**Error:** `database "myleague" does not exist`

**Solution:**
1. **List databases:**
   ```sql
   \l
   ```

2. **Verify database name:**
   ```powershell
   terraform output postgres_database_name
   ```

3. **Create database if missing:**
   ```sql
   CREATE DATABASE myleague;
   ```

### Firewall Rule Not Working

**Symptoms:** Connection works from some IPs but not others

**Solution:**
1. **Check current IP:**
   ```powershell
   curl https://ifconfig.me
   ```

2. **Update firewall rule:**
   ```powershell
   az postgres flexible-server firewall-rule update \
     --resource-group <rg> \
     --name <server-name> \
     --rule-name <rule-name> \
     --start-ip-address <new-ip> \
     --end-ip-address <new-ip>
   ```

3. **Or update Terraform and reapply:**
   ```powershell
   # Edit terraform.tfvars.dev
   allowed_ip_addresses = ["NEW_IP"]
   
   terraform apply -var-file="terraform.tfvars.dev"
   ```

### Port Forwarding Not Working

**Symptoms:** Can't connect through SSH tunnel

**Solutions:**
1. **Verify SSH connection:**
   ```powershell
   ssh azureuser@<jumpbox-ip>
   # Should connect successfully
   ```

2. **Check tunnel is active:**
   - Keep the SSH session open
   - Verify port is listening: `netstat -an | findstr 5432` (Windows)

3. **Try different local port:**
   ```powershell
   ssh -L 5433:<db-fqdn>:5432 azureuser@<jumpbox-ip>
   # Then use port 5433 in DBeaver
   ```

---

## Quick Reference

### Development Connection String

```
Host: <postgres-server-fqdn>
Port: 5432
Database: myleague
Username: myleague_admin
Password: <from-terraform-output>
SSL: Required
```

### Production Connection (via Jump Box)

```
Host: localhost (if using port forwarding)
Port: 5432
Database: myleague
Username: myleague_admin
Password: <from-terraform-output>
SSL: Required
```

### Useful Commands

```powershell
# Get all connection details
cd terraform
terraform output postgres_server_fqdn
terraform output postgres_database_name
terraform output postgres_admin_user
terraform output postgres_admin_password

# Test connection from command line (if psql is installed)
psql "host=<server-fqdn> port=5432 dbname=<database> user=<username> sslmode=require"
```

---

## Security Best Practices

1. **Never commit passwords** to version control
2. **Use Key Vault** for storing sensitive credentials
3. **Rotate passwords** regularly
4. **Limit firewall rules** to specific IPs
5. **Use SSL/TLS** for all connections
6. **Use jump box** for production database access
7. **Enable audit logging** for database access

---

## Next Steps

- [Deployment Guide](./DEPLOYMENT-GUIDE.md)
- [Application Insights Queries](./APPLICATION-INSIGHTS-QUERIES.md)
- [Development vs Production](./DEV-VS-PROD-DEPLOYMENT.md)

---

**Last Updated:** November 2025

