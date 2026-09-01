using System.Data.Entity;
using DataConcentrator.Model;

namespace DataConcentrator
{
    public class ContextClass : DbContext
    {
        private static ContextClass instance;

        public static ContextClass Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ContextClass();
                }

                return instance;
            }
        }

        public ContextClass() : base("name=ScadaContext")
        {
        }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<Alarm> Alarms { get; set; }

        public DbSet<ActivatedAlarm> ActivatedAlarms { get; set; }
    }
}