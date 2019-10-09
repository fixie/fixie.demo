function find-dependency($name) {
    $exes = @(gci packages -rec -filter $name)

    if ($exes.Length -ne 1) {
        throw "Expected to find 1 $name, but found $($exes.Length)."
    }

    return $exes[0].FullName
}

function connection-string($environmentVariablePrefix, $appsettingsPath) {
    $environmentVariableName = "$($environmentVariablePrefix):Database:ConnectionString"

    if (test-path env:$environmentVariableName) {
        return (get-item env:$environmentVariableName).Value
    }

    return (get-content $appsettingsPath | out-string | convertfrom-json).Database.ConnectionString
}

function update-database([Parameter(ValueFromRemainingArguments)]$environments) {
    $migrationsProject =  (get-item -path .).Name
    $roundhouseExePath = find-dependency rh.exe
    $roundhouseOutputDir = [System.IO.Path]::GetDirectoryName($roundhouseExePath) + "\output"

    $migrationScriptsPath ="Scripts"
    $roundhouseVersionFile = "bin\$configuration\$targetFramework\$migrationsProject.dll"

    foreach ($environment in $environments) {
        $connectionString = $connectionStrings[$environment]

        write-host "Executing RoundhousE for environment:" $environment

        execute { & $roundhouseExePath --connectionstring $connectionString `
                                       --commandtimeout 300 `
                                       --env $environment `
                                       --output $roundhouseOutputDir `
                                       --sqlfilesdirectory $migrationScriptsPath `
                                       --versionfile $roundhouseVersionFile `
                                       --transaction `
                                       --silent }
    }
}

function rebuild-database([Parameter(ValueFromRemainingArguments)]$environments) {
    $migrationsProject = (get-item -path .).Name
    $roundhouseExePath = find-dependency rh.exe
    $roundhouseOutputDir = [System.IO.Path]::GetDirectoryName($roundhouseExePath) + "\output"

    $migrationScriptsPath ="Scripts"
    $roundhouseVersionFile = "bin\$configuration\$targetFramework\$migrationsProject.dll"

    foreach ($environment in $environments) {
        $connectionString = $connectionStrings[$environment]

        write-host "Executing RoundhousE for environment:" $environment

        execute { & $roundhouseExePath --connectionstring $connectionString `
                                       --commandtimeout 300 `
                                       --env $environment `
                                       --output $roundhouseOutputDir `
                                       --silent `
                                       --drop }

        execute { & $roundhouseExePath --connectionstring $connectionString `
                                       --commandtimeout 300 `
                                       --env $environment `
                                       --output $roundhouseOutputDir `
                                       --sqlfilesdirectory $migrationScriptsPath `
                                       --versionfile $roundhouseVersionFile `
                                       --transaction `
                                       --silent `
                                       --simple }
    }
}

function task($heading, $command, $path) {
    write-host
    write-host $heading -fore CYAN
    execute $command $path
}

function execute($command, $path) {
    if ($path -eq $null) {
        $global:lastexitcode = 0
        & $command
    } else {
        Push-Location $path
        $global:lastexitcode = 0
        & $command
        Pop-Location
    }

    if ($lastexitcode -ne 0) {
        throw "Error executing command:$command"
    }
}

function main($mainBlock) {
    try {
        &$mainBlock
        write-host
        write-host "Build Succeeded" -fore GREEN
        exit 0
    } catch [Exception] {
        write-host
        write-host $_.Exception.Message -fore DARKRED
        write-host
        write-host "Build Failed" -fore DARKRED
        exit 1
    }
}