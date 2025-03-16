using System.Drawing;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using rcsa.Data;
using rcsa.Models;  
public class AssessmentController : Controller
{
    private readonly DatabaseContext _context;

    public AssessmentController(DatabaseContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var assessments = _context.Assessments.ToList();

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");

        var headings = _context.Headings
   .Include(h => h.Subheadings) 
   .ToList();
        ViewBag.Headings = headings;
        var Subheadings = _context.SubHeadings.ToList();
        ViewBag.SubHeadings = Subheadings;
        return View(assessments);
    }

    [HttpPost]
    public IActionResult Edit(int Id, string Question, double Marks ,int NAflag)
    {

        var assessment = _context.Assessments.Find(Id);
        if (assessment == null)
        {
            return Json(new { success = false });
        }

        assessment.Question = Question;
        assessment.Marks = Marks;
        assessment.NAflag = NAflag;
        _context.SaveChanges();

        return Json(new { success = true, updatedQuestion = Question, updatedMarks = Marks, updatedNAflag = NAflag });
      

    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var assessment = _context.Assessments.Find(id);
        if (assessment == null)
        {
            return NotFound();
        }

        _context.Assessments.Remove(assessment);
        _context.SaveChanges();

        return RedirectToAction("Index"); 
    }
    [HttpPost]
    public IActionResult Index(Assessment model)
    {
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");


        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index"); 
        }
        if (ModelState.IsValid)
        {
            //model.subheadingTitel = "Your value here";
            //model.headingTitel = "Your value here";
            _context.Assessments.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        var headings = _context.Headings
   .Include(h => h.Subheadings) 
   .ToList();
        ViewBag.Headings = headings;
        return View(model);
    }
  


    [HttpPost]
    public async Task<IActionResult> InsertAllAssessments(DateTime fromdate, DateTime todate ,string Astype)
    {
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");

        var assessmentAnswers = await _context.AssessmentAnswers.ToListAsync();
  

        bool exists = await _context.AssessmentsPe
      .AnyAsync(a => fromdate <= a.todate && todate >= a.fromdate && a.Astype == Astype);

        if (exists)
        {
            TempData["ErrorMessage"] = "You have already submitted an assessment for this date range and type.";
            return RedirectToAction("Index");
        }

        var assessments = await _context.Assessments.AsNoTracking().ToListAsync();

       
        var assessmentsPeList = assessments.Select(a => new assessments_pe
        {
            QuestionId = a.Id,  
            QuestionText = a.Question, 
            Marks = a.Marks,
            SubHeading = a.SubHeading,
            MainHeading = a.MainHeading,
            fromdate = fromdate,
            todate = todate,
            Astype = Astype,
            subheadingTitel = a.subheadingTitel,
            headingTitel= a.headingTitel,
            NAflag = a.NAflag,
            SubmitBy = ViewBag.UserName
        }).ToList();

        if (assessmentsPeList.Any())
        {
           
            await _context.AssessmentsPe.AddRangeAsync(assessmentsPeList);
            await _context.SaveChangesAsync();
            TempData["ErrorMessage"] = "Successfully Save.";
        }

        return RedirectToAction("Index");
    }

    public IActionResult OldOwnreport(DateTime fromdate)
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        int branchId = HttpContext.Session.GetInt32("BranchID") ?? 0;

        var branchType = HttpContext.Session.GetString("BranchType");

        var daterange = _context.AssessmentAnswers
             .Where(sc => sc.ServiceCenterId == branchId )
             .Select(sc => sc.fromdate)
             .Distinct()
             .ToList();
              ViewBag.fromdate = daterange;

              var AssessmentAnswers = _context.AssessmentAnswers
              .Where(a => a.ServiceCenterId == branchId && a.fromdate == fromdate )
              .ToList();



        double? totalScore = null;

        if (branchType == "BRANCH")
        {
            totalScore = _context.AssessmentAnswers
                .Where(sc => sc.ServiceCenterId == branchId && sc.fromdate == fromdate && sc.Astype == "B")
                .Select(sc => sc.BrRTotalScore)
                .Distinct()
                .FirstOrDefault();
        }
        else if (branchType == "SERV_CEN")
        {
            totalScore = _context.AssessmentAnswers
                .Where(sc => sc.ServiceCenterId == branchId && sc.fromdate == fromdate && sc.Astype == "S")
                .Select(sc => sc.TotalScore)
                .Distinct()
                .FirstOrDefault();
        }

        ViewBag.TotalScore = totalScore;




        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = branchId;
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");


        return View(AssessmentAnswers);
    }
   
  
    public IActionResult BranchOwnreview(int ServiceCenterId)
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        //   int branchId = HttpContext.Session.GetInt32("BranchID") ?? 0; // Get BranchID from session
        int BranchID = HttpContext.Session.GetInt32("BranchID") ?? 0; 

       
        var assessmentAnswers = _context.AssessmentAnswers
            .Where(a => a.ServiceCenterBranch == BranchID && a.ServiceCenterId == ServiceCenterId && a.branchflag == 1 && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate)
            .ToList();

        var serviceCenters = _context.AssessmentAnswers
       .Where(sc => sc.ServiceCenterBranch == BranchID && sc.branchflag == 1 && sc.ServiceCenterId != BranchID && DateTime.Now >= sc.fromdate && DateTime.Now <= sc.todate)
       .GroupBy(sc => sc.ServiceCenterId)
       .Select(group => new
       {
           ServiceCenterId = group.Key,
           BranchName = _context.Branches
               .Where(b => b.BranchID == group.Key)
               .Select(b => b.Name)
               .FirstOrDefault()
       })
       .ToList();

        ViewBag.ServiceCenters = serviceCenters;

        string branchNameForServiceCenter = _context.Branches
       .Where(b => b.BranchID == ServiceCenterId)
       .Select(b => b.Name)
       .FirstOrDefault();

        ViewBag.BranchNameForServiceCenter = branchNameForServiceCenter;

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = BranchID;
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");

     
        return View(assessmentAnswers);
    }

    public IActionResult OldBranchOwnreview(int ServiceCenterId,DateTime fromdate)
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        int BranchID = HttpContext.Session.GetInt32("BranchID") ?? 0;


        var assessmentAnswers = _context.AssessmentAnswers
            .Where(a => a.ServiceCenterBranch == BranchID && a.ServiceCenterId == ServiceCenterId && a.branchflag == 1 && a.fromdate == fromdate)
            .ToList();

        
               var serviceCenters = _context.AssessmentAnswers
              .Where(sc => sc.ServiceCenterBranch == BranchID && sc.branchflag == 1  && sc.ServiceCenterId != BranchID)
              .GroupBy(sc => sc.ServiceCenterId)
              .Select(group => new
               {
                ServiceCenterId = group.Key,
               BranchName = _context.Branches
       .Where(b => b.BranchID == group.Key)
       .Select(b => b.Name)
       .FirstOrDefault()
            })
        .ToList();

        ViewBag.ServiceCenters = serviceCenters;
        var daterange = _context.AssessmentAnswers
      .Where(sc => sc.ServiceCenterBranch == BranchID && sc.branchflag == 1)
      .Select(sc => sc.fromdate )
      .Distinct()
      .ToList();
        ViewBag.fromdate = daterange;

        string branchNameForServiceCenter = _context.Branches
      .Where(b => b.BranchID == ServiceCenterId)
      .Select(b => b.Name)
      .FirstOrDefault();

        ViewBag.BranchNameForServiceCenter = branchNameForServiceCenter;

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = BranchID;
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");

     
        return View(assessmentAnswers);
    }

    public IActionResult RegionOwnReport(int ServiceCenterId,DateTime fromdate)
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

     //   int branchId = HttpContext.Session.GetInt32("BranchID") ?? 0; // Get BranchID from session
        int RegionID = HttpContext.Session.GetInt32("RegionID") ?? 0; // Get BranchID from session

     
        var assessmentAnswers = _context.AssessmentAnswers
            .Where(a => a.RegionId == RegionID &&  a.ServiceCenterId ==  ServiceCenterId && a.Regionflag==1 && a.fromdate == fromdate)
            .ToList();
        var daterange = _context.AssessmentAnswers
      .Where(sc => sc.RegionId == RegionID && sc.branchflag == 1)
      .Select(sc => sc.fromdate)
      .Distinct()
      .ToList();
        ViewBag.fromdate = daterange;

        var serviceCenters = _context.AssessmentAnswers
     .Where(sc => sc.ServiceCenterRegion == RegionID && sc.Regionflag == 1)
      .GroupBy(sc => sc.ServiceCenterId)
      .Select(group => new
      {
          ServiceCenterId = group.Key,
          BranchName = _context.Branches
              .Where(b => b.BranchID == group.Key)
              .Select(b => b.Name)
              .FirstOrDefault()
      })
      .ToList();
        ViewBag.ServiceCenters = serviceCenters;

        string branchNameForServiceCenter = _context.Branches
   .Where(b => b.BranchID == ServiceCenterId)
   .Select(b => b.Name)
   .FirstOrDefault();

        ViewBag.BranchNameForServiceCenter = branchNameForServiceCenter;

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = RegionID;
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");

        return View(assessmentAnswers);
    }
   

    public IActionResult SericeCenterAssesment()
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }


        int branchId = HttpContext.Session.GetInt32("BranchID") ?? 0;
         var BranchType= HttpContext.Session.GetString("BranchType");
        //chnage the date range fillter *********
        bool isAssessmentSubmitted = _context.AssessmentAnswers
        .Any(a => (BranchType == "BRANCH" ? a.branchflag == 1 : a.ServiceCenterflag == 1 )
                  && a.ServiceCenterId == branchId && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate /*&& BranchType == "BRANCH" ? a.Astype == "B" : a.Astype == "S"*/);


        if (isAssessmentSubmitted)
        {
            ViewBag.AssessmentMessage = "You have  submitted the assessment.";
        }

        var latestFromDate = _context.AssessmentsPe
                  .Where(a => (BranchType == "BRANCH" ? a.Astype == "B" : a.Astype == "S"))
            .OrderByDescending(a => a.fromdate)
            .Select(a => a.fromdate)
            .FirstOrDefault();

        var scdaysEntry = _context.Rangs
            .Select(a => a.scdays)
            .FirstOrDefault();


        int daysToAdd = scdaysEntry;

        if (latestFromDate == default || DateTime.Now > latestFromDate.AddDays(daysToAdd))
        {
            ViewBag.AssessmentMessage = "Assessment has expired.";
     
        }
        var AssessmentsPe = _context.AssessmentsPe
            .Where(a => a.fromdate == latestFromDate &&
                        DateTime.Now >= a.fromdate &&
                        DateTime.Now <= a.fromdate.AddDays(daysToAdd) &&
                        (BranchType == "BRANCH" ? a.Astype == "B" : a.Astype == "S"))
            .ToList();


        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = branchId;
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");

        return View(AssessmentsPe);
    }
    public IActionResult RroAssessment(DateTime fromdate)
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        var userId = HttpContext.Session.GetInt32("UserId");

        var UserName = HttpContext.Session.GetString("UserName");
        var RegionId = HttpContext.Session.GetInt32("RegionId");
        var serviceCenters = _context.Branches
            .Where(sc => sc.RegionID == RegionId)
            .Select(sc => new { ServiceCenterId = sc.BranchID, sc.Name }) // Select required fields
            .ToList();

        ViewBag.ServiceCenters = new SelectList(serviceCenters, "ServiceCenterId", "Name");



        var AssessmentsPe = _context.AssessmentsPe
        .Where(a => a.Astype == "R" && a.fromdate== fromdate)
        .ToList();

        ViewBag.Id = HttpContext.Session.GetInt32("UserId");

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.ServiceNo = HttpContext.Session.GetInt32("ServiceNo");

        ViewBag.UserName = HttpContext.Session.GetString("RRoname");
        ViewBag.ServiceNo = HttpContext.Session.GetInt32("RegionId");
        return View(AssessmentsPe);
    }
  
    public IActionResult GetRroAssessment(int servicecenterId , DateTime fromdate )
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        var ServiceNo = HttpContext.Session.GetInt32("ServiceNo");


        var assessments = _context.RROassessmentanswers
          .Where(raa => raa.ServiceCenterId == servicecenterId && raa.fromdate == fromdate)
          
          .ToList();
      


        ViewBag.Id = HttpContext.Session.GetInt32("UserId");

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.ServiceNo = HttpContext.Session.GetInt32("ServiceNo");
        return View(assessments);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveRRoNewAnswers(int ServiceCenterId, DateTime DateofVist, TimeSpan Intime, TimeSpan OutTime, DateTime fromdate, List<RRoAssessmentAnswerViewModel> answers)
    {
        if (answers == null || !answers.Any())
        {
            return BadRequest("No data received.");
        }
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        int ServiceNo = HttpContext.Session.GetInt32("ServiceNo") ?? 0;

        ViewBag.Id = HttpContext.Session.GetInt32("UserId");
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.ServiceNo = HttpContext.Session.GetInt32("ServiceNo");
        ViewBag.RRoname = HttpContext.Session.GetString("RRoname");
        ViewBag.RegionId = HttpContext.Session.GetInt32("RegionId");
        int  RegionId = HttpContext.Session.GetInt32("RegionId")?? 0;
        // Find the MAIN_BRANCH_ID for the given ServiceCenterId
        var branchRecord = _context.Branches
            .Where(b => b.BranchID == ServiceCenterId)
            .Select(b => b.MAIN_BRANCH_ID)
            .FirstOrDefault();  // Get the first matching record

        int? BranchId = branchRecord ; // Assign 0 if no branch is found

     


        var RROassessmentanswers = answers.Select(a => new RROassessmentanswers
        {
            QuestionId = a.QuestionId,
            AnswerText = a.QuestionText,
            RRoAnswer = a.RRoAnswer,
            SaveAnswer = a.SaveAnswer,
            Marks = a.Marks,
            RegionId = RegionId,
            MainHeading = a.MainHeading,
            subheading = a.subheading,
            TotalMarks = ((a.RRoAnswer == "Yes" || a.RRoAnswer == "NA") ? 1 : 0) * a.Marks,
            RRoComment = a.RRoComment ?? "00",
            DateofVist = DateofVist,
            Intime = Intime,
            OutTime = OutTime,
            BranchId = BranchId ?? 0,
            //todate =a.todate,
            ServiceCenterId = ServiceCenterId,
            fromdate = fromdate,
           
            ASubimteddate = DateTime.Now,
            Suser = ServiceNo,
            headingTitel = a.headingTitel,
            subheadingTitel = a.subheadingTitel
        }).ToList();

        int TotalScore = RROassessmentanswers.Sum(a => a.TotalMarks);

        foreach (var answer in RROassessmentanswers)
        {
            answer.TotalScore = TotalScore;
        }

        _context.RROassessmentanswers.AddRange(RROassessmentanswers);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Data saved successfully!";

        return RedirectToAction("RroDashboard", "Home");
    }

    public IActionResult BranchReview(int ServiceCenterId)
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        int? mainBranchId = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
   

        if (mainBranchId == null)
        {
            return RedirectToAction("Index"); 
        }

    

      var serviceCenters = _context.AssessmentAnswers
     .Where(sc => sc.ServiceCenterBranch == mainBranchId && sc.ServiceCenterId != mainBranchId)
     .GroupBy(sc => sc.ServiceCenterId)
     .Select(group => new
      {
      ServiceCenterId = group.Key,
      BranchName = _context.Branches
          .Where(b => b.BranchID == group.Key)
          .Select(b => b.Name)
          .FirstOrDefault()
      })
      .ToList();

        ViewBag.ServiceCenters = serviceCenters;


         var latestFromDate = _context.AssessmentAnswers
             .Where(a => a.Astype == "S")
           .OrderByDescending(a => a.fromdate)
           .Select(a => a.fromdate)
           .FirstOrDefault();

        var BRdays = _context.Rangs
        .Select(a => a.BRdays)
        .FirstOrDefault();


        int Brdays = BRdays;

        if ( DateTime.Now > latestFromDate.AddDays(Brdays))
        {
            ViewBag.AssessmentMessage = "Assessment has expired.";
        }

        var assessmentAnswers = _context.AssessmentAnswers
           .Where(a => a.fromdate == latestFromDate &&
                       DateTime.Now >= a.fromdate &&
                       DateTime.Now <= a.fromdate.AddDays(Brdays) &&
                       a.ServiceCenterBranch == mainBranchId &&
                       a.ServiceCenterflag == 1 &&
                       a.branchflag == 0 &&
                       a.ServiceCenterId == ServiceCenterId)
           .ToList();

        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = mainBranchId;  
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");

        return View(assessmentAnswers);
    }
  
   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveSeAnswers(List<AssessmentAnswerViewModel> answers)
    {
        if (answers == null || !answers.Any())
        {
            return BadRequest("No data received.");
        }
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");

        var UserName = HttpContext.Session.GetString("UserName");
        var BranchType = HttpContext.Session.GetString("BranchType");

        int? BranchID = HttpContext.Session.GetInt32("BranchID");

        var existingSubmission = _context.AssessmentAnswers
       .FirstOrDefault(a => a.ServiceCenterId == BranchID && a.ServiceCenterflag == 1);

     
        var assessmentAnswers = answers.Select(a => new AssessmentAnswer
        {
            QuestionId = a.QuestionId,
            AnswerText = a.QuestionText,
            Marks = a.Marks,
            MainHeading =a.MainHeading,
            SubHeading=a.SubHeading,
            ServiceCenterAnswer = a.ServiceCenterAnswer,
            BranchAnswer = a.ServiceCenterAnswer,
            ServiceCenterId = ViewBag.BranchID,
            ServiceCenterBranch = ViewBag.MAIN_BRANCH_ID,
            ServiceCenterRegion = ViewBag.RegionID,
            BranchId = ViewBag.MAIN_BRANCH_ID,
            //ServiceCenterflag = 1,
            ServiceCenterflag = ((BranchType == "SERV_CEN") ? 1 : 0),
            branchflag = ((BranchType == "BRANCH") ? 1 : 0),
            TotalMarks = (BranchType == "BRANCH") ? 0 : ((a.ServiceCenterAnswer == "Yes" || a.ServiceCenterAnswer == "NA") ? 1 : 0) * a.Marks,
            BrRTotalMarks = (BranchType == "SERV_CEN") ? 0 : ((a.ServiceCenterAnswer == "Yes" || a.ServiceCenterAnswer == "NA") ? 1 : 0) * a.Marks,
            ReRTotalScore = 0,
            RegionId = ViewBag.RegionID,
            RegionAnswer = a.ServiceCenterAnswer,
            ReRTotalMarks = 0,
            Scomment = ((BranchType == "SERV_CEN") ? a.Comment : "00") ?? "00",
            Bcomment = ((BranchType == "BRANCH") ? a.Comment : "00") ?? "00",
        
            Rcomment = "00",
            fromdate = a.fromdate,
            todate = a.todate,
            headingTitel = a.headingTitel,
            subheadingTitel = a.subheadingTitel,
            AnswerDate = DateTime.Now,
            AnswerUser  =  UserName,
            NAflag =a.NAflag,
            Astype =a.Astype

        }).ToList();

        double totalScore = assessmentAnswers.Sum(a => (BranchType == "BRANCH") ? a.BrRTotalMarks : a.TotalMarks);


        foreach (var answer in assessmentAnswers)
        {
            if (BranchType == "BRANCH")
            {
                answer.BrRTotalScore = totalScore;
            }
            else
            {
                answer.TotalScore = totalScore;
            }
        }
        _context.AssessmentAnswers.AddRange(assessmentAnswers);
        _context.SaveChanges();

        return RedirectToAction("SericeCenterAssesment");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveBranchAnswers(List<AssessmentAnswerViewModel> answers )
    {
        if (answers == null || !answers.Any())
        {
            return BadRequest("No data received.");
        }
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");

        var latestFromDate = _context.AssessmentAnswers
      .OrderByDescending(a => a.fromdate)
      .Select(a => a.fromdate)
      .FirstOrDefault();

        var BRdays = _context.Rangs
        .Select(a => a.BRdays)
        .FirstOrDefault();


        int Brdays = BRdays;
        //var existingSubmission = _context.AssessmentAnswers
        //      .FirstOrDefault(a =>  a.branchflag == 1);

        var updatedAnswers = new List<AssessmentAnswer>();

        foreach (var answer in answers)
        {
            var existingAnswer = _context.AssessmentAnswers
                     .FirstOrDefault(a => a.QuestionId == answer.QuestionId && a.ServiceCenterId == answer.ServiceCenterId && a.branchflag==0 && DateTime.Now <= a.fromdate.AddDays(Brdays));


            if (existingAnswer != null)
            {
               existingAnswer.BranchAnswer = answer.BranchAnswer;
                existingAnswer.BranchId = ViewBag.BranchID;
                existingAnswer.BrRTotalMarks = ((answer.BranchAnswer == "Yes" || answer.BranchAnswer == "NA") ? 1 : 0) * answer.Marks;
                existingAnswer.branchflag = 1;
                existingAnswer.Bcomment = string.IsNullOrEmpty(answer.BComment) ? "00" : answer.BComment;


                updatedAnswers.Add(existingAnswer); 
            }
        }

        double BrRTotalScore = updatedAnswers.Sum(a => a.BrRTotalMarks);

        foreach (var answer in updatedAnswers)
        {
            answer.BrRTotalScore = BrRTotalScore;
            _context.AssessmentAnswers.Update(answer);
        }

        _context.SaveChanges();

        return RedirectToAction("AdminDashBoard","Home");
    }
    public IActionResult RegionReview(int ServiceCenterId)
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
        int? RegionID = HttpContext.Session.GetInt32("RegionID");
        if (RegionID == null)
        {
            return RedirectToAction("Index"); 
        }
       
        var serviceCenters = _context.AssessmentAnswers
           .Where(sc => sc.ServiceCenterRegion == RegionID && sc.Regionflag==0 &&sc.ServiceCenterflag == 1)//&& sc.ServiceCenterflag == 1 && sc.branchflag == 1 )//&& sc.Regionflag == 0//
           .Select(sc => sc.ServiceCenterId)
         .Distinct()
           .ToList();
        var subbranchs = _context.AssessmentAnswers
          .Where(sc => sc.ServiceCenterRegion == RegionID && sc.branchflag == 1 && sc.ServiceCenterflag == 0 && sc.Regionflag == 0)//&& sc.Regionflag == 0// 
          .Select(sc => sc.ServiceCenterId)
           .Distinct()
            .ToList();
        ViewBag.ServiceCenters = serviceCenters;
        ViewBag.Subbranchs = subbranchs;

      

        var latestFromDate = _context.AssessmentAnswers
         .OrderByDescending(a => a.fromdate)
         .Select(a => a.fromdate)
         .FirstOrDefault();

        var RRdays = _context.Rangs
        .Select(a => a.RRdays)
        .FirstOrDefault();


        int Rrdays = RRdays;
        if (latestFromDate == default || DateTime.Now > latestFromDate.AddDays(Rrdays))
        {
            ViewBag.AssessmentMessage = "Assessment has expired.";
        }

        var assessmentAnswers = _context.AssessmentAnswers
           .Where(a => a.fromdate == latestFromDate &&
                       DateTime.Now >= a.fromdate &&
                       DateTime.Now <= a.fromdate.AddDays(Rrdays) &&
                       a.ServiceCenterRegion == RegionID && 
                       a.branchflag == 1 && 
                       a.ServiceCenterId == ServiceCenterId 
                       && a.Regionflag == 0)
           .ToList();
        string branchNameForServiceCenter = _context.Branches
    .Where(b => b.BranchID == ServiceCenterId)
    .Select(b => b.Name)
    .FirstOrDefault();

        ViewBag.BranchNameForServiceCenter = branchNameForServiceCenter;
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");
        return View(assessmentAnswers);
    }
   

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveRegionAnswers(List<AssessmentAnswerViewModel> answers)
    {
        if (answers == null || !answers.Any())
        {
            return BadRequest("No data received.");
        }
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");


        var latestFromDate = _context.AssessmentAnswers
         .OrderByDescending(a => a.fromdate)
         .Select(a => a.fromdate)
         .FirstOrDefault();

        var RRdays = _context.Rangs
        .Select(a => a.RRdays)
        .FirstOrDefault();
        int Rrdays = RRdays;

        var updatedAnswers = new List<AssessmentAnswer>();

        foreach (var answer in answers)
        {
            var existingAnswer = _context.AssessmentAnswers
                .FirstOrDefault(a => a.QuestionId == answer.QuestionId && a.ServiceCenterId == answer.ServiceCenterId && a.Regionflag== 0 &&   DateTime.Now <= a.fromdate.AddDays(Rrdays));

            if (existingAnswer != null)
            {
               
                existingAnswer.RegionAnswer = answer.RegionAnswer;
                existingAnswer.RegionId = ViewBag.RegionId;
                existingAnswer.ReRTotalMarks = ((answer.RegionAnswer == "Yes" || answer.RegionAnswer == "NA") ? 1 : 0) * answer.Marks;
                existingAnswer.Regionflag = 1;
             
                existingAnswer.Rcomment = string.IsNullOrEmpty(answer.Rcomment) ? "00" : answer.Rcomment;

                existingAnswer.Rcomment = answer.Rcomment;
                updatedAnswers.Add(existingAnswer);
            }
        }

        double ReRTotalScore = updatedAnswers.Sum(a => a.ReRTotalMarks);

        foreach (var answer in updatedAnswers)
        {
            answer.ReRTotalScore = ReRTotalScore;
            _context.AssessmentAnswers.Update(answer);
        }

        _context.SaveChanges();

        return RedirectToAction("RegionReview");
    }
  
    public IActionResult AdminReview(int ServiceCenterId, DateTime fromdate)
    {

        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        int? RegionID = HttpContext.Session.GetInt32("RegionID");
        if (RegionID == null)
        {
            return RedirectToAction("Index");
        }

        var serviceCenters = _context.AssessmentAnswers
           .Where(sc =>  sc.ServiceCenterflag == 1 && sc.branchflag == 1 && sc.Regionflag == 1)
           .GroupBy(sc => sc.ServiceCenterId)
      .Select(group => new
      {
          ServiceCenterId = group.Key,
          BranchName = _context.Branches
              .Where(b => b.BranchID == group.Key)
              .Select(b => b.Name)
              .FirstOrDefault()
      })
           .ToList();
        var subbranchs = _context.AssessmentAnswers
          .Where(sc =>  sc.branchflag == 1 && sc.ServiceCenterflag == 0 && sc.Regionflag == 1)
          .GroupBy(sc => sc.ServiceCenterId)
      .Select(group => new
      {
          ServiceCenterId = group.Key,
          BranchName = _context.Branches
              .Where(b => b.BranchID == group.Key)
              .Select(b => b.Name)
              .FirstOrDefault()
      })
            .ToList();
        ViewBag.ServiceCenters = serviceCenters;
        ViewBag.Subbranchs = subbranchs;
        var assessmentAnswers = _context.AssessmentAnswers
            .Where(a =>  a.branchflag == 1 && a.ServiceCenterId == ServiceCenterId && a.Regionflag == 1 && a.fromdate == fromdate)
            .ToList();


        var daterange = _context.AssessmentAnswers
        .Where(sc => sc.Regionflag== 1)
        .Select(sc => sc.fromdate)
        .Distinct()
         .ToList();
        ViewBag.fromdate = daterange;

        string branchNameForServiceCenter = _context.Branches
         .Where(b => b.BranchID == ServiceCenterId)
          .Select(b => b.Name)
        .FirstOrDefault();

        ViewBag.BranchNameForServiceCenter = branchNameForServiceCenter;

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = RegionID;
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");
        return View(assessmentAnswers);
    }
    public IActionResult AssessmentEdit()
    {

        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        int? RegionID = HttpContext.Session.GetInt32("RegionID");
        if (RegionID == null)
        {
            return RedirectToAction("Index");
        }
        // Fetch all records from the Rangs table
        var assessments = _context.Rangs.ToList();


        var fromDates = _context.AssessmentsPe
            .Select(a => new { a.fromdate, a.todate, a.Astype })
            .Distinct()
            .OrderBy(d => d.fromdate)
            .ToList();

        // Convert to a list of objects (avoid anonymous type issue)
        ViewBag.FromDates = fromDates.Select(a => new Dictionary<string, object>
            {
            { "fromdate", a.fromdate },
            { "todate", a.todate },
            { "Astype", a.Astype }
            }).ToList();



        if (!assessments.Any())
        {
            ViewBag.Message = "No records found in the Rangs table.";
            return View(new List<Rangs>()); // Return empty list to prevent errors
        }

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = RegionID;
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");
        return View(assessments);
    }

    [HttpPost]
    public JsonResult SaveEdit(int Id, int scdays, int BRdays, int RRdays)
    {
        var assessment = _context.Rangs.FirstOrDefault(a => a.Id == Id);
        if (assessment != null)
        {
            assessment.scdays = scdays;
            assessment.BRdays = BRdays;
            assessment.RRdays = RRdays;
            _context.SaveChanges();
            return Json(new { success = true });
        }

        return Json(new { success = false });
    }
    public IActionResult HistoryReport(string fromDate, string toDate, string asType)
    {

        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        int? RegionID = HttpContext.Session.GetInt32("RegionID");
        if (RegionID == null)
        {
            return RedirectToAction("Index");
        }
        if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate) || string.IsNullOrEmpty(asType))
        {
            return RedirectToAction("AssessmentEdit"); // Handle empty parameters
        }

        DateTime start = DateTime.Parse(fromDate);
        DateTime end = DateTime.Parse(toDate);

   
        var assessments = _context.AssessmentsPe
            .Where(a => a.fromdate >= start && a.todate <= end && a.Astype == asType)
            .ToList();

        if (!assessments.Any())
        {
            ViewBag.Message = "No assessments found for the selected period and type.";
        }

        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        ViewBag.AsType = asType;
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = RegionID;
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");
        return View(assessments);
    }
    [HttpPost]
    public IActionResult DeleteAssessments(DateTime FromDate, DateTime ToDate, string AsType)
    {
        try
        {
            // Your logic to delete the assessment records from the database
            var recordsToDelete = _context.AssessmentsPe
                                          .Where(a => a.fromdate == FromDate &&
                                                      a.todate == ToDate &&
                                                      a.Astype == AsType)
                                          .ToList();

            var recordsToDeleteAnswer = _context.AssessmentAnswers
                                    .Where(a => a.fromdate == FromDate &&
                                                a.todate == ToDate &&
                                                a.Astype == AsType)
                                    .ToList();

            if (recordsToDelete.Any())
            {
                _context.AssessmentsPe.RemoveRange(recordsToDelete);
                _context.SaveChanges();


                ViewBag.AssessmentMessage = "Assessment records successfully deleted.";
            }

            if (recordsToDeleteAnswer.Any())
            {
                _context.AssessmentAnswers.RemoveRange(recordsToDeleteAnswer);
                _context.SaveChanges();


                ViewBag.AssessmentMessage = "Assessment records successfully deleted.";
            }
            else
            {
                ViewBag.AssessmentMessage = "No matching records found to delete.";
            }
        }
        catch (Exception ex)
        {
            ViewBag.AssessmentMessage = "Error occurred while deleting the assessment records: " + ex.Message;
        }

        // Return to the same page with a success or error message
        return RedirectToAction("AssessmentEdit"); // Assuming your assessment list page is called "Index"
    }





    public IActionResult ViewRroAssessment(int servicecenterId, DateTime fromdate)
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        int? RegionID = HttpContext.Session.GetInt32("RegionID");
        if (RegionID == null)
        {
            return RedirectToAction("Index");
        }

        int? BranchID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        string? BranchType = HttpContext.Session.GetString("BranchType");

        List<RROassessmentanswers> assessments = new List<RROassessmentanswers>();

        if (BranchType == "BRANCH" || BranchType == "SERV_CEN")
        {
            assessments = _context.RROassessmentanswers
                .Where(raa => raa.ServiceCenterId == servicecenterId &&
                              raa.fromdate == fromdate &&
                              raa.RegionId == RegionID &&
                              raa.BranchId == BranchID)
                .ToList();
        }
        else if (BranchType == "REGION")
        {
            assessments = _context.RROassessmentanswers
                .Where(raa => raa.ServiceCenterId == servicecenterId &&
                              raa.fromdate == fromdate &&
                              raa.RegionId == RegionID)
                .ToList();
        }
        else if (BranchType == "DEPT")
        {
            assessments = _context.RROassessmentanswers
                .Where(raa => raa.ServiceCenterId == servicecenterId &&
                              raa.fromdate == fromdate)
                .ToList();
        }

      
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.RegionID = RegionID;
        ViewBag.Branch_type_code = BranchType;
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID"); 

        return View(assessments);
    }


    public IActionResult RegionOwnReportRRoview(int ServiceCenterId, DateTime fromdate)
    {

        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        var userId = HttpContext.Session.GetInt32("UserId");

        var UserName = HttpContext.Session.GetString("UserName");
        var RegionId = HttpContext.Session.GetInt32("RegionId");

        int? RegionID = HttpContext.Session.GetInt32("RegionId");
        if (RegionID == null)
        {
            return RedirectToAction("Index");
        }

        int? BranchID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");

        var assessmentAnswers = _context.AssessmentAnswers
            .Where(a => a.RegionId == RegionID && a.ServiceCenterId == ServiceCenterId && a.Regionflag == 1 && a.fromdate == fromdate)
            .ToList();
        var daterange = _context.AssessmentAnswers
      .Where(sc => sc.RegionId == RegionID && sc.branchflag == 1)
      .Select(sc => sc.fromdate)
      .Distinct()
      .ToList();
        ViewBag.fromdate = daterange;

        var serviceCenters = _context.AssessmentAnswers
     .Where(sc => sc.ServiceCenterRegion == RegionID && sc.Regionflag == 1)
      .GroupBy(sc => sc.ServiceCenterId)
      .Select(group => new
      {
          ServiceCenterId = group.Key,
          BranchName = _context.Branches
              .Where(b => b.BranchID == group.Key)
              .Select(b => b.Name)
              .FirstOrDefault()
      })
      .ToList();
        ViewBag.ServiceCenters = serviceCenters;

        string branchNameForServiceCenter = _context.Branches
   .Where(b => b.BranchID == ServiceCenterId)
   .Select(b => b.Name)
   .FirstOrDefault();

        ViewBag.BranchNameForServiceCenter = branchNameForServiceCenter;



        ViewBag.Id = HttpContext.Session.GetInt32("UserId");

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.ServiceNo = HttpContext.Session.GetInt32("ServiceNo");

        ViewBag.UserName = HttpContext.Session.GetString("RRoname");
        ViewBag.ServiceNo = HttpContext.Session.GetInt32("RegionId");
        return View(assessmentAnswers);
    }

    [HttpPost]
    public IActionResult DeleteHeading(int id)
    {
        var heading = _context.Headings
            .Include(h => h.Subheadings) // Include subheadings to delete them
            .FirstOrDefault(h => h.Id == id);

        if (heading == null)
        {
            return Json(new { success = false, message = "Heading not found." });
        }

        // Remove all related subheadings first to avoid foreign key constraint issues
        _context.SubHeadings.RemoveRange(heading.Subheadings);
        _context.Headings.Remove(heading);
        _context.SaveChanges();

        return Json(new { success = true, message = "Heading deleted successfully." });
    }
    [HttpPost]
    public IActionResult DeleteSubheading(int id)
    {
        var subheading = _context.SubHeadings.Find(id);
        if (subheading == null)
        {
            return Json(new { success = false, message = "Subheading not found." });
        }

        _context.SubHeadings.Remove(subheading);
        _context.SaveChanges();

        return Json(new { success = true, message = "Subheading deleted successfully." });
    }
    public async Task<IActionResult> EditHeading(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var heading = await _context.Headings.FindAsync(id);
        if (heading == null)
        {
            return NotFound();
        }

        return View(heading);
    }
}
