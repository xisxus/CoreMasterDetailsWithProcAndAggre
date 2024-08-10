using CoreMasterDetailsWithProcAndAggre.Models;
using CoreMasterDetailsWithProcAndAggre.Models.VM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CoreMasterDetailsWithProcAndAggre.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EmployeeController(AppDbContext db, IWebHostEnvironment hostEnvironment)
        {
            this.db = db;
            _hostEnvironment = hostEnvironment;
        }

        //public IActionResult Index()
        //{
        //    var data = db.Employees;
        //    var min = data.Min(e => e.Salary);
        //    var max = data.Max(e => e.Salary);
        //    var sum = data.Sum(e => e.Salary);
        //    var avg = data.Average(e => e.Salary);
        //    var count = data.Count();

        //    var groupbyresult = data.GroupBy(i => i.EmployeeId).Select(c => new GroupByViewModel
        //    {
        //        EmployeeId = c.Key,
        //        MinValue = c.Min(e => e.Salary),
        //        MaxValue = c.Max(e => e.Salary),
        //        SumValue = c.Sum(e => e.Salary),
        //        AvgValue = Convert.ToDecimal(c.Average(e => e.Salary)),
        //        Count = c.Count()
        //    }).ToList();

        //    var model = new AggregateViewModels
        //    {
        //        MinValue = min,
        //        MaxValue = max,
        //        SumValue = sum,
        //        AvgValue = Convert.ToDecimal(avg),
        //        GroupByResult = groupbyresult
        //    };

        //    return View(model);
        //}

        //public async Task<IActionResult> IndexEmp()
        //{
        //    var employees = await db.Employees
        //        .Include(e => e.Experiences)
        //        .Select(e => new EmployeeVM
        //        {
        //            EmployeeId = e.EmployeeId,
        //            Name = e.Name,
        //            IsActive = e.IsActive,
        //            JoinDate = e.JoinDate,
        //            Salary = e.Salary,
        //            ImageUrl = e.ImageUrl,
        //            Experiences = e.Experiences.Select(exp => new ExperienceViewModel
        //            {
        //                Title = exp.Title,
        //                Duration = exp.Duration
        //            }).ToList()
        //        }).ToListAsync();

        //    return View(employees);
        //}

        public async Task<IActionResult> Index()
        {
            var data = db.Employees;

            // Aggregate operations
            var min = data.Min(e => e.Salary);
            var max = data.Max(e => e.Salary);
            var sum = data.Sum(e => e.Salary);
            var avg = data.Average(e => e.Salary);
            var count = data.Count();

            var groupbyresult = data.GroupBy(i => i.EmployeeId).Select(c => new GroupByViewModel
            {
                EmployeeId = c.Key,
                MinValue = c.Min(e => e.Salary),
                MaxValue = c.Max(e => e.Salary),
                SumValue = c.Sum(e => e.Salary),
                AvgValue = Convert.ToDecimal(c.Average(e => e.Salary)),
                Count = c.Count()
            }).ToList();

            // Fetch employees with experiences
            var employees = await data
                .Include(e => e.Experiences)
                .Select(e => new EmployeeVM
                {
                    EmployeeId = e.EmployeeId,
                    Name = e.Name,
                    IsActive = e.IsActive,
                    JoinDate = e.JoinDate,
                    Salary = e.Salary,
                    ImageUrl = e.ImageUrl,
                    Experiences = e.Experiences.Select(exp => new ExperienceViewModel
                    {
                        Title = exp.Title,
                        Duration = exp.Duration
                    }).ToList()
                }).ToListAsync();

            // Combine both into a single view model
            var model = new AggregateEmployeeViewModel
            {
                MinValue = min,
                MaxValue = max,
                SumValue = sum,
                AvgValue = Convert.ToDecimal(avg),
                GroupByResult = groupbyresult,
                Employees = employees
            };

            return View(model);
        }



        [HttpGet]
        public IActionResult Create()
        {
            var model = new EmployeeVM();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeVM model)
        {
            //if (ModelState.IsValid)
            //{
                string imageName = null;
                string imageUrl = null;

                if (model.ImageFile != null)
                {
                    // Save the image file to wwwroot/images
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                    imageName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, imageName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    imageUrl = "/images/" + imageName;
                }

                var expTable = new DataTable();
                expTable.Columns.Add("Title", typeof(string));
                expTable.Columns.Add("Duration", typeof(int));

                foreach (var exp in model.Experiences)
                {
                    expTable.Rows.Add(exp.Title, exp.Duration);
                }

                var parameters = new[]
                {
                new SqlParameter("@Name", model.Name),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@JoinDate", model.JoinDate),
                new SqlParameter("@ImageName", imageName ?? (object)DBNull.Value),
                new SqlParameter("@ImageUrl", imageUrl ?? (object)DBNull.Value),
                new SqlParameter("@Salary", model.Salary),
                new SqlParameter("@Exp", expTable) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.ParamExpType" }
                };

                await db.Database.ExecuteSqlRawAsync("EXEC InsertEmployeeSP @Name, @IsActive, @JoinDate, @ImageName, @ImageUrl, @Salary, @Exp", parameters);

                return RedirectToAction(nameof(Index)); 
            //}

            //return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await db.Employees.Include(e => e.Experiences)
                                                   .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            var model = new EmployeeVM
            {
                EmployeeId = employee.EmployeeId,
                Name = employee.Name,
                IsActive = employee.IsActive,
                JoinDate = employee.JoinDate,
                Salary = employee.Salary,
                ImageName = employee.ImageName,
                ImageUrl = employee.ImageUrl,
                Experiences = employee.Experiences.Select(e => new ExperienceViewModel
                {
                    Title = e.Title,
                    Duration = e.Duration
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeVM model)
        {
            //if (ModelState.IsValid)
            //{

                string imageName = model.ImageName;
                string imageUrl = model.ImageUrl;
                var Emp = db.Employees.FirstOrDefault(e => e.EmployeeId == model.EmployeeId);

                if (model.ImageFile != null)
                {
                    
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                    imageName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, imageName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                   
                    if (!string.IsNullOrEmpty(model.ImageName))
                    {
                        string oldImagePath = Path.Combine(uploadsFolder, model.ImageName);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                     imageUrl = "/images/" + imageName;
                }
                if (model.ImageFile == null)
                {
                    imageName = Emp.ImageName;
                    imageUrl = Emp.ImageUrl;
                }


                var expTable = new DataTable();
                expTable.Columns.Add("Title", typeof(string));
                expTable.Columns.Add("Duration", typeof(int));

                foreach (var exp in model.Experiences)
                {
                    expTable.Rows.Add(exp.Title, exp.Duration);
                }

                var parameters = new[]
                {
                new SqlParameter("@EmployeeId", model.EmployeeId),
                new SqlParameter("@Name", model.Name),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@JoinDate", model.JoinDate),
                new SqlParameter("@ImageName", imageName ?? (object)DBNull.Value),
                new SqlParameter("@ImageUrl", imageUrl ?? (object)DBNull.Value),
                new SqlParameter("@Salary", model.Salary),
                new SqlParameter("@Exp", expTable) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.ParamExpType" }
            };

                await db.Database.ExecuteSqlRawAsync("EXEC UpdateEmployeeSP @EmployeeId, @Name, @IsActive, @JoinDate, @ImageName, @ImageUrl, @Salary, @Exp", parameters);

                return RedirectToAction(nameof(Index)); 
            //}

            //return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var parameters = new[] { new SqlParameter("@EmployeeId", id) };
            await db.Database.ExecuteSqlRawAsync("EXEC DeleteEmployeeSP @EmployeeId", parameters);

            return RedirectToAction(nameof(Index));
        }


    }
}
