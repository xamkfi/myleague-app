// ============================================================================
// Azure Communication Services - Email with Azure-managed Domain
// ============================================================================
// Provisions:
// - Email Communication Service
// - Azure-managed email domain (auto-verified, suitable for dev/test)
// - DoNotReply sender username
// - Communication Service linked to the email domain
// ============================================================================

@description('The name of the Communication Service resource')
param name string

@description('The name of the Email Service resource')
param emailServiceName string

@description('Tags to apply to the resources')
param tags object = {}

// ============================================================================
// Email Communication Service
// ============================================================================

resource emailService 'Microsoft.Communication/emailServices@2023-03-31' = {
  name: emailServiceName
  location: 'global'
  tags: tags
  properties: {
    dataLocation: 'Europe'
  }
}

// ============================================================================
// Azure-managed Email Domain (auto-verified)
// ============================================================================

resource emailDomain 'Microsoft.Communication/emailServices/domains@2023-03-31' = {
  parent: emailService
  name: 'AzureManagedDomain'
  location: 'global'
  tags: tags
  properties: {
    domainManagement: 'AzureManaged'
    userEngagementTracking: 'Disabled'
  }
}

// ============================================================================
// Sender Username (DoNotReply)
// ============================================================================

resource senderUsername 'Microsoft.Communication/emailServices/domains/senderUsernames@2023-03-31' = {
  parent: emailDomain
  name: 'donotreply'
  properties: {
    username: 'DoNotReply'
    displayName: 'MyLeague'
  }
}

// ============================================================================
// Communication Service (linked to the email domain)
// ============================================================================

resource communicationService 'Microsoft.Communication/communicationServices@2023-03-31' = {
  name: name
  location: 'global'
  tags: tags
  properties: {
    dataLocation: 'Europe'
    linkedDomains: [
      emailDomain.id
    ]
  }
}

// ============================================================================
// Outputs
// ============================================================================

@description('The connection string for the Communication Service')
output connectionString string = communicationService.listKeys().primaryConnectionString

@description('The sender email address (DoNotReply@<azure-managed-domain>)')
output senderAddress string = 'DoNotReply@${emailDomain.properties.mailFromSenderDomain}'

@description('The name of the Communication Service')
output name string = communicationService.name

@description('The name of the Email Service')
output emailServiceName string = emailService.name
