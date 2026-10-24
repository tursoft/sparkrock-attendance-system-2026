#!/usr/bin/env bash
# Creates and initializes the Attendance database once, run by the compose db-init service:
# 00 on master, then the legacy schema, then db/scripts 01-04 on Attendance.
set -Eeuo pipefail

: "${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD must be set}"
# sqlcmd reads the password from here, which keeps it out of the process list.
export SQLCMDPASSWORD="$MSSQL_SA_PASSWORD"

DB_HOST="${DB_HOST:-db}"
DB_NAME="Attendance"
SCRIPTS_DIR="${SCRIPTS_DIR:-/db/scripts}"
LEGACY_DIR="${LEGACY_DIR:-/db/legacy}"

sql() {
    # -C trusts the container's self-signed certificate; -b fails on SQL errors; -I turns QUOTED_IDENTIFIER on.
    /opt/mssql-tools18/bin/sqlcmd -S "$DB_HOST" -U sa -C -b -I "$@"
}

run_script() {
    local database="$1" script="$2"
    echo "Running $(basename "$script") on $database"
    sql -d "$database" -i "$script"
}

drop_partial_database() {
    echo "Initialization failed; dropping $DB_NAME so the next run starts clean." >&2
    sql -d master -Q "IF DB_ID(N'$DB_NAME') IS NOT NULL BEGIN ALTER DATABASE [$DB_NAME] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$DB_NAME]; END" || true
}

exists=$(sql -d master -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$DB_NAME') IS NULL THEN 0 ELSE 1 END")
if [[ "$exists" == "1" ]]; then
    echo "Database $DB_NAME already exists; nothing to do."
    exit 0
fi

trap drop_partial_database ERR

run_script master "$SCRIPTS_DIR/00-create-database.sql"
run_script "$DB_NAME" "$LEGACY_DIR/schema.sql"
run_script "$DB_NAME" "$SCRIPTS_DIR/01-missing-tables.sql"
run_script "$DB_NAME" "$SCRIPTS_DIR/02-integrity.sql"
run_script "$DB_NAME" "$SCRIPTS_DIR/03-reference-data.sql"
run_script "$DB_NAME" "$SCRIPTS_DIR/04-demo-data.sql"

echo "Database $DB_NAME initialized."
