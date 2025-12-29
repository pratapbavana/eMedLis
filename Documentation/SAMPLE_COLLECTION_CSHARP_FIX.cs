// ===================================================================
// FIX: Partial Sample Collection - C# Implementation Changes
// File: DAL/SampleCollection/SampleCollectionDB.cs
// ===================================================================
// This file contains the C# methods to add to SampleCollectionDB class
// to properly handle collected and pending samples
// ===================================================================

using eMedLis.Models.SampleCollection;
using eMedLis.Models.PatientBilling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace eMedLis.DAL.SampleCollection
{
    public partial class SampleCollectionDB
    {
        /// <summary>
        /// Get only PENDING samples for collection
        /// Modified to filter out already collected samples
        /// </summary>
        public SampleCollectionViewModel GetBillForCollectionPending(int billSummaryId)
        {
            var viewModel = new SampleCollectionViewModel();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("usp_GetBillForSampleCollection", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);
                    cmd.Parameters.AddWithValue("@OnlyPending", 1); // Filter to pending only

                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        var billDetails = new List<BillDetail>();

                        while (reader.Read())
                        {
                            if (viewModel.PatientInfo == null)
                            {
                                viewModel.PatientInfo = new PatientInfo
                                {
                                    PatientInfoId = SafeGetInt(reader, "PatientInfoId"),
                                    UHID = SafeGetString(reader, "UHID"),
                                    PatName = SafeGetString(reader, "PatName"),
                                    MobileNo = SafeGetString(reader, "MobileNo"),
                                    Age = SafeGetInt(reader, "Age"),
                                    AgeType = SafeGetString(reader, "AgeType"),
                                    Gender = SafeGetString(reader, "Gender"),
                                    Area = SafeGetString(reader, "Area"),
                                    City = SafeGetString(reader, "City"),
                                    Ref = SafeGetString(reader, "Ref")
                                };

                                viewModel.BillSummary = new BillSummary
                                {
                                    BillSummaryId = SafeGetInt(reader, "BillSummaryId"),
                                    BillNo = SafeGetString(reader, "BillNo"),
                                    BillDate = SafeGetDateTime(reader, "BillDate"),
                                    NetAmount = SafeGetDecimal(reader, "NetAmount"),
                                    TotalBill = SafeGetDecimal(reader, "TotalBill"),
                                    TotalDiscountAmount = SafeGetDecimal(reader, "TotalDiscountAmount"),
                                    PaidAmount = SafeGetDecimal(reader, "PaidAmount"),
                                    DueAmount = SafeGetDecimal(reader, "DueAmount")
                                };
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("InvId")))
                            {
                                billDetails.Add(new BillDetail
                                {
                                    InvId = SafeGetString(reader, "InvId"),
                                    InvName = SafeGetString(reader, "InvName"),
                                    Rate = SafeGetDecimal(reader, "Rate"),
                                    SpecimenType = SafeGetString(reader, "SpecimenName", "Serum"),
                                    ContainerType = SafeGetString(reader, "VacutainerName", "Plain Vacutainer"),
                                    FastingRequired = SafeGetBoolean(reader, "FastingRequired"),
                                    SpecialInstructions = SafeGetString(reader, "SpecialInstructions")
                                });
                            }
                        }

                        viewModel.BillDetails = billDetails;
                    }
                }
            }

            return viewModel;
        }

        /// <summary>
        /// Get collection summary for a bill
        /// Shows all previously collected samples for this bill
        /// </summary>
        public SampleCollectionViewModel GetCollectionSummaryForBill(int billSummaryId)
        {
            var viewModel = new SampleCollectionViewModel();
            var allDetails = new List<SampleCollectionDetail>();
            var collectedSamples = new List<SampleCollectionDetail>();
            var pendingSamples = new List<SampleCollectionDetail>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("usp_GetCollectionSummaryForBill", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);

                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        // Use dictionary to get the latest entry per investigation
                        var sampleDict = new Dictionary<int, SampleCollectionDetail>();

                        while (reader.Read())
                        {
                            var rowNum = SafeGetInt(reader, "RowNum");
                            if (rowNum == 1) // Only take the latest
                            {
                                int invMasterId = SafeGetInt(reader, "InvMasterId");
                                var detail = new SampleCollectionDetail
                                {
                                    SampleDetailId = SafeGetInt(reader, "SampleDetailId"),
                                    InvMasterId = invMasterId,
                                    InvestigationName = SafeGetString(reader, "InvestigationName"),
                                    SpecimenType = SafeGetString(reader, "SpecimenType"),
                                    ContainerType = SafeGetString(reader, "ContainerType"),
                                    SampleBarcode = SafeGetString(reader, "SampleBarcode"),
                                    SampleStatus = SafeGetString(reader, "SampleStatus"),
                                    CollectionDate = SafeGetDateTime(reader, "CollectionDate", null),
                                    CollectionTime = SafeGetTimeSpan(reader, "CollectionTime", null),
                                    CollectedQuantity = SafeGetString(reader, "CollectedQuantity"),
                                    RejectionReason = SafeGetString(reader, "RejectionReason"),
                                    RejectionDate = SafeGetDateTime(reader, "RejectionDate", null),
                                    IsCollected = SafeGetBoolean(reader, "IsCollected"),
                                    IsRejected = SafeGetBoolean(reader, "IsRejected")
                                };

                                sampleDict[invMasterId] = detail;
                                allDetails.Add(detail);

                                if (detail.SampleStatus == "Collected")
                                    collectedSamples.Add(detail);
                                else if (detail.SampleStatus == "Pending" || string.IsNullOrEmpty(detail.SampleStatus))
                                    pendingSamples.Add(detail);
                            }
                        }
                    }
                }
            }

            viewModel.SampleDetails = pendingSamples;
            viewModel.CollectedSamples = collectedSamples;
            return viewModel;
        }

        /// <summary>
        /// Get only truly pending samples for a bill
        /// (samples that haven't been collected yet)
        /// </summary>
        public List<BillDetail> GetTrulyPendingSamplesForBill(int billSummaryId)
        {
            var pendingSamples = new List<BillDetail>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("usp_GetPendingSamplesForBill", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillSummaryId", billSummaryId);

                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pendingSamples.Add(new BillDetail
                            {
                                InvId = SafeGetString(reader, "InvId"),
                                InvName = SafeGetString(reader, "InvName"),
                                SpecimenType = SafeGetString(reader, "SpecimenName"),
                                ContainerType = SafeGetString(reader, "ContainerName"),
                                FastingRequired = SafeGetBoolean(reader, "FastingRequired"),
                                SpecialInstructions = SafeGetString(reader, "SpecialInstructions")
                            });
                        }
                    }
                }
            }

            return pendingSamples;
        }

        /// <summary>
        /// Check if all samples for a bill have been collected
        /// </summary>
        public bool AreAllSamplesCollected(int billSummaryId)
        {
            var summary = GetCollectionSummaryForBill(billSummaryId);
            return summary.SampleDetails == null || summary.SampleDetails.Count == 0;
        }

        /// <summary>
        /// Get sample collection progress for a bill
        /// Returns counts of collected, pending, and rejected samples
        /// </summary>
        public SampleCollectionProgress GetCollectionProgress(int billSummaryId)
        {
            var summary = GetCollectionSummaryForBill(billSummaryId);
            var totalFromBill = GetBillForCollection(billSummaryId).BillDetails?.Count ?? 0;

            return new SampleCollectionProgress
            {
                BillSummaryId = billSummaryId,
                TotalSamples = totalFromBill,
                CollectedCount = summary.CollectedSamples?.Count ?? 0,
                PendingCount = summary.SampleDetails?.Count ?? 0,
                RejectedCount = summary.CollectedSamples?.Count(s => s.IsRejected) ?? 0
            };
        }
    }

    /// <summary>
    /// Model for sample collection progress
    /// </summary>
    public class SampleCollectionProgress
    {
        public int BillSummaryId { get; set; }
        public int TotalSamples { get; set; }
        public int CollectedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }

        /// <summary>
        /// Get completion percentage
        /// </summary>
        public decimal GetCompletionPercentage()
        {
            if (TotalSamples == 0) return 0;
            return Math.Round((decimal)CollectedCount / TotalSamples * 100, 2);
        }

        /// <summary>
        /// Check if collection is complete
        /// </summary>
        public bool IsCollectionComplete()
        {
            return PendingCount == 0 && RejectedCount == 0;
        }

        /// <summary>
        /// Get human-readable status
        /// </summary>
        public string GetStatus()
        {
            if (PendingCount == 0 && RejectedCount == 0)
                return "Completed";
            if (PendingCount == TotalSamples)
                return "Pending";
            if (CollectedCount > 0 && PendingCount > 0)
                return "Partial";
            if (RejectedCount > 0)
                return "Rejected";
            return "Unknown";
        }
    }
}

// ===================================================================
// USAGE IN CONTROLLER:
// ===================================================================

/*
public ActionResult CollectSample(int billSummaryId)
{
    try
    {
        var db = new SampleCollectionDB();
        
        // Check collection progress
        var progress = db.GetCollectionProgress(billSummaryId);
        
        if (progress.IsCollectionComplete())
        {
            ViewBag.Message = $"All samples collected. Completion: {progress.GetCompletionPercentage()}%";
            return RedirectToAction("BillsList");
        }
        
        // Get pending samples only
        var viewModel = db.GetBillForCollectionPending(billSummaryId);
        viewModel.CollectionProgress = progress;
        
        // Get previous collections for reference
        var summary = db.GetCollectionSummaryForBill(billSummaryId);
        viewModel.CollectedSamples = summary.CollectedSamples;
        
        return View(viewModel);
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine(ex.Message);
        return RedirectToAction("Error");
    }
}
*/
