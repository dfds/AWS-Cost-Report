terraform {
  backend "s3" {}
}


provider "aws" {
  # default
  region  = var.aws_region
  version = "~> 4.31.0"
  max_retries = 3
}

# ---------------------------------------------------------------------------------------------------------------------
# CREATE THE LAMBDA FUNCTION
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_lambda_function" "this_lambda" {
  function_name = var.base_name
  handler = "MySimpleFunction::MySimpleFunction.Function::FunctionHandler"
  runtime = "dotnet6"
  timeout = 50
  filename = "${path.module}/../lambda/MySimpleFunction/src/MySimpleFunction/Output/cost_export_lambda.zip"
  source_code_hash = data.archive_file.lambda_zip.output_base64sha256
  role = aws_iam_role.this_lambda.arn
  memory_size = 512
  environment {
    variables = {
      LAMBDA_COST_EXPLORER = var.slack_token
    }
  }

}



# ---------------------------------------------------------------------------------------------------------------------
# CREATE AN IAM ROLE FOR THE LAMBDA FUNCTION
# ---------------------------------------------------------------------------------------------------------------------


resource "aws_iam_role" "this_lambda" {
  assume_role_policy = data.aws_iam_policy_document.this_trust_lambda.json
  description = "Role for ${var.base_name} lambda"
  name = "${var.base_name}-lambda"
  path = "/"
}


# ---------------------------------------------------------------------------------------------------------------------
# GIVE THE LAMBDA FUNCTION PERMISSIONS TO LOG TO CLOUDWATCH
# ---------------------------------------------------------------------------------------------------------------------

resource "aws_iam_role_policy" "this_lambda" {
  name = "${var.base_name}-lambda"
  role = aws_iam_role.this_lambda.id
  policy = data.aws_iam_policy_document.this_lambda.json
}