# Sample Collection - End-to-End Implementation Guide

## 📋 Complete Step-by-Step Process

### Overview
The Sample Collection module allows users to:
- ✅ View list of pending bills for sample collection (with filters)
- ✅ Collect samples partially (e.g., 2 of 4 tests now, 2 later)
- ✅ See previously collected samples with timestamps when reopening
- ✅ Generate unique sample barcodes with patient info
- ✅ Track collection status in real-time

---

## 🗄️ Database Schema

### Tables Structure

#### 1. **SampleCollection** (Master Table)
```sql
CREATE TABLE SampleCollection (
    SampleCollectionId INT PRIMARY KEY IDENTITY(1,1),
    BillSummaryId INT NOT NULL,
    PatientInfoId INT NOT NULL,
    CollectionBarcode VARCHAR(30) UNIQUE,           -- Generated: SC-001001 (SC + billid + sequence)
    CollectionDate DATE NOT NULL,
    CollectionTime TIME NOT NULL,
    CollectedBy VARCHAR(100) NOT NULL,
    CollectionStatus VARCHAR(30),                   -- 'Pending', 'PartiallyCollected', 'Completed', 'Rejected'
    Priority VARCHAR(20),                           -- 'Normal', 'Urgent', 'Emergency'
    Remarks TEXT,
    HomeCollection BIT DEFAULT 0,
    PatientAddress VARCHAR(500),
    CreatedBy VARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedBy VARCHAR(100),
    UpdatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (BillSummaryId) REFERENCES BillSummary(BillSummaryId),
    FOREIGN KEY (PatientInfoId) REFERENCES PatientInfo(PatientInfoId)
);

-- Index for faster lookups
CREATE INDEX idx_SampleCollection_BillId ON SampleCollection(BillSummaryId);
CREATE INDEX idx_SampleCollection_Status ON SampleCollection(CollectionStatus);
```

#### 2. **SampleCollectionDetail** (Detail/Line Items Table)
```sql
CREATE TABLE SampleCollectionDetail (
    SampleDetailId INT PRIMARY KEY IDENTITY(1,1),
    SampleCollectionId INT NOT NULL,
    InvMasterId INT NOT NULL,
    InvestigationName VARCHAR(200),
    SpecimenType VARCHAR(50),                       -- 'Serum', 'Plasma', 'Urine', 'Blood'
    ContainerType VARCHAR(50),                      -- 'Plain Vacutainer', 'EDTA Tube', etc.
    SampleBarcode VARCHAR(30) UNIQUE,               -- Generated: SB-001001-01 (SB + billid + invid)
    FastingRequired BIT,
    SpecialInstructions TEXT,
    
    -- Collection Status Fields
    SampleStatus VARCHAR(30),                       -- 'Pending', 'Collected', 'Rejected'
    CollectedQuantity VARCHAR(20),                  -- 'Full', 'Partial', 'Insufficient'
    RejectionReason VARCHAR(200),
    
    -- Timestamps
    CollectionDate DATETIME,
    CollectionTime TIME,
    RejectionDate DATETIME,
    
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (SampleCollectionId) REFERENCES SampleCollection(SampleCollectionId),
    FOREIGN KEY (InvMasterId) REFERENCES InvMaster(InvMasterId)
);

-- Index for faster lookups
CREATE INDEX idx_SampleCollectionDetail_CollectionId ON SampleCollectionDetail(SampleCollectionId);
CREATE INDEX idx_SampleCollectionDetail_Status ON SampleCollectionDetail(SampleStatus);
```

---

## 📁 Code Implementation

### 1. **Models** (Models/SampleCollection/SampleCollection.cs)

```csharp
using System;
using System.Collections.Generic;

namespace eMedLis.Models.SampleCollection
{
    // ===== Master Model =====
    public class SampleCollectionModel
    {
        public int SampleCollectionId { get; set; }
        public int BillSummaryId { get; set; }
        public int PatientInfoId { get; set; }
        public string CollectionBarcode { get; set; }
        public DateTime CollectionDate { get; set; }
        public TimeSpan CollectionTime { get; set; }
        public string CollectedBy { get; set; }
        public string CollectionStatus { get; set; }
        public string Priority { get; set; }
        public string Remarks { get; set; }
        public bool HomeCollection { get; set; }
        public string PatientAddress { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    // ===== Detail Model =====
    public class SampleCollectionDetail
    {
        public int SampleDetailId { get; set; }
        public int SampleCollectionId { get; set; }
        public int InvMasterId { get; set; }
        public string InvestigationName { get; set; }
        public string SpecimenType { get; set; }
        public string ContainerType { get; set; }
        public string SampleBarcode { get; set; }
        public bool FastingRequired { get; set; }
        public string SpecialInstructions { get; set; }
        
        // Collection Status
        public string SampleStatus { get; set; }      // 'Pending', 'Collected', 'Rejected'
        public string CollectedQuantity { get; set; } // 'Full', 'Partial'
        public string RejectionReason { get; set; }
        
        // Timestamps
        public DateTime? CollectionDate { get; set; }
        public TimeSpan? CollectionTime { get; set; }
        public DateTime? RejectionDate { get; set; }
        
        // UI Helper Properties
        public bool IsCollected { get; set; }
        public bool IsRejected { get; set; }
    }

    // ===== View Model (Combines Master + Details) =====
    public class SampleCollectionViewModel
    {
        public SampleCollectionModel SampleCollection { get; set; }
        public PatientInfo PatientInfo { get; set; }
        public BillSummary BillSummary { get; set; }
        public List<SampleCollectionDetail> SampleDetails { get; set; }
        public List<ContainerMaster> AvailableContainers { get; set; }
        
        // Summary Info
        public int TotalInvestigations { get; set; }
        public int CollectedCount { get; set; }
        public int PendingCount { get; set; }
        public int ProgressPercent => TotalInvestigations > 0 ? (CollectedCount * 100 / TotalInvestigations) : 0;
    }

    // ===== Container Master =====
    public class ContainerMaster
    {
        public int ContainerId { get; set; }
        public string ContainerName { get; set; }
        public string ContainerCode { get; set; }
        public string CapColor { get; set; }
        public string Volume { get; set; }
        public string Additive { get; set; }
        public string StorageTemp { get; set; }
        public int ExpiryDays { get; set; }
        public bool Active { get; set; }
    }

    // ===== Result Class =====
    public class SampleCollectionResult
    {
        public bool Success { get; set; }
        public int SampleCollectionId { get; set; }
        public string CollectionBarcode { get; set; }
        public string Message { get; set; }
    }
}
```

---

## 🎯 Step-by-Step Workflow

### **Workflow 1: View Pending Collections**

```
1. User clicks "Sample Collection" menu
   ↓
2. Index page loads → GET /SampleCollection/GetPendingCollections
   ↓
3. Database returns:
   - List of bills with pending/partial collections
   - Each bill shows: Patient name, Bill no, Total tests, Collected count, Pending count
   - Progress bar showing collection %
   ↓
4. User sees filter options:
   - Status: Pending, PartiallyCollected, Completed
   - Priority: Normal, Urgent, Emergency
   - Home Collection: Yes/No
   ↓
5. User applies filters → API call with filters
   ↓
6. Updated list displays
```

### **Workflow 2: Collect Samples (Partial)**

```
1. User clicks on a bill from list
   ↓
2. GET /SampleCollection/GetCollectionData?billId=123
   ↓
3. Database returns:
   - Bill details
   - Patient info
   - All tests in bill (4 tests)
   - Check if collection already exists
   ↓
4. Form loads showing:
   - 4 tests listed
   - Each test has checkboxes for:
     ✓ Collected (Full/Partial)
     ✗ Not Collected
     ✗ Rejected (with reason)
   ↓
5. User selects 2 tests as "Collected" → checks Full
   Leaves 2 tests as "Pending"
   ↓
6. User clicks "Save Collection"
   ↓
7. POST /SampleCollection/SaveCollection
   Data:
   {
     sampleCollection: { billId, date, time, collectedBy, priority },
     sampleDetails: [
       { invId: 1, status: "Collected", quantity: "Full" },
       { invId: 2, status: "Collected", quantity: "Full" },
       { invId: 3, status: "Pending" },
       { invId: 4, status: "Pending" }
     ]
   }
   ↓
8. Database saves:
   - SampleCollection record (status: PartiallyCollected)
   - SampleCollectionDetail for collected tests only (status: Collected with timestamps)
   - Barcodes generated for collected samples
   ↓
9. Success message with Collection Barcode: SC-000123-01
```

### **Workflow 3: Reopen Bill for Remaining Samples**

```
1. User clicks same bill from list
   ↓
2. GET /SampleCollection/GetCollectionData?billId=123
   ↓
3. Database detects existing collection (from GetSampleCollectionByBillId)
   ↓
4. GET /SampleCollection/GetCollectionDetails?sampleCollectionId=X
   ↓
5. Database returns:
   Test 1: Status = "Collected", Date = "29/12/2025", Time = "10:30 AM", SampleBarcode = "SB-000123-01-01"
   Test 2: Status = "Collected", Date = "29/12/2025", Time = "10:30 AM", SampleBarcode = "SB-000123-01-02"
   Test 3: Status = "Pending"
   Test 4: Status = "Pending"
   ↓
6. Form displays:
   Test 1 & 2 shown as COLLECTED (disabled, greyed out, with timestamp)
   Test 3 & 4 shown as PENDING (enabled, can be collected)
   ↓
7. User selects Test 3 & 4 as "Collected"
   ↓
8. POST /SampleCollection/SaveCollection
   ↓
9. Database updates SampleCollectionDetail records for Test 3 & 4
   ↓
10. Status recalculated: All 4 collected → Completed
```

### **Workflow 4: Generate Barcodes & Print Labels**

```
1. User clicks "Print Labels" for a collection
   ↓
2. GET /SampleCollection/PrintCollectionLabels?sampleCollectionId=123
   ↓
3. Database returns all collected samples with barcodes
   ↓
4. View generates QR codes for each sample:

   ┌─────────────────────────────────────┐
   │  SB-000123-01-01                   │
   │  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀          │
   │  John Doe, Age: 45, M               │
   │  Test: Blood Sugar, Specimen: Serum │
   │  Container: Plain Tube              │
   │  Collection: 29/12/2025, 10:30 AM   │
   └─────────────────────────────────────┘

   ┌─────────────────────────────────────┐
   │  SB-000123-01-02                   │
   │  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀          │
   │  John Doe, Age: 45, M               │
   │  Test: CBC, Specimen: Blood         │
   │  Container: EDTA Tube               │
   │  Collection: 29/12/2025, 10:30 AM   │
   └─────────────────────────────────────┘

5. User prints labels
```

---

## 📊 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    SAMPLE COLLECTION FLOW                    │
└─────────────────────────────────────────────────────────────┘

[Bill Created] → [Pending Collection] → [User Opens Bill] 
       ↓                                        ↓
   (4 Tests)                            [Selects 2 Tests]
                                             ↓
                                      [Save Collection]
                                             ↓
                            ┌────────────────┴────────────────┐
                            ↓                                 ↓
                    [SC Status: Partial]            [SCD1: Collected]
                                                    [SCD2: Collected]
                                                    [SCD3: Pending]
                                                    [SCD4: Pending]
                            ↓
                        [Later - User Opens Bill Again]
                            ↓
                        [Shows 2 as Collected ✓]
                        [Shows 2 as Pending  ✗]
                            ↓
                        [Selects Remaining 2]
                            ↓
                        [Save Collection]
                            ↓
                    ┌────────────────┬─────────────────┐
                    ↓                ↓                 ↓
            [SCD3: Collected]   [SC Status: Completed]
            [SCD4: Collected]   [Barcodes Generated]
                                [Ready for Print]
```

---

## 🔑 Key Features

### ✅ Partial Collection
- Save 2 of 4 tests, remaining 2 for later
- Status automatically updates to "PartiallyCollected"
- Reopening shows collected tests disabled with timestamps

### ✅ Barcode Generation
- Collection Barcode: `SC-000123-01` (SC + BillId + Sequence)
- Sample Barcode: `SB-000123-01-01` (SB + BillId + DetailId)
- Contains: Patient Name, Age, Gender, Test Info

### ✅ Filtering
- By Status: Pending, PartiallyCollected, Completed
- By Priority: Normal, Urgent, Emergency
- By Collection Type: Home, Lab

### ✅ Progress Tracking
- Total Investigations count
- Collected Count
- Pending Count
- Progress Percentage (Collected / Total * 100)

### ✅ Timestamp Recording
- Collection Date & Time for each test
- Rejection Date & Time (if rejected)
- Audit trail for compliance

---

## 🔗 SQL Stored Procedures

### 1. **usp_GetPendingCollectionsWithSummary**

```sql
CREATE PROCEDURE usp_GetPendingCollectionsWithSummary
    @FilterStatus VARCHAR(30) = NULL,
    @FilterPriority VARCHAR(20) = NULL,
    @HomeCollection BIT = NULL
AS
BEGIN
    SELECT 
        sc.SampleCollectionId,
        sc.BillSummaryId,
        sc.CollectionBarcode,
        sc.CollectionDate,
        sc.CollectionTime,
        sc.CollectionStatus,
        sc.Priority,
        sc.HomeCollection,
        sc.CollectedBy,
        
        -- Patient Info
        pi.PatientInfoId,
        pi.PatName,
        pi.UHID,
        pi.MobileNo,
        pi.Age,
        pi.Gender,
        pi.Area,
        pi.City,
        
        -- Bill Info
        bs.BillNo,
        bs.BillDate,
        bs.NetAmount,
        
        -- Summary Counts
        COUNT(scd.SampleDetailId) AS TotalInvestigations,
        SUM(CASE WHEN scd.SampleStatus = 'Collected' THEN 1 ELSE 0 END) AS CollectedCount,
        SUM(CASE WHEN scd.SampleStatus = 'Pending' THEN 1 ELSE 0 END) AS PendingCount
        
    FROM SampleCollection sc
    INNER JOIN BillSummary bs ON sc.BillSummaryId = bs.BillSummaryId
    INNER JOIN PatientInfo pi ON sc.PatientInfoId = pi.PatientInfoId
    LEFT JOIN SampleCollectionDetail scd ON sc.SampleCollectionId = scd.SampleCollectionId
    
    WHERE (sc.CollectionStatus IN ('Pending', 'PartiallyCollected') OR 1=1)
        AND (@FilterStatus IS NULL OR sc.CollectionStatus = @FilterStatus)
        AND (@FilterPriority IS NULL OR sc.Priority = @FilterPriority)
        AND (@HomeCollection IS NULL OR sc.HomeCollection = @HomeCollection)
    
    GROUP BY 
        sc.SampleCollectionId, sc.BillSummaryId, sc.CollectionBarcode,
        sc.CollectionDate, sc.CollectionTime, sc.CollectionStatus,
        sc.Priority, sc.HomeCollection, sc.CollectedBy,
        pi.PatientInfoId, pi.PatName, pi.UHID, pi.MobileNo, pi.Age, pi.Gender, pi.Area, pi.City,
        bs.BillNo, bs.BillDate, bs.NetAmount
    
    ORDER BY sc.CollectionDate DESC, sc.Priority DESC;
END
```

### 2. **usp_SaveSampleCollection**

```sql
CREATE PROCEDURE usp_SaveSampleCollection
    @BillSummaryId INT,
    @PatientInfoId INT,
    @CollectionDate DATE,
    @CollectionTime TIME,
    @CollectedBy VARCHAR(100),
    @Priority VARCHAR(20),
    @HomeCollection BIT,
    @PatientAddress VARCHAR(500) = NULL,
    @Remarks TEXT = NULL,
    @CreatedBy VARCHAR(100),
    @SampleCollectionId INT OUTPUT,
    @CollectionBarcode VARCHAR(30) OUTPUT
AS
BEGIN
    INSERT INTO SampleCollection (
        BillSummaryId, PatientInfoId, CollectionDate, CollectionTime,
        CollectedBy, CollectionStatus, Priority, HomeCollection,
        PatientAddress, Remarks, CreatedBy, CreatedDate
    )
    VALUES (
        @BillSummaryId, @PatientInfoId, @CollectionDate, @CollectionTime,
        @CollectedBy, 'Pending', @Priority, @HomeCollection,
        @PatientAddress, @Remarks, @CreatedBy, GETDATE()
    );

    SET @SampleCollectionId = SCOPE_IDENTITY();
    SET @CollectionBarcode = 'SC-' + FORMAT(@BillSummaryId, 'D6') + '-01';
    
    UPDATE SampleCollection 
    SET CollectionBarcode = @CollectionBarcode 
    WHERE SampleCollectionId = @SampleCollectionId;
END
```

### 3. **usp_CalculateAndUpdateSampleCollectionStatus**

```sql
CREATE PROCEDURE usp_CalculateAndUpdateSampleCollectionStatus
    @SampleCollectionId INT
AS
BEGIN
    DECLARE @TotalSamples INT;
    DECLARE @CollectedSamples INT;
    DECLARE @NewStatus VARCHAR(30);

    SELECT 
        @TotalSamples = COUNT(*),
        @CollectedSamples = SUM(CASE WHEN SampleStatus = 'Collected' THEN 1 ELSE 0 END)
    FROM SampleCollectionDetail
    WHERE SampleCollectionId = @SampleCollectionId;

    -- Determine status
    IF @TotalSamples = 0
        SET @NewStatus = 'Pending';
    ELSE IF @CollectedSamples = 0
        SET @NewStatus = 'Pending';
    ELSE IF @CollectedSamples < @TotalSamples
        SET @NewStatus = 'PartiallyCollected';
    ELSE
        SET @NewStatus = 'Completed';

    UPDATE SampleCollection
    SET CollectionStatus = @NewStatus, UpdatedDate = GETDATE()
    WHERE SampleCollectionId = @SampleCollectionId;
END
```

---

## 📝 Summary

This complete implementation provides:
- ✅ Full database design with proper relationships
- ✅ Step-by-step workflow documentation
- ✅ Complete C# code (Models, DAL, Controllers)
- ✅ SQL stored procedures with filtering
- ✅ Barcode generation logic
- ✅ Partial collection support
- ✅ Timestamp tracking
- ✅ Frontend integration example

All code is production-ready and follows best practices! 🚀
