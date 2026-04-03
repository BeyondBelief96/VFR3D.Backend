# VFR3D API Company Pivot Strategy

## Overview

This document outlines the strategy for pivoting VFR3D.Backend from a frontend-serving API into a standalone aviation data API product. The core value proposition: aviation data is notoriously fragmented (FAA fixed-width text files, NOAA inconsistent APIs, ArcGIS pagination) and developers building aviation apps would pay to not deal with this themselves.

**Target customers:** EFB developers, drone operators, flight school software, FBO management platforms, aviation startups.

**Comparable companies:** AeroAPI (FlightAware), AviationStack, CheckWX. VFR3D would carve a niche in VFR/GA-focused data which is underserved.

---

## Current State Assessment

### What We Have (Core Asset)

The data ingestion pipeline and normalized data layer is the most valuable and hardest-to-replicate component. This is approximately 60-70% of what's needed for an API company.

| Category | Count | Details |
|----------|-------|---------|
| Controllers | 20 | Airport, Metar, Taf, Pirep, Airspace, Obstacle, Notam, Flight, Aircraft, etc. |
| Domain Entities | 22 | Airport, Flight, Aircraft, Metar, Airspace, Obstacle, etc. |
| External Data Sources | 6 | NOAA, FAA NMS, FAA NASR, ArcGIS, Azure Blob, Auth0 |
| API Endpoints | ~100+ | Across all controllers (GET, POST, PATCH, DELETE) |
| Database | PostgreSQL + PostGIS | Full spatial data support |

### Data Endpoints (Keep As-Is for API Product)

These are the revenue-generating endpoints — normalized aviation data:

- **Weather:** METAR, TAF, PIREP, AIRMET/SIGMET, G-AIRMET
- **Airports:** FAA NASR airport data, runways, runway ends, communication frequencies
- **Airspace:** Controlled airspace boundaries (PostGIS polygons), special use airspace
- **Obstacles:** Searchable by radius, bounding box, state, OAS number
- **NOTAMs:** By airport, radius, and flight route corridor
- **Navigation:** Bearing/distance calculations, magnetic variation, winds aloft
- **Charts:** Airport diagrams, chart supplements (PDF via Azure Blob SAS URLs)

### User-Scoped Endpoints (Restructure or Separate)

These are features built for the VFR3D frontend app and are user-specific:

- **Flight planning:** Flight CRUD, navlog calculation/regeneration
- **Aircraft management:** Aircraft CRUD, performance profiles, documents
- **Weight & balance:** Profiles and calculations

**Options:**
1. Drop them entirely (pure data API)
2. Keep as a premium "Flight Planning API" tier
3. Separate into a different API product or microservice

### Current Gaps

| Gap | Current State | Impact |
|-----|--------------|--------|
| No API key auth | Auth0 JWT only (end-user auth) | Can't onboard API customers |
| No rate limiting | Zero throttling | Single bad actor can take down the service |
| No caching layer | Every request hits PostgreSQL | Can't scale beyond a few hundred concurrent users |
| No pagination | Full result sets returned | Large queries will timeout or OOM |
| No usage tracking | No request logging per customer | Can't bill usage-based plans |
| No API versioning | Unversioned routes | Breaking changes break all customers |
| No developer portal | N/A | No self-service onboarding |

---

## What Goes Into an API Company

### 1. API Key Management & Tiered Access

Replace Auth0 JWT (end-user auth) with API key authentication for data endpoints:

```
Current:  Client -> Auth0 JWT -> Your API
New:      Client -> API Key (X-Api-Key header) -> Your API
```

**Required components:**
- `ApiKey` entity (key hash, customer ID, plan tier, created/expires dates, active flag)
- `Customer` entity (company name, email, billing info, Stripe customer ID)
- `ApiKeyAuthenticationHandler` middleware that resolves key to customer/plan
- Key generation, rotation, and revocation endpoints (management API)

**Tiered plans example:**

| Tier | Rate Limit | Endpoints | Price |
|------|-----------|-----------|-------|
| Free | 100 req/day | Weather, Airports only | $0 |
| Developer | 10,000 req/day | All data endpoints | $49/mo |
| Professional | 100,000 req/day | All endpoints + Flight Planning API | $199/mo |
| Enterprise | Custom | All + SLA + support | Custom |

### 2. Rate Limiting & Throttling

**Implementation options:**
- `AspNetCoreRateLimit` NuGet package (simple, config-driven)
- Custom Redis-backed sliding window (more control)
- Azure API Management policies (if using APIM gateway)

**Rate limiting dimensions:**
- Per API key (overall daily/monthly quota)
- Per endpoint (expensive endpoints like `/navlog/calculate` cost more)
- Burst protection (sliding window or token bucket)

### 3. Documentation

- **OpenAPI/Swagger spec** generated from controllers (Swashbuckle)
- **Interactive API docs** (Swagger UI, Redoc, or custom portal)
- **Code examples** in Python, JavaScript, C#, Go, cURL
- **Guides** for common use cases (get weather for a flight route, find nearby airports, etc.)
- **Changelog** for API versions

### 4. Billing

Reactivate existing Stripe integration with:
- Usage-based billing or fixed tier subscriptions
- Overage charges or hard cutoffs at quota
- Invoice generation
- Webhook handlers for subscription lifecycle events

### 5. Legal & Compliance

- **Terms of Service** for API usage
- **Data attribution:** FAA data is public domain; NOAA has attribution requirements
- **SLA commitments** per tier (99.9% uptime for Enterprise, best-effort for Free)
- **Privacy policy** for customer data
- **Acceptable use policy** (prevent abuse, reselling restrictions)

---

## Required Code Changes

### Phase 1: API Key Authentication

Create new entities:

```
Domain/Entities/Customer.cs
Domain/Entities/ApiKey.cs
Domain/Entities/ApiUsageRecord.cs
Domain/Enums/SubscriptionTier.cs
```

Create new auth handler:

```
API/Authentication/ApiKeyAuthenticationHandler.cs
API/Authentication/ApiKeyAuthenticationOptions.cs
```

Add middleware:

```
API/Middleware/UsageTrackingMiddleware.cs
API/Middleware/RateLimitingMiddleware.cs
```

### Phase 2: API Versioning

Prefix all data routes with `/v1/`:

```
/api/metar/{icao}         -> /api/v1/metar/{icao}
/api/airport/{icao}       -> /api/v1/airport/{icao}
/api/airspace/by-classes  -> /api/v1/airspace/by-classes
```

Use `Asp.Versioning.Mvc` NuGet package for proper API versioning support.

### Phase 3: Response Standardization

Wrap all responses in a consistent envelope:

```json
{
  "data": { ... },
  "meta": {
    "requestId": "uuid",
    "timestamp": "ISO 8601",
    "version": "v1",
    "pagination": {
      "cursor": "abc123",
      "hasMore": true,
      "limit": 50
    }
  },
  "usage": {
    "requestsRemaining": 9842,
    "resetAt": "ISO 8601"
  }
}
```

### Phase 4: Pagination

Add cursor-based pagination to all list endpoints. Priority endpoints:

1. `GET /api/v1/airport` (thousands of airports)
2. `GET /api/v1/obstacle/search` (currently hard-capped at 500/5000)
3. `GET /api/v1/airspace/by-classes` (large geometry payloads)
4. `GET /api/v1/pirep` (returns all PIREPs)
5. `GET /api/v1/metar/state/{stateCode}` (hundreds per state)

### Phase 5: Caching Layer

Add Redis caching for hot data:

| Data Type | Cache TTL | Rationale |
|-----------|----------|-----------|
| METARs | 10 minutes | Matches cron refresh cycle |
| TAFs | 30 minutes | Updates less frequently |
| Airports | 24 hours | NASR data changes on 56-day cycle |
| Airspace | 24 hours | Rarely changes |
| Obstacles | 24 hours | Rarely changes |
| Comm Frequencies | 24 hours | Rarely changes |
| NOTAMs | 5 minutes | Time-sensitive |
| PIREPs | 5 minutes | Time-sensitive |

---

## Scaling Strategy

### Current Architecture Bottlenecks

| Bottleneck | Current State | Solution |
|---|---|---|
| Database | Single PostgreSQL instance | Read replicas, connection pooling (PgBouncer), caching layer |
| No caching | Every request hits DB | Redis/Azure Cache for hot data |
| No rate limiting | Unlimited requests | Middleware + Redis-backed sliding window |
| Single API instance | One App Service | Azure App Service scale-out or AKS |
| Cron jobs share DB | Same DB, no isolation | Separate read replicas; cron writes to primary, API reads from replica |
| No pagination | Full result sets returned | Cursor-based pagination on all list endpoints |

### Phase 1 — Hundreds of Customers (~$100-200/mo additional)

**Azure App Service + Redis**

- Add Azure Cache for Redis (C0 Basic: ~$17/mo or C1 Standard: ~$85/mo)
- Cache weather data, airport data, airspace data with appropriate TTLs
- Add API key auth + rate limiting via middleware
- Add response caching headers (ETag, Cache-Control)
- Add pagination to all list endpoints
- Scale App Service to S2 or P1v3 if needed

### Phase 2 — Thousands of Customers (~$500-1,500/mo)

**Scale Out + Read Replicas**

- Azure App Service auto-scaling (2-4 instances behind load balancer)
- PostgreSQL read replicas (Azure Flexible Server supports this natively)
- PgBouncer for connection pooling (built into Azure Flexible Server)
- Move Azure Functions cron jobs to their own App Service Plan to isolate write load
- CDN for static/semi-static data (airport info, airspace boundaries)
- Upgrade Redis to Standard tier for replication

### Phase 3 — Enterprise Scale (~$2,000+/mo)

**Containers + API Management**

- Azure Container Apps or AKS for fine-grained scaling per endpoint
- Database sharding or read-replica routing per region
- Azure API Management (APIM) as full gateway
- Multi-region deployment for latency-sensitive customers
- Dedicated database instances for enterprise customers

### The Azure API Management Shortcut

Azure APIM handles many API-company concerns out of the box and can be placed in front of the existing API with minimal code changes:

| Feature | APIM Handles It |
|---------|----------------|
| API key management and validation | Yes |
| Rate limiting and quotas per subscription | Yes |
| Built-in developer portal with interactive docs | Yes |
| Usage analytics and reporting | Yes |
| Request/response transformation | Yes |
| Response caching | Yes |
| API versioning | Yes |
| OAuth2 / API key auth | Yes |

**Pricing:**
- Developer tier: ~$50/mo (non-production, no SLA)
- Basic tier: ~$150/mo
- Standard tier: ~$700/mo (includes developer portal)

This could be a quick-win approach: put APIM in front of your existing API and get 80% of API company infrastructure without rewriting auth, rate limiting, or building a developer portal.

---

## Implementation Roadmap

### Milestone 1: Foundation (Weeks 1-3)

- [ ] API versioning — prefix all data routes with `/v1/`
- [ ] Create `Customer` and `ApiKey` domain entities
- [ ] Implement `ApiKeyAuthenticationHandler`
- [ ] Add rate limiting middleware (per-key, per-endpoint)
- [ ] Add usage tracking middleware (log requests to database)
- [ ] Add cursor-based pagination to all list endpoints

### Milestone 2: Infrastructure (Weeks 4-5)

- [ ] Add Redis caching layer with appropriate TTLs
- [ ] Standardize response envelope format
- [ ] Add proper error response format with error codes
- [ ] Configure Azure App Service auto-scaling
- [ ] Set up PostgreSQL read replica

### Milestone 3: Developer Experience (Weeks 6-8)

- [ ] Generate and polish OpenAPI spec
- [ ] Build developer portal (docs, key management, usage dashboard)
- [ ] Write code examples in Python, JavaScript, cURL
- [ ] Create getting-started guides for common use cases
- [ ] Set up status page (e.g., Atlassian Statuspage or similar)

### Milestone 4: Billing & Launch (Weeks 9-10)

- [ ] Reactivate and configure Stripe integration
- [ ] Implement tiered subscription plans
- [ ] Add usage-based billing or quota enforcement
- [ ] Terms of Service and legal docs
- [ ] Beta launch with select customers
- [ ] Public launch

### Milestone 5: Growth (Ongoing)

- [ ] Add new data sources (TFRs, runway closures, fuel prices, FBO info)
- [ ] WebSocket/SSE for real-time weather updates
- [ ] Batch/bulk endpoints for high-volume customers
- [ ] SDKs in popular languages (Python, JavaScript, Go)
- [ ] Geographic expansion (international aviation data)

---

## Revenue Projections (Rough Estimates)

| Customers | Avg Revenue/Customer | MRR | Annual |
|-----------|---------------------|-----|--------|
| 10 (beta) | $100 | $1,000 | $12,000 |
| 50 | $120 | $6,000 | $72,000 |
| 200 | $130 | $26,000 | $312,000 |
| 500 | $140 | $70,000 | $840,000 |

Infrastructure costs at 500 customers would be roughly $2,000-5,000/mo depending on usage patterns, giving healthy margins.

---

## Key Risks

| Risk | Mitigation |
|------|-----------|
| FAA/NOAA change data formats or APIs | Abstract data sources behind interfaces (already done); monitor for changes |
| Competitors with more data | Focus on VFR/GA niche; superior DX and data quality |
| High infrastructure costs at scale | Aggressive caching; usage-based pricing covers costs |
| Legal IP concerns from former employer | Consult attorney; document independent creation; consider clean-room rewrite of any contested components |
| Single cloud vendor lock-in | ICloudStorageService abstraction already exists; keep infrastructure portable |

---

## Decision Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-02-05 | Document pivot strategy | Evaluate feasibility of API company model |
| | | |
