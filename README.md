<h1 align="center">📚 NCB INVENTORY SYSTEM</h1>
<h2 align="center">A High-Performance Hybrid Warehouse Management Solution</h2>

<h1 align="center">🚀 Key Functions</h1>
📦 Intelligent Bulk Processing
Mass Update Engine: Optimized to process thousands of entries via Excel (.xlsx, .xls) for both Stock-In and Stock-Release operations.

Buffered Logging: Uses a custom aggregation strategy to log bulk actions. Instead of single entries, it saves formatted lists of ISBNs, Titles, and Quantities into a single transaction document to maintain a clean audit trail.

Automated HTML Reporting: Generates instant, color-coded diagnostic reports after every import to track success rates and identify missing items.

🔍 Real-Time Inventory Control
Dynamic DataGrid: Features live-refreshing views with status highlighting (e.g., Red for out-of-stock items).

Instant Search: Multi-parameter search functionality to locate stock across the entire warehouse database.

🔐 Enterprise-Grade Security
Cryptographic Hashing: Uses SHA-256 encryption for user passwords, ensuring data privacy.

Role-Based Audit Logs: Every action is timestamped and tied to the DisplayName of the authenticated user.

🗄️ Database Architecture
This system is built for 100% Uptime using a two-tier approach:

MongoDB (Cloud Tier): Acts as the primary global repository for all book data and transaction history.

SQLite (Local Fallback): Ensures the warehouse remains operational during internet outages by allowing offline data entry and retrieval.

Data Consistency: Implements custom mapping for nested MongoDB _id objects to ensure seamless C# object-to-document translation.

🛠️ Technical Stack
Language: C# / .NET (WinForms)

Data Parsing: ExcelDataReader (configured with CodePagesEncodingProvider for legacy support)

Database Drivers: MongoDB.Driver, System.Data.SQLite

UI/UX: Custom HTML/CSS Template Engine for reporting

📦 Deployment
The application is published as a Single-File Self-Contained Executable.

No Runtime Required: The .NET environment is bundled inside the .exe.

Zero Install: Fully portable; can be run directly from a USB drive or network share on any Windows x64 system.
