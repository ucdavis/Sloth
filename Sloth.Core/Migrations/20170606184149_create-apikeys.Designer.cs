using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Sloth.Core;

namespace Sloth.Core.Migrations
{
    [DbContext(typeof(SlothDbContext))]
    [Migration("20170606184149_create-apikeys")]
    partial class createapikeys
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Sloth.Core.Models.ApiKey", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Issued");

                    b.Property<string>("Owner");

                    b.Property<DateTime?>("Revoked");

                    b.HasKey("Id");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("Sloth.Core.Models.Scrubber", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Id");

                    b.ToTable("Scrubbers");
                });
        }
    }
}
