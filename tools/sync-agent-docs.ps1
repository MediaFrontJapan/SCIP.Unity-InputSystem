param(
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot)
)

$resolvedRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$source = Join-Path $resolvedRoot "docs\AGENT_GUIDE.md"

if (-not (Test-Path -LiteralPath $source)) {
    throw "Source file not found: $source"
}

$targets = @(
    (Join-Path $resolvedRoot "AGENTS.md"),
    (Join-Path $resolvedRoot "CLAUDE.md"),
    (Join-Path $resolvedRoot "GEMINI.md"),
    (Join-Path $resolvedRoot ".github\copilot-instructions.md")
)

$notice = "<!-- Generated from docs/AGENT_GUIDE.md by tools/sync-agent-docs.ps1. Edit the source file instead. -->`n`n"
$sourceContent = Get-Content -Raw -LiteralPath $source -Encoding UTF8
$utf8WithBom = [System.Text.UTF8Encoding]::new($true)

foreach ($target in $targets) {
    $directory = Split-Path -Parent $target
    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    [System.IO.File]::WriteAllText($target, $notice + $sourceContent, $utf8WithBom)
}

Write-Host "Synchronized agent markdown files:"
foreach ($target in $targets) {
    Write-Host " - $target"
}
