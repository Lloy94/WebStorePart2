using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebStore.DAL.Context;
using WebStore.Domain.Models;
using WebStore.Interfaces.Services;

namespace WebStore.Services.Services.InSQL
{
    public class SqlEmployeeData : IEmployeeData
    {
        private readonly WebStoreDB _db;

        public SqlEmployeeData(WebStoreDB db) 
        { 
            _db = db;
        }

        public int Add(Employee employee)
        {
            if (employee is null) throw new ArgumentNullException(nameof(employee));

            if (_db.Employees.Contains(employee)) return employee.Id;

            _db.Employees.Add(employee);
            _db.SaveChanges();

            return employee.Id;

        }

        public bool Delete(int id)
        {
            var db_employee = GetById(id);
            if (db_employee is null) return false;

            _db.Employees.Remove(db_employee);

            return true;
        }

        public IEnumerable<Employee> GetAll()
        {
           return _db.Employees;
        }

        public  Employee GetById(int id)
        {
           return _db.Employees.Find(id);
        }

        public void Update(Employee employee)
        {
            if (employee is null) throw new ArgumentNullException(nameof(employee));

            if (_db.Employees.Contains(employee)) return;

            var db_employee = GetById(employee.Id);

            if (db_employee is null) return;

            db_employee.LastName = employee.LastName;
            db_employee.Name = employee.Name;
            db_employee.Patronymic = employee.Patronymic;
            db_employee.Age = employee.Age;
            db_employee.Info = employee.Info;

            _db.SaveChanges();
        }
    }
}
