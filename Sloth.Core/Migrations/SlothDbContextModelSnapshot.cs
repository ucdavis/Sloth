﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Sloth.Core;

namespace Sloth.Core.Migrations
{
    [DbContext(typeof(SlothDbContext))]
    partial class SlothDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("Sloth.Core.Models.Scrubber", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("BatchDate");

                    b.Property<int>("BatchSequenceNumber");

                    b.Property<string>("CampusCode")
                        .IsRequired()
                        .HasMaxLength(2);

                    b.Property<string>("Chart")
                        .IsRequired()
                        .HasMaxLength(2);

                    b.Property<string>("ContactAddress")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<string>("ContactDepartment")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<string>("ContactEmail")
                        .IsRequired()
                        .HasMaxLength(40);

                    b.Property<string>("ContactPhone")
                        .IsRequired()
                        .HasMaxLength(10);

                    b.Property<string>("ContactUserId")
                        .IsRequired()
                        .HasMaxLength(8);

                    b.Property<string>("CreatorId");

                    b.Property<string>("OrganizationCode")
                        .IsRequired()
                        .HasMaxLength(4);

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("Scrubbers");
                });

            modelBuilder.Entity("Sloth.Core.Models.Transaction", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Account")
                        .IsRequired()
                        .HasMaxLength(7);

                    b.Property<decimal>("Amount");

                    b.Property<string>("BalanceType")
                        .IsRequired()
                        .HasMaxLength(2);

                    b.Property<int>("Chart")
                        .HasMaxLength(2);

                    b.Property<string>("DebitCredit")
                        .IsRequired();

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(40);

                    b.Property<string>("DocType")
                        .IsRequired();

                    b.Property<string>("DocumentNumber")
                        .IsRequired()
                        .HasMaxLength(14);

                    b.Property<int>("FiscalPeriod");

                    b.Property<int>("FiscalYear");

                    b.Property<string>("ObjectCode")
                        .IsRequired()
                        .HasMaxLength(4);

                    b.Property<string>("ObjectType")
                        .HasMaxLength(2);

                    b.Property<string>("OriginCode")
                        .IsRequired()
                        .HasMaxLength(2);

                    b.Property<string>("Project")
                        .HasMaxLength(10);

                    b.Property<string>("ReferenceId")
                        .HasMaxLength(8);

                    b.Property<string>("ScrubberId");

                    b.Property<int>("SequenceNumber");

                    b.Property<string>("SubAccount")
                        .HasMaxLength(5);

                    b.Property<string>("SubObjectCode")
                        .HasMaxLength(3);

                    b.Property<string>("TrackingNumber")
                        .HasMaxLength(10);

                    b.Property<DateTime>("TransactionDate");

                    b.HasKey("Id");

                    b.HasIndex("ScrubberId");

                    b.ToTable("Transaction");
                });

            modelBuilder.Entity("Sloth.Core.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Sloth.Core.Models.ApiKey", b =>
                {
                    b.HasOne("Sloth.Core.Models.User", "User")
                        .WithMany("Keys")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Sloth.Core.Models.Scrubber", b =>
                {
                    b.HasOne("Sloth.Core.Models.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId");
                });

            modelBuilder.Entity("Sloth.Core.Models.Transaction", b =>
                {
                    b.HasOne("Sloth.Core.Models.Scrubber")
                        .WithMany("Transactions")
                        .HasForeignKey("ScrubberId");
                });
        }
    }
}
