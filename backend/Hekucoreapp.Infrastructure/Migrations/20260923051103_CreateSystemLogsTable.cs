using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hekucoreapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSystemLogsTable : Migration
    {
        // system_logs is intentionally NOT part of the EF model (no DbSet on HekucoreappDbContext,
        // no entity/configuration class) — it's written by SystemLogPostgresSink and read by the
        // admin log viewer, both via raw Npgsql, never through EF change tracking. Hand-written SQL
        // here, ported from gestamind's CreateSystemLogsTable migration (dropped its tenant_id
        // column — hekucoreapp is single-tenant).
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE system_logs (
                    id bigserial PRIMARY KEY,
                    timestamp timestamp without time zone NOT NULL,
                    level varchar(20) NOT NULL,
                    category varchar(20) NOT NULL,
                    message text NOT NULL,
                    exception text NULL,
                    user_id text NULL,
                    trace_id text NULL
                );

                CREATE INDEX ix_system_logs_filter ON system_logs (category, level, timestamp);
                CREATE INDEX ix_system_logs_timestamp ON system_logs (timestamp);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS system_logs;");
        }
    }
}
