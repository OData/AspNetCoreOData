using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay
{
    public class EfDateAndTimeOfDayModelContext : DbContext
    {
        public EfDateAndTimeOfDayModelContext(DbContextOptions<EfDateAndTimeOfDayModelContext> options)
            : base(options)
        {
        }

        public DbSet<DateAndTimeOfDayModel> DateTimes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DateAndTimeOfDayModel>().Property(c => c.EndDay).HasColumnType("date");
            modelBuilder.Entity<DateAndTimeOfDayModel>().Property(c => c.DeliverDay).HasColumnType("date");
        }
    }

}
