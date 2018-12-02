using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FrequencyTimeSheetFunctions
{
    public static class generateJWTTokenForTwilioVerification
    {
        [FunctionName("generateJWTTokenForTwilioVerification")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {

            // parse query parameter
            string phonenumber = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "phonenumber", true) == 0)
                .Value;

            if (phonenumber == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                phonenumber = data?.phonenumber;
            }

            //Check if the parameter is null
            if (phonenumber != null)
            {
                //This is the Twilio API Key
                string api_key = "vnRqCfsca8nP984DMlEixrb9Scj6Dzq6";
                var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(api_key));
                var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
                //Generate the Token Header
                var header = new JwtHeader(credentials);

                //Create the JWT Payload
                JwtPayload payload = new JwtPayload
                {

                    { "app_id", "149961"},
                    { "phone_number", phonenumber },
                    { "iat", System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
                };

                //Create security Token
                var secToken = new JwtSecurityToken(header, payload);
                var handler = new JwtSecurityTokenHandler();

                //Write token to string
                var tokenString = handler.WriteToken(secToken);

                //Create the JSON Object
                var jObject = new JObject
                {
                    { "jwt_token", tokenString }
                };
            
                string jwt_json = JsonConvert.SerializeObject(jObject, Formatting.None);

                //Send the JSON Object back to the client. Cheers
                return req.CreateResponse(HttpStatusCode.OK, jwt_json);

            }

            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a phone number to generate the JWT for verification");

            }


           
        }
    }
}
