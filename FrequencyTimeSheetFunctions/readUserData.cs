using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace FrequencyTimeSheetFunctions
{
    public static class readUserData
    {
        [FunctionName("readUserData")]
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

            if (phonenumber == null)
            { return req.CreateResponse(HttpStatusCode.BadRequest, "Please enter a phone number to process"); }

            else
            {

                try
                {
                    // Build connection string
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                    {
                        DataSource = "tcp:frequency-timesheet.database.windows.net,1433",
                        UserID = "jasonb",
                        Password = "Freq4899!",
                        InitialCatalog = "frequency-timesheet-db"
                    };

                    string output = "";

                    // Connect to SQL
                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {
                        connection.Open();
                        string sql = "SELECT * FROM UserTable";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            SqlDataReader dataReader = await command.ExecuteReaderAsync();
                            while (await dataReader.ReadAsync())
                            {
                                output = output + dataReader.GetValue(0) + " and : " + dataReader.GetValue(1) + "and : " + dataReader.GetValue(2) + "and : " + dataReader.GetValue(3);
                            }
                            connection.Close();
                            return req.CreateResponse(HttpStatusCode.OK, "Test This Output : " + output);
                        }


                    }



                }
                catch(SqlException e)
                {
                    return req.CreateResponse(HttpStatusCode.OK, "Number of rows returned : ");
                }


            }

                




        }
    }
}
