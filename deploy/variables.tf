variable "RESOURCE_GROUP" {
  description = "The Resource group where to deploy"
}

variable "ENVIRONMENT" {
  default = "dev"
  description = "The environment where to deploy"
}

variable "POSTGRES_SERVER_SKU" {
  default = "B_Standard_B1ms"
  description = "SKU for the Postgres Server"
}

variable "USER_ASSIGNED_IDENTITY_NAME" {
  description = "User Assigned Identity name to use from the container environment"
}

variable "POSTGRES_ADMIN_USERNAME" {
  description = "Username for the Postgres Administrator user"
}

variable "POSTGRES_ADMIN_PASSWORD" {
  description = "Password for the Postgres Administrator user"
}

variable "PRODUCT_NAME" {
  description = "Product name"
}

variable "VERSION_TAG" {
  description = "Version tag"
}

variable "AUTH_SECRET_KEY" {
  description = "Secret key for the authentication"
}