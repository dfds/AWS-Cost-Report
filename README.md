
# :warning: Repository not maintained :warning:

Please note that this repository is currently archived, and is no longer being maintained.

- It may contain code, or reference dependencies, with known vulnerabilities
- It may contain out-dated advice, how-to's or other forms of documentation

The contents might still serve as a source of inspiration, but please review any contents before reusing elsewhere.


# slack_users_cleanup
 The program send message to slack channel with information about the last and this week aws services cost and a forecast for the next week cost.
## Initial Adjustments 
Set the following environment variables: 
| Variable| Description |
| :---: | :---: |
| LAMBDA_COST_EXPLORER | Generate Token using Slack API/ OAuth Scope: channels:read, chat:write, chat:write.public |
| :---: | :---: |


## Instruction
  - navigate to the project folder
  - Run TF_VAR_slack_token=CHANGEME  terragrunt run-all --terragrunt-source-update --terragrunt-working-dir environments/hritote-sandbox apply
  
  
