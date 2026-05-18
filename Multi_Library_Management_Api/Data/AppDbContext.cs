using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Models;

namespace Multi_Library_Management_Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ─── DbSets ───────────────────────────────────────────────────────────
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<TableSeat> TableSeats { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentRegistration> StudentRegistrations { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }
        public DbSet<GateAccessLog> GateAccessLogs { get; set; }
        public DbSet<GeneralSetting> GeneralSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ─── Library ──────────────────────────────────────────────────────
            modelBuilder.Entity<Library>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Name).IsRequired().HasMaxLength(200);
                entity.Property(l => l.City).HasMaxLength(100);
                entity.Property(l => l.State).HasMaxLength(100);
                entity.Property(l => l.Pincode).HasMaxLength(10);
                entity.Property(l => l.Mobile).HasMaxLength(20);
                entity.Property(l => l.Email).HasMaxLength(200);
                entity.Property(l => l.LibraryIcon).HasMaxLength(500);
                entity.Property(l => l.DocumentImage).HasMaxLength(500);
                entity.Property(l => l.DocumentType).HasMaxLength(100);
                entity.Property(l => l.IsActive).HasDefaultValue(true);
                entity.Property(l => l.CreatedDate).HasDefaultValueSql("(UTC_TIMESTAMP())");
            });

            // ─── Roles ────────────────────────────────────────────────────────
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(150);
                entity.Property(r => r.IsActive).HasDefaultValue(true);
                entity.Property(r => r.CreatedDate).HasDefaultValueSql("(UTC_TIMESTAMP())");

                entity.HasOne(r => r.Library)
                      .WithMany(l => l.Roles)
                      .HasForeignKey(r => r.LibraryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ─── Permissions ──────────────────────────────────────────────────
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Module).IsRequired().HasMaxLength(100);
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.CreatedDate).HasDefaultValueSql("(UTC_TIMESTAMP())");
            });

            // ─── RolePermissions ──────────────────────────────────────────────
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => rp.Id);

                entity.HasOne(rp => rp.Role)
                      .WithMany(r => r.RolePermissions)
                      .HasForeignKey(rp => rp.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(rp => rp.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── Users ────────────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);
                entity.Property(u => u.Mobile).IsRequired().HasMaxLength(20);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Password).IsRequired().HasMaxLength(255);
                entity.Property(u => u.IsSuperadmin).HasDefaultValue(false);
                entity.Property(u => u.ProfileImage).HasMaxLength(500);
                entity.Property(u => u.IsActive).HasDefaultValue(true);
                entity.Property(u => u.CreatedDate).HasDefaultValueSql("(UTC_TIMESTAMP())");

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Library)
                      .WithMany(l => l.Users)
                      .HasForeignKey(u => u.LibraryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ─── Floors ───────────────────────────────────────────────────────
            modelBuilder.Entity<Floor>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Name).IsRequired().HasMaxLength(100);
                entity.Property(f => f.IsActive).HasDefaultValue(true);

                entity.HasOne(f => f.Library)
                      .WithMany(l => l.Floors)
                      .HasForeignKey(f => f.LibraryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── TableSeats (merged Table + Seat) ────────────────────────────
            modelBuilder.Entity<TableSeat>(entity =>
            {
                entity.HasKey(ts => ts.Id);
                entity.Property(ts => ts.TableNumber).IsRequired().HasMaxLength(50);
                entity.Property(ts => ts.SeatNumber).IsRequired().HasMaxLength(50);
                entity.Property(ts => ts.IsOccupied).HasDefaultValue(false);
                entity.Property(ts => ts.IsActive).HasDefaultValue(true);

                entity.HasOne(ts => ts.Floor)
                      .WithMany(f => f.TableSeats)
                      .HasForeignKey(ts => ts.FloorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ts => ts.Library)
                      .WithMany()
                      .HasForeignKey(ts => ts.LibraryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ─── Students ─────────────────────────────────────────────────────
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.FullName).IsRequired().HasMaxLength(150);
                entity.Property(s => s.Mobile).IsRequired().HasMaxLength(20);
                entity.Property(s => s.Email).HasMaxLength(200);
                entity.Property(s => s.IsActive).HasDefaultValue(true);
                entity.Property(s => s.CreatedDate).HasDefaultValueSql("(UTC_TIMESTAMP())");

                entity.HasOne(s => s.Library)
                      .WithMany(l => l.Students)
                      .HasForeignKey(s => s.LibraryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── StudentRegistrations ─────────────────────────────────────────
            modelBuilder.Entity<StudentRegistration>(entity =>
            {
                entity.HasKey(sr => sr.Id);
                entity.Property(sr => sr.MonthlyAmount).HasColumnType("decimal(10,2)");
                entity.Property(sr => sr.SecurityAmount).HasColumnType("decimal(10,2)");
                entity.Property(sr => sr.RFIDCode).HasMaxLength(100);
                entity.Property(sr => sr.RegistrationDate).HasDefaultValueSql("(UTC_TIMESTAMP())");
                entity.Property(sr => sr.Status).HasConversion<int>();

                entity.HasOne(sr => sr.Student)
                      .WithMany(s => s.StudentRegistrations)
                      .HasForeignKey(sr => sr.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.TableSeat)
                      .WithMany(ts => ts.StudentRegistrations)
                      .HasForeignKey(sr => sr.TableSeatId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.Batch)
                      .WithMany()
                      .HasForeignKey(sr => sr.BatchId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(sr => sr.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.Library)
                      .WithMany()
                      .HasForeignKey(sr => sr.LibraryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── Batch ────────────────────────────────────────────────────────
            modelBuilder.Entity<Batch>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(150);
                entity.Property(b => b.StartTime).HasMaxLength(50);
                entity.Property(b => b.EndTime).HasMaxLength(50);
                entity.Property(b => b.IsActive).HasDefaultValue(true);
                entity.Property(b => b.CreatedDate).HasDefaultValueSql("(UTC_TIMESTAMP())");

                entity.HasOne(b => b.Library)
                      .WithMany()
                      .HasForeignKey(b => b.LibraryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── Payments ─────────────────────────────────────────────────────
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Amount).HasColumnType("decimal(10,2)");
                entity.Property(p => p.PaymentMode).IsRequired().HasMaxLength(50);
                entity.Property(p => p.TransactionId).HasMaxLength(100);

                entity.HasOne(p => p.StudentRegistration)
                      .WithMany(sr => sr.Payments)
                      .HasForeignKey(p => p.RegistrationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(p => p.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Library)
                      .WithMany()
                      .HasForeignKey(p => p.LibraryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── AttendanceLogs ───────────────────────────────────────────────
            modelBuilder.Entity<AttendanceLog>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.HasOne(a => a.Student)
                      .WithMany(s => s.AttendanceLogs)
                      .HasForeignKey(a => a.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── GateAccessLogs ───────────────────────────────────────────────
            modelBuilder.Entity<GateAccessLog>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.RFIDCode).IsRequired().HasMaxLength(100);

                entity.HasOne(g => g.Student)
                      .WithMany(s => s.GateAccessLogs)
                      .HasForeignKey(g => g.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // ─── GeneralSettings ──────────────────────────────────────────────
            modelBuilder.Entity<GeneralSetting>(entity =>
            {
                entity.HasKey(gs => gs.Id);
                entity.Property(gs => gs.Key).IsRequired().HasMaxLength(100);
                entity.Property(gs => gs.Value).IsRequired();
                entity.Property(gs => gs.CreatedDate).HasDefaultValueSql("(UTC_TIMESTAMP())");

                entity.HasOne(gs => gs.Library)
                      .WithMany()
                      .HasForeignKey(gs => gs.LibraryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── Seed Data ────────────────────────────────────────────────────
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // SuperAdmin Role
            modelBuilder.Entity<Role>().HasData(new Role
            {
                Id = 1,
                Name = "SuperAdmin",
                Description = "Full system access",
                LibraryId = null,
                IsActive = true,
                CreatedDate = seedDate
            });

            // Permissions
            var permissions = new List<Permission>
            {
                new() { Id = 1,  Name = "CREATE_LIBRARY",      Module = "Library",      Description = "Create library",              IsActive = true, CreatedDate = seedDate },
                new() { Id = 2,  Name = "EDIT_LIBRARY",        Module = "Library",      Description = "Edit library",                IsActive = true, CreatedDate = seedDate },
                new() { Id = 3,  Name = "DELETE_LIBRARY",      Module = "Library",      Description = "Delete library",              IsActive = true, CreatedDate = seedDate },
                new() { Id = 4,  Name = "VIEW_LIBRARY",        Module = "Library",      Description = "View library",                IsActive = true, CreatedDate = seedDate },
                new() { Id = 5,  Name = "CREATE_FLOOR",        Module = "Floor",        Description = "Create floor",                IsActive = true, CreatedDate = seedDate },
                new() { Id = 6,  Name = "EDIT_FLOOR",          Module = "Floor",        Description = "Edit floor",                  IsActive = true, CreatedDate = seedDate },
                new() { Id = 7,  Name = "DELETE_FLOOR",        Module = "Floor",        Description = "Delete floor",                IsActive = true, CreatedDate = seedDate },
                new() { Id = 8,  Name = "VIEW_FLOOR",          Module = "Floor",        Description = "View floor",                  IsActive = true, CreatedDate = seedDate },
                new() { Id = 13, Name = "CREATE_TABLE",        Module = "Table",        Description = "Create table",                IsActive = true, CreatedDate = seedDate },
                new() { Id = 14, Name = "EDIT_TABLE",          Module = "Table",        Description = "Edit table",                  IsActive = true, CreatedDate = seedDate },
                new() { Id = 15, Name = "DELETE_TABLE",        Module = "Table",        Description = "Delete table",                IsActive = true, CreatedDate = seedDate },
                new() { Id = 16, Name = "VIEW_TABLE",          Module = "Table",        Description = "View table",                  IsActive = true, CreatedDate = seedDate },
                new() { Id = 17, Name = "CREATE_SEAT",         Module = "Seat",         Description = "Create seat",                 IsActive = true, CreatedDate = seedDate },
                new() { Id = 18, Name = "EDIT_SEAT",           Module = "Seat",         Description = "Edit seat",                   IsActive = true, CreatedDate = seedDate },
                new() { Id = 19, Name = "DELETE_SEAT",         Module = "Seat",         Description = "Delete seat",                 IsActive = true, CreatedDate = seedDate },
                new() { Id = 20, Name = "VIEW_SEAT",           Module = "Seat",         Description = "View seat",                   IsActive = true, CreatedDate = seedDate },
                new() { Id = 21, Name = "CREATE_STUDENT",      Module = "Student",      Description = "Create student",              IsActive = true, CreatedDate = seedDate },
                new() { Id = 22, Name = "EDIT_STUDENT",        Module = "Student",      Description = "Edit student",                IsActive = true, CreatedDate = seedDate },
                new() { Id = 23, Name = "DELETE_STUDENT",      Module = "Student",      Description = "Delete student",              IsActive = true, CreatedDate = seedDate },
                new() { Id = 24, Name = "VIEW_STUDENT",        Module = "Student",      Description = "View student",                IsActive = true, CreatedDate = seedDate },
                new() { Id = 25, Name = "CREATE_REGISTRATION", Module = "Registration", Description = "Create registration",         IsActive = true, CreatedDate = seedDate },
                new() { Id = 26, Name = "EDIT_REGISTRATION",   Module = "Registration", Description = "Edit registration",           IsActive = true, CreatedDate = seedDate },
                new() { Id = 27, Name = "VIEW_REGISTRATION",   Module = "Registration", Description = "View registration",           IsActive = true, CreatedDate = seedDate },
                new() { Id = 28, Name = "CREATE_PAYMENT",      Module = "Payment",      Description = "Create payment",              IsActive = true, CreatedDate = seedDate },
                new() { Id = 29, Name = "VIEW_PAYMENT",        Module = "Payment",      Description = "View payment",                IsActive = true, CreatedDate = seedDate },
                new() { Id = 30, Name = "VIEW_REPORT",         Module = "Report",       Description = "View reports",                IsActive = true, CreatedDate = seedDate },
                new() { Id = 31, Name = "RFID_ACCESS",         Module = "RFID",         Description = "Manage RFID gate access",     IsActive = true, CreatedDate = seedDate },
            };
            modelBuilder.Entity<Permission>().HasData(permissions);

            // Assign all remaining permissions to SuperAdmin Role
            var rolePermissions = permissions.Select((p, index) => new RolePermission
            {
                Id = index + 1,
                RoleId = 1,
                PermissionId = p.Id
            }).ToList();
            modelBuilder.Entity<RolePermission>().HasData(rolePermissions);

            // SuperAdmin User
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FullName = "Super Admin",
                Mobile = "9999999999",
                Email = "superadmin@library.com",
                Password = "Admin@123",
                IsSuperadmin = true,
                RoleId = 1,
                LibraryId = null,
                IsActive = true,
                CreatedDate = seedDate
            });
        }
    }
}
