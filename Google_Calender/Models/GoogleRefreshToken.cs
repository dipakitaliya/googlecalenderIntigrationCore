using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Google_Calender.Models
{
    public class GoogleRefreshToken
    {
        [Key]
        public int RefreshTokenId { get; set; }
        public string UserName { get; set; }
        public string RefreshToken { get; set; }
    }
}