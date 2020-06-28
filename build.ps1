param($target="default")

. .\build-helpers

# $ build
#     Optimized for local development: updates databases instead of rebuilding them.
#
# $ build rebuild
#     Builds a clean local copy, rebuilding databases instead of updating them.

main {
    $targetFramework = "netcoreapp3.1"
    $configuration = 'Release'

    $connectionStrings = @{
        DEV = connection-string ContactList src/ContactList/appsettings.Development.json;
        TEST = connection-string ContactList src/ContactList.Tests/appsettings.json
    }

    task ".NET SDK" { dotnet --version }
    task "Clean" { dotnet clean --configuration $configuration --nologo -v minimal } src
    task "Restore (Database Migration)" { dotnet restore --packages ./packages/ } src/ContactList.DatabaseMigration
    task "Restore (Solution)" { dotnet restore } src
    task "Build" { dotnet build --configuration $configuration --no-restore --nologo } src

    if ($target -eq "default") {
        task "Update DEV/TEST Databases" { update-database DEV TEST } src/ContactList.DatabaseMigration
    } elseif ($target -eq "rebuild") {
        task "Rebuild DEV/TEST Databases" { rebuild-database DEV TEST } src/ContactList.DatabaseMigration
    }

    task "Test" { dotnet fixie --configuration $configuration --no-build } src/ContactList.Tests
}