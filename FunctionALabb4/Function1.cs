using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace FunctionALabb4
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string mode = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "mode", true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            mode = mode ?? data?.pictureURL;

            if(mode== "viewReviewQueue")
            {
            var picture = GetPicture(mode);
            return req.CreateResponse(HttpStatusCode.OK, picture, "application/json");

            }
            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please enter mode.");
            }



        }

        private static List<Picture> GetPicture(string email)
        {

            string EndpointUrl = "https://picturesdb.documents.azure.com:443/";
            string PrimaryKey = "cHRKIwWfOVFQOxDG8h33OIr0YoIpWZQRe3G1DF7ha43ZfxVhr7Ev8wdc0wgvMUpDoCWsI50dYrOlpswocncohg==";
            string databaseName = "PicturesDB";
            string collectionName = "Pending pictures";

            var client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

            var query = client.CreateDocumentQuery<Picture>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                    "SELECT * FROM Pending pictures",
                    queryOptions);

            var picture = query.ToList();
            return picture;

        }
    }
}