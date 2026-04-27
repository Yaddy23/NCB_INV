<h1 align="center">📚 NCB INVENTORY SYSTEM</h1>
<h2 align="center">The NCB Inventory System is a professional-grade warehouse management application built with C# (.NET WinForms). It is specifically designed to handle high-volume book distribution with a focus on data integrity, speed, and 100% operational uptime.</h2>

<h3>🏛️ Core Architecture</h3>
The system uses a Two-Tier Database Strategy to ensure the warehouse never stops moving:

Cloud Tier (MongoDB): Acts as the global "source of truth." All book data and transaction histories are stored here for multi-location access.

Local Tier (SQLite): Acts as a high-speed local cache. This allows staff to perform Stock-In and Stock-Release operations even if the internet goes down, with a built-in Delta Sync engine to reconcile data once the connection is restored.

<h3>🚀 Key Functional Modules</h3>
1. Intelligent Bulk Processing
Mass Update Engine: Specifically optimized to handle thousands of records via Excel (.xlsx/.xls) imports.

Diagnostic Reporting: Automatically generates color-coded HTML reports after every bulk import to show success rates and highlight missing items.

Buffered Logging: Instead of logging every single book individually, bulk actions are aggregated into single documents to maintain a clean and searchable audit trail.

2. Real-Time Inventory Control
Dynamic Dashboard: A live-refreshing DataGrid with status highlighting (e.g., color-coding items that are low or out-of-stock).

Instant Search: A multi-parameter search system that lets users find stock by ISBN, Title, Author, or Publisher instantly.

3. Security & Accountability
Enterprise Encryption: Uses SHA-256 cryptographic hashing for user passwords.

Role-Based Audit Logs: Every stock modification is timestamped and permanently tied to the specific user who performed the action.

<h3>🛠️ Technical Stack</h3>
Framework: .NET / WinForms

Data Parsing: ExcelDataReader (configured for legacy support)

Database Drivers: MongoDB.Driver & System.Data.SQLite

Deployment: Distributed as a Single-File Self-Contained Executable, meaning it runs on any Windows x64 machine without needing to install .NET runtimes separately.
