variable "aws_region" {
  type = string
}

variable "base_name" {
  type = string
  default = "aws-weekly-cost-explorer"
}

variable "lambda_dir" {
  type = string
}

variable "slack_token" {
  type = string
}