using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebStore.Domain.Entities.Identity;
using WebStore.Domain.Models;
using WebStore.Domain.ViewModels;
using WebStore.Interfaces.Services;

namespace WebStore.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeData _EmployeeData;
        private readonly ILogger<EmployeesController> _Logger;

        public EmployeesController(IEmployeeData EmployeeData, ILogger<EmployeesController> Logger)
        {
            _EmployeeData = EmployeeData;
            _Logger = Logger;
        }
        public IActionResult Index()
        {
            return View(_EmployeeData.GetAll());
        }

        public IActionResult Details(int id)
        {
            var employee = _EmployeeData.GetById(id);
            if (employee is null)
                return NotFound();

            return View(employee);
        }
        [Authorize(Roles = Role.Administrators)]
        public IActionResult Create() => View("Edit", new EmployeeViewModel());

        [Authorize(Roles = Role.Administrators)]
        public IActionResult Edit(int? id)
        {
            if (id is null) return View(new EmployeeViewModel());

            var employee = _EmployeeData.GetById((int)id);
            var model = new EmployeeViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                LastName = employee.LastName,
                Patronymic = employee.Patronymic,
                Age = employee.Age,
                Info=employee.Info,
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = Role.Administrators)]
        public IActionResult Edit(Employee model)
        {
            if (!ModelState.IsValid) return View(model);

            var employee = new Employee
            {
                Id = model.Id,
                Name = model.Name,
                LastName = model.LastName,
                Patronymic = model.Patronymic,
                Age = model.Age,
                Info = model.Info,
            };

            if (employee.Id == 0) _EmployeeData.Add(employee);
            else _EmployeeData.Update(employee);

            return RedirectToAction(nameof(Index));

        }

        [Authorize(Roles = Role.Administrators)]
        public IActionResult Delete(int id)
        {
            if (id < 0) return BadRequest();

            var employee = _EmployeeData.GetById(id);
            if (employee is null) return NotFound();

            return View(new EmployeeViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                LastName = employee.LastName,
                Patronymic = employee.Patronymic,
                Age = employee.Age,
                Info = employee.Info,
            });
        }

        [HttpPost]
        [Authorize(Roles = Role.Administrators)]
        public IActionResult DeleteConfirmed(int id)
        {
            _EmployeeData.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
