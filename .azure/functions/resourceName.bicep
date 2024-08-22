@description('This function generates a unique string based on the subscription ID and resource group ID')
@export()
func uniqueStringBySubscriptionAndResourceGroup() string => uniqueString('${subscription().id}${resourceGroup().id}')

@description('This function generates a unique resource name by appending a unique string to the given name, ensuring the total length does not exceed the specified limit. It also ensures that the name is always postfixed with the full length of the unique string, which is 13 characters plus a dash.')
// Example:
// uniqueResourceName(name: 'my-resource', limit: 50) => 'my-resource-1234567890123'
// Example:
// uniqueResourceName(name: 'my-resource', limit: 20) => 'my-res-1234567890123'
@export()
func uniqueResourceName(name string, limit int) string =>
  '${take(name, limit - 13 - 1)}-${uniqueStringBySubscriptionAndResourceGroup()}'
