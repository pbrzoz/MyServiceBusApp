using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace ServiceBusReceiver
{
    public class Program
    {
        static string connectionString = "";
        static string serviceBusConnectionString = "";
        static string queueName = "insertQueue";

        static ServiceBusClient client;
        static ServiceBusProcessor processor;

        //Ładowanie i wypisywanie wiadomości
        static async Task MessageHandler(ProcessMessageEventArgs args)
        {            
            string body = Encoding.UTF8.GetString(args.Message.Body);
            Console.WriteLine(body);
            User user = JsonSerializer.Deserialize<User>(body);
            Console.WriteLine($"RECEIVER - User received: {user!.Id}, {user.Name}, {user.LastName}, {user.Email}, {user.Age}");
           
            
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@Id", user.Id);
                string query = "";
                //Jeśli użytkownik poprawny ustawiam flagę na 1
                if (user.ValidateUser())
                {
                    query = "Update UsersBus set IsActive=1 Where ID=@Id";
                }
                else
                {
                    //Jeśli nie, ustawiam flagę na 0
                    query= "Update UsersBus set IsActive=0 Where ID=@Id";
                }
                await db.ExecuteAsync(query, dynamicParameters);                    
            }
            
            await args.CompleteMessageAsync(args.Message);
        }

        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        //Tutaj czekam na przychodzące wiadomości
        static async Task Main()
        {
            client = new ServiceBusClient(serviceBusConnectionString);

            processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            try
            {
                processor.ProcessMessageAsync += MessageHandler;

                processor.ProcessErrorAsync += ErrorHandler;

                await processor.StartProcessingAsync();

                //Czekam na wiadomości aż do wciśnięcia przycisku
                Console.WriteLine("RECEIVER - Wait for a minute and then press any key to end the processing");
                Console.ReadKey();
                
                Console.WriteLine("\nRECEIVER - Stopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("RECEIVER - Stopped receiving messages");
            }
            finally
            {
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}