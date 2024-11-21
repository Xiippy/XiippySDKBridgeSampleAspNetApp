using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Xiippy.POSeComSDK.Light.Models;
using Xiippy.POSeComSDK.Light.XiippySDKBridgeApiClient;
using XiippySDKBridgeSampleAspNetApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XiippySDKBridgeSampleAspNetApp.Pages
{
    public class RefundModel : PageModel
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



        public RefundModel(ILogger<IndexModel> logger, IOptions<Config> TheConfig)
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
                    await RefundPaymentAsync(StatementID, StatementTimeStamp, Amount);
                }
                catch (Exception x)
                {
                    // show the error message, if any
                    ErrorMessage = x.ToString();
                }


            }

            return Page();
        }

        /// <summary>
        /// Refunds a payment with RandomStatementID and StatementTimeStamp
        /// </summary>
        /// <param name="RandomStatementID"></param>
        /// <param name="StatementTimeStamp"></param>
        /// <param name="AmountInDollars">Optional, when null passed, refunds the full amount otherwise has to be smaller than the transaction amount</param>
        /// <returns></returns>
        public async Task RefundPaymentAsync(string RandomStatementID, string StatementTimeStamp, float? AmountInDollars)
        {


            RefundCardPaymentRequest req = new RefundCardPaymentRequest
            {

                MerchantGroupID = config.MerchantGroupID,
                MerchantID = config.MerchantID,
                RandomStatementID = RandomStatementID,
                StatementTimestamp = StatementTimeStamp,
                AmountInDollars= AmountInDollars
            };


            // instantiate the SDK objects and feed them with the right parameters
            XiippySDKBridgeApiClient client = new XiippySDKBridgeApiClient(true, config.Config_ApiSecretKey, config.Config_BaseAddress, config.MerchantID, config.MerchantGroupID);
            // initiate the payment
            var response = await client.RefundCardPayment(req);
            SuccessMessage = @$"Refund was processed successfully. Response:\r\n{System.Text.Json.JsonSerializer.Serialize(response)}";

        }
    }
}
