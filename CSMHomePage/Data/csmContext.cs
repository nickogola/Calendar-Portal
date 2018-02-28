namespace CSMHomePage.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class csmContext : DbContext
    {
        public csmContext()
            : base("name=csmContext")
        {
        }

        public virtual DbSet<Announcement> Announcements { get; set; }
        public virtual DbSet<CalendarGroup> CalendarGroups { get; set; }
        public virtual DbSet<Link> Links { get; set; }
        public virtual DbSet<UserCalendar> UserCalendars { get; set; }
        public virtual DbSet<ApplicationPermission> ApplicationPermissions { get; set; }
        public virtual DbSet<UserPermission> UserPermissions { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Announcement>()
                .Property(e => e.AnnouncementText)
                .IsUnicode(false);

            modelBuilder.Entity<CalendarGroup>()
                .Property(e => e.UserID)
                .IsUnicode(false);

            modelBuilder.Entity<CalendarGroup>()
                .Property(e => e.CalendarGroup1)
                .IsUnicode(false);

            modelBuilder.Entity<Link>()
                .Property(e => e.LinkType)
                .IsUnicode(false);

            modelBuilder.Entity<Link>()
                .Property(e => e.LinkDescription)
                .IsUnicode(false);

            modelBuilder.Entity<Link>()
                .Property(e => e.LinkURL)
                .IsUnicode(false);

            modelBuilder.Entity<UserCalendar>()
                .Property(e => e.UserID)
                .IsUnicode(false);

            modelBuilder.Entity<UserCalendar>()
                .Property(e => e.EventDescription)
                .IsUnicode(false);

            modelBuilder.Entity<ApplicationPermission>()
                .Property(e => e.PermissionName)
                .IsUnicode(false);

            modelBuilder.Entity<ApplicationPermission>()
                .HasMany(e => e.UserPermissions)
                .WithRequired(e => e.ApplicationPermission)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserPermission>()
                .Property(e => e.UserID)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.UserID)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.UserFirstName)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.UserLastName)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.UserRole)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.UserTitle)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.UserEmailAddress)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.InsertedBy)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.UpdatedBy)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.UserColor)
                .IsUnicode(false);
        }
    }
}
