using System.Data.Entity;
using DataConcentrator.Model;

namespace DataConcentrator
{
    /*
     * Predstavlja Entity Framework kontekst baze podataka.
     * Omogucava pristup tabelama tagova, alarma i aktiviranih alarma.
     * Koristi se za perzistiranje podataka u SCADA sistemu.
     */
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnalogInput>();
            modelBuilder.Entity<AnalogOutput>();
            modelBuilder.Entity<DigitalInput>();
            modelBuilder.Entity<DigitalOutput>();

            base.OnModelCreating(modelBuilder);
        }
    }
}