using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<AppUserEntity> Users { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var bytesConverter = new UlidToBytesConverter();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Don't use database-generated values for primary keys
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<Ulid>(nameof(BaseEntity.Id)).ValueGeneratedNever();
                }
                if (typeof(ImageEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<Ulid>(nameof(ImageEntity.AppUserId)).ValueGeneratedNever();
                }

                // Convert Ulids to bytea when persisting
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(Ulid) || property.ClrType == typeof(Ulid?))
                    {
                        property.SetValueConverter(bytesConverter);
                    }
                }
            }
        }

        public class UlidToBytesConverter : ValueConverter<Ulid, byte[]>
        {
            private static readonly ConverterMappingHints DefaultHints = new ConverterMappingHints(size: 16);

            public UlidToBytesConverter(ConverterMappingHints mappingHints = null)
                : base(
                        convertToProviderExpression: x => x.ToByteArray(),
                        convertFromProviderExpression: x => new Ulid(x),
                        mappingHints: DefaultHints.With(mappingHints))
            {
            }
        }
    }
}