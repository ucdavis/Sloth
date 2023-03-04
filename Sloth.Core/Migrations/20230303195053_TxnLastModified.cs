using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sloth.Core.Migrations
{
    public partial class TxnLastModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            // update LastModified with max EventDate, falling back to TransactionDate if it has no status events
            migrationBuilder.Sql(@"
                with tse as (
                    select
                        TransactionId,
                        MAX(EventDate) as MaxDate
                    from
                        TransactionStatusEvents
                    group by
                        TransactionId
                )
                update
                    t
                set
                    t.LastModified=isnull(tse.MaxDate, t.TransactionDate)
                from
                    Transactions as t
                    left join tse on t.Id = TSE.TransactionId
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_LastModified",
                table: "Transactions",
                column: "LastModified");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_LastModified",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Transactions");
        }
    }
}
