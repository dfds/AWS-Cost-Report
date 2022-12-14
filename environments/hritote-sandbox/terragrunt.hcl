# Terragrunt will copy the Terraform configurations specified by the source parameter, along with any files in the
# working directory, into a temporary folder, and execute your Terraform commands in that folder.
terraform {
  source = "../..//module"
}

# Include all settings from the root terragrunt.hcl file
include {
  path = find_in_parent_folders()
}

inputs = {
    lambda_dir = "/mnt/c/Users/hritote.ladm/repos/aws_cost_explorer"
    aws_region = "eu-central-1"
}