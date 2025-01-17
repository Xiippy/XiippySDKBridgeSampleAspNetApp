using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xiippy.POSeComSDK.Light.Models;
using Xiippy.POSeComSDK.Light.XiippySDKBridgeApiClient;
using XiippySDKBridgeSampleAspNetApp.Models;

namespace XiippySDKBridgeSampleAspNetApp.Pages
{
    public class GetPaymentStatusModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Statement ID is required.")]
        public string StatementID { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Statement TimeStamp is required.")]
        public string StatementTimeStamp { get; set; }


        [BindProperty]
        public float? Amount { get; set; }


        public string SuccessMessage { get; set; }


        public string ErrorMessage { get; set; }





        private Config config;
        private readonly ILogger<IndexModel> _logger;

        public GetPaymentStatusModel(ILogger<IndexModel> logger, IOptions<Config> TheConfig)
        {
            config = TheConfig.Value;
            _logger = logger;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await GetPaymentStatusAsync(StatementID, StatementTimeStamp);
                }
                catch (Exception x)
                {
                    // show the error message, if any
                    ErrorMessage = x.ToString();
                }


            }

            return Page();
        }


        public async Task GetPaymentStatusAsync(string RandomStatementID, string StatementTimeStamp)
        {


            GetPaymentStatusRequest req = new GetPaymentStatusRequest
            {

                MerchantGroupID = config.MerchantGroupID,
                MerchantID = config.MerchantID,
                RandomStatementID = RandomStatementID,
                Timestamp = StatementTimeStamp,
            };


            // instantiate the SDK objects and feed them with the right parameters
            XiippySDKBridgeApiClient client = new XiippySDKBridgeApiClient(true, config.Config_ApiSecretKey, config.Config_BaseAddress, config.MerchantID, config.MerchantGroupID);
            // initiate the payment
            var response = await client.GetPaymentStatus(req);
            SuccessMessage = @$"Payment status was retrieved successfully. Response:\r\n{System.Text.Json.JsonSerializer.Serialize(response)}";

        }
    }
}
