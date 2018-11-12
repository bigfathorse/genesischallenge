using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GenesisChallenge.Models
{
    public class User
    {

        #region Constractor
        public User()
        {
        }
        #endregion


        #region Properties
        [Key]
        public Guid Id { get; set; }  
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public DateTime LastLoginOn { get; set; }
        public Telephone[] Telephones { get; set; }

        #endregion

    }
}
