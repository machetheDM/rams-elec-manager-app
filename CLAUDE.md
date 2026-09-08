# CLAUDE.md — Rams @Elec Manager App

Context for AI assistants and for future-me returning after a break. Read this before changing
anything.

## Maintaining this file — for the assistant

**Update this file in the same commit as the change, without being asked.** It is the only thing
that survives between sessions; if it drifts, the next session starts blind. Triggers:

- A new project, namespace, or top-level directory → update the architecture table
- A convention discovered the hard way → add it under Conventions
- A module completed, deferred, or abandoned → update State
- An open thread closed → remove it from Open
- A deliberate refusal (fabricated data, unverifiable claim) → record it under the honesty
  principle

Keep it scannable. This is an operating manual, not a changelog.

---

## What this is

A **tablet/phone companion app** for the owner/manager of **Rams @Elec (Pty) Ltd**, a real
South African electrical & refrigeration services company. It syncs with the existing
[Rams @Elec Intelligence Platform](https://github.com/machetheDM/rams-elec-intelligence-platform-deployment)
(Next.js + 6 FastAPI microservices + Prisma/Postgres).

**Why it exists:**
1. The manager needs to generate and send invoices from the field — not from a laptop
2. Analytics from the web platform should be visible on a tablet
3. Payment reconciliation should be automated — not manual Excel tracking
4. Portfolio evidence for C# / .NET / mobile development skills

**Author:** Dingaan Mahlatse Machethe — dual MSc candidate (Data Science, UEL;
Cybersecurity/Cloud Security Architecture, EC-Council University).

---

## The non-negotiable principle: honesty over impressiveness

Inherited from the parent platform. Every claim must be verifiable:
- Do not fabricate payment processing capabilities that aren't implemented
- Do not present mock data as real business data
- Disclose when features are designed but not yet integrated with real services
- The company's banking details, tax number, and registration are real — treat them
  with appropriate care (they appear on invoices which are client-facing documents,
  so they are semi-public, but don't scatter them unnecessarily)

---

## Company Details (from actual invoice)

```
Company:        RAMS@ELEC (PTY) LTD
Registration:   2017/525813/07
Tax Number:     9486744189          (Income Tax, NOT VAT — company is not VAT-registered)
Address:        Stand No: A276B, Mogaladi Park, Paledi, 0727
Contact:        071 101 8493
Current Email:  ramsatelec@gmail.com  (to be replaced with @ramsatelec.co.za)
Bank:           FNB
Account Name:   RAMS@ELEC (PTY) LTD
Account Number: 62816356796
Branch Code:    210805
```

**IMPORTANT: The company is NOT VAT-registered.** Invoices must NOT say "Tax Invoice" or
include a VAT line. They should say "Invoice" or "Proforma Invoice". If VAT registration
happens later, add a toggle in company settings — do not hardcode VAT assumptions.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Rams @Elec Manager App                       │
│                                                                 │
│  ┌──────────────────┐     ┌──────────────────────────────────┐  │
│  │  .NET MAUI App   │     │  ASP.NET Core Web API            │  │
│  │  (Android/iOS/   │────▶│  (EF Core → PostgreSQL)          │  │
│  │   Windows tablet)│     │                                  │  │
│  │                  │     │  Controllers:                    │  │
│  │  Views (XAML)    │     │  ├─ InvoiceController            │  │
│  │  ViewModels      │     │  ├─ JobController                │  │
│  │  Services:       │     │  ├─ CustomerController           │  │
│  │  ├─ SyncService  │     │  ├─ AnalyticsController          │  │
│  │  ├─ PdfService   │     │  ├─ SyncController               │  │
│  │  ├─ ApiClient    │     │  └─ PaymentController            │  │
│  │  └─ LocalDb      │     │                                  │  │
│  │     (SQLite)     │     │  Services:                       │  │
│  └──────────────────┘     │  ├─ AuthService                  │  │
│                           │  ├─ InvoiceService               │  │
│                           │  ├─ PdfService                   │  │
│                           │  ├─ CompanyInfoService           │  │
│                           │  ├─ AnalyticsService             │  │
│                           │  ├─ S3Service (PDF upload)       │  │
│                           │  ├─ TwilioService (SMS)          │  │
│                           │  ├─ GraphService (email monitor) │  │
│                           │  ├─ WhatsAppService (n8n trigger)│  │
│                           │  └─ PaymentMatchingService       │  │
│                           └───────────────┬──────────────────┘  │
│                                           │                     │
└───────────────────────────────────────────┼─────────────────────┘
                                            │
                                            ▼
                              ┌──────────────────────┐
                              │    PostgreSQL         │
                              │    (same DB as the    │
                              │     web platform)     │
                              │                       │
                              │  Existing tables:     │
                              │  customers, jobs,     │
                              │  quotes, technicians, │
                              │  equipment, etc.      │
                              │                       │
                              │  NEW tables:          │
                              │  invoices,            │
                              │  invoice_line_items,  │
                              │  payments             │
                              └──────────┬────────────┘
                                         │
                            ┌────────────┼────────────┐
                            │            │            │
                     ┌──────┴──┐  ┌──────┴────┐  ┌───┴──────────┐
                     │ Next.js │  │ FastAPI    │  │ n8n          │
                     │ Frontend│  │ Services   │  │ WhatsApp/SMS │
                     │ (web)   │  │ (triage,   │  │ workflows    │
                     │         │  │  dispatch) │  │              │
                     └─────────┘  └───────────┘  └──────────────┘
```

### Project Structure

```
rams-elec-manager-app/
├── src/
│   ├── RamsElec.App/                 # .NET MAUI app
│   │   ├── Views/                    # XAML pages
│   │   │   ├── DashboardPage.xaml
│   │   │   ├── JobsPage.xaml
│   │   │   ├── JobDetailPage.xaml
│   │   │   ├── InvoiceListPage.xaml
│   │   │   ├── InvoiceCreatePage.xaml
│   │   │   ├── InvoicePreviewPage.xaml
│   │   │   ├── CustomersPage.xaml
│   │   │   ├── AnalyticsPage.xaml
│   │   │   ├── PaymentsPage.xaml
│   │   │   ├── SettingsPage.xaml
│   │   │   └── LoginPage.xaml
│   │   ├── ViewModels/               # MVVM ViewModels
│   │   ├── Models/                   # Local domain models
│   │   ├── Services/
│   │   │   ├── ApiClient.cs          # HTTP client → API
│   │   │   ├── InvoiceService.cs     # Invoice CRUD + numbering
│   │   │   ├── PdfService.cs         # QuestPDF generation
│   │   │   ├── SyncService.cs        # Offline sync engine
│   │   │   ├── LocalDatabase.cs      # SQLite wrapper
│   │   │   └── AuthService.cs        # JWT token management
│   │   ├── Converters/               # XAML value converters
│   │   ├── Resources/
│   │   │   ├── Fonts/
│   │   │   ├── Images/
│   │   │   └── Styles/
│   │   ├── Platforms/                # Platform-specific code
│   │   ├── App.xaml
│   │   ├── AppShell.xaml             # Tab/flyout navigation
│   │   └── MauiProgram.cs            # DI registration
│   │
│   ├── RamsElec.Api/                 # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── InvoiceController.cs
│   │   │   ├── JobController.cs
│   │   │   ├── CustomerController.cs
│   │   │   ├── AnalyticsController.cs
│   │   │   ├── SyncController.cs
│   │   │   └── PaymentController.cs
│   │   ├── Services/
│   │   │   ├── AuthService.cs        # JWT token generation
│   │   │   ├── InvoiceService.cs     # Invoice CRUD and status lifecycle
│   │   │   ├── PdfService.cs         # QuestPDF invoice generation
│   │   │   ├── CompanyInfoService.cs # Company settings persistence
│   │   │   ├── AnalyticsService.cs   # Dashboard KPI aggregation
│   │   │   ├── S3Service.cs          # PDF upload, presigned URLs
│   │   │   ├── TwilioService.cs      # SMS invoice delivery
│   │   │   ├── GraphService.cs       # M365 email monitoring
│   │   │   ├── WhatsAppService.cs    # n8n webhook trigger
│   │   │   └── PaymentMatchingService.cs  # EFT reconciliation agent
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs       # EF Core DbContext
│   │   │   └── Migrations/
│   │   ├── Middleware/
│   │   │   └── ApiKeyMiddleware.cs
│   │   └── Program.cs
│   │
│   └── RamsElec.Shared/              # Shared library
│       ├── Models/                   # Domain models
│       │   ├── Invoice.cs
│       │   ├── InvoiceLineItem.cs
│       │   ├── Payment.cs
│       │   ├── Job.cs
│       │   ├── Customer.cs
│       │   └── CompanyInfo.cs
│       ├── DTOs/                     # API request/response DTOs
│       └── Enums/
│           ├── InvoiceStatus.cs
│           ├── PaymentMethod.cs
│           └── DeliveryChannel.cs
│
├── tests/
│   ├── RamsElec.App.Tests/
│   ├── RamsElec.Api.Tests/
│   └── RamsElec.Shared.Tests/
│
├── docs/
│   ├── invoice-design.md             # Invoice template spec
│   ├── payment-agent.md              # EFT reconciliation agent design
│   └── sync-protocol.md             # Offline sync protocol
│
├── docker-compose.yml                # API + Postgres for local dev
├── .github/workflows/
│   └── ci.yml
├── .gitignore
├── CLAUDE.md                         # This file
├── README.md
└── RamsElec.sln                      # Solution file
```

---

## Tech Stack

| Layer | Technology | Notes |
|---|---|---|
| Mobile App | .NET MAUI 9 | Android + iOS + Windows tablet |
| Architecture | MVVM | CommunityToolkit.Mvvm |
| Local DB | SQLite | sqlite-net-pcl, offline-first |
| Charts | LiveCharts2 | SkiaSharp-based, cross-platform |
| PDF Generation | QuestPDF | Community license (free for <$1M revenue) |
| Backend API | ASP.NET Core 9 | EF Core 9 → PostgreSQL |
| Database | PostgreSQL 16 | **Same instance** as the web platform |
| File Storage | AWS S3 | Invoice PDFs, presigned URLs |
| SMS | Twilio | Invoice delivery via SMS link |
| WhatsApp | n8n webhook | Triggers existing WhatsApp workflow |
| Email Monitor | Microsoft Graph API | Read FNB payment emails |
| Email Hosting | Microsoft 365 Business | @ramsatelec.co.za |
| Auth | JWT Bearer | Issued by API, stored in SecureStorage |

---

## Module Plan (Phased Build)

### Phase 1 — Foundation
- [ ] .NET solution structure (MAUI + API + Shared)
- [ ] MAUI Shell navigation (tab bar: Dashboard, Jobs, Invoices, Analytics, Settings)
- [ ] ASP.NET Core API scaffold with EF Core
- [ ] Shared models: Invoice, InvoiceLineItem, Payment, Job, Customer, CompanyInfo
- [ ] SQLite local database in MAUI app
- [ ] JWT authentication (API issues token, MAUI stores in SecureStorage)
- [ ] Basic sync service (pull jobs/customers from API on login)

### Phase 2 — Invoicing Core
- [ ] Invoice CRUD (create from job, edit line items, save draft)
- [ ] Sequential invoice numbering: `INV-YYYY-NNNN`
- [ ] QuestPDF professional invoice template (matching and improving current design)
- [ ] PDF preview in-app
- [ ] Quote-to-invoice conversion (one tap)
- [ ] Company settings page (logo, bank details, address — persisted)
- [ ] Invoice status lifecycle: draft → sent → paid → overdue → cancelled

### Phase 3 — Delivery & Payments
- [ ] SMS invoice delivery via Twilio (presigned S3 link)
- [ ] WhatsApp invoice delivery via n8n webhook trigger
- [ ] PDF upload to S3 with presigned URL generation
- [ ] FNB SpeedPoint payment recording (manual entry: approval code + last 4 digits)
- [ ] EFT payment recording (manual confirmation)
- [ ] Payment history per invoice
- [ ] Due date tracking + overdue status auto-transition

### Phase 4 — Payment Reconciliation Agent
- [ ] Microsoft 365 email setup (invoices@ramsatelec.co.za)
- [ ] Microsoft Graph API integration (read inbox)
- [ ] FNB payment notification email parser
- [ ] Payment matching service (reference → invoice lookup)
- [ ] Confidence scoring (high = auto-process, low = flag for review)
- [ ] Push notification to app for low-confidence matches
- [ ] Auto-mark-paid + receipt generation + delivery for high-confidence

### Phase 5 — Analytics Mirror
- [ ] LiveCharts2 integration
- [ ] Dashboard: KPI cards (revenue, outstanding, overdue, jobs today)
- [ ] 7 analytics pages mirroring the web:
  - Overview (donut + bar)
  - Inquiries (line + pie)
  - Revenue (area + horizontal bar)
  - Equipment (gauge + bar)
  - Technicians (grouped bar + radar)
  - Load-shedding (timeline + scatter)
  - Follow-ups (bar + pie)
- [ ] Offline chart data caching (SQLite)
- [ ] "Last synced" indicator

### Phase 6 — Polish & Deployment
- [ ] Push notifications (Firebase Cloud Messaging)
- [ ] Automated payment reminders (n8n: overdue 7d → WhatsApp reminder)
- [ ] Tablet-optimized layouts (master-detail for landscape)
- [ ] App icon and splash screen (Rams @Elec branding)
- [ ] CI pipeline (build, test, Android APK artifact)
- [ ] Google Play Store / TestFlight preparation

---

## Invoice Design Specification

### Current problems with the existing invoice
1. Generic/template look — no brand identity beyond a clip-art lightbulb
2. No due date — impossible to track overdue
3. No payment reference instruction — clients don't know what to put as EFT reference
4. Gmail address — looks unprofessional
5. No sequential numbering system visible to the platform
6. No VAT/non-VAT clarity
7. Layout wastes space — terms & conditions cramped at bottom

### New design principles
- **Dark navy header band** matching the web platform's industrial theme
- **Company logo** (proper vector, not clip art) prominent but not overwhelming
- **Clean typography** — Inter or similar professional sans-serif
- **Clear visual hierarchy** — company info → recipient → line items → totals → payment
- **Auto-populated reference** — "Use INV-2026-0110 as your EFT reference"
- **Due date** prominently displayed
- **Professional email** — invoices@ramsatelec.co.za
- **Status watermark** — DRAFT / PAID / OVERDUE as subtle diagonal watermark
- **Footer** — bank details in a structured grid, not a text block

### Template sections (top to bottom)
1. **Header band** — navy background, white text: company name, reg, tax no, address
2. **Logo** — right-aligned in header
3. **Invoice meta** — invoice number, date, due date (right column)
4. **Recipient block** — "BILL TO:" customer name, address, phone
5. **Job reference** — job description in a subtle callout box
6. **Line items table** — qty, description, unit price, amount (alternating row shading)
7. **Totals block** — subtotal, (VAT if registered), total — right-aligned, bold
8. **Payment section** — structured grid: bank name, account, branch, reference
9. **Terms & conditions** — small print
10. **Footer** — "Thank you for your business" + contact details

---

## Database Schema (New Tables)

These tables are added to the SAME Postgres database the web platform uses.
The web platform uses Prisma; this API uses EF Core. Both ORMs can coexist on
the same database — EF Core reads/writes its tables, Prisma reads/writes its.
Shared tables (customers, jobs, quotes) are read by both.

```sql
-- Invoice table
CREATE TABLE invoices (
    id                  TEXT PRIMARY KEY,  -- CUID format to match Prisma convention
    invoice_number      TEXT UNIQUE NOT NULL,  -- INV-YYYY-NNNN
    job_id              TEXT REFERENCES jobs(id),
    customer_id         TEXT NOT NULL REFERENCES customers(id),
    quote_id            TEXT REFERENCES quotes(id),
    status              TEXT NOT NULL DEFAULT 'draft',
        -- draft | sent | viewed | paid | overdue | cancelled
    subtotal            DECIMAL(12,2) NOT NULL,
    vat_rate            DECIMAL(5,4) DEFAULT 0,  -- 0 until VAT-registered
    vat_amount          DECIMAL(12,2) DEFAULT 0,
    total               DECIMAL(12,2) NOT NULL,
    pdf_url             TEXT,
    sent_via            TEXT,  -- sms | whatsapp | email | null
    sent_at             TIMESTAMPTZ,
    viewed_at           TIMESTAMPTZ,
    paid_at             TIMESTAMPTZ,
    due_date            DATE NOT NULL,
    notes               TEXT,
    created_by          TEXT NOT NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_invoices_status ON invoices(status);
CREATE INDEX idx_invoices_customer ON invoices(customer_id);
CREATE INDEX idx_invoices_job ON invoices(job_id);
CREATE INDEX idx_invoices_due_date ON invoices(due_date);

-- Invoice line items
CREATE TABLE invoice_line_items (
    id                  TEXT PRIMARY KEY,
    invoice_id          TEXT NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    description         TEXT NOT NULL,
    quantity            DECIMAL(10,2) NOT NULL,
    unit_price          DECIMAL(12,2) NOT NULL,
    total               DECIMAL(12,2) NOT NULL,
    category            TEXT NOT NULL DEFAULT 'service',
        -- service | labour | materials | callout | equipment
    sort_order          INT NOT NULL DEFAULT 0,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_line_items_invoice ON invoice_line_items(invoice_id);

-- Payment records
CREATE TABLE payments (
    id                  TEXT PRIMARY KEY,
    invoice_id          TEXT NOT NULL REFERENCES invoices(id),
    amount              DECIMAL(12,2) NOT NULL,
    method              TEXT NOT NULL,
        -- eft | fnb_speedpoint | cash
    reference           TEXT,  -- EFT reference or SpeedPoint approval code
    payer_name          TEXT,
    matched_confidence  DECIMAL(3,2),  -- 0.00-1.00, null if manual
    matched_by          TEXT,  -- agent | manual
    bank_notification   TEXT,  -- raw email content for audit
    recorded_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_payments_invoice ON payments(invoice_id);
CREATE INDEX idx_payments_reference ON payments(reference);
```

---

## Conventions

**Naming** — C# conventions throughout: PascalCase for public members, camelCase
for private fields with `_` prefix. Database columns are snake_case (matching the
existing Prisma convention).

**CUID IDs** — the web platform uses Prisma's `cuid()` for IDs. EF Core must
generate CUIDs too, not GUIDs, so `JOIN` queries across tables work. Use a CUID
NuGet package or implement the algorithm.

**Decimal for money** — never `float` or `double` for currency. Always `decimal`
in C# and `DECIMAL(12,2)` in Postgres.

**South African formatting** — currency is `R` prefix (not `ZAR` or `$`).
Phone numbers are `0XX XXX XXXX` format locally, `+27XX XXX XXXX` for Twilio.
Dates are `dd/MM/yyyy` (not US format).

**Offline-first** — every write goes to SQLite first, then syncs to the API.
The sync service tracks `last_synced_at` per entity type. Conflict resolution:
server wins for jobs/customers (web platform is source of truth), client wins
for invoices/payments (app is source of truth for billing).

**No secrets in code** — API keys, connection strings, JWT secrets go in
`appsettings.json` (API) or `SecureStorage` (MAUI). Never committed.

---

## Payment Reconciliation Agent Design

### Flow
1. **Poll** — ASP.NET Core Background Service polls `invoices@ramsatelec.co.za`
   via Microsoft Graph API every 2 minutes for new FNB payment notifications
2. **Parse** — extract amount, reference, payer name, date from FNB email format
3. **Match** — search `invoices` table WHERE `status IN ('sent', 'viewed')`:
   - Reference matches invoice_number → confidence 0.95
   - Amount matches AND payer name fuzzy-matches customer → confidence 0.80
   - Amount matches only → confidence 0.60
4. **Act** — if confidence >= 0.85:
   - Insert payment record
   - Update invoice status to 'paid'
   - Generate receipt PDF
   - Send receipt via original delivery channel (SMS/WhatsApp)
   - Log as `matched_by = 'agent'`
5. **Flag** — if confidence < 0.85:
   - Push notification to manager app
   - Show payment details + suggested invoice match
   - Manager confirms or reassigns
   - Log as `matched_by = 'manual'`

### FNB Email Parsing
FNB payment notification emails follow a consistent format. The parser extracts:
- Amount (regex for `R XX,XXX.XX` patterns)
- Reference (the field the payer typed)
- Payer name / account
- Date and time

**Note:** The exact FNB email format needs to be captured from a real notification.
Build the parser with a sample, then refine.

---

## Professional Email Setup

### Domain: ramsatelec.co.za
1. Register via a `.za` registrar (e.g., domains.co.za, ~R60/year)
2. Set up Microsoft 365 Business Basic (~R90/user/month)
3. Configure MX records pointing to Microsoft 365
4. Create mailboxes:
   - `invoices@ramsatelec.co.za` — invoice delivery + payment monitoring
   - `info@ramsatelec.co.za` — general inquiries (replace gmail on website)
   - `admin@ramsatelec.co.za` — platform admin, system notifications
   - `support@ramsatelec.co.za` — customer support / follow-ups

### Microsoft Graph API
- Register an Azure AD (Entra ID) app
- Grant `Mail.Read` permission on the invoices mailbox
- Use client credentials flow (daemon/service) for the payment agent
- No user interaction needed — background service reads mail

---

## State as of 2026-09-07

**Phase 1 scaffold complete.** `dotnet build RamsElec.sln` passes with 0 errors across all
target frameworks (Android, iOS, Mac Catalyst, Windows).

**Implemented (Phase 1 + 2 + 3 + 4 + Quotes):**
- [x] Solution structure: RamsElec.App + RamsElec.Api + RamsElec.Shared
- [x] Shared models: Invoice, InvoiceLineItem, Payment, Customer, Job, CompanyInfo, BankPaymentNotification, PaymentMatch, Quote, QuoteLineItem, QuotePayment
- [x] Shared enums: InvoiceStatus (8 states), PaymentMethod, DeliveryChannel, QuoteStatus
- [x] Shared DTOs: CreateInvoice, Invoice, SendInvoice, RecordPayment, FnbPayment, Login, Sync, Analytics, Quote, CreateQuote, SendQuote
- [x] API: EF Core DbContext with snake_case mappings, JWT auth, InvoiceService, AuthService
- [x] API: QuestPDF PdfService with branded invoice template
- [x] API: S3Service, TwilioService, WhatsAppService, PaymentService, OverdueInvoiceService, QuoteService
- [x] API: Microsoft Graph email reader (GraphMailReader) + MockMailReader for dev
- [x] API: FNB payment email parser scaffold (must be verified with real FNB samples)
- [x] API: PaymentMatchingService with confidence scoring and manager review queue
- [x] API: Quote approval → payment reference → auto-convert to invoice on full payment
- [x] API: PaymentAgentHostedService (scans inbox every 15 min)
- [x] API: Controllers — Auth, Invoice, Quote, Customer, Job, Sync, Analytics, CompanyInfo, Payment, PaymentMatch, Health
- [x] API: SMS/WhatsApp invoice delivery via presigned S3 URL
- [x] API: EFT, cash, and FNB SpeedPoint payment recording
- [x] API: Overdue invoice background service (6-hour checks)
- [x] MAUI: Quotes tab with list, create, detail pages
- [x] MAUI: InvoiceListPage with tappable items
- [x] MAUI: InvoiceDetailPage with PDF preview, SMS/WhatsApp send, payment recording
- [x] MAUI: FNB SpeedPoint payment dialog (approval code + last 4 digits)
- [x] MAUI: Payment history on invoice detail
- [x] MAUI: PaymentsPage for reviewing and approving auto-detected payment matches
- [x] MAUI: Quote approve/reject/convert-to-invoice workflow
- [x] MAUI: Analytics tab with LiveCharts2 bar and pie charts
- [x] MAUI: Dashboard charts (revenue by month, jobs by status)
- [x] Docker Compose (API + Postgres 16)
- [x] Dockerfile for API
- [x] GitHub repo: github.com/machetheDM/rams-elec-manager-app

**Not yet implemented:**
- Phase 6: App store prep (Google Play / TestFlight), production push credentials

**Known warnings (harmless):**
- MVVMTK0045: CommunityToolkit.Mvvm suggests partial properties for WinRT AOT compat.
  Not blocking; can be migrated later.
- XC0022/XC0024/XC0045: XAML compiled binding warnings in InvoiceCreatePage DataTemplate.
  The bindings work at runtime; these are compile-time type hints that can be refined.

**Pending decisions:**
- [ ] Exact FNB payment email format (need a real sample to build the parser)
- [ ] Company logo in vector format (current invoice has a clip-art lightbulb)
- [ ] ramsatelec.co.za domain registration (action for the business owner)
- [ ] Microsoft 365 subscription setup (action for the business owner)
- [ ] FNB SpeedPoint model/integration details (once the device arrives)

**Open:**
- The web platform's Prisma schema does NOT yet have Invoice/Payment tables.
  When we add them via EF Core migration, the Prisma schema in the web platform
  must be updated to include them (read-only from Prisma's side, write via EF Core).
  Alternatively, add the migration via Prisma in the web platform first, and have
  EF Core map to existing tables. Decision deferred to Phase 2.

---

## Verification Commands

```bash
# Build solution
dotnet build RamsElec.sln

# Run tests
dotnet test

# Run API
cd src/RamsElec.Api && dotnet run

# Run MAUI app (Windows)
cd src/RamsElec.App && dotnet build -t:Run -f net9.0-windows10.0.19041.0

# Run MAUI app (Android emulator)
cd src/RamsElec.App && dotnet build -t:Run -f net9.0-android

# Docker (API + Postgres)
docker compose up -d
```

---

## Related Repositories

- **[rams-elec-intelligence-platform](https://github.com/machetheDM/rams-elec-intelligence-platform)** —
  Public showcase (architecture, code samples, documentation)
- **[rams-elec-intelligence-platform-deployment](https://github.com/machetheDM/rams-elec-intelligence-platform-deployment)** —
  Production web platform (Next.js + FastAPI + Prisma/Postgres + Terraform)
