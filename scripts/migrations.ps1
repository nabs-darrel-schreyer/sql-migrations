# Store the script's directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Change to the script directory to ensure relative paths work correctly
Push-Location $ScriptDir

try {
    # Navigate to the repository root (one level up from scripts folder)
    $RepoRoot = Split-Path -Parent $ScriptDir
    
    # Create database folder if it doesn't exist
    $DatabaseFolder = Join-Path $RepoRoot "database"
    if (-not (Test-Path $DatabaseFolder)) {
        New-Item -ItemType Directory -Path $DatabaseFolder | Out-Null
        Write-Host "Created database folder: $DatabaseFolder" -ForegroundColor Green
    }
    
    # Find all .csproj files in the repository
    $ProjectFiles = Get-ChildItem -Path $RepoRoot -Filter "*.csproj" -Recurse
    
    Write-Host "Found $($ProjectFiles.Count) project(s) in the repository" -ForegroundColor Cyan
    
    # Track projects with DbContext
    $ProjectsWithDbContext = @()
    
    foreach ($ProjectFile in $ProjectFiles) {
        $ProjectDir = Split-Path -Parent $ProjectFile.FullName
        $ProjectName = $ProjectFile.BaseName
        
        # Search for files containing DbContext inheritance in the project directory
        $CsFiles = Get-ChildItem -Path $ProjectDir -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue
        
        $HasDbContext = $false
        foreach ($CsFile in $CsFiles) {
            $Content = Get-Content -Path $CsFile.FullName -Raw -ErrorAction SilentlyContinue
            # Look for class inheritance from DbContext (: DbContext)
            if ($Content -match '\s*:\s*DbContext(\s|<|\()') {
                $HasDbContext = $true
                Write-Host "Found DbContext in project: $ProjectName" -ForegroundColor Yellow
                break
            }
        }
        
        if ($HasDbContext) {
            $ProjectsWithDbContext += @[
                Name = $ProjectName
                Path = $ProjectFile.FullName
            ]
        }
    }
    
    if ($ProjectsWithDbContext.Count -eq 0) {
        Write-Host "No projects with DbContext found!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "`nGenerating migration scripts for $($ProjectsWithDbContext.Count) project(s)..." -ForegroundColor Cyan
    
    # Generate migration script for each project
    foreach ($Project in $ProjectsWithDbContext) {
        $ProjectName = $Project.Name
        $ProjectPath = $Project.Path
        $OutputFile = Join-Path $DatabaseFolder "$ProjectName.sql"
        
        Write-Host "`nGenerating migration script for: $ProjectName" -ForegroundColor Green
        Write-Host "Output file: $OutputFile" -ForegroundColor Gray
        
        # Generate the migration script and save to file
        dotnet ef migrations script --project $ProjectPath --output $OutputFile --idempotent
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully generated migration script for $ProjectName" -ForegroundColor Green
        } else {
            Write-Host "Failed to generate migration script for $ProjectName" -ForegroundColor Red
        }
    }
    
    Write-Host "`nMigration script generation complete!" -ForegroundColor Cyan
    Write-Host "Scripts location: $DatabaseFolder" -ForegroundColor Cyan
}
finally {
    # Restore the original directory
    Pop-Location
}
