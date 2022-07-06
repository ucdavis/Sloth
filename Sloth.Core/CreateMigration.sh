[ "$#" -eq 1 ] || { echo "1 argument required, $# provided. Usage: sh CreateMigrationAndExecute <MigrationName>"; exit 1; }

dotnet ef migrations add $1 --context SlothDbContext --output-dir Migrations --startup-project ../Sloth.Web/Sloth.Web.csproj -- --provider SqlServer
dotnet ef database update --startup-project ../Sloth.Web/Sloth.Web.csproj --context SlothDbContext
# usage from PM console in the Sloth.Core directory: sh CreateMigrationAndExecute.sh <MigrationName>

echo 'All done';
