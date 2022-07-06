using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sloth.Core.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "Document_Number_Seq",
                minValue: 1L,
                maxValue: 999999999L,
                cyclic: true);

            migrationBuilder.CreateSequence(
                name: "KFS_Tracking_Number_Seq",
                minValue: 1L,
                maxValue: 9999999999L,
                cyclic: true);

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blobs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Uri = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Container = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CybersourceBankReconcileJobRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RanOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CybersourceBankReconcileJobRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KfsScrubberUploadJobRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RanOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KfsScrubberUploadJobRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    KfsContactUserId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    KfsContactEmail = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    KfsContactPhoneNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    KfsContactMailingAddress = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    KfsContactDepartmentName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebHookRequestResendJobRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RanOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookRequestResendJobRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TeamId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Issued = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revoked = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Chart = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    OrganizationCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    OriginCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    KfsFtpUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KfsFtpPasswordKeyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sources_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTeamRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeamId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTeamRoles", x => new { x.UserId, x.RoleId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_UserTeamRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTeamRoles_TeamRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "TeamRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTeamRoles_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebHooks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebHooks_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Source = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "xml", nullable: true),
                    LogEvent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    JobId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    JobName = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logs_CybersourceBankReconcileJobRecords_JobId",
                        column: x => x.JobId,
                        principalTable: "CybersourceBankReconcileJobRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Logs_KfsScrubberUploadJobRecords_JobId",
                        column: x => x.JobId,
                        principalTable: "KfsScrubberUploadJobRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Logs_WebHookRequestResendJobRecords_JobId",
                        column: x => x.JobId,
                        principalTable: "WebHookRequestResendJobRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Integrations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportPasswordKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClearingAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoldingAccount = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Integrations_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Integrations_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Scrubbers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatchSequenceNumber = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BlobId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scrubbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scrubbers_Blobs_BlobId",
                        column: x => x.BlobId,
                        principalTable: "Blobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Scrubbers_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebHookRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WebHookId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseStatus = table.Column<int>(type: "int", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestCount = table.Column<int>(type: "int", nullable: true),
                    LastRequestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Persist = table.Column<bool>(type: "bit", nullable: false),
                    WebHookRequestResendJobId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebHookRequests_WebHookRequestResendJobRecords_WebHookRequestResendJobId",
                        column: x => x.WebHookRequestResendJobId,
                        principalTable: "WebHookRequestResendJobRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WebHookRequests_WebHooks_WebHookId",
                        column: x => x.WebHookId,
                        principalTable: "WebHooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CybersourceBankReconcileJobBlobs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IntegrationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CybersourceBankReconcileJobRecordId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BlobId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CybersourceBankReconcileJobBlobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CybersourceBankReconcileJobBlobs_Blobs_BlobId",
                        column: x => x.BlobId,
                        principalTable: "Blobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CybersourceBankReconcileJobBlobs_CybersourceBankReconcileJobRecords_CybersourceBankReconcileJobRecordId",
                        column: x => x.CybersourceBankReconcileJobRecordId,
                        principalTable: "CybersourceBankReconcileJobRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CybersourceBankReconcileJobBlobs_Integrations_IntegrationId",
                        column: x => x.IntegrationId,
                        principalTable: "Integrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MerchantTrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MerchantTrackingUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessorTrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    KfsTrackingNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScrubberId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReversalOfTransactionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReversalTransactionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CybersourceBankReconcileJobRecordId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    KfsScrubberUploadJobRecordId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_CybersourceBankReconcileJobRecords_CybersourceBankReconcileJobRecordId",
                        column: x => x.CybersourceBankReconcileJobRecordId,
                        principalTable: "CybersourceBankReconcileJobRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_KfsScrubberUploadJobRecords_KfsScrubberUploadJobRecordId",
                        column: x => x.KfsScrubberUploadJobRecordId,
                        principalTable: "KfsScrubberUploadJobRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Scrubbers_ScrubberId",
                        column: x => x.ScrubberId,
                        principalTable: "Scrubbers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Transactions_ReversalOfTransactionId",
                        column: x => x.ReversalOfTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Transactions_ReversalTransactionId",
                        column: x => x.ReversalTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatusEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatusEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionStatusEvents_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Chart = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Account = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SubAccount = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ObjectCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    SubObjectCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObjectType = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    SequenceNumber = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: true),
                    FiscalPeriod = table.Column<int>(type: "int", nullable: true),
                    Project = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_Key",
                table: "ApiKeys",
                column: "Key",
                unique: true,
                filter: "[Key] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_TeamId",
                table: "ApiKeys",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Blobs_FileName",
                table: "Blobs",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_Blobs_UploadedDate",
                table: "Blobs",
                column: "UploadedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Blobs_Uri",
                table: "Blobs",
                column: "Uri");

            migrationBuilder.CreateIndex(
                name: "IX_CybersourceBankReconcileJobBlobs_BlobId",
                table: "CybersourceBankReconcileJobBlobs",
                column: "BlobId");

            migrationBuilder.CreateIndex(
                name: "IX_CybersourceBankReconcileJobBlobs_CybersourceBankReconcileJobRecordId",
                table: "CybersourceBankReconcileJobBlobs",
                column: "CybersourceBankReconcileJobRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CybersourceBankReconcileJobBlobs_IntegrationId",
                table: "CybersourceBankReconcileJobBlobs",
                column: "IntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_SourceId",
                table: "Integrations",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_TeamId",
                table: "Integrations",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_CorrelationId",
                table: "Logs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_JobId",
                table: "Logs",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_JobName_JobId",
                table: "Logs",
                columns: new[] { "JobName", "JobId" });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_Source",
                table: "Logs",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Scrubbers_BlobId",
                table: "Scrubbers",
                column: "BlobId");

            migrationBuilder.CreateIndex(
                name: "IX_Scrubbers_SourceId",
                table: "Scrubbers",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Sources_TeamId",
                table: "Sources",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamRoles_Name",
                table: "TeamRoles",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Name",
                table: "Teams",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatorId",
                table: "Transactions",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CybersourceBankReconcileJobRecordId",
                table: "Transactions",
                column: "CybersourceBankReconcileJobRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_KfsScrubberUploadJobRecordId",
                table: "Transactions",
                column: "KfsScrubberUploadJobRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReversalOfTransactionId",
                table: "Transactions",
                column: "ReversalOfTransactionId",
                unique: true,
                filter: "[ReversalOfTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReversalTransactionId",
                table: "Transactions",
                column: "ReversalTransactionId",
                unique: true,
                filter: "[ReversalTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ScrubberId",
                table: "Transactions",
                column: "ScrubberId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SourceId",
                table: "Transactions",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatusEvents_EventDate",
                table: "TransactionStatusEvents",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatusEvents_Status",
                table: "TransactionStatusEvents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatusEvents_TransactionId",
                table: "TransactionStatusEvents",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransactionId",
                table: "Transfers",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTeamRoles_RoleId",
                table: "UserTeamRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTeamRoles_TeamId",
                table: "UserTeamRoles",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_WebHookRequests_LastRequestDate",
                table: "WebHookRequests",
                column: "LastRequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_WebHookRequests_ResponseStatus",
                table: "WebHookRequests",
                column: "ResponseStatus");

            migrationBuilder.CreateIndex(
                name: "IX_WebHookRequests_WebHookId",
                table: "WebHookRequests",
                column: "WebHookId");

            migrationBuilder.CreateIndex(
                name: "IX_WebHookRequests_WebHookRequestResendJobId",
                table: "WebHookRequests",
                column: "WebHookRequestResendJobId");

            migrationBuilder.CreateIndex(
                name: "IX_WebHooks_TeamId",
                table: "WebHooks",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CybersourceBankReconcileJobBlobs");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "TransactionStatusEvents");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "UserTeamRoles");

            migrationBuilder.DropTable(
                name: "WebHookRequests");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Integrations");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "TeamRoles");

            migrationBuilder.DropTable(
                name: "WebHookRequestResendJobRecords");

            migrationBuilder.DropTable(
                name: "WebHooks");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CybersourceBankReconcileJobRecords");

            migrationBuilder.DropTable(
                name: "KfsScrubberUploadJobRecords");

            migrationBuilder.DropTable(
                name: "Scrubbers");

            migrationBuilder.DropTable(
                name: "Blobs");

            migrationBuilder.DropTable(
                name: "Sources");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropSequence(
                name: "Document_Number_Seq");

            migrationBuilder.DropSequence(
                name: "KFS_Tracking_Number_Seq");
        }
    }
}
