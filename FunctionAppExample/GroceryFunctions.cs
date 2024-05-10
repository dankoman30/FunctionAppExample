using MySql.Data.MySqlClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace FunctionAppExample
{
    public class GroceryFunctions
    {
        private readonly ILogger _logger;

        public GroceryFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GroceryFunctions>();
        }

        public static string GetConnectionString()
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = Environment.GetEnvironmentVariable("hostname"),
                Database = Environment.GetEnvironmentVariable("database"),
                UserID = Environment.GetEnvironmentVariable("user_name"),
                Password = Environment.GetEnvironmentVariable("password"),
                SslMode = MySqlSslMode.Required,
            };
            return builder.ConnectionString;
        }

        [Function("CreateGroceryItem")]
        public async Task<HttpResponseData> CreateGroceryItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "grocery/create")] HttpRequestData req,
            FunctionContext executionContext)
        {

            _logger.LogInformation("Creating a new grocery item.");
            var response = req.CreateResponse();
            var connectionString = GetConnectionString();

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var newItem = JsonConvert.DeserializeObject<GroceryItem>(requestBody);
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand($"INSERT INTO GroceryItems (ItemName, Quantity) VALUES ('{newItem.ItemName}', {newItem.Quantity});", conn);
                    cmd.ExecuteNonQuery();
                }
                response.WriteString("Grocery item created successfully.");
                response.StatusCode = HttpStatusCode.Created;
            }
            catch (Exception ex)
            {
                response.WriteString($"Error creating grocery item: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        [Function("GetGroceryItem")]
        public async Task<HttpResponseData> GetGroceryItem(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "grocery/get/{id?}")] HttpRequestData req,
            FunctionContext executionContext, string id)
        {

            _logger.LogInformation("Fetching grocery item(s).");

            var response = req.CreateResponse();
            var connectionString = GetConnectionString();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var query = string.IsNullOrEmpty(id) ? "SELECT * FROM GroceryItems;" : $"SELECT * FROM GroceryItems WHERE Id = {id};";
                    var cmd = new MySqlCommand(query, conn);
                    var reader = cmd.ExecuteReader();
                    var items = new List<GroceryItem>();
                    while (reader.Read())
                    {
                        items.Add(new GroceryItem
                        {
                            Id = reader.GetInt32("Id"),
                            ItemName = reader.GetString("ItemName"),
                            Quantity = reader.GetInt32("Quantity"),
                            IsPurchased = reader.GetBoolean("IsPurchased")
                        });
                    }
                    response.WriteString(JsonConvert.SerializeObject(items));
                }
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.WriteString($"Error fetching grocery items: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }


        [Function("UpdateGroceryItem")]
        public async Task<HttpResponseData> UpdateGroceryItem(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "grocery/update/{id}")] HttpRequestData req,
            FunctionContext executionContext, string id)
        {

            _logger.LogInformation("Updating grocery item.");

            var response = req.CreateResponse();
            var connectionString = GetConnectionString();

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updateItem = JsonConvert.DeserializeObject<GroceryItem>(requestBody);
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand($"UPDATE GroceryItems SET ItemName = '{updateItem.ItemName}', Quantity = {updateItem.Quantity}, IsPurchased = {updateItem.IsPurchased} WHERE Id = {id};", conn);
                    cmd.ExecuteNonQuery();
                }
                response.WriteString("Grocery item updated successfully.");
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.WriteString($"Error updating grocery item: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }


        [Function("DeleteGroceryItem")]
        public async Task<HttpResponseData> DeleteGroceryItem(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "grocery/delete/{id}")] HttpRequestData req,
            FunctionContext executionContext, string id)
        {

            _logger.LogInformation("Deleting grocery item.");

            var response = req.CreateResponse();
            var connectionString = GetConnectionString();

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand($"DELETE FROM GroceryItems WHERE Id = {id};", conn);
                    cmd.ExecuteNonQuery();
                }
                response.WriteString("Grocery item deleted successfully.");
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.WriteString($"Error deleting grocery item: {ex.Message}");
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }



    }
}
