using eMedLis.DAL.SampleCollection;
using eMedLis.Models.SampleCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace eMedLis.Controllers
{
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
                var collectionBarcode = _db.SaveSampleCollection(sampleCollection, sampleDetails);

                return Json(new
                {
                    success = true,
                    message = "Sample collection created successfully!",
                    collectionBarcode = collectionBarcode
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error creating sample collection: " + ex.Message
                });
            }
        }

        [HttpGet]
        public JsonResult GetPendingCollections()
        {
            try
            {
                var collections = _db.GetPendingCollections();

                var result = collections.Select(c => new
                {
                    sampleCollectionId = c.SampleCollection.SampleCollectionId,
                    collectionBarcode = c.SampleCollection.CollectionBarcode,
                    collectionDate = c.SampleCollection.CollectionDate.ToString("dd/MM/yyyy"),
                    collectionTime = c.SampleCollection.CollectionTime.ToString(@"hh\:mm"),
                    patientName = c.PatientInfo.PatName,
                    uhid = c.PatientInfo.UHID,
                    billNo = c.BillSummary.BillNo,
                    priority = c.SampleCollection.Priority,
                    status = c.SampleCollection.CollectionStatus,
                    homeCollection = c.SampleCollection.HomeCollection,
                    collectedBy = c.SampleCollection.CollectedBy
                });

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
    }
}