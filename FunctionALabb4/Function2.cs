using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FunctionALabb4
{
    public static class Function2
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

        [FunctionName("Function2")]
        public static void Run([TimerTrigger("*/20 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {

            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var pictures = client.CreateDocumentQuery<Picture>(pendingCollectionLink)
                               .Where(r => r.PictureURL.EndsWith(".png"))
                               .AsEnumerable();
            foreach(var p in pictures)
            {

                client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, pendingCollection, p._id));

                client.CreateDocumentAsync(reviewedCollectionLink, p);
                    
                    
                    //createdoc.
            }

            //var queue = new Queue<Picture>(picture);

            //for (int i = 0; i < queue.Count; i++)
            //{
            //    //client.CreateDocumentAsync(reviewedCollectionLink, queue.Dequeue());

            //}


        }
    }
}
