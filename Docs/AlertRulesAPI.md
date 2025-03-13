# Prometheus Alert Rules API Documentation

## Overview
The Prometheus Alert Rules API allows managing alert rules for monitoring system conditions. It provides endpoints to:

- Retrieve all alert rules
- Fetch a specific alert rule by name
- Create a new alert rule
- Update an existing alert rule
- Delete an alert rule

All endpoints require authentication via a Bearer Token.

## Base URL
```
http://localhost:5057/api/alerts
```

---

## Endpoints

### 1. Get All Alert Rules

#### Description
Fetches all defined Prometheus alert rules.

#### Endpoint:
```
GET /api/alerts/rules
```

#### Request:
```sh
curl -X GET "http://localhost:5057/api/alerts/rules" \
-H "Authorization: Bearer <your_token>"
```

#### Response (JSON):
```json
{
    "success": true,
    "message": "Alert rules retrieved successfully.",
    "data": [
        {
            "alert": "HighCPUUsage",
            "expr": "cpu_usage > 80",
            "duration": "5m",
            "labels": {
                "severity": "critical"
            },
            "annotations": {
                "summary": "CPU usage is too high"
            }
        }
    ]
}
```

---

### 2. Get Alert Rule by Name

#### Description
Fetches a specific alert rule by name.

#### Endpoint:
```
GET /api/alerts/rules/{alertName}
```

#### Request:
```sh
curl -X GET "http://localhost:5057/api/alerts/rules/HighCPUUsage" \
-H "Authorization: Bearer <your_token>"
```

#### Response (JSON):
```json
{
    "success": true,
    "message": "Alert rules retrieved successfully.",
    "data": [
        {
            "alert": "HighCPUUsage",
            "expr": "cpu_usage > 80",
            "duration": "5m",
            "labels": {
                "severity": "critical"
            },
            "annotations": {
                "summary": "CPU usage is too high"
            }
        }
    ]
}
```

---

### 3. Create Alert Rule

#### Description
Creates a new Prometheus alert rule.

#### Endpoint:
```
POST /api/alerts/rules
```

#### Request:
```sh
curl -X POST "http://localhost:5057/api/alerts/rules" \
-H "Authorization: Bearer <your_token>" \
-H "Content-Type: application/json" \
-d '{
    "alert": "HighMemoryUsage",
    "expr": "memory_usage > 90",
    "duration": "5m",
    "labels": {
        "severity": "critical"
    },
    "annotations": {
        "summary": "Memory usage is critically high"
    }
}'
```

#### Request Body (JSON):
```json
{
    "alert": "HighMemoryUsage",
    "expr": "memory_usage > 90",
    "duration": "5m",
    "labels": {
        "severity": "critical"
    },
    "annotations": {
        "summary": "Memory usage is critically high"
    }
}
```

#### Response (JSON):
```json
{
    "success": true,
    "message": "Alert rule created successfully.",
    "data": {
        "alert": "HighMemoryUsage",
        "expr": "memory_usage > 90",
        "duration": "5m",
        "labels": {
            "severity": "critical"
        },
        "annotations": {
            "summary": "Memory usage is critically high"
        }
    }
}
```

---

### 4. Update Alert Rule

#### Description
Updates an existing Prometheus alert rule.

#### Endpoint:
```
PUT /api/alerts/rules/{alertName}
```

#### Request:
```sh
curl -X PUT "http://localhost:5057/api/alerts/rules/HighMemoryUsage" \
-H "Authorization: Bearer <your_token>" \
-H "Content-Type: application/json" \
-d '{
    "expr": "memory_usage > 95",
    "duration": "10m",
    "labels": {
        "severity": "warning"
    },
    "annotations": {
        "summary": "Memory usage is high"
    }
}'
```

#### Request Body (JSON):
```json
{
    "expr": "memory_usage > 95",
    "duration": "10m",
    "labels": {
        "severity": "warning"
    },
    "annotations": {
        "summary": "Memory usage is high"
    }
}
```

#### Response (JSON):
```json
{
    "success": true,
    "message": "Alert rule updated successfully.",
    "data": {
        "alert": "HighMemoryUsage",
        "expr": "memory_usage > 95",
        "duration": "10m",
        "labels": {
            "severity": "warning"
        },
        "annotations": {
            "summary": "Memory usage is high"
        }
    }
}
```

---

### 5. Delete Alert Rule

#### Description
Deletes an alert rule by name.

#### Endpoint:
```
DELETE /api/alerts/rules/{alertName}
```

#### Request:
```sh
curl -X DELETE "http://localhost:5057/api/alerts/rules/HighMemoryUsage" \
-H "Authorization: Bearer <your_token>"
```

#### Response (JSON):
```json
{
    "success": true,
    "message": "Alert rule deleted successfully.",
    "data": true
}
```

