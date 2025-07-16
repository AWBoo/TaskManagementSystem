using backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq; // For .ToList() and .Any()
using System.Security.Claims; // For ClaimTypes
using Microsoft.AspNetCore.Http; // For IHttpContextAccessor
using Microsoft.EntityFrameworkCore.ChangeTracking; // For EntityEntry

namespace backend.Data
{
    public class ApplicationDBContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<AuditEntry> AuditEntries { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Project -> Tasks Relationship: ON DELETE CASCADE
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); // Explicitly CASCADE

            // TaskItem -> User Relationship: ON DELETE SET NULL
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull); 

            // User-UserRole relationship (One-to-Many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction); // Prevent deletion of User if UserRoles exist

            // Role-UserRole relationship (One-to-Many)
            modelBuilder.Entity<Role>()
                .HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction); // Prevent deletion of Role if UserRoles exist

        }

        // Override SaveChanges to automatically generate audit entries
        public override int SaveChanges()
        {
            GenerateAuditEntries();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            GenerateAuditEntries();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void GenerateAuditEntries()
        {
            Guid currentUserId = Guid.Empty; // Default if user ID cannot be determined
            if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid parsedUserId))
                {
                    currentUserId = parsedUserId;
                }
            }

            // Get all entries that are being added, modified, or deleted
            var changedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList(); // Materialize to avoid issues during iteration

            foreach (var entry in changedEntries)
            {
                Guid entityId;
                string entityType;
                DateTime changeTimestamp = DateTime.UtcNow;

                // Determine EntityId and EntityType based on the entity type
                if (entry.Entity is TaskItem taskItem)
                {
                    entityId = taskItem.Id;
                    entityType = nameof(TaskItem);
                }
                else if (entry.Entity is Project project)
                {
                    entityId = project.Id;
                    entityType = nameof(Project);
                }
                else if (entry.Entity is User user)
                {
                    entityId = user.Id;
                    entityType = nameof(User);
                }
                else
                {
                    // Skip if not one of the audited types
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        // Log creation of the entity
                        AuditEntries.Add(new AuditEntry
                        {
                            EntityId = entityId,
                            EntityType = entityType,
                            PropertyName = "Entity", // General name for creation
                            OldValue = null,
                            NewValue = "Created",
                            ChangedByUserId = currentUserId,
                            ChangeTimestamp = changeTimestamp,
                            ChangeType = "Created"
                        });

                        // log each property of the newly created entity
                         foreach (var property in entry.Properties)
                        {
                            if (property.Metadata.IsShadowProperty()) continue;
                            AuditEntries.Add(new AuditEntry
                            {
                                EntityId = entityId,
                                EntityType = entityType,
                                PropertyName = property.Metadata.Name,
                                OldValue = null,
                                NewValue = property.CurrentValue?.ToString(),
                                ChangedByUserId = currentUserId,
                                ChangeTimestamp = changeTimestamp,
                                ChangeType = "Created"
                            });
                        }
                        break;

                    case EntityState.Modified:
                        // Log only properties that actually changed
                        foreach (var property in entry.Properties)
                        {
                            if (property.IsModified && !property.Metadata.IsShadowProperty())
                            {
                                AuditEntries.Add(new AuditEntry
                                {
                                    EntityId = entityId,
                                    EntityType = entityType,
                                    PropertyName = property.Metadata.Name,
                                    OldValue = property.OriginalValue?.ToString(),
                                    NewValue = property.CurrentValue?.ToString(),
                                    ChangedByUserId = currentUserId,
                                    ChangeTimestamp = changeTimestamp,
                                    ChangeType = "Updated"
                                });
                            }
                        }
                        break;

                    case EntityState.Deleted:
                        // Log the deletion of the entity
                        AuditEntries.Add(new AuditEntry
                        {
                            EntityId = entityId,
                            EntityType = entityType,
                            PropertyName = "Entity", // General name for deletion
                            OldValue = $"{entityType} (ID: {entityId})", // More descriptive old value
                            NewValue = null,
                            ChangedByUserId = currentUserId,
                            ChangeTimestamp = changeTimestamp,
                            ChangeType = "Deleted"
                        });
                        break;
                }
            }
        }
    }
}