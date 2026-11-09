$baseUrl = "http://localhost:3000"
$apiName = "python_orders_shipments"

$endpoints = @(
    "/api/v1/orders",
    "/api/v1/shipments"
)

$levels = @(1, 5, 10, 20)
$totalRequests = 20

$levels = @(1, 5, 10, 20)
$totalRequests = 20

$apiKey = $global:cargoHubApiKey

if ([string]::IsNullOrWhiteSpace($apiKey) -or $apiKey.Length -ne 19) {
    throw "API-key ontbreekt of heeft een ongeldige lengte."
}

Add-Type -AssemblyName System.Net.Http

$client = New-Object System.Net.Http.HttpClient
$client.Timeout = [TimeSpan]::FromSeconds(300)

if ($apiKey) {
    $client.DefaultRequestHeaders.Add("API_KEY", $apiKey)
}

$results = @()
$rawResults = @()

try {
    foreach ($endpoint in $endpoints) {

        $url = "$baseUrl$endpoint"

        foreach ($concurrency in $levels) {

            Write-Host "Test: $endpoint | Concurrency: $concurrency"

            $timer = [System.Diagnostics.Stopwatch]::StartNew()
            $measurements = @()
            $completed = 0

            while ($completed -lt $totalRequests) {

                $batchSize = [Math]::Min(
                    $concurrency,
                    $totalRequests - $completed
                )

                $tasks = @()

                for ($i = 0; $i -lt $batchSize; $i++) {
                    $requestTimer = [System.Diagnostics.Stopwatch]::StartNew()
                    $task = $client.GetAsync(
                        $url,
                        [System.Net.Http.HttpCompletionOption]::ResponseContentRead
                    )

                    $tasks += [PSCustomObject]@{
                        Task = $task
                        Timer = $requestTimer
                    }
                }

                foreach ($item in $tasks) {
                    $status = 0

                    try {
                        $response = $item.Task.GetAwaiter().GetResult()
                        $status = [int]$response.StatusCode
                        $response.Dispose()
                    }
                    catch {
                        Write-Warning $_.Exception.Message
                    }
                    finally {
                        $item.Timer.Stop()
                    }

                    $measurements += [PSCustomObject]@{
                        Status = $status
                        TimeMs = [Math]::Round(
                            $item.Timer.Elapsed.TotalMilliseconds, 2
                        )
                    }
                }

                $completed += $batchSize
            }

            $timer.Stop()

            $successful = @(
                $measurements | Where-Object Status -eq 200
            ).Count

            $duration = $timer.Elapsed.TotalSeconds

            $average = (
                $measurements |
                Measure-Object TimeMs -Average
            ).Average

            $results += [PSCustomObject]@{
                Endpoint = $endpoint
                Concurrency = $concurrency
                TotalRequests = $totalRequests
                Successful = $successful
                Failed = $totalRequests - $successful
                AverageMs = [Math]::Round($average, 2)
                TotalSeconds = [Math]::Round($duration, 2)
                RequestsPerSecond = [Math]::Round(
                    $totalRequests / $duration, 2
                )
            }

            $requestNumber = 0

            foreach ($measurement in $measurements) {
                $requestNumber++

                $rawResults += [PSCustomObject]@{
                    Endpoint = $endpoint
                    Concurrency = $concurrency
                    Request = $requestNumber
                    Status = $measurement.Status
                    TimeMs = $measurement.TimeMs
                }
            }
        }
    }
}
finally {
    $client.Dispose()
}

$results | Export-Csv "concurrency_$apiName.csv" -NoTypeInformation

$rawResults | Export-Csv "concurrency_${apiName}_raw.csv" -NoTypeInformation

$results | Format-Table -AutoSize
