using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sloth.Core.Migrations
{
    public partial class create_kfs_tracking_sequence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "KFS_Tracking_Number_Seq",
                minValue: 1L,
                maxValue: 9999999999L,
                cyclic: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "KFS_Tracking_Number_Seq");
        }
    }
}
