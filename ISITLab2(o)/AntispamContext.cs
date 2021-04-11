namespace ISITLab2_o_
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AntispamContext : DbContext
    {
        public AntispamContext()
            : base("name=AntispamContext")
        {
        }

        public virtual DbSet<antispam> antispam { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<antispam>()
                .Property(e => e.Word)
                .IsFixedLength();
        }
    }
}
