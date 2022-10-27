data "aws_iam_policy_document" "this_trust_lambda" {
	statement {
		actions = ["sts:AssumeRole"]
		effect = "Allow"
		principals {
			identifiers = ["lambda.amazonaws.com"]
			type = "Service"
		}
	}
}


data "aws_iam_policy_document" "this_lambda" {
  statement {
    effect = "Allow"

    actions = [
      "ce:Get*",
      "ce:Describe*",
      "ce:List*"
    ]

    resources = ["*"]
  }
  statement {
    effect = "Allow"

    actions = [
       "logs:CreateLogGroup" 
    ]

    resources = ["arn:aws:logs:eu-central-1:993619277050:*"]
  }
  statement {
    effect = "Allow"

    actions = [
       "logs:CreateLogStream",
        "logs:PutLogEvents"
    ]

    resources = ["arn:aws:logs:eu-central-1:993619277050:log-group:/aws/lambda/query-cost-data-lambda:*"]
  }
  statement {
    effect = "Allow"

    actions = [
       "iam:ListAccountAliases"
    ]

    resources = ["*"]
    
  }
}

resource "null_resource" "lambda_build" {
  triggers = {
    always_run = timestamp()
  }
  provisioner "local-exec" {
    command = "cd ${path.module}/../lambda/MySimpleFunction/src/MySimpleFunction && dotnet publish --use-current-runtime -c Release -o Output"
  }
}

data "archive_file" "lambda_zip" {
  depends_on = [null_resource.lambda_build]
  source_dir = "${path.module}/../lambda/MySimpleFunction/src/MySimpleFunction/Output"
  type = "zip"
  output_path = "${path.module}/../lambda/MySimpleFunction/src/MySimpleFunction/Output/cost_export_lambda.zip"
}

