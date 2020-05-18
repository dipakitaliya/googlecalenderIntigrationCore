using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Google_Calender.Models
{
    public class CalSiteDbContext:DbContext
    {
        public CalSiteDbContext():base("DefaultConnection")
        {
        }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<GoogleRefreshToken> GoogleRefreshTokens { get; set; }
    }
}