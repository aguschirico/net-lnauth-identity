data "azurerm_resource_group" "main" {
  name = var.RESOURCE_GROUP
}

data "azurerm_container_registry" "main" {
  name = "acrlnauthdemo"
  resource_group_name = "${var.RESOURCE_GROUP}"
}

data "azurerm_user_assigned_identity" "main" {
  name = "${var.USER_ASSIGNED_IDENTITY_NAME}"
  resource_group_name = "${var.RESOURCE_GROUP}"
}
