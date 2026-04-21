using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using TicketSellingModule.Domain;

namespace TicketSellingModule.Repo
{
    public class EmployeeRepo
    {
        private readonly DbConnectionFactory _connectionFactory;
        public EmployeeRepo(DbConnectionFactory factory)
        {
            _connectionFactory = factory;
        }


        public List<Employee> GetAllEmployees()
        {
            List<Employee> allEmployees = new List<Employee>();
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "SELECT id, name, role, birthday, salary, hiring_date FROM Employees";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Employee emp = new Employee();
                            emp.Id = reader.GetInt32(0);
                            emp.Name = reader.GetString(1);
                            emp.Role = reader.GetString(2);
                            emp.Birthday = DateOnly.FromDateTime(reader.GetDateTime(3));
                            emp.Salary = reader.GetInt32(4);
                            emp.HiringDate = DateOnly.FromDateTime(reader.GetDateTime(5));

                            allEmployees.Add(emp);
                        }
                    }
                }
            }
            return allEmployees;
        }

        public Employee GetEmployeeById(int id)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = "SELECT id, name, role, birthday, salary, hiring_date FROM Employees WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Employee foundEmployee = new Employee();
                            foundEmployee.Id = reader.GetInt32(0);
                            foundEmployee.Name = reader.GetString(1);
                            foundEmployee.Role = reader.GetString(2);
                            foundEmployee.Birthday = DateOnly.FromDateTime(reader.GetDateTime(3));
                            foundEmployee.Salary = reader.GetInt32(4);
                            foundEmployee.HiringDate = DateOnly.FromDateTime(reader.GetDateTime(5));

                            return foundEmployee;
                        }
                    }
                }
            }
            return null;
        }

        public int AddEmployee(Employee newEmployee)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = @"INSERT INTO Employees (name, role, birthday, salary, hiring_date) 
                                 OUTPUT INSERTED.id 
                                 VALUES (@name, @role, @birthday, @salary, @hiring_date)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", newEmployee.Name);
                    cmd.Parameters.AddWithValue("@role", newEmployee.Role);
                    cmd.Parameters.AddWithValue("@salary", newEmployee.Salary);
                    cmd.Parameters.AddWithValue("@birthday", newEmployee.Birthday.ToDateTime(TimeOnly.MinValue));
                    cmd.Parameters.AddWithValue("@hiring_date", newEmployee.HiringDate.ToDateTime(TimeOnly.MinValue));

                    int newId = (int)cmd.ExecuteScalar();
                    return newId;
                }
            }
        }

        public void UpdateEmployee(Employee updatedEmployee)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                string query = @"UPDATE Employees SET 
                                 name = @name, 
                                 role = @role, 
                                 birthday = @birthday, 
                                 salary = @salary, 
                                 hiring_date = @hiring_date 
                                 WHERE id = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", updatedEmployee.Id);
                    cmd.Parameters.AddWithValue("@name", updatedEmployee.Name);
                    cmd.Parameters.AddWithValue("@role", updatedEmployee.Role);
                    cmd.Parameters.AddWithValue("@salary", updatedEmployee.Salary);
                    cmd.Parameters.AddWithValue("@birthday", updatedEmployee.Birthday.ToDateTime(TimeOnly.MinValue));
                    cmd.Parameters.AddWithValue("@hiring_date", updatedEmployee.HiringDate.ToDateTime(TimeOnly.MinValue));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteEmployee(int id)
        {
            using (SqlConnection conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        //string deleteJunctionQuery = "DELETE FROM Flight_employees WHERE id_employee = @id";
                        //using (SqlCommand junctionCmd = new SqlCommand(deleteJunctionQuery, conn, transaction))
                        //{
                        //    junctionCmd.Parameters.AddWithValue("@id", id);
                        //    junctionCmd.ExecuteNonQuery();
                        //}

                        string deleteEmpQuery = "DELETE FROM Employees WHERE id = @id";
                        using (SqlCommand empCmd = new SqlCommand(deleteEmpQuery, conn, transaction))
                        {
                            empCmd.Parameters.AddWithValue("@id", id);
                            empCmd.ExecuteNonQuery();
                        }
                        
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}