using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using rcsa.Data;
using rcsa.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;
using Org.BouncyCastle.Utilities;

public class HomeController : Controller
{
    private readonly DatabaseContext _context;

    public HomeController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {

        return View();
    }

    
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        // Hash the password
        string hashedPassword = BitConverter.ToString(
            System.Security.Cryptography.SHA256.Create()
            .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password))
        ).Replace("-", "").ToLower();

        // Validate user
        var user = _context.Users.FirstOrDefault(u => u.Name == username && u.Password == hashedPassword);

        if (user != null)
        {
            // Get branch details
            var branch = _context.Branches.FirstOrDefault(b => b.BranchID == user.BranchID);

            if (branch != null)
            {
                // Create session
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetInt32("BranchID", user.BranchID);
                HttpContext.Session.SetString("BranchType", branch.Branch_type_code);
                HttpContext.Session.SetString("BranchName", branch.Name);
                HttpContext.Session.SetInt32("MAIN_BRANCH_ID", branch.MAIN_BRANCH_ID);
                 HttpContext.Session.SetInt32("RegionID", branch.RegionID);



                // Log login details
                var log = new Log
                {
                    UserName = user.Name,
                    BranchID = user.BranchID,
                    LoginTime = DateTime.Now
                };
                _context.Logs.Add(log);
                _context.SaveChanges();

			
                {
                    return RedirectToAction("AdminDashboard");
                }
            }
        }

        ViewBag.Message = "Invalid username or password.";
        return View("Index");
    }

    [HttpPost]
    public IActionResult RROLogin(string UserName, string Password)
    {
        var user = _context.RROUsers.FirstOrDefault(u => u.UserName == UserName);
        if (user == null || user.PasswordHash != HashPassword(Password))
        {
            
            ViewBag.Message = "Invalid username or password.";
            return View("Index");
        }

        // Store user session
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetInt32("ServiceNo", user.ServiceNo);
        HttpContext.Session.SetString("UserName", user.UserName);
        HttpContext.Session.SetString("RRoname", user.RRoname);
        HttpContext.Session.SetInt32("RegionId", user.RegionId);


        var slog = new RLog
        {
            UserName = user.UserName,
            ServiceNo = user.ServiceNo,
            LoginTime = DateTime.Now
        };
        _context.RLogs.Add(slog);
        _context.SaveChanges();
        return RedirectToAction("RroDashboard", "Home");
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    public IActionResult GetSubHeadingsByHeading(int headingId)
    {
        var subHeadings = _context.SubHeadings
                                  .Where(s => s.HeadingId == headingId)
                                  .Select(s => new { s.Id, s.SubHeadingT }) // Only return necessary fields
                                  .ToList();

        return Json(subHeadings);
    }
   
   
    public IActionResult AdminDashboard()
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }



        int? mainBranchId = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");

        int? RegionID = HttpContext.Session.GetInt32("RegionID");
        int? MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");


        var serviceCentersb = _context.Branches
            .Where(sc => sc.MAIN_BRANCH_ID == MAIN_BRANCH_ID/* && sc.BranchID != MAIN_BRANCH_ID*/)
            .Select(sc => new
            {
                BranchID = sc.BranchID,
                BranchName = sc.Name
            })
            .ToList();

        ViewBag.ServiceCentersb = serviceCentersb;

        var serviceCentersR = _context.Branches
         .Where(sc => sc.RegionID == RegionID  && sc.BranchID != RegionID/* && sc.BranchID != MAIN_BRANCH_ID*/)
         .Select(sc => new
         {
             BranchID = sc.BranchID,
             BranchName = sc.Name
         })
         .ToList();

        ViewBag.ServiceCentersr = serviceCentersR;
        var serviceCentersA = _context.Branches
        .Where(sc => sc.Branch_type_code != "DEPT")
        .Select(sc => new
        {
         BranchID = sc.BranchID,
         BranchName = sc.Name
         })
        .ToList();

        ViewBag.ServiceCentersa = serviceCentersA;



        var fromdatesubmited = _context.RROassessmentanswers
        .Where(a => a.fromdate != null) // Ensure dates are not null
        .Select(a => a.fromdate) // Convert DateTime to "yyyy-MM-dd"
        .Distinct()
        .OrderBy(date => date)
        .ToList();

        // Pass the list directly
        ViewBag.fromdatesubmited = fromdatesubmited;




        var latestFromDate = _context.AssessmentAnswers

   .OrderByDescending(a => a.fromdate)
   .Select(a => a.fromdate)
   .FirstOrDefault();

        var RRdays = _context.Rangs
        .Select(a => a.RRdays)
        .FirstOrDefault();


        int rRdays = RRdays;

        var regionlistserice = _context.AssessmentAnswers
      .Where(sc => sc.ServiceCenterRegion == RegionID && sc.ServiceCenterId != RegionID && sc.ServiceCenterflag ==1 && sc.branchflag==1 && sc.Regionflag == 0 && DateTime.Now >= sc.fromdate && DateTime.Now <= sc.todate)
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

        ViewBag.Regionlistserice = regionlistserice;


        var regionlistsBranch = _context.AssessmentAnswers
      .Where(sc => sc.ServiceCenterRegion == RegionID && sc.ServiceCenterId != RegionID && sc.ServiceCenterflag == 0 && sc.branchflag == 1 && sc.Regionflag == 0 && DateTime.Now >= sc.fromdate && DateTime.Now <= sc.todate)
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

        ViewBag.RegionlistsBranch = regionlistsBranch;
        //Pending branch asseemt review

    
        var BRdays = _context.Rangs
        .Select(a => a.BRdays)
        .FirstOrDefault();


        int Brdays = BRdays;
        var serviceCenters = _context.AssessmentAnswers
       .Where(sc => sc.ServiceCenterBranch == mainBranchId && sc.ServiceCenterId != mainBranchId  && sc.branchflag==0 && DateTime.Now >= sc.fromdate && DateTime.Now <= sc.todate)
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

        var rreviewserviceCenters = _context.AssessmentAnswers
       .Where(sc => sc.ServiceCenterBranch == mainBranchId && sc.ServiceCenterId != mainBranchId && sc.branchflag == 1 && DateTime.Now >= sc.fromdate && DateTime.Now <= sc.todate)
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

        ViewBag.RreviewserviceCenters = rreviewserviceCenters;

        //Branch under service centers 
        var branchUnderCenters = _context.Branches
            .Where(b => b.MAIN_BRANCH_ID == mainBranchId && b.BranchID != mainBranchId )
            .Select(b => new
            {
                BranchName = b.Name,
                Status = _context.AssessmentAnswers.Any(a => a.ServiceCenterId == b.BranchID && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate) ? "Submitted" : "Pending"
            })
            .ToList();

        ViewBag.BranchNames = branchUnderCenters;
        //Branch and  service centers  under Region
        var regionUnderCenters = _context.Branches
            .Where(b => b.RegionID == RegionID && b.BranchID != RegionID && b.Branch_type_code == "SERV_CEN" )
            .Select(b => new
            {
                BranchName = b.Name,
                Status = _context.AssessmentAnswers.Any(a => a.ServiceCenterId == b.BranchID && a.ServiceCenterflag==1 && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate) ? "Submitted" : "Pending"
            })
            .ToList();

        ViewBag.RegionUnderCenters = regionUnderCenters;

        var regionUnderBranch = _context.Branches
          .Where(b => b.RegionID == RegionID && b.BranchID != RegionID && b.Branch_type_code == "BRANCH")
          .Select(b => new
          {
              BranchName = b.Name,
              Status = _context.AssessmentAnswers.Any(a => a.ServiceCenterId == b.BranchID && a.ServiceCenterflag == 0 && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate) ? "Submitted" : "Pending"
          })
          .ToList();

        ViewBag.RegionUnderBranch = regionUnderBranch;


        var regionUnderCenters1 = _context.Branches
    .Where(b => b.RegionID == RegionID && b.BranchID != RegionID && b.Branch_type_code == "SERV_CEN")
    .Select(b => new
    {
        BranchName = b.Name,
        Status = _context.AssessmentAnswers.Any(a => a.ServiceCenterId == b.BranchID && a.ServiceCenterflag == 1 && a.branchflag == 1 && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate) ? "Submitted" : "Pending"
    })
    .ToList();

        ViewBag.RegionUnderCenters1 = regionUnderCenters1;
        //Branch and  service centers  under Department
        var allregionUnderCenters = _context.Branches
          .Where(b =>   b.Branch_type_code == "SERV_CEN")
          .Select(b => new
          {
              BranchName = b.Name,
              Status = _context.AssessmentAnswers.Any(a => a.ServiceCenterId == b.BranchID && a.ServiceCenterflag == 1 && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate) ? "Submitted" : "Pending"
          })
          .ToList();

        ViewBag.AllRegionUnderCenters = allregionUnderCenters;


        // service centers  under Department barnach reviewd 
        var allregionUnderCentersBr = _context.Branches
          .Where(b => b.Branch_type_code == "SERV_CEN")
          .Select(b => new
          {
              BranchName = b.Name,
              Status = _context.AssessmentAnswers.Any(a => a.ServiceCenterId == b.BranchID && a.ServiceCenterflag == 1 && a.branchflag == 1   && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate) ? "Submitted" : "Pending"
          })
          .ToList();

        ViewBag.AllRegionUnderCentersBr = allregionUnderCentersBr;
        // service centers  under Department barnach reviewd 
        var allregionUnderCentersRr = _context.Branches
          .Where(b => b.Branch_type_code == "SERV_CEN")
          .Select(b => new
          {
              BranchName = b.Name,
              Status = _context.AssessmentAnswers.Any(a => a.ServiceCenterId == b.BranchID && a.ServiceCenterflag == 1 && a.branchflag == 1 && a.Regionflag == 1 && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate) ? "Submitted" : "Pending"
          })
          .ToList();

        ViewBag.AllRegionUnderCentersRr = allregionUnderCentersRr;

        var allregionUnderBranch = _context.Branches
          .Where(b =>  b.Branch_type_code == "BRANCH")
          .Select(b => new
          {
              BranchName = b.Name,
              Status = _context.AssessmentAnswers.Any(a => a.ServiceCenterId == b.BranchID && a.ServiceCenterflag == 0 && a.branchflag == 1 && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate) ? "Submitted" : "Pending"
          })
          .ToList();

        ViewBag.AllRegionUnderBranch = allregionUnderBranch;

        var allregionUnderBranchRr = _context.Branches
          .Where(b => b.Branch_type_code == "BRANCH")
          .Select(b => new
          {
              BranchName = b.Name,
              Status = _context.AssessmentAnswers.Any(a => a.ServiceCenterId == b.BranchID && a.ServiceCenterflag == 0 && a.branchflag == 1 && a.Regionflag == 1 && DateTime.Now >= a.fromdate && DateTime.Now <= a.todate) ? "Submitted" : "Pending"
          })
          .ToList();

        ViewBag.AllRegionUnderBranchRr = allregionUnderBranchRr;

        int? branchId = HttpContext.Session.GetInt32("BranchID");

        var daterange = _context.AssessmentAnswers
            .Where(sc => sc.ServiceCenterId == branchId)
            .Select(sc => sc.fromdate)
            .Distinct()
            .ToList();
        ViewBag.fromdate = daterange;


        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");
        
        return View();
    }

    public IActionResult RroDashboard()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToAction("Index");
        }
        var userId = HttpContext.Session.GetInt32("UserId");
        var ServiceNo = HttpContext.Session.GetInt32("ServiceNo");
        var RegionId = HttpContext.Session.GetInt32("RegionId");
        var ServiceCenterId = HttpContext.Session.GetInt32("ServiceCenterId");

        var serviceCenters = _context.Branches
            .Where(sc => sc.RegionID == RegionId && sc.Branch_type_code !="REGION")
            .Select(sc => new { ServiceCenterId = sc.BranchID, sc.Name }) // Select required fields
            .ToList();

        ViewBag.ServiceCenters = new SelectList(serviceCenters, "ServiceCenterId", "Name");


        var fromdates = _context.AssessmentsPe
            .Where(sc => sc.Astype == "R" )
                           .Select(a => a.fromdate)
                           .Distinct() // Ensure unique dates
                           .OrderBy(date => date) // Optional: Sort the dates
                           .ToList();

        ViewBag.FromDates = fromdates;


        var fromdatesubmited = _context.RROassessmentanswers
    
      .Select(a => a.fromdate)
      .Distinct()
      .OrderBy(date => date)
      .ToList();

        ViewBag.fromdatesubmited = new SelectList(fromdatesubmited);

        var fromdatesubmitedNorml = _context.AssessmentAnswers

     .Select(a => a.fromdate)
     .Distinct()
     .OrderBy(date => date)
     .ToList();

        ViewBag.fromdatesubmitedNorml = new SelectList(fromdatesubmitedNorml);


        // Create a dictionary to store hidden ServiceCenterIds for each date
        var hiddenServiceCenterIdsByDate = new Dictionary<DateTime, List<int>>();

        foreach (var date in fromdates)
        {
            // Fetch ServiceCenterIds from RROassessmentanswers table for the specific date
            var hiddenServiceCenterIds = _context.RROassessmentanswers
                .Where(a => a.fromdate == date) // Filter for the specific date
                .Select(a => a.ServiceCenterId) // Select only the ServiceCenterId column
                .ToList(); // Convert to a list of integers

            // Add to the dictionary
            hiddenServiceCenterIdsByDate[date] = hiddenServiceCenterIds;
        }

        // Pass the dictionary to the view
        ViewBag.HiddenServiceCenterIdsByDate = hiddenServiceCenterIdsByDate;

        ViewBag.Id= HttpContext.Session.GetInt32("UserId");

        ViewBag.RRoname = HttpContext.Session.GetString("RRoname");
        ViewBag.RegionId = HttpContext.Session.GetInt32("RegionId");

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.ServiceNo = HttpContext.Session.GetInt32("ServiceNo");
        ViewBag.ServiceCenterId = HttpContext.Session.GetInt32("ServiceCenterId");
        return View();
    }

    public IActionResult HeadingManagement()
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

        var headings = _context.Headings
     .Include(h => h.Subheadings) // This includes related subheadings
     .ToList();
        ViewBag.Headings = headings;
      

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");


        return View(headings);
    }
  
    [HttpPost] 
    [ValidateAntiForgeryToken] 
    public IActionResult HeadingManagement(string MainHeading)
    { 
        // Check if the user is logged in
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index"); // Redirect to login if the user is not logged in
        }
          ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");

        // Validate the model (ensure required fields are filled and valid)
        if (ModelState.IsValid)
        {
            try
            {
                // Create a new Heading object with the form data
                var newHeading = new Heading
                {
                    MainHeadings = MainHeading
                };

                // Add the new heading to the database
                _context.Headings.Add(newHeading);
                _context.SaveChanges(); // Save changes to the database

                // Redirect to the same page to refresh and show the updated list
                return RedirectToAction(nameof(HeadingManagement));
            }
            catch (Exception ex)
            {
                // Log the exception (you can replace this with a proper logging mechanism)
                Console.WriteLine("Error saving heading: " + ex.Message);

                // Add a generic error message to the ModelState
                ModelState.AddModelError("", "An error occurred while saving the heading. Please try again.");
            }
        }
        else
        {
            // Log validation errors (optional)
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            foreach (var error in errors)
            {
                Console.WriteLine("Validation Error: " + error);
            }
        }

        // If we got this far, something failed (validation or database error)
        // Repopulate ViewBag and return the view with the existing data
      
        var headings = _context.Headings
    .Include(h => h.Subheadings) // This includes related subheadings
    .ToList();
     

        return View(headings);

    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubHeadingManagement(string SubHeadingT, int HeadingId)
    {
        if (HttpContext.Session.GetString("UserName") == null)
        {
            return RedirectToAction("Index");
        }

     
            try
            {
                var newSubHeading = new SubHeading
                {
					 SubHeadingT = SubHeadingT,
					HeadingId = HeadingId
                };

                _context.SubHeadings.Add(newSubHeading);
                _context.SaveChanges();

                return RedirectToAction(nameof(HeadingManagement));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving subheading: " + ex.Message);
                ModelState.AddModelError("", "An error occurred while saving the subheading. Please try again.");
            }
        

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.BranchID = HttpContext.Session.GetInt32("BranchID");
        ViewBag.BranchName = HttpContext.Session.GetString("BranchName");
        ViewBag.MAIN_BRANCH_ID = HttpContext.Session.GetInt32("MAIN_BRANCH_ID");
        ViewBag.RegionID = HttpContext.Session.GetInt32("RegionID");
        ViewBag.Branch_type_code = HttpContext.Session.GetString("BranchType");


        var headings = _context.Headings.ToList();
        return View("HeadingManagement", headings);
    }
   
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult UpdateHeadingOrder(int id, string direction)
    {
        var heading = _context.Headings.FirstOrDefault(h => h.Id == id);
        if (heading == null)
        {
            return Json(new { success = false, message = "Heading not found." });
        }

        // Get all headings ordered by their current order
        var headings = _context.Headings.OrderBy(h => h.Orders).ToList();

        // Find the index of the current heading
        int currentIndex = headings.FindIndex(h => h.Id == id);

        if (direction == "up" && currentIndex > 0)
        {
            // Swap with the previous heading
            var previousHeading = headings[currentIndex - 1];
            int tempOrder = heading.Orders;
            heading.Orders = previousHeading.Orders;
            previousHeading.Orders = tempOrder;

            _context.SaveChanges();
        }
        else if (direction == "down" && currentIndex < headings.Count - 1)
        {
            // Swap with the next heading
            var nextHeading = headings[currentIndex + 1];
            int tempOrder = heading.Orders;
            heading.Orders = nextHeading.Orders;
            nextHeading.Orders = tempOrder;

            _context.SaveChanges();
        }
        else
        {
            return Json(new { success = false, message = "Cannot move further." });
        }

        return Json(new { success = true });
    }



}
