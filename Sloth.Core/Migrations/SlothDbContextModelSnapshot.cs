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
    partial class SlothDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("Relational:Sequence:.KFS_Tracking_Number_Seq", "'KFS_Tracking_Number_Seq', '', '1', '1', '1', '9999999999', 'Int64', 'True'")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

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

            modelBuilder.Entity("Sloth.Core.Models.Integration", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DefaultAccount");

                    b.Property<string>("MerchantId");

                    b.Property<string>("ReportPasswordKey");

                    b.Property<string>("ReportUsername");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.ToTable("Integrations");
                });

            modelBuilder.Entity("Sloth.Core.Models.Scrubber", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("BatchDate");

                    b.Property<int>("BatchSequenceNumber");

                    b.Property<string>("Chart")
                        .IsRequired()
                        .HasMaxLength(2);

                    b.Property<string>("OrganizationCode")
                        .IsRequired()
                        .HasMaxLength(4);

                    b.Property<string>("Uri");

                    b.HasKey("Id");

                    b.ToTable("Scrubbers");
                });

            modelBuilder.Entity("Sloth.Core.Models.Transaction", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatorId");

                    b.Property<string>("DocumentNumber")
                        .IsRequired()
                        .HasMaxLength(14);

                    b.Property<string>("KfsTrackingNumber")
                        .HasMaxLength(10);

                    b.Property<string>("MerchantTrackingNumber");

                    b.Property<string>("OriginCode")
                        .IsRequired()
                        .HasMaxLength(2);

                    b.Property<string>("ProcessorTrackingNumber");

                    b.Property<string>("ScrubberId");

                    b.Property<int>("Status");

                    b.Property<DateTime>("TransactionDate");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("ScrubberId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("Sloth.Core.Models.Transfer", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Account")
                        .IsRequired()
                        .HasMaxLength(7);

                    b.Property<decimal>("Amount");

                    b.Property<int>("Chart")
                        .HasMaxLength(2);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(40);

                    b.Property<int>("Direction");

                    b.Property<int>("FiscalPeriod");

                    b.Property<int>("FiscalYear");

                    b.Property<string>("ObjectCode")
                        .IsRequired()
                        .HasMaxLength(4);

                    b.Property<string>("ObjectType")
                        .HasMaxLength(2);

                    b.Property<string>("Project")
                        .HasMaxLength(10);

                    b.Property<string>("ReferenceId")
                        .HasMaxLength(8);

                    b.Property<int>("SequenceNumber");

                    b.Property<string>("SubAccount")
                        .HasMaxLength(5);

                    b.Property<string>("SubObjectCode")
                        .HasMaxLength(3);

                    b.Property<string>("TransactionId");

                    b.HasKey("Id");

                    b.HasIndex("TransactionId");

                    b.ToTable("Transfers");
                });

            modelBuilder.Entity("Sloth.Core.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Sloth.Core.Models.User")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Sloth.Core.Models.User")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Sloth.Core.Models.User")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Sloth.Core.Models.ApiKey", b =>
                {
                    b.HasOne("Sloth.Core.Models.User", "User")
                        .WithMany("Keys")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Sloth.Core.Models.Transaction", b =>
                {
                    b.HasOne("Sloth.Core.Models.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId");

                    b.HasOne("Sloth.Core.Models.Scrubber", "Scrubber")
                        .WithMany("Transactions")
                        .HasForeignKey("ScrubberId");
                });

            modelBuilder.Entity("Sloth.Core.Models.Transfer", b =>
                {
                    b.HasOne("Sloth.Core.Models.Transaction", "Transaction")
                        .WithMany("Transfers")
                        .HasForeignKey("TransactionId");
                });
        }
    }
}
