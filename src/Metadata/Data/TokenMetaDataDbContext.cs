
using Metadata.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace Metadata.Data;

public class TokenMetaDataDbContext(DbContextOptions<TokenMetaDataDbContext> options) : DbContext(options)
{
    
    public DbSet<TokenMetadata> TokenMetadatas => Set<TokenMetadata>();
    public DbSet<SyncState> SyncStates => Set<SyncState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TokenMetadata>(entity => 
        {
            entity.HasKey(e => e.Subject);
        });

        modelBuilder.Entity<SyncState>(entity => 
        {
            entity.HasKey(e => e.Sha);
        });
    }


}