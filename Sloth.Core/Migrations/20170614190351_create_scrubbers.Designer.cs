using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sloth.Core.Migrations
{
    [DbContext(typeof(SlothDbContext))]
    [Migration("20170614190351_create_scrubbers")]
    partial class create_scrubbers
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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

                    b.HasKey("Id");

                    b.ToTable("Scrubbers");
                });

            modelBuilder.Entity("Sloth.Core.Models.Transaction", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatorId");

                    b.Property<string>("ScrubberId");

                    b.Property<int>("Status");

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

                    b.Property<int>("SequenceNumber");

                    b.Property<string>("SubAccount")
                        .HasMaxLength(5);

                    b.Property<string>("SubObjectCode")
                        .HasMaxLength(3);

                    b.Property<string>("TrackingNumber")
                        .HasMaxLength(10);

                    b.Property<DateTime>("TransactionDate");

                    b.Property<string>("TransactionId");

                    b.HasKey("Id");

                    b.HasIndex("TransactionId");

                    b.ToTable("Transfers");
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
                    b.HasOne("Sloth.Core.Models.Transaction")
                        .WithMany("Transfers")
                        .HasForeignKey("TransactionId");
                });
        }
    }
}
