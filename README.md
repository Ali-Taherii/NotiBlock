# NotiBlock

**Tagline:** Built as a final year project to demonstrate end-to-end distributed systems, microservice patterns, and supply chain transparency

## Project Overview

NotiBlock enables manufacturers, resellers, consumers, and regulators to collaborate on product safety and recalls through a unified backend API. Key features include product lifecycle management, consumer reporting, escalation workflows, and immutable blockchain anchoring for regulatory compliance and transparency.

Stakeholders
- Consumers: Register products they own, report issues, track recalls
- Resellers: Manage product distribution, handle consumer reports, escalate to manufacturers/regulators
- Manufacturers: Create products, issue recalls, respond to safety concerns
- Regulators: Review escalated tickets, approve recalls, ensure compliance

## Status
- Version: 1.0.0
- Status: Active Development
- Last updated: January 2025

## Table of Contents

- Project Overview
- Technology Stack
- Project Structure
- Prerequisites
- Installation
- Blockchain Integration
- API Endpoints
- Frontend Integration
- Authentication & Security
- Development Guide
- Project Deliverables
- Testing & Demonstration
- Resources
- Future Enhancements
- License & Metadata

## Technology Stack

- Backend: .NET 8, C# 12
- Database: PostgreSQL, Entity Framework Core
- Authentication: JWT tokens, Role-based authorization
- Blockchain: Nethereum, Polygon Mumbai testnet
- Logging: Serilog
- API Documentation: Swagger / OpenAPI

## Project Structure

- Data/
  - AppDbContext.cs — EF Core context & model configuration
- Models/
  - Product.cs — Product entity with blockchain fields
  - Recall.cs — Recall entity
  - ConsumerReport.cs — Consumer issue reports
  - ResellerTicket.cs — Escalation tickets
  - Consumer.cs, Manufacturer.cs, Reseller.cs, Regulator.cs — User entities
- DTOs/
  - API request/response DTOs (e.g. ProductResponseDTO.cs, ResellerTicketResponseDTO.cs)
- Services/
  - ProductService.cs, RecallService.cs, ConsumerReportService.cs, ResellerTicketService.cs, AuthService.cs, BlockchainService.cs, BlockchainSyncService.cs
- Controllers/
  - AuthController.cs, ProductController.cs, RecallController.cs, ConsumerReportController.cs, ResellerTicketController.cs, BlockchainController.cs
- Migrations/
  - EF Core migration files

## Prerequisites

- .NET 8 SDK (required)
- PostgreSQL 12+ (required)
- MetaMask wallet (optional — for blockchain interactions)
- Git (required)

## Installation

1. Clone the repository
   ```bash
   git clone https://github.com/Ali-Taherii/NotiBlock.git && cd NotiBlock
   ```

2. Configure environment variables or appsettings.json
   - Never commit private keys. Use environment variables in production.
   - Example configuration:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=NotiBlockDb;Username=postgres;Password=yourpassword"
     },
     "JwtSettings": {
       "Key": "<your-random-256-bit-base64-encoded-key>",
       "Issuer": "NotiBlockAPI",
       "Audience": "NotiBlockUsers",
       "ExpirationMinutes": 480,
       "RefreshTokenExpirationDays": 7
     },
     "Blockchain": {
       "RpcUrl": "https://rpc-mumbai.maticvigil.com",
       "PrivateKey": "<your-test-private-key>",
       "ContractAddress": "<deployed-contract-address>"
     },
     "CorsSettings": {
       "AllowedOrigin": "http://localhost:5173"
     }
   }
   ```

3. Apply database migrations
   ```bash
   dotnet tool restore
   dotnet ef database update
   ```

4. Install dependencies
   ```bash
   dotnet restore
   ```

5. Run the API
   ```bash
   dotnet run
   ```

   The API will be available at https://localhost:xxxx and Swagger UI at /swagger

## Blockchain Integration

Smart contract: NotiBlockDemo.sol (Polygon Mumbai testnet)

Key contract functions:
- registerProduct(bytes32 productHash) — Record product registration
- transferOwnership(bytes32 productHash, address newOwner) — Record ownership transfer
- issueRecall(bytes32 recallHash, bytes32 productHash, string reason) — Record recall issuance
- verifyProduct(bytes32 productHash) — Query product history

Deployment steps (recommended):
1. Navigate to Remix IDE: https://remix.ethereum.org/
2. Create new file NotiBlockDemo.sol and paste contract code
3. Compile with Solidity 0.8.20
4. Connect MetaMask wallet to Polygon Mumbai testnet
5. Obtain test MATIC: https://faucet.polygon.technology/
6. Deploy contract and record the deployed address
7. Add ContractAddress to configuration

Recording strategy:
- Asynchronous (Recommended): BlockchainSyncService polls pending DB records and sends transactions in the background. Benefit: Enables request responses before on-chain confirmation.
- Synchronous (Simpler): Direct transaction calls from services. Note: Blocks requests but simpler to implement.

## API Endpoints (summary)

Authentication:
- POST /api/auth/consumer/register — Register consumer account
- POST /api/auth/reseller/register — Register reseller account
- POST /api/auth/manufacturer/register — Register manufacturer account
- POST /api/auth/regulator/register — Register regulator account
- POST /api/auth/login — Login any user type
- POST /api/auth/refresh — Refresh JWT token

Products:
- POST /api/products/create (manufacturer)
- POST /api/products/register — Register product to reseller or consumer
- POST /api/products/unregister — Unregister from reseller or self-unregister (consumer)
- GET /api/products/{serialNumber} — Get product with full details
- GET /api/products/manufacturer — List manufacturer's products (manufacturer)
- GET /api/products/reseller — List reseller's products (reseller)
- GET /api/products/consumer — List consumer's owned products (consumer)
- PUT /api/products/{serialNumber} — Update product
- DELETE /api/products/{serialNumber} — Soft delete product

Consumer Reports:
- POST /api/consumer-reports — Submit consumer report
- GET /api/consumer-reports/{id} — Get report details
- GET /api/consumer-reports/consumer — List consumer's reports
- GET /api/consumer-reports/reseller/related-reports — List reports for reseller's sold products
- GET /api/consumer-reports/product/{serial} — Get reports by product
- PUT /api/consumer-reports/{id} — Update report (consumer only, pending status)
- DELETE /api/consumer-reports/{id} — Delete report (consumer only, pending status)
- POST /api/consumer-reports/{id}/action — Reseller actions (review, resolve, escalate)

Reseller Tickets:
- POST /api/reseller-tickets — Create escalation ticket (reseller)
- GET /api/reseller-tickets/{id} — Get ticket with linked reports and reviews
- GET /api/reseller-tickets/my-tickets — List reseller's tickets
- GET /api/reseller-tickets/all — List all tickets (regulator)
- GET /api/reseller-tickets/status/{status} — Filter by status
- PUT /api/reseller-tickets/{id} — Update ticket
- DELETE /api/reseller-tickets/{id} — Soft delete ticket
- POST /api/reseller-tickets/{id}/link-reports — Link consumer reports to ticket

Recalls:
- POST /api/recalls — Create recall (manufacturer)
- GET /api/recalls/{id} — Get recall details
- GET /api/recalls/manufacturer — List manufacturer's recalls
- GET /api/recalls/product/{serial} — Get recalls for product
- GET /api/recalls?status=Active — Filter recalls by status
- PUT /api/recalls/{id} — Update recall status
- DELETE /api/recalls/{id} — Soft delete recall

Blockchain:
- GET /api/blockchain/verify-product/{serial} — Verify product authenticity on-chain
- GET /api/blockchain/recall/{recallId}/proof — Get recall blockchain proof with explorer link

## Frontend Integration

ProductResponseDTO (example shape):
- id: uuid
- serialNumber: string
- model: string
- manufacturerId: uuid
- manufacturer: { id, companyName, email, walletAddress }
- resellerId: uuid
- reseller: object
- ownerId: uuid
- owner: object
- registeredAt: datetime

RecallResponseDTO (example shape):
- id: uuid
- productSerialNumber: string
- manufacturerId: uuid
- reason: string
- actionRequired: string
- status: string
- issuedAt: datetime
- blockchainTxHash: string
- blockchainIssuedAt: datetime

Dashboards & features for each role (Manufacturer, Consumer, Reseller, Regulator) are described in the project documentation.

Polling strategy for proof confirmation:
- Endpoint: GET /api/blockchain/recall/{recallId}/proof
- Interval: 5-10 seconds
- Max attempts: 30
- Strategy: Exponential backoff
- States: Pending, Confirmed

## Authentication & Security

- Token storage: HTTP-only cookies
- Refresh token expiration: 7 days
- Access token expiration: 8 hours
- Roles: consumer, reseller, manufacturer, regulator

Best practices:
- Environment variables for all secrets
- Use Azure Key Vault or similar for production deployments
- Implement rate limiting for login endpoints
- Regular security audits of blockchain interactions

## Development Guide

Adding new features:
1. Define model in Models/
2. Add DbSet in AppDbContext.cs with relationships
3. Create migration:
   ```bash
   dotnet ef migrations add DescriptiveNameOfChange && dotnet ef database update
   ```
4. Create DTO in DTOs/
5. Implement service in Services/
6. Add controller in Controllers/
7. Test endpoints via Swagger

Database changes:
- Review migrations: `dotnet ef migrations script <LastMigration> <NewMigration>`
- Note: Always review generated migrations before applying

Logging:
- Framework: Serilog
- Output locations: Console and logs/ directory with daily rolling intervals

## Project Deliverables

Phase 1: Core Backend (Completed)
- Multi-tenant user authentication
- Product lifecycle management
- Consumer reporting system
- Reseller escalation workflow
- Recall issuance and tracking

Phase 2: Blockchain Integration (Current)
- Smart contract deployment to Polygon Mumbai
- Nethereum service layer
- Background sync for reliable recording
- Blockchain proof endpoints
- Frontend UI for proof display (In Progress)

Phase 3: Frontend Application (Upcoming)
- React-based dashboards for all user roles
- Real-time notifications
- Blockchain proof visualization

## Testing & Demonstration

Manual workflow example:
1. Create Manufacturer Account: POST /api/auth/manufacturer/register
2. Create Product: POST /api/products/create
   ```json
   { "serialNumber": "ABC123", "model": "Test Product" }
   ```
3. Monitor Blockchain Confirmation: GET /api/blockchain/verify-product/ABC123
4. Issue Recall: POST /api/recalls
   ```json
   { "productSerialNumber": "ABC123", "reason": "Safety concern", "actionRequired": "Return to retailer" }
   ```
5. View Recall Proof: GET /api/blockchain/recall/{recallId}/proof

Explorer verification:
- Polygon Mumbai explorer: https://mumbai.polygonscan.com/tx/{transactionHash}

## Resources

- [Polygon Mumbai Testnet Documentation](https://polygon.technology/developers/)
- [Nethereum Documentation](https://docs.nethereum.com/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

## Future Enhancements

- WebSocket integration for real-time notifications
- Advanced filtering and search capabilities
- Batch recall support for multiple products
- Machine learning for anomaly detection in reports
- Integration with additional blockchain networks
- Mobile application support

## Metadata & License

- Repository: https://github.com/Ali-Taherii/NotiBlock
- Last updated: January 2025
- Status: Active Development
- License: Educational Purposes
