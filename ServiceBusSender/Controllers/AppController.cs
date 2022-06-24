using Dapper;
using Microsoft.AspNetCore.Mvc;
using ServiceBusSender.DTOs;
using ServiceBusSender.Models;
using ServiceBusSender.Services;
using System.Data;
using System.Data.SqlClient;

namespace ServiceBusSender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly string serviceBusConnectionString;
        private readonly string insertServiceName = "insertQueue";
        private QueueService queueService;
        public AppController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DBConnectionString");
            serviceBusConnectionString = _configuration.GetConnectionString("ServiceBusConnectionString");
            queueService = new QueueService(configuration);
        }

        //Pobieranie aktywnych użytkowników
        [HttpGet(nameof(GetActiveUsers))]
        public async Task<IActionResult> GetActiveUsers()
        {
            try
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    var result = await db.QueryAsync<GetUserDTO>("Select ID,Name,LastName,Email,Age,IsActive from UsersBus where IsActive=1");
                    return Ok(result);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error while getting users:\n" + ex);
            }
        }

        //Wstawianie użytkownika
        [HttpPost(nameof(InsertUser))]
        public async Task<IActionResult> InsertUser([FromForm] InsertUserDTO insertUser)
        {
            try
            {
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    //Muszę uzyskać parametr ID z wstawionego obiektu przez opcję OUTPUT INSERTED.Id
                    string query = "Insert into UsersBus (Name,LastName,Email,Age,IsActive) OUTPUT INSERTED.Id values(@Name,@LastName,@Email,@Age,0);";
                    int insertedId=await db.QuerySingleAsync<int>(query, insertUser);
                    Console.WriteLine(insertedId);
                    //Ostatecznie konwertuję obiekt z typu InsertUserDTO na typ User i wysyłam wiadomość
                    User user = insertUser.getUser(insertedId);
                    await queueService.SendMessageAsync(user);
                }
                return Ok("Inserted User");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error while inserting User:\n" + ex);
            }
        }

        //Aktualizacja użytkownika
        [HttpPut(nameof(UpdateUser))]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserDTO updateUser)
        {
            if(updateUser.Id <= 0)
            {
                return NotFound($"No user with id {updateUser.Id}");
            }
            else
            {
                try
                {
                    using (IDbConnection db = new SqlConnection(connectionString))
                    {
                        string query = "Update UsersBus set Name=@Name,LastName=@LastName,Email=@Email,Age=@Age Where ID=@Id";
                        await queueService.SendMessageAsync(updateUser);
                        await db.ExecuteAsync(query, updateUser);
                        return Ok($"Updated User with Id:{updateUser.Id}");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Error while updating User:\n" + ex);
                }
            }
        }

    }
}
