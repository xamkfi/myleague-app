// ============================================================================
// Monitoring & Alerting Module
// ============================================================================
// Deploys automatic problem detection with email alerts to the admin:
// - Action Group (email receiver)
// - Metric alerts: App Service health/errors/latency, plan CPU/memory,
//   PostgreSQL CPU/storage/failed connections, App Insights exceptions
// - Availability web test (optional, recommended for prod only)
// - Smart Detection (Failure Anomalies) routed to the action group
// - Monthly cost budget with email notifications at 80% / 100%
// ============================================================================

@description('Resource name prefix, e.g. myleague-prod')
param namePrefix string

@description('The Azure region for the availability web test (alerts themselves are global)')
param location string = resourceGroup().location

@description('The admin email address that receives all alerts')
param alertEmail string

@description('The resource ID of the App Service to monitor')
param appServiceId string

@description('The resource ID of the App Service Plan to monitor')
param appServicePlanId string

@description('The resource ID of the PostgreSQL Flexible Server to monitor')
param postgresServerId string

@description('The resource ID of the Application Insights component')
param appInsightsId string

@description('The name of the Application Insights component (used for the Smart Detection rule name)')
param appInsightsName string

@description('The hostname of the API used by the availability test, e.g. myleague-prod-api.azurewebsites.net')
param apiHostname string

@description('Deploy an external availability (uptime) test against /health/ready. Recommended for prod only to keep costs down.')
param enableAvailabilityTest bool = false

@description('Monthly cost budget for this resource group in USD. Email notifications at 80% and 100%.')
param monthlyBudgetAmount int = 35

@description('Start date of the budget period (first day of the current month). Do not override.')
param budgetStartDate string = utcNow('yyyy-MM-01')

@description('Tags to apply to created resources')
param tags object = {}

// ============================================================================
// Action Group - email to admin
// ============================================================================

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: '${namePrefix}-alerts-ag'
  location: 'Global'
  tags: tags
  properties: {
    groupShortName: 'MyLeague'
    enabled: true
    emailReceivers: [
      {
        name: 'AdminEmail'
        emailAddress: alertEmail
        useCommonAlertSchema: true
      }
    ]
  }
}

// ============================================================================
// App Service alerts
// ============================================================================

// Health check failing => the app is down or /health/ready is failing.
// HealthCheckStatus reports 100 when all instances are healthy.
resource healthCheckAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${namePrefix}-alert-health-check'
  location: 'global'
  tags: tags
  properties: {
    description: 'API health check (/health/ready) is failing - the application is down or unhealthy.'
    severity: 1
    enabled: true
    scopes: [appServiceId]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          name: 'HealthCheckStatus'
          metricNamespace: 'Microsoft.Web/sites'
          metricName: 'HealthCheckStatus'
          operator: 'LessThan'
          threshold: 100
          timeAggregation: 'Average'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

resource http5xxAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${namePrefix}-alert-http-5xx'
  location: 'global'
  tags: tags
  properties: {
    description: 'API is returning an elevated number of HTTP 5xx server errors.'
    severity: 2
    enabled: true
    scopes: [appServiceId]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          name: 'Http5xx'
          metricNamespace: 'Microsoft.Web/sites'
          metricName: 'Http5xx'
          operator: 'GreaterThan'
          threshold: 10
          timeAggregation: 'Total'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

resource responseTimeAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${namePrefix}-alert-response-time'
  location: 'global'
  tags: tags
  properties: {
    description: 'API average response time is above 5 seconds - performance degradation.'
    severity: 3
    enabled: true
    scopes: [appServiceId]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          name: 'HttpResponseTime'
          metricNamespace: 'Microsoft.Web/sites'
          metricName: 'HttpResponseTime'
          operator: 'GreaterThan'
          threshold: 5
          timeAggregation: 'Average'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// ============================================================================
// App Service Plan alerts (CPU / memory of the underlying VM)
// ============================================================================

resource planCpuAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${namePrefix}-alert-plan-cpu'
  location: 'global'
  tags: tags
  properties: {
    description: 'App Service Plan CPU is above 85% sustained - consider scaling up or investigating load.'
    severity: 3
    enabled: true
    scopes: [appServicePlanId]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          name: 'CpuPercentage'
          metricNamespace: 'Microsoft.Web/serverfarms'
          metricName: 'CpuPercentage'
          operator: 'GreaterThan'
          threshold: 85
          timeAggregation: 'Average'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

resource planMemoryAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${namePrefix}-alert-plan-memory'
  location: 'global'
  tags: tags
  properties: {
    description: 'App Service Plan memory is above 85% sustained - risk of restarts/OOM.'
    severity: 3
    enabled: true
    scopes: [appServicePlanId]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          name: 'MemoryPercentage'
          metricNamespace: 'Microsoft.Web/serverfarms'
          metricName: 'MemoryPercentage'
          operator: 'GreaterThan'
          threshold: 85
          timeAggregation: 'Average'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// ============================================================================
// PostgreSQL alerts
// ============================================================================

resource postgresCpuAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${namePrefix}-alert-postgres-cpu'
  location: 'global'
  tags: tags
  properties: {
    description: 'PostgreSQL CPU is above 90% sustained - database under heavy load.'
    severity: 2
    enabled: true
    scopes: [postgresServerId]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          name: 'cpu_percent'
          metricNamespace: 'Microsoft.DBforPostgreSQL/flexibleServers'
          metricName: 'cpu_percent'
          operator: 'GreaterThan'
          threshold: 90
          timeAggregation: 'Average'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// Storage filling up is the most common cause of a hard Postgres outage:
// the server goes read-only when the disk is full. Alert early at 80%.
resource postgresStorageAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${namePrefix}-alert-postgres-storage'
  location: 'global'
  tags: tags
  properties: {
    description: 'PostgreSQL storage is above 80% - increase storage before the server becomes read-only.'
    severity: 1
    enabled: true
    scopes: [postgresServerId]
    evaluationFrequency: 'PT15M'
    windowSize: 'PT30M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          name: 'storage_percent'
          metricNamespace: 'Microsoft.DBforPostgreSQL/flexibleServers'
          metricName: 'storage_percent'
          operator: 'GreaterThan'
          threshold: 80
          timeAggregation: 'Average'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

resource postgresFailedConnectionsAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${namePrefix}-alert-postgres-conn-failed'
  location: 'global'
  tags: tags
  properties: {
    description: 'PostgreSQL is rejecting connections - possible connection pool exhaustion or auth issues.'
    severity: 2
    enabled: true
    scopes: [postgresServerId]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          name: 'connections_failed'
          metricNamespace: 'Microsoft.DBforPostgreSQL/flexibleServers'
          metricName: 'connections_failed'
          operator: 'GreaterThan'
          threshold: 10
          timeAggregation: 'Total'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// ============================================================================
// Application Insights alerts
// ============================================================================

resource exceptionsAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${namePrefix}-alert-exceptions'
  location: 'global'
  tags: tags
  properties: {
    description: 'Elevated number of unhandled server exceptions in the API.'
    severity: 2
    enabled: true
    scopes: [appInsightsId]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          name: 'ServerExceptions'
          metricNamespace: 'microsoft.insights/components'
          metricName: 'exceptions/server'
          operator: 'GreaterThan'
          threshold: 10
          timeAggregation: 'Count'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// ============================================================================
// Smart Detection - Failure Anomalies (ML-based, no threshold tuning needed)
// ============================================================================
// Azure auto-creates this rule with App Insights; declaring it here takes it
// over so its alerts are routed to the admin action group.
// The name MUST be exactly 'Failure Anomalies - <app insights name>'.

resource failureAnomaliesRule 'microsoft.alertsManagement/smartDetectorAlertRules@2021-04-01' = {
  name: 'Failure Anomalies - ${appInsightsName}'
  location: 'global'
  tags: tags
  properties: {
    description: 'Detects abnormal rises in failed request rate using machine learning.'
    state: 'Enabled'
    severity: 'Sev3'
    frequency: 'PT1M'
    detector: {
      id: 'FailureAnomaliesDetector'
    }
    scope: [appInsightsId]
    actionGroups: {
      groupIds: [actionGroup.id]
    }
  }
}

// ============================================================================
// Availability (uptime) test - external "is the site up" signal
// ============================================================================

resource availabilityTest 'Microsoft.Insights/webtests@2022-06-15' = if (enableAvailabilityTest) {
  name: '${namePrefix}-availability'
  location: location
  tags: union(tags, {
    'hidden-link:${appInsightsId}': 'Resource'
  })
  kind: 'standard'
  properties: {
    SyntheticMonitorId: '${namePrefix}-availability'
    Name: '${namePrefix} health check'
    Description: 'Pings /health/ready every 5 minutes from multiple European regions.'
    Enabled: true
    Frequency: 300
    Timeout: 30
    Kind: 'standard'
    RetryEnabled: true
    Locations: [
      { Id: 'emea-nl-ams-azr' } // West Europe (Amsterdam)
      { Id: 'emea-gb-db3-azr' } // North Europe (Dublin)
      { Id: 'emea-fr-pra-edge' } // France Central (Paris)
    ]
    Request: {
      RequestUrl: 'https://${apiHostname}/health/ready'
      HttpVerb: 'GET'
      ParseDependentRequests: false
    }
    ValidationRules: {
      ExpectedHttpStatusCode: 200
      SSLCheck: true
      SSLCertRemainingLifetimeCheck: 7
    }
  }
}

resource availabilityAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = if (enableAvailabilityTest) {
  name: '${namePrefix}-alert-availability'
  location: 'global'
  tags: tags
  properties: {
    description: 'The API is unreachable from at least 2 of 3 external test locations - the site is down.'
    severity: 1
    enabled: true
    scopes: [
      availabilityTest.id
      appInsightsId
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.WebtestLocationAvailabilityCriteria'
      webTestId: availabilityTest.id
      componentId: appInsightsId
      failedLocationCount: 2
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// ============================================================================
// Cost budget - catches runaway spend
// ============================================================================

resource budget 'Microsoft.Consumption/budgets@2023-11-01' = {
  name: '${namePrefix}-budget'
  properties: {
    category: 'Cost'
    amount: monthlyBudgetAmount
    timeGrain: 'Monthly'
    timePeriod: {
      startDate: budgetStartDate
    }
    notifications: {
      actual80Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 80
        thresholdType: 'Actual'
        contactEmails: [alertEmail]
      }
      actual100Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 100
        thresholdType: 'Actual'
        contactEmails: [alertEmail]
      }
    }
  }
}

// ============================================================================
// Outputs
// ============================================================================

@description('The resource ID of the action group')
output actionGroupId string = actionGroup.id

@description('The name of the action group')
output actionGroupName string = actionGroup.name
