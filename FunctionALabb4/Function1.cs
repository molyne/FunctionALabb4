using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace FunctionALabb4
{
    public static class Function1
    {
        static string EndpointUrl = "https://picturesdb.documents.azure.com:443/";
        static string PrimaryKey = "cHRKIwWfOVFQOxDG8h33OIr0YoIpWZQRe3G1DF7ha43ZfxVhr7Ev8wdc0wgvMUpDoCWsI50dYrOlpswocncohg==";
        static DocumentClient client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);


        static string databaseName = "PicturesDB";
        static string pendingCollection = "Pending pictures";
        static string reviewedCollection = "Reviewed pictures";

        //connections to collections
        static Uri pendingCollectionLink = UriFactory.CreateDocumentCollectionUri(databaseName, pendingCollection);
        static Uri reviewedCollectionLink = UriFactory.CreateDocumentCollectionUri(databaseName, reviewedCollection);


        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string mode = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "mode", true) == 0)
                .Value;

            string id = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "id", true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            mode = mode ?? data?.pictureURL;

            id = id ?? data?._id;

       

            if(mode== "viewReviewQueue")
            {
            var picture = GetPicture(mode);
            return req.CreateResponse(HttpStatusCode.OK, picture, "application/json");

            }
            if(mode== "approve" && id!=null)
            {
                var approvePicture = ApprovePicture(id);
                return req.CreateResponse(HttpStatusCode.OK, approvePicture, "application/json");

            }
            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please enter mode.");
            }

        }

        private static List<Picture> GetPicture(string email)
        {
            
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

            var printPending = client.CreateDocumentQuery<Picture>(
                pendingCollectionLink, "SELECT * FROM Pending pictures",
                    queryOptions);

            var picture = printPending.ToList();
            return picture;

        }
        private static async Task<string> ApprovePicture(string selectedId)
        {
            //hittar id man valt i webbläsaren
            var picture = client.CreateDocumentQuery<Picture>(pendingCollectionLink)
                               .Where(r => r._id == selectedId)
                              .AsEnumerable()
                              .SingleOrDefault();

            //lägger till valda dokumentet i reviewed pictures
            ResourceResponse<Document> response  = await client.CreateDocumentAsync(reviewedCollectionLink, picture);

            var createdDocument = response.Resource;

            Console.WriteLine("Document with id {0} created", createdDocument.Id);
            Console.WriteLine("Request charge of operation: {0}", response.RequestCharge);

            //tar bort det valda dokumentet från pending pictures
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, pendingCollection, selectedId));


            //ändra så den returner om det lyckades eller inte
            return $"Picture with id {createdDocument.Id} approved";
        }
       


    }
}