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
    [Migration("20170908223839_create_kfs_tracking_sequence")]
    partial class create_kfs_tracking_sequence
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("Relational:Sequence:.KFS_Tracking_Number_Seq", "'KFS_Tracking_Number_Seq', '', '1', '1', '1', '9999999999', 'Int64', 'True'")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
        }
    }
}
