#!/usr/bin/env bash
# Re-scaffolds the EF Core model (database-first) from a database built by db/init.sh, e.g. the compose db.
# Never edit the generated files; customize in partial classes or in
# src/AttendanceSystem.Infrastructure/CodeTemplates/EFCore/*.t4.
#
# Usage (from any directory):
#   ATTENDANCE_CONNECTION_STRING='Server=localhost,1433;Database=Attendance;User Id=sa;Password=<password>;TrustServerCertificate=True' db/scaffold.sh
set -euo pipefail

: "${ATTENDANCE_CONNECTION_STRING:?Set ATTENDANCE_CONNECTION_STRING to the Attendance database connection string}"

cd "$(dirname "$0")/.."
dotnet tool restore
dotnet ef dbcontext scaffold "$ATTENDANCE_CONNECTION_STRING" Microsoft.EntityFrameworkCore.SqlServer \
    --project src/AttendanceSystem.Infrastructure \
    --startup-project src/AttendanceSystem.Api \
    --context AttendanceDbContext \
    --context-dir Persistence \
    --output-dir Persistence/Entities \
    --namespace AttendanceSystem.Infrastructure.Persistence.Entities \
    --context-namespace AttendanceSystem.Infrastructure.Persistence \
    --no-onconfiguring \
    --force
