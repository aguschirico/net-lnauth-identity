resource "azurerm_postgresql_flexible_server" "postgres_server" {
  name                   = "pg-${var.PRODUCT_NAME}"
  resource_group_name    = data.azurerm_resource_group.main.name
  location               = data.azurerm_resource_group.main.location
  version                = "15"
  administrator_login    = var.POSTGRES_ADMIN_USERNAME
  administrator_password = var.POSTGRES_ADMIN_PASSWORD
  storage_mb             = 32768
  sku_name               = var.POSTGRES_SERVER_SKU
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "firewall_azure" {
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
  name                = "allow_all_services_in_azure"
  server_id         = azurerm_postgresql_flexible_server.postgres_server.id
}

resource "azurerm_postgresql_flexible_server_database" "postgres_db" {
  name      = "${var.PRODUCT_NAME}-db"
  server_id = azurerm_postgresql_flexible_server.postgres_server.id
  collation = "en_US.utf8"
  charset   = "utf8"
}