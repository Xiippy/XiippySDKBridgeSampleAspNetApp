using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        // these are the config items that the config page of the XiippySDKBridge generates at /Config.
        // A POS or ecommernce system must get these from an admin and save them for the current merchant.
        //at page load, these values must be loaded from configuration repository and initialized!
        // for ease of use, they have been hard-coded here which is not to be practiced!

        private Config config;




        [BindProperty]
        public string XiippyFrameUrl { get; set; }

        [BindProperty]
        public string ErrorText { get; set; }

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IOptions<Config> TheConfig)
        {
            config = TheConfig.Value;
            _logger = logger;
        }

        /// <summary>
        /// Page initialization
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // try initiating the payment and loading the payment card screen
                XiippyFrameUrl = await InitiatePaymentAndGetiFrameUrlAsync();

            }
            catch (Exception x)
            {
                // show the error message, if any
                ErrorText = x.ToString();
            }

            return Page();
        }


        /// <summary>
        /// Initiate the payment flow and get the URL to be loaded in the iFrame
        /// </summary>
        /// <returns></returns>
        public async Task<string> InitiatePaymentAndGetiFrameUrlAsync()
        {
            // depending on the basket, shipping and billing address entered, as well as amounts, the payment is initialized:
            string StatementID = Guid.NewGuid().ToString();
            string UniqueStatementID = Guid.NewGuid().ToString();
            PaymentProcessingRequest req = new PaymentProcessingRequest
            {
                MerchantGroupID = config.MerchantGroupID,
                MerchantID = config.MerchantID,
                Amount = 2.5F,
                Currency = "aud",
                ExternalUniqueID = UniqueStatementID,
                IsPreAuth = false,
                IsViaTerminal = false,
                // customer is optional
                Customer = new PaymentRecordCustomer
                {
                    CustomerAddress = new PaymentRecordCustomerAddress
                    {
                        CityOrSuburb = "Brisbane",
                        Country = "Australia",
                        FullName = "Full StatementID",
                        Line1 = "100 Queen St",
                        PhoneNumber = "+61400000000",
                        PostalCode = "4000",
                        StateOrPrivince = "Qld"
                    },
                    CustomerEmail = "dont@contact.me",
                    CustomerName = "Full StatementID",
                    CustomerPhone = "+61400000000"

                },
                IssuerStatementRecord = new IssuerStatementRecord
                {
                    // this could be a different id than RandomStatementID which is a Xiippy identifier
                    UniqueStatementID = UniqueStatementID,
                    RandomStatementID = StatementID,
                    StatementCreationDate = DateTime.Now.ToUniversalTime().Ticks.ToString(),
                    StatementTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmss"),

                    Description = "Test transaction #1",
                    DetailsInBodyBeforeItems = "Description on the receipt before items",
                    DetailsInBodyAfterItems = "Description on the receipt after items",
                    DetailsInFooter = "Description on the footer",
                    DetailsInHeader = "Description on the header",
                    StatementItems = new List<StatementItem>
                    {
                        new StatementItem
                        {

                            Description = "Description",
                            UnitPrice = 11,
                            Url = "Url",
                            Quantity = 1,
                            Identifier = "Identifier",
                            Tax=1,
                            TotalPrice=11


                        },
                        new StatementItem
                        {
                            Description = "Description2",
                            UnitPrice = 33,
                            Url = "Url2",
                            Quantity =1,
                            Identifier = "Identifier2",
                            Tax=3,
                            TotalPrice=33,

                        }


                    },

                    TotalAmount = 44,
                    TotalTaxAmount = 4
                }



            };


            // instantiate the SDK objects and feed them with the right parameters
            XiippySDKBridgeApiClient client = new XiippySDKBridgeApiClient(true, config.Config_ApiSecretKey, config.Config_BaseAddress, config.MerchantID, config.MerchantGroupID);
            // initiate the payment
            var response = await client.InitiateXiippyPayment(req);

            string QueryString = Utils.Utils.BuildQueryString(new Dictionary<string, string>
            {
                {Constants.QueryStringParam_rsid,response.RandomStatementID },
                {Constants.QueryStringParam_sts,response.StatementTimeStamp },
                {Constants.QueryStringParam_ca,response.ClientAuthenticator },
                {Constants.QueryStringParam_spw,"true" }, // show plain view
                {Constants.QueryStringParam_MerchantID,  config.MerchantID},
                {Constants.QueryStringParam_MerchantGroupID,  config.MerchantGroupID}, // important
                {Constants.QueryStringParam_cs,response.ClientSecret },
                {Constants.QueryStringParam_ShowLongXiippyText,"true" }, // show the long xiippy description text
                

            });

            string FullPaymentPageUrl = $"{config.Config_BaseAddress}/Payments/Process?{QueryString}";
            Debug.WriteLine($"The payment page can not be browsed at '{FullPaymentPageUrl}'");
            return FullPaymentPageUrl;
        }





    }
}
