using DataAnnotationsExtensions;
using System.ComponentModel.DataAnnotations;

namespace ServiceBusReceiver
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        [Email]
        public string? Email { get; set; }
        [Required]
        public int? Age { get; set; }
        public bool IsActive { get; set; }

        //Zwracam czy użytkownik jest poprawny
        public bool ValidateUser()
        {
            ICollection<ValidationResult> results = null;
            Validator.TryValidateObject(this,
                new ValidationContext(this),
                results, true);
            
            //Wypisuję błędy walidacji
            if(!Validate(this,out results)){
                Console.WriteLine(string.Join("\n",results.Select(x=>x.ErrorMessage)));
                return false;
            }
            else
            {
                //Jeśli jest ok zwracam true
                return true;
            }
        }

        static bool Validate<T>(T obj,out ICollection<ValidationResult> results)
        {
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);
        }
    }
}
