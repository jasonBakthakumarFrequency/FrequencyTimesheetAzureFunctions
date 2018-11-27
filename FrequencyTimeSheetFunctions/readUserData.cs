using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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



                    

                    // Connect to SQL
                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {
                        connection.Open();
                        
                        string sql = "SELECT UserTable.UserName, UserTable.PhoneNumber, ProjectTable.ProjectName, ContractorTable.ContractorName, JobTable.JobName, JobTable.JobDescription FROM UserTable, ProjectTable, ContractorTable, JobTable, JobAssignTable WHERE UserTable.UserID = JobAssignTable.UserID AND " +
                            "JobAssignTable.JobID = JobTable.JobID AND JobTable.ProjectID = ProjectTable.ProjectID AND ProjectTable.ContractorID = ContractorTable.ContractorID AND UserTable.PhoneNumber = '" + phonenumber + "'";

                        

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            SqlDataReader dataReader = await command.ExecuteReaderAsync();


                            Dictionary<string, object> goodresult = new Dictionary<string, object>();

                            List<JObject> jObjects = new List<JObject>();

                            while (await dataReader.ReadAsync())
                            {
                                JObject jobject = new JObject
                                {
                                    { "UserName", JToken.FromObject(dataReader.GetValue(0)) },
                                    { "PhoneNumber", JToken.FromObject(dataReader.GetValue(1)) },
                                    { "ProjectName",  JToken.FromObject(dataReader.GetValue(2)) },
                                    { "ContractorName", JToken.FromObject(dataReader.GetValue(3)) },
                                    { "JobName", JToken.FromObject(dataReader.GetValue(4)) },
                                    { "JobDescription", JToken.FromObject(dataReader.GetValue(5)) }
                                };

                                jObjects.Add(jobject);
                            }

                            string json = JsonConvert.SerializeObject(jObjects, Formatting.Indented);
                            connection.Close();
                            return req.CreateResponse(HttpStatusCode.OK, json);
                        }


                    }



                }
                catch(SqlException e)
                {
                    return req.CreateResponse(HttpStatusCode.OK, "A runtime Exception occured :  " + e.Message);
                }


            }

                




        }
    }
}
