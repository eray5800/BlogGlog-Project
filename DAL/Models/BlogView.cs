using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class BlogView : Base  //SessionID kullan üye olmayanlar için
    {
        public Guid BlogViewID { get; set; }


        public Blog Blog {  get; set; }



        public string SessionID {  get; set; }
    }
}
