using MongoDB.Driver;
using MongoDB.Bson;

namespace LifeCicklsService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "mongodb://localhost:55000/";
            Console.WriteLine("Hello, World!");
            //var connectionString = Environment.GetEnvironmentVariable("MONGODB_URI");
            if (connectionString == null)
            {
                Console.WriteLine("You must set your 'MONGODB_URI' environment variable. To learn how to set it, see https://www.mongodb.com/docs/drivers/csharp/current/quick-start/#set-your-connection-string");
                Environment.Exit(0);
            }
            var client = new MongoClient(connectionString);
           
            // Send a ping to confirm a successful connection
            try
            {
                var admin = client.GetDatabase("admin");
                admin.CreateCollection("test");
                Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }
    }
}
