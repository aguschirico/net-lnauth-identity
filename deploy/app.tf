
resource "azurerm_log_analytics_workspace" "main" {
  name                = "ws${var.PRODUCT_NAME}"
  location            = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_container_app_environment" "main" {
  name                       = "env${var.PRODUCT_NAME}"
  location                   = data.azurerm_resource_group.main.location
  resource_group_name        = data.azurerm_resource_group.main.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
}

resource "azurerm_container_app" "main" {
  name                         = "app-${var.PRODUCT_NAME}"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.main.name
  revision_mode                = "Single"
  identity {
    type         = "UserAssigned"
    identity_ids = [data.azurerm_user_assigned_identity.main.id]
  }

  ingress {
    external_enabled = true
    target_port = 80
    traffic_weight {
      latest_revision = true
      percentage = 100
    }
  }
  
  registry {
    server   = data.azurerm_container_registry.main.login_server
    identity = data.azurerm_user_assigned_identity.main.id
  }
  
  template {
    min_replicas = 1
    container {
      name   = "${var.PRODUCT_NAME}app"
      image  = "${data.azurerm_container_registry.main.login_server}/${var.PRODUCT_NAME}:${var.VERSION_TAG}"
      cpu    = 0.25
      memory = "0.5Gi"

      readiness_probe {
        transport = "HTTP"
        port      = 80
        path = "status"
      }

      liveness_probe {
        transport = "HTTP"
        port      = 80
        path = "status"
      }

      startup_probe {
        transport = "HTTP"
        port      = 80
        path = "status"
      }
      
      env {
        name = "ASPNETCORE_HTTP_PORTS"
        value = "80"
      }
      env {
        name = "Auth__Issuer"
        value = "${var.PRODUCT_NAME}"
      }
      env {
        name = "Auth__Audience"
        value = var.ENVIRONMENT
      }
      env {
        name = "Auth__TokenExpireSeconds"
        value = 3600
      }
      env {
        name = "Auth__TokenExpireSeconds"
        value = 3600
      }
      env {
        name = "Auth__RefreshTokenExpireSeconds"
        value = 25200
      }
      env {
        name = "Auth__SecretKey"
        value = "${var.AUTH_SECRET_KEY}"
      }
      env {
        name = "ConnectionStrings__AppDbContext"
        value = "Server=${azurerm_postgresql_flexible_server.postgres_server.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.postgres_db.name};User Id=${var.POSTGRES_ADMIN_USERNAME};Password=${var.POSTGRES_ADMIN_PASSWORD};"
      }
      env {
        name = "ApplicationInsights__ConnectionString"
        value = "${azurerm_application_insights.insights.connection_string}"
      }
    }
  }
}