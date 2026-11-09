
import csv
import time
import getpass
import statistics
import urllib.request
import urllib.error
from concurrent.futures import ThreadPoolExecutor, as_completed

# Kies "python" of "dotnet"
API_NAME = "python"

BASE_URL = (
    "http://localhost:3000"
    if API_NAME == "python"
    else "http://localhost:5029"
)

ENDPOINT = "/api/v1/clients/1/orders"

CONCURRENCY_LEVELS = [1, 2, 5]
TOTAL_REQUESTS = 5

api_key = (
    getpass.getpass("Voer je API-key in: ")
    if API_NAME == "python"
    else ""
)

headers = {"API_KEY": api_key} if api_key else {}

def send_request(number):
    start = time.perf_counter()
    status = 0
    error = ""

    try:
        request = urllib.request.Request(
            BASE_URL + ENDPOINT,
            headers=headers
        )

        with urllib.request.urlopen(request, timeout=360) as response:
            status = response.status
            response.read()

    except urllib.error.HTTPError as e:
        status = e.code
        e.read()
    except Exception as e:
        error = str(e)

    return {
        "request": number,
        "status": status,
        "time_ms": round(
            (time.perf_counter() - start) * 1000, 2
        ),
        "error": error
    }


summary = []
raw = []

for concurrency in CONCURRENCY_LEVELS:
    print(f"\nConcurrency: {concurrency}")

    start = time.perf_counter()

    with ThreadPoolExecutor(max_workers=concurrency) as executor:
        futures = [
            executor.submit(send_request, i)
            for i in range(1, TOTAL_REQUESTS + 1)
        ]

        measurements = [
            future.result()
            for future in as_completed(futures)
        ]

    duration = time.perf_counter() - start

    for result in measurements:
        raw.append({
            "concurrency": concurrency,
            **result
        })

    successful = [
        r for r in measurements if r["status"] == 200
    ]

    average = (
        statistics.mean(r["time_ms"] for r in successful)
        if successful else 0
    )

    result = {
        "endpoint": ENDPOINT,
        "concurrency": concurrency,
        "total": TOTAL_REQUESTS,
        "successful": len(successful),
        "failed": TOTAL_REQUESTS - len(successful),
        "average_ms": round(average, 2),
        "total_seconds": round(duration, 2),
        "requests_per_second": round(
            TOTAL_REQUESTS / duration, 2
        )
    }

    summary.append(result)
    print(result)


with open(f"concurrency_{API_NAME}_light.csv", "w", newline="") as f:
    writer = csv.DictWriter(f, fieldnames=summary[0].keys())
    writer.writeheader()
    writer.writerows(summary)

with open(f"concurrency_{API_NAME}_light_raw.csv", "w", newline="") as f:
    writer = csv.DictWriter(f, fieldnames=raw[0].keys())
    writer.writeheader()
    writer.writerows(raw)

print("\nTests afgerond!")
