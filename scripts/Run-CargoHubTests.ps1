param(
    [string]$ProjectPath = "",
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if ([string]::IsNullOrWhiteSpace($ProjectPath)) {
    $ProjectPath = Join-Path $repoRoot "CargoHub.Tests\CargoHub.Tests\CargoHub.Tests.csproj"
}
if (-not (Test-Path -LiteralPath $ProjectPath)) {
    throw "Testproject niet gevonden: $ProjectPath"
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet niet gevonden. Installeer de .NET SDK en probeer opnieuw."
}

$runId = Get-Date -Format "yyyyMMdd-HHmmss"
$reportDir = Join-Path $repoRoot "TestReports\$runId"
New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
$trxName = "CargoHub-tests.trx"
$trxPath = Join-Path $reportDir $trxName

Write-Host "CargoHub tests uitvoeren..." -ForegroundColor Cyan
$dotnetArgs = @('test', $ProjectPath, '--logger', "trx;LogFileName=$trxName", '--results-directory', $reportDir)
if (-not [string]::IsNullOrWhiteSpace($Filter)) {
    $dotnetArgs += @('--filter', $Filter)
}
& dotnet @dotnetArgs
$testExitCode = $LASTEXITCODE

if (-not (Test-Path -LiteralPath $trxPath)) {
    throw "Geen TRX-resultaat gevonden in $reportDir. dotnet exitcode: $testExitCode"
}

[xml]$trx = Get-Content -LiteralPath $trxPath -Raw
$definitions = @{}
foreach ($unit in $trx.SelectNodes("//*[local-name()='UnitTest']")) {
    $method = $unit.SelectSingleNode("*[local-name()='TestMethod']")
    if ($null -ne $method) {
        $definitions[$unit.GetAttribute('id')] = @{
            Class = [string]$method.GetAttribute('className')
            Method = [string]$method.GetAttribute('name')
        }
    }
}

$results = @()
foreach ($result in $trx.SelectNodes("//*[local-name()='UnitTestResult']")) {
    $testId = $result.GetAttribute('testId')
    $displayName = [string]$result.GetAttribute('testName')
    $testClass = ''
    $testMethod = ''
    if ($definitions.ContainsKey($testId)) {
        $testClass = $definitions[$testId].Class
        $testMethod = $definitions[$testId].Method
    }
    if ([string]::IsNullOrEmpty($testClass)) {
        $testClass = '(onbekend)'
    }
    if ([string]::IsNullOrEmpty($testMethod)) {
        $testMethod = $displayName
    }
    $kind = 'Overig'
    if ($testClass -match 'ApiTests$') { $kind = 'Endpoint/Integration' }
    elseif ($testClass -match 'ServiceTests$') { $kind = 'Service' }
    elseif ($testClass -match 'AccesTests$|AccessTests$') { $kind = 'DataAccess' }
    elseif ($testClass -match 'MiddlewareTests$') { $kind = 'Middleware' }

    $outcome = [string]$result.GetAttribute('outcome')
    $errorText = ''
    if ($outcome -ne 'Passed') {
        $msg = $result.SelectSingleNode("*[local-name()='Output']/*[local-name()='ErrorInfo']/*[local-name()='Message']")
        if ($null -ne $msg) { $errorText = ($msg.InnerText -replace '[\r\n]+', ' ').Trim() }
    }

    $results += [pscustomobject]@{
        TestClass = $testClass
        TestName = $testMethod
        Weergavenaam = $displayName
        Type = $kind
        Resultaat = $outcome
        Duur = [string]$result.GetAttribute('duration')
        Foutmelding = $errorText
    }
}

if ($results.Count -eq 0) { throw "TRX bevat geen testresultaten." }
$allCsv = Join-Path $reportDir 'Alle-testresultaten.csv'
$results | Sort-Object Type, TestClass, TestName | Export-Csv -LiteralPath $allCsv -NoTypeInformation -Encoding UTF8

$manifestFile = Join-Path $PSScriptRoot 'warehouse-scenarios.csv'
$manifest = @(Import-Csv -LiteralPath $manifestFile -Encoding UTF8)
$cases = @()
foreach ($scenario in $manifest) {
    $matchedResults = @($results | Where-Object {
        ($_.TestClass -eq $scenario.TestClass -or $_.TestClass.EndsWith('.' + $scenario.TestClass)) -and
        ($_.TestName -eq $scenario.TestName -or $_.TestName.StartsWith($scenario.TestName + '('))
    })
    $status = 'Niet uitgevoerd/gevonden'
    if ($matchedResults.Count -gt 0) {
        $failed = @($matchedResults | Where-Object { $_.Resultaat -ne 'Passed' })
        if ($failed.Count -eq 0) { $status = 'Geslaagd' } else { $status = 'Mislukt' }
    }
    $cases += [pscustomobject]@{
        CaseId = $scenario.CaseId
        Endpoint = $scenario.Endpoint
        Flow = $scenario.Flow
        Scenario = $scenario.Scenario
        Waarom = $scenario.Waarom
        TestClass = $scenario.TestClass
        TestName = $scenario.TestName
        VerwachteHttpStatus = $scenario.VerwachteHttpStatus
        Status = $status
    }
}
$cases | Export-Csv -LiteralPath (Join-Path $reportDir 'Warehouse-scenarios-resultaten.csv') -NoTypeInformation -Encoding UTF8

function Md([string]$s) {
    return (($s -replace '\|', '\|') -replace '[\r\n]+', ' ').Trim()
}
function Count-Statuses($items, [string]$flow, [string]$status) {
    return @($items | Where-Object { $_.Flow -eq $flow -and $_.Status -eq $status }).Count
}
$passed = @($results | Where-Object Resultaat -eq 'Passed').Count
$failed = @($results | Where-Object Resultaat -eq 'Failed').Count
$other = $results.Count - $passed - $failed

$lines = New-Object 'System.Collections.Generic.List[string]'
$lines.Add('# CargoHub - Integration- en endpointtestrapport')
$lines.Add('')
$lines.Add('**Uitgevoerd op:** ' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
$lines.Add('')
$lines.Add(('**Command:** `dotnet test`' + $(if ($Filter) { ' met filter `' + $Filter + '`' } else { ' (alle tests)' })))
$lines.Add('')
$lines.Add("**Uitkomst:** $passed geslaagd, $failed gefaald, $other overige (totaal $($results.Count)).")
$lines.Add('')
$lines.Add('**Let op:** dit rapport gaat over testscenario-dekking, niet over regel- of branchcoverage. De aantallen zijn gebaseerd op expliciet gedefinieerde scenario''s in `warehouse-scenarios.csv`.')
$lines.Add('')
$lines.Add('## Resultaten per soort test')
$lines.Add('')
$lines.Add('| Type | Geslaagd | Gefaald | Overige | Totaal |')
$lines.Add('| --- | ---: | ---: | ---: | ---: |')
foreach ($group in @($results | Group-Object Type | Sort-Object Name)) {
    $groupPass = @($group.Group | Where-Object Resultaat -eq 'Passed').Count
    $groupFail = @($group.Group | Where-Object Resultaat -eq 'Failed').Count
    $lines.Add("| $($group.Name) | $groupPass | $groupFail | $($group.Count - $groupPass - $groupFail) | $($group.Count) |")
}
$lines.Add('')
$lines.Add('## Warehouses - happy en alternative flows')
$lines.Add('')
$lines.Add('De noemer is het aantal geplande scenario''s. De teller is het aantal geslaagde tests. Een mislukte of ontbrekende test telt niet als gedekt.')
$lines.Add('')
$lines.Add('| Endpoint | Happy flow | Alternative flow | Totaal |')
$lines.Add('| --- | ---: | ---: | ---: |')
foreach ($group in @($cases | Group-Object Endpoint | Sort-Object { [array]::IndexOf(@('GET /api/v1/warehouses', 'GET /api/v1/warehouses/{id}', 'POST /api/v1/warehouses', 'PUT /api/v1/warehouses/{id}', 'DELETE /api/v1/warehouses/{id}', 'GET /api/v1/warehouses/{id}/locations'), $_.Name) })) {
    $hp = Count-Statuses $group.Group 'Happy' 'Geslaagd'
    $ap = Count-Statuses $group.Group 'Alternative' 'Geslaagd'
    $ht = @($group.Group | Where-Object Flow -eq 'Happy').Count
    $at = @($group.Group | Where-Object Flow -eq 'Alternative').Count
    $lines.Add("| $(Md $group.Name) | $hp/$ht | $ap/$at | $($hp + $ap)/$($ht + $at) |")
}
$happyPass = Count-Statuses $cases 'Happy' 'Geslaagd'
$altPass = Count-Statuses $cases 'Alternative' 'Geslaagd'
$happyTotal = @($cases | Where-Object Flow -eq 'Happy').Count
$altTotal = @($cases | Where-Object Flow -eq 'Alternative').Count
$lines.Add("| **Totaal** | **$happyPass/$happyTotal** | **$altPass/$altTotal** | **$($happyPass + $altPass)/$($happyTotal + $altTotal)** |")
$lines.Add('')
$lines.Add('## Waarom deze alternative flows?')
$lines.Add('')
$lines.Add('De gekozen alternatives controleren ongeldige invoer (400), een niet-bestaand warehouse (404) en het voorkomen van verwijderen bij gekoppelde locations (409). Deze situaties komen in echte API-aanroepen voor en beschermen tegen foutieve opslag, misleidende succesresponses en geschonden relaties.')
$lines.Add('')
$lines.Add('## Warehouse scenario''s met testresultaat')
$lines.Add('')
$lines.Add('| ID | Flow | Scenario | Verwacht HTTP | xUnit-test | Resultaat |')
$lines.Add('| --- | --- | --- | ---: | --- | --- |')
foreach ($c in $cases) {
    $lines.Add("| $($c.CaseId) | $($c.Flow) | $(Md $c.Scenario) | $($c.VerwachteHttpStatus) | $($c.TestName) | $($c.Status) |")
}
$lines.Add('')
$lines.Add('## Nog niet afgedekt (Warehouses)')
$lines.Add('')
$missingCases = @($cases | Where-Object Status -ne 'Geslaagd')
if ($missingCases.Count -eq 0) {
    $lines.Add('Alle 14 geplande Warehouse-scenario''s zijn geslaagd.')
} else {
    foreach ($c in $missingCases) { $lines.Add("- **$($c.CaseId)** - $($c.Scenario): $($c.Status).") }
}
$lines.Add('')
$lines.Add('**Buiten scope van deze schatting:** andere velden met ongeldige invoer, grenswaarden, gelijktijdige requests, performance, security en onverwachte databasefouten. De scenario''s zijn een gekozen steekproef en geen bewijs dat alle mogelijke paden getest zijn.')
$lines.Add('')
$lines.Add('## Alle uitgevoerde tests per testklasse')
$lines.Add('')
$lines.Add('| Testklasse | Soort | Geslaagd | Gefaald | Totaal |')
$lines.Add('| --- | --- | ---: | ---: | ---: |')
foreach ($g in @($results | Group-Object TestClass | Sort-Object Name)) {
    $p = @($g.Group | Where-Object Resultaat -eq 'Passed').Count
    $f = @($g.Group | Where-Object Resultaat -eq 'Failed').Count
    $lines.Add("| $(Md $g.Name) | $($g.Group[0].Type) | $p | $f | $($g.Count) |")
}
$lines.Add('')
$lines.Add('Het volledige testoverzicht met iedere testnaam, duur, uitkomst en eventuele foutmelding staat in `Alle-testresultaten.csv`. De exacte warehouse-scenariomapping staat in `Warehouse-scenarios-resultaten.csv`. Voor andere API-modules moet eerst een vergelijkbare scenario-CSV worden opgesteld om een betrouwbare happy/alternative-coverage te berekenen.')

$mdPath = Join-Path $reportDir 'Testverslag.md'
$lines | Set-Content -LiteralPath $mdPath -Encoding UTF8
Write-Host "`nRapport opgeslagen in: $reportDir" -ForegroundColor Green
Write-Host "- Testverslag.md (samenvatting en flowdekking)"
Write-Host "- Alle-testresultaten.csv (alle uitgevoerde tests)"
Write-Host "- Warehouse-scenarios-resultaten.csv (scenario's en resultaat)"
Write-Host "- CargoHub-tests.trx (volledige xUnit-data)"
exit $testExitCode
