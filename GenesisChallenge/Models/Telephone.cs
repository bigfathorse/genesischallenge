using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GenesisChallenge.Models
{
    [Table("Telephones")]
    public class Telephone
    {
        #region Constractor
        public Telephone()
        {
        }
        #endregion


        #region Properties
        [Key]
        public Guid ID { get; set; }
        public string Number { get; set; }
       
        #endregion
    }
}
