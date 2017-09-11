using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Sloth.Core;
using Sloth.Core.Models;

namespace Sloth.Core.Migrations
{
    [DbContext(typeof(SlothDbContext))]
    [Migration("20170614190351_create_apikeys")]
    partial class create_apikeys
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

                    b.Property<DateTime?>("Revoked");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("Sloth.Core.Models.ApiKey", b =>
                {
                    b.HasOne("Sloth.Core.Models.User", "User")
                        .WithMany("Keys")
                        .HasForeignKey("UserId");
                });
        }
    }
}
