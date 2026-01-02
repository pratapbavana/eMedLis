using eMedLis.DAL.SampleCollection;
using eMedLis.Models.SampleCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
    [Authorize]
    public class SampleCollectionController : Controller
    {
        private readonly SampleCollectionDB _db;

        public SampleCollectionController()
        {
            _db = new SampleCollectionDB();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CreateCollection(int billId)
        {
            try
            {
                var viewModel = _db.GetBillForCollection(billId);
                viewModel.AvailableContainers = _db.GetActiveContainers();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading bill details: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public JsonResult SaveCollection(SampleCollectionModel sampleCollection, List<SampleCollectionDetail> sampleDetails)
        {
            try
            {
                var dbResult = _db.SaveSampleCollection(sampleCollection, sampleDetails);

                if (dbResult.Success)
                {
                    // Calculate status after saving all details
                    _db.CalculateAndUpdateCollectionStatus(dbResult.SampleCollectionId);

                    return Json(new
                    {
                        success = true,
                        message = "Sample collection saved successfully",
                        sampleCollectionId = dbResult.SampleCollectionId,
                        collectionBarcode = dbResult.CollectionBarcode
                    });
                }

                return Json(new { success = false, message = dbResult.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetPendingCollections()
        {
            try
            {
                var collections = _db.GetPendingCollectionsWithSummary();

                var result = collections.Select(c => new
                {
                    sampleCollectionId = c.SampleCollection.SampleCollectionId,
                    billSummaryId = c.SampleCollection.BillSummaryId,
                    collectionBarcode = string.IsNullOrEmpty(c.SampleCollection.CollectionBarcode)
                        ? "New"
                        : c.SampleCollection.CollectionBarcode,
                    collectionDate = c.SampleCollection.CollectionDate.ToString("dd/MM/yyyy"),
                    collectionTime = c.SampleCollection.CollectionTime.ToString(@"hh\:mm"),
                    patientName = c.PatientInfo.PatName,
                    uhid = c.PatientInfo.UHID,
                    ageGender = c.PatientInfo.Age + " / " + c.PatientInfo.Gender,
                    mobileNo = c.PatientInfo.MobileNo,
                    billNo = c.BillSummary.BillNo,
                    billDate = c.BillSummary.BillDate.ToString("dd/MM/yyyy"),
                    netAmount = c.BillSummary.NetAmount.ToString("F2"),
                    priority = c.SampleCollection.Priority,
                    status = c.SampleCollection.CollectionStatus,
                    homeCollection = c.SampleCollection.HomeCollection,
                    collectedBy = c.SampleCollection.CollectedBy,
                    totalInvestigations = c.TotalInvestigations,
                    collectedCount = c.CollectedCount,
                    pendingCount = c.PendingCount,
                    progressPercent = c.TotalInvestigations > 0 ? (c.CollectedCount * 100 / c.TotalInvestigations) : 0
                }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult PrintCollectionLabels(int sampleCollectionId)
        {
            // Implementation for printing sample labels with barcodes
            return View();
        }
        [HttpGet]
        public JsonResult GetCollectionData(int? billId)
        {
            try
            {
                if (!billId.HasValue || billId == 0)
                {
                    return Json(new { success = false, message = "Invalid Bill ID" }, JsonRequestBehavior.AllowGet);
                }

                var viewModel = _db.GetBillForCollection(billId.Value);

                if (viewModel == null || viewModel.BillSummary == null)
                {
                    return Json(new { success = false, message = "Bill not found" }, JsonRequestBehavior.AllowGet);
                }

                // Check if this bill already has a sample collection
                var existingCollection = _db.GetSampleCollectionByBillId(billId.Value);
                int sampleCollectionId = existingCollection?.SampleCollectionId ?? 0;
                string collectionBarcode = existingCollection?.CollectionBarcode ?? "";
                string collectionStatus = existingCollection?.CollectionStatus ?? "New";

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        billSummaryId = viewModel.BillSummary.BillSummaryId,
                        billNo = viewModel.BillSummary.BillNo,
                        billDate = viewModel.BillSummary.BillDate.ToString("dd/MM/yyyy"),
                        netAmount = viewModel.BillSummary.NetAmount.ToString("F2"),

                        // Include existing collection info if available
                        sampleCollectionId = sampleCollectionId,
                        collectionBarcode = collectionBarcode,
                        collectionStatus = collectionStatus,
                        collectionDate = existingCollection?.CollectionDate.ToString("dd/MM/yyyy"),
                        collectionTime = existingCollection?.CollectionTime.ToString(@"hh\:mm"),

                        patientInfo = new
                        {
                            patientInfoId = viewModel.PatientInfo.PatientInfoId,
                            patName = viewModel.PatientInfo.PatName,
                            uhid = viewModel.PatientInfo.UHID,
                            mobileNo = viewModel.PatientInfo.MobileNo,
                            age = viewModel.PatientInfo.Age,
                            gender = viewModel.PatientInfo.Gender,
                            area = viewModel.PatientInfo.Area,
                            city = viewModel.PatientInfo.City
                        },

                        billDetails = viewModel.BillDetails.Select(d => new
                        {
                            invId = d.InvId,
                            invName = d.InvName,
                            rate = d.Rate.ToString("F2"),
                            specimenType = d.SpecimenType ?? "Serum",
                            containerType = d.ContainerType ?? "Plain Vacutainer",
                            fastingRequired = d.FastingRequired,
                            specialInstructions = d.SpecialInstructions
                        }).ToList()
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult GetCollectionDetails(int? sampleCollectionId = null)
        {
            try
            {
                // Validate parameter
                if (!sampleCollectionId.HasValue || sampleCollectionId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid sampleCollectionId: " + sampleCollectionId);
                    return Json(new { success = false, message = "Invalid Collection ID" }, JsonRequestBehavior.AllowGet);
                }

                System.Diagnostics.Debug.WriteLine("Getting collection details for ID: " + sampleCollectionId);

                var viewModel = _db.GetSampleCollectionDetailsForEdit(sampleCollectionId.Value);

                if (viewModel == null)
                {
                    return Json(new { success = false, message = "Collection not found" }, JsonRequestBehavior.AllowGet);
                }

                if (viewModel.SampleCollection == null)
                {
                    return Json(new { success = false, message = "Collection master record not found" }, JsonRequestBehavior.AllowGet);
                }

                // Build sample details list
                var sampleDetailsArray = new List<object>();

                if (viewModel.SampleDetails != null && viewModel.SampleDetails.Count > 0)
                {
                    foreach (var d in viewModel.SampleDetails)
                    {
                        sampleDetailsArray.Add(new
                        {
                            sampleDetailId = d.SampleDetailId,
                            invMasterId = d.InvMasterId,
                            investigationName = d.InvestigationName,
                            sampleStatus = d.SampleStatus ?? "Pending",
                            collectedQuantity = d.CollectedQuantity,
                            rejectionReason = d.RejectionReason,
                            collectionDate = d.CollectionDate.HasValue ? d.CollectionDate.Value.ToString("dd/MM/yyyy") : "",
                            collectionTime = d.CollectionTime.HasValue ? d.CollectionTime.Value.ToString(@"hh\:mm") : "",
                            rejectionDate = d.RejectionDate.HasValue ? d.RejectionDate.Value.ToString("dd/MM/yyyy HH:mm") : "",
                            isCollected = d.SampleStatus == "Collected",
                            isRejected = d.SampleStatus == "Rejected"
                        });
                    }
                }

                System.Diagnostics.Debug.WriteLine("Returning " + sampleDetailsArray.Count + " sample details");

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        sampleCollectionId = viewModel.SampleCollection.SampleCollectionId,
                        collectionBarcode = viewModel.SampleCollection.CollectionBarcode ?? "",
                        collectionDate = viewModel.SampleCollection.CollectionDate.ToString("dd/MM/yyyy"),
                        collectionTime = viewModel.SampleCollection.CollectionTime.ToString(@"hh\:mm"),
                        collectedBy = viewModel.SampleCollection.CollectedBy ?? "Admin",
                        priority = viewModel.SampleCollection.Priority ?? "Normal",
                        remarks = viewModel.SampleCollection.Remarks ?? "",
                        homeCollection = viewModel.SampleCollection.HomeCollection,
                        patientAddress = viewModel.SampleCollection.PatientAddress ?? "",
                        sampleDetails = sampleDetailsArray
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetCollectionDetails Error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack: " + ex.StackTrace);

                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }



    }
}