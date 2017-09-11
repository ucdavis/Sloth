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
    [Migration("20170908223739_create_integrations")]
    partial class create_integrations
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);


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
        }
    }
}
