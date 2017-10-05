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

            modelBuilder.Entity("Sloth.Core.Models.Role", b =>
            {
                b.Property<string>("Id")
                    .ValueGeneratedOnAdd();

                b.HasKey("Id");

                b.ToTable("Roles");
            });

            modelBuilder.Entity("Sloth.Core.Models.Team", b =>
            {
                b.Property<string>("Id")
                    .ValueGeneratedOnAdd();

                b.HasKey("Id");

                b.ToTable("Teams");
            });

            modelBuilder.Entity("Sloth.Core.Models.User", b =>
            {
                b.Property<string>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Email");

                b.Property<string>("UserName");

                b.HasKey("Id");

                b.ToTable("Users");
            });

            modelBuilder.Entity("Sloth.Core.Models.UserRole", b =>
            {
                b.Property<string>("UserId");

                b.Property<string>("RoleId");

                b.HasKey("UserId", "RoleId");

                b.HasIndex("RoleId");

                b.ToTable("UserRoles");
            });

            modelBuilder.Entity("Sloth.Core.Models.UserTeam", b =>
            {
                b.Property<string>("UserId");

                b.Property<string>("TeamId");

                b.HasKey("UserId", "TeamId");

                b.HasIndex("TeamId");

                b.ToTable("UserTeams");
            });

            modelBuilder.Entity("Sloth.Core.Models.UserRole", b =>
            {
                b.HasOne("Sloth.Core.Models.Role", "Role")
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Sloth.Core.Models.User", "User")
                    .WithMany("UserRoles")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity("Sloth.Core.Models.UserTeam", b =>
            {
                b.HasOne("Sloth.Core.Models.Team", "Team")
                    .WithMany()
                    .HasForeignKey("TeamId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Sloth.Core.Models.User", "User")
                    .WithMany("UserTeams")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity("Sloth.Core.Models.ApiKey", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<DateTime>("Issued");

                    b.Property<DateTime?>("Revoked");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("Key");

                    b.HasIndex("UserId");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("Sloth.Core.Models.ApiKey", b =>
                {
                    b.HasOne("Sloth.Core.Models.User", "User")
                        .WithMany("ApiKeys")
                        .HasForeignKey("UserId");
                });
        }
    }
}
