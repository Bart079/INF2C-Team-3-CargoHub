import requests
import pytest

API_ROOT = "http://localhost:3000/api/v1"

API_KEY_ANALYTICS_DASHBOARD = "d4s2a0b0a1n4a0l0y7t"
#server must respond 401 Unauthorized if no API_KEY header is provided
@pytest.mark.run(order=1)
def test_api_endpoint():
    response = requests.get(f"{API_ROOT}")
    assert response.status_code == 401

# Test for unauthorized access warehouses with no API_KEY header
@pytest.mark.run(order=2)
def test_unauthorized_warehouses():
    response = requests.get(f"{API_ROOT}/warehouses")
    assert response.status_code == 401

# Test for unauthorized access warehouses with wrong API_KEY header
@pytest.mark.run(order=3)
def test_unauthorized_warehouses_with_wrong_key():
    headers = {"API_KEY": "not-a-valid-key"}
    response = requests.get(f"{API_ROOT}/warehouses", headers=headers)
    assert response.status_code == 401

# Test for authorized access warehouses with user analytics_dashboard
@pytest.mark.run(order=4)
def test_authorized_warehouses():
    # Valid API_KEY of analytics_dashboard -> server must respond 200 OK
    headers = {"API_KEY": API_KEY_ANALYTICS_DASHBOARD}
    response = requests.get(f"{API_ROOT}/warehouses", headers=headers)
    assert response.status_code == 200

# Test for fetching all warehouses with user analytics_dashboard

#-----------------------------------------------------------

# @pytest.mark.run(order=5)
# def test_fetch_all_warehouses():
#     headers = {"API_KEY": API_KEY_ANALYTICS_DASHBOARD}
#     response = requests.get(f"{API_ROOT}/warehouses", headers=headers)
#     assert response.status_code == 200

#     warehouses = response.json()
#     assert isinstance(warehouses, list)
#     # There must be at least one warehouse and every row has an id
#     assert len(warehouses) > 0
#     assert all("id" in warehouse for warehouse in warehouses)

#-----------------------------------------------------------

# Test for fetching a particular warehouse with user analytics_dashboard

#-----------------------------------------------------------

# @pytest.mark.run(order=6)
# def test_fetch_warehouse_by_id():
#     headers = {"API_KEY": API_KEY_ANALYTICS_DASHBOARD}
#     response = requests.get(f"{API_ROOT}/warehouses/1", headers=headers)
#     assert response.status_code == 200

#     warehouse = response.json()
#     assert isinstance(warehouse, dict)
#     assert warehouse["id"] == 1

#-----------------------------------------------------------

# Which scenario's could we test next?

#-----------------------------------------------------------

# - No API_KEY header at all            -> 401
# - Unknown/invalid API_KEY             -> 401
# - Valid key, resource not permitted   -> 403 (has_access returns False)
# - Valid key, correct method permitted -> 200 with JSON body
# - GET /warehouses/<unknown id>        -> 200 with null (server does not 404)
# - Wrong method, e.g. POST warehouses  -> 403 (analytics_dashboard has post: false)

# ...

# Tip: construct your own data setup to test specifically
#
# - A user.json list with an app with full access
# - An empty warehouse.json such that you can check basic CRUD actions
# ...