NCB Inventory System
A robust, hybrid-database inventory management solution designed for warehouse operations. This system ensures 100% data reliability by utilizing a local-fallback architecture for intermittent internet environments.

🚀 Key Functions
1. Hybrid Bulk Update System
Intelligent Stock Management: Processes massive Excel datasets (up to 20,000+ rows) for bulk "Stock-In" or "Release" actions.

Transaction Aggregation: To optimize database performance, the system batches updates and logs a single, detailed summary for bulk operations instead of flooding the database with individual entries.

Real-time Report Generation: Automatically generates an HTML-based audit report after every bulk process, providing instant visual feedback on successes, stock-outs, and "Not Found" items.

2. Advanced Data Grid & Search
Dynamic Search: Real-time filtering of the book database with automated list refreshing.

Color-Coded Status: Visual indicators for stock levels (e.g., Red for out-of-stock, Light Red for missing database entries).

3. Secure Authentication
SHA-256 Hashing: Implements industry-standard cryptographic hashing for user passwords, ensuring no plain-text credentials are stored.

Session Management: Tracks the DisplayName of the active user for every transaction log to maintain a strict audit trail.

🗄️ Database Architecture
The project utilizes a Multi-Tiered Database Strategy to balance cloud scalability with local reliability:

MongoDB (Primary Cloud Store): Handles the global book repository and transaction history. Uses a nested _id structure to tie user identity directly to their credentials.

SQLite (Offline Fallback): Integrated as a local buffer to ensure warehouse operations can continue even if the internet connection is lost.

Consolidated Logging: Transaction logs are structured to hold vertical lists of ISBNs, Titles, and Quantities, allowing for rich historical data without hitting MongoDB's document size limits.

🛠️ Technical Stack
Language: C# / .NET

Framework: WinForms

Data Processing: ExcelDataReader (supporting Encoding 1252 for legacy Excel formats)

Database: MongoDB (.NET Driver), SQLite

Reporting: Dynamic HTML/CSS Generator

📦 Deployment & Installation
The application is published as a Self-Contained Single-File Executable.

No dependencies required: All .NET Runtimes are bundled within the .exe.

Portable: Can be run directly on any Windows x64 machine without pre-installing system libraries.
