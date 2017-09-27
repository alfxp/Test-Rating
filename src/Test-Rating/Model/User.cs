using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Test_Rating.Model
{
    public class User
    {

        [Key]
        public int UserId {get;set;}

        public string Name { get; set; }
        

    }
}
