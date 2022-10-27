using Amazon.Lambda.Core;
using Amazon;
using Amazon.CostExplorer;
using Amazon.CostExplorer.Model;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MySimpleFunction;

public class Function
{
    private static readonly RegionEndpoint region = RegionEndpoint.EUCentral1;

    private static readonly string slackAPIToken = Environment.GetEnvironmentVariable("LAMBDA_COST_EXPLORER");
    private static readonly HttpClient client = new HttpClient();

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<string> FunctionHandler(Lambdainput input, ILambdaContext context)
    {
        var client = new AmazonCostExplorerClient(region);
        var iamClient = new AmazonIdentityManagementServiceClient();

        DateTime dt = DateTime.Now;

        var lastMonday = dt.AddDays(-7).ToString("yyyy-MM-dd");
        var lastSunday = dt.AddDays(-1).ToString("yyyy-MM-dd");
        var MondayTowWeeksAgo = dt.AddDays(-14).ToString("yyyy-MM-dd");
        var SundayWeeksAgo = dt.AddDays(-8).ToString("yyyy-MM-dd");

        var forecastStartDate = dt.ToString("yyyy-MM-dd"); ;
        var forecastEndDate = dt.AddDays(+7).ToString("yyyy-MM-dd");

        double totalLastWeekAmount = 0;
        double totalTwoWeeksAgoAmount = 0;

        var notificationData = new List<CostData>();

        string Unit = string.Empty;
        bool validCurrency = true;


        // Query the Cost Explorer API Forecast
        var forecastrequest = new GetCostForecastRequest()
        {
            Granularity = "DAILY",
            TimePeriod = new DateInterval()
            {
                Start = forecastStartDate,
                End = forecastEndDate
            },
            Metric = Metric.UNBLENDED_COST
        };

        // Query the Cost Explorer API Cost usage
        var request = new GetCostAndUsageRequest()
        {
            Granularity = "DAILY",
            GroupBy = { new GroupDefinition() { Key = "SERVICE", Type = GroupDefinitionType.DIMENSION } },
            TimePeriod = new DateInterval()
            {
                Start = MondayTowWeeksAgo,
                End = lastSunday
            },
            Metrics = { "UnblendedCost" }
        };

        var aliasRequest = new ListAccountAliasesRequest();




        //API resoponses
        var forecastResponse = await client.GetCostForecastAsync(forecastrequest);
        var response = await client.GetCostAndUsageAsync(request);
        var iamResponse = await iamClient.ListAccountAliasesAsync(aliasRequest);

        var alies = iamResponse.AccountAliases[0];

        foreach (var result in response.ResultsByTime)
        {
            foreach (var group in result.Groups)
            {
                foreach (var key in group.Keys)
                {
                    var amount = group.Metrics.FirstOrDefault().Value.Amount;

                    if (DateTime.Parse(result.TimePeriod.Start) < DateTime.Parse(lastMonday))
                    {
                        totalTwoWeeksAgoAmount += Double.Parse(amount);
                    }
                    else totalLastWeekAmount += Double.Parse(amount);
                 
                }

                if (!Unit.Equals(string.Empty) && !Unit.Equals(group.Metrics.FirstOrDefault().Value.Unit))
                    validCurrency = false;

                Unit = group.Metrics.FirstOrDefault().Value.Unit;
            }
        }

        notificationData.Add(new CostData(CostTypes.LastWeekCost, Math.Round(totalTwoWeeksAgoAmount, 4), Unit));
        notificationData.Add(new CostData(CostTypes.ThisWeekCost, Math.Round(totalLastWeekAmount, 4), Unit));
        notificationData.Add(new CostData(CostTypes.PercentageIncreaseDecrease, Math.Round(PercentageIncreaseDecrease(totalLastWeekAmount, totalTwoWeeksAgoAmount), 4), Unit));
        notificationData.Add(new CostData(CostTypes.ForecastCost, Math.Round(Double.Parse(forecastResponse.Total.Amount), 4), forecastResponse.Total.Unit));

        
        await PostMsgToSlack("C046PQ91ZDE", slackAPIToken, notificationData, validCurrency, alies);

        return "Ok";
    }

    public static double PercentageIncreaseDecrease(double lastWeek, double twoWeeksAgo)
    {
        var result = ((lastWeek - twoWeeksAgo) / twoWeeksAgo) * 100;

        return result;

    }


    public  async Task PostMsgToSlack(string channelName, string slackBotToken, List<CostData> notificationsData, bool validCurrency, string user)
    {

        string percentageValue;
        
        StringBuilder sb = new StringBuilder();

        if (notificationsData.Where(x => x.Type == CostTypes.PercentageIncreaseDecrease).FirstOrDefault().Value >= 0)
            percentageValue = "increase";
        else percentageValue = "decrease";

        sb.AppendLine($"Account: {user}");
        sb.AppendLine($"Cost of account on all services this week: { notificationsData[1].Value } { notificationsData[1].Currency }  (Percentage {percentageValue} = { notificationsData[2].Value }%)");
        sb.AppendLine($"Cost of account on all services last week: {notificationsData[0].Value} { notificationsData[1].Currency }");
        sb.AppendLine($"Forecast for the next week cost: {notificationsData[3].Value} { notificationsData[1].Currency }");

        if (!validCurrency)
        {
            sb.AppendLine($"************WARNING************");
            sb.AppendLine($"The currencies do not match!");
        }
           
        const string botendpoint = "https://slack.com/api/chat.postMessage";

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", slackBotToken);
        var postObject = new { channel = channelName, text = sb.ToString() };
        var json = JsonConvert.SerializeObject(postObject);
        Console.WriteLine(json);

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        

        var response = await client.PostAsync(botendpoint, content);
    }
}


public class Lambdainput
{
    public string test { get; set; }
}





