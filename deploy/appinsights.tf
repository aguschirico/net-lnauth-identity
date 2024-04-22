resource "azurerm_application_insights" "insights" {
  name                     = "appi-${var.PRODUCT_NAME}"
  location                 = data.azurerm_resource_group.main.location
  resource_group_name      = data.azurerm_resource_group.main.name
  workspace_id             = azurerm_log_analytics_workspace.main.id
  application_type         = "web"
}
