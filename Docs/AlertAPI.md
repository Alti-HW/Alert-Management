# Alert Management API Documentation

## Overview
The Alert Management API handles incoming alerts from Prometheus and provides endpoints to:

- Receive and process alerts
- Retrieve all active alerts
- Resolve an alert
- Delete an alert

All endpoints require authentication via a Bearer Token.

## Base URL
```
http://localhost:5057/api/alerts
```

---

## Endpoints

### 1. Receive Alert (Backend Only)

#### Description
Receives alerts from Prometheus and processes them in the system.

#### Endpoint:
```
POST /api/alerts/receive
```

#### Request:
```sh
curl -X POST "http://localhost:5057/api/alerts/receive" \
-H "Authorization: Bearer <your_token>" \
-H "Content-Type: application/json" \
-d '{
    "alerts": [
        {
            "status": "firing",
            "labels": {
                "alertname": "HighCPUUsage",
                "instance": "server1"
            },
            "annotations": {
                "description": "CPU usage exceeded 80%"
            }
        }
    ]
}'
```

#### Request Body (JSON):
```json
{
    "alerts": [
        {
            "status": "firing",
            "labels": {
                "alertname": "HighCPUUsage",
                "instance": "server1"
            },
            "annotations": {
                "description": "CPU usage exceeded 80%"
            }
        }
    ]
}
```

#### Response (JSON):
```json
{
    "success": true,
    "message": "Alert processed successfully",
    "data": null
}
```

---

### 2. Get All Alerts

#### Description
Retrieves a list of all active alerts.

#### Endpoint:
```
GET /api/alerts/get_alerts
```

#### Request:
```sh
curl -X GET "http://localhost:5057/api/alerts/get_alerts" \
-H "Authorization: Bearer <your_token>"
```

#### Response (JSON):
```json
{
    "success": true,
    "message": "Alerts retrieved successfully",
    "data": [
        {
            "id": "123e4567-e89b-12d3-a456-426614174000",
            "applicationId": "e6f01516-dcdf-4970-9133-69ab5477f082",
            "ruleId": "5f8280dd-3168-413f-a6e9-093de3d06dd0",
            "message": "CPU usage exceeded 80%",
            "status": "firing",
            "triggeredAt": "2024-03-12T12:00:00Z"
        }
    ]
}
```

---

### 3. Resolve Alert

#### Description
Marks a specific alert as resolved.

#### Endpoint:
```
POST /api/alerts/resolve_alert/{alertId}
```

#### Request:
```sh
curl -X POST "http://localhost:5057/api/alerts/resolve_alert/123e4567-e89b-12d3-a456-426614174000" \
-H "Authorization: Bearer <your_token>"
```

#### Response (JSON):
```json
{
    "success": true,
    "message": "Alert resolved successfully",
    "data": null
}
```

---

### 4. Delete Alert

#### Description
Deletes a specific alert.

#### Endpoint:
```
DELETE /api/alerts/delete_alert/{alertId}
```

#### Request:
```sh
curl -X DELETE "http://localhost:5057/api/alerts/delete_alert/123e4567-e89b-12d3-a456-426614174000" \
-H "Authorization: Bearer <your_token>"
```

#### Response (JSON):
```json
{
    "success": true,
    "message": "Alert deleted successfully",
    "data": null
}
```

---

## Model Definitions

### 1. Alert Model
| Field          | Type    | Required | Description                        |
|---------------|--------|----------|------------------------------------|
| id           | string | Yes      | Unique identifier for the alert.  |
| applicationId | string | Yes      | The application ID associated with the alert. |
| ruleId       | string | Yes      | The alert rule ID that triggered the alert. |
| message      | string | Yes      | Detailed description of the alert. |
| status       | string | Yes      | Status of the alert (`firing`, `resolved`). |
| triggeredAt  | string | Yes      | Timestamp when the alert was triggered. |

---

## Authentication
All endpoints require authentication using a Bearer Token. Include the token in the Authorization header as follows:

```sh
-H "Authorization: Bearer <your_token>"
```

---

## Error Handling
- If an invalid or missing `alertId` is provided in a delete or resolve request, the API returns an error.
- If a request body is missing required fields, the API returns an error with a descriptive message.

