using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GenesisChallenge.Models;

namespace GenesisChallenge.Models
{
    public interface IGenesisChallengeContext : IDisposable
    {
        DbSet<User> User { get; set; }
        int SaveChanges();
    }
    public class GenesisChallengeContext : DbContext, IGenesisChallengeContext
    {
        public GenesisChallengeContext (DbContextOptions options)
            : base(options)
        {
            this.Database.EnsureCreated();
        }

        public DbSet<User> User { get; set; }
    }
}
