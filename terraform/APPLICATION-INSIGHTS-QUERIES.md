# Application Insights Log Queries

This guide shows you how to view and query logs in Azure Application Insights, which is the Azure equivalent of Seq.

## Access Application Insights Logs

1. Go to Azure Portal
2. Navigate to: **myleague-insights-om63tv** (Application Insights)
3. Click **"Logs"** in the left menu (you're already there!)

## Basic Queries

### View Recent Application Logs
```
traces
| where TimeGenerated > ago(1h)
| order by TimeGenerated desc
| take 100
```

### View Errors Only
```
traces
| where SeverityLevel >= 3
| order by TimeGenerated desc
| take 50
```

### View HTTP Requests
```
requests
| where TimeGenerated > ago(1h)
| order by TimeGenerated desc
| project TimeGenerated, Name, Success, Duration, Url, ResultCode
| take 100
```

### Search for Specific Text
```
traces
| where Message contains "error"
| order by TimeGenerated desc
```

### View by Log Level
```
traces
| where TimeGenerated > ago(1h)
| summarize count() by SeverityLevel
| order by SeverityLevel
```

### View Database Queries
```
traces
| where Message contains "database" or Message contains "PostgreSQL"
| order by TimeGenerated desc
```

### Performance - Slow Requests
```
requests
| where TimeGenerated > ago(1h)
| where Duration > 1000
| order by Duration desc
| project TimeGenerated, Name, Duration, Url, ResultCode
```

### View Exceptions
```
exceptions
| where TimeGenerated > ago(1h)
| order by TimeGenerated desc
| project TimeGenerated, Type, Message, InnermostMessage
```

### Count Requests by Endpoint
```
requests
| where TimeGenerated > ago(1h)
| summarize count() by Name
| order by count_ desc
```

### Real-time Log Stream (Alternative)
Instead of queries, you can also use:
- **Logs stream** - Real-time log streaming (like `docker-compose logs -f`)
- **Live Metrics** - Real-time metrics dashboard

## Time Range Shortcuts

Common time ranges:
- `ago(5m)` - Last 5 minutes
- `ago(1h)` - Last hour
- `ago(24h)` - Last 24 hours
- `ago(7d)` - Last 7 days

## Comparison with Seq

| Seq Feature | Application Insights Equivalent |
|------------|--------------------------------|
| `@Level = 'Error'` | `SeverityLevel >= 3` |
| `@Message contains 'text'` | `Message contains 'text'` |
| Time range filter | `TimeGenerated > ago(1h)` |
| Event properties | CustomDimensions column |
| Saved queries | Favorites / Saved queries |

## Tips

1. **Save queries** - Click "Save" to save frequently used queries
2. **Pin to dashboard** - Click "Pin to dashboard" for quick access
3. **Create alerts** - Set up alerts based on queries
4. **Export** - Export query results to CSV/Excel

## Quick Access

**Direct Link:**
```
https://portal.azure.com/#@/resource/subscriptions/c8d51cd3-afd4-4f93-8587-bf42566ad130/resourceGroups/myleague-rg/providers/Microsoft.Insights/components/myleague-insights-om63tv
```

