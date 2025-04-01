using api.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace StudentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly string? _connectionString;

        public StudentController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is missing in the configuration.");
            }
        }

        // 1. Endpoint untuk mendapatkan semua siswa
        [HttpGet]
        public IActionResult GetStudents()
        {
            List<Student> students = new List<Student>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM students";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new Student
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Age = reader.GetInt32("age"),
                                Class = reader.GetString("class")
                            });
                        }
                    }
                }
                // catch (MySqlException ex)
                // {
                //     return StatusCode(500, $"Database error: {ex.Message}");
                // }
                    catch (MySqlException ex)
                {
                     Console.WriteLine($"[DB ERROR] {ex.Message}"); // Tambahan log
                    return StatusCode(500, $"Database error: {ex.Message}");
                }
            }

            return Ok(students); // Mengembalikan list siswa dalam format JSON
        }

        // 2. Endpoint untuk mendapatkan siswa berdasarkan ID
        [HttpGet("{id}")]
        public IActionResult GetStudentById(int id)
        {
            Student student = null;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM students WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            student = new Student
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Age = reader.GetInt32("age"),
                                Class = reader.GetString("class")
                            };
                        }
                    }
                }
                    catch (MySqlException ex)
                {
                    Console.WriteLine($"[DB ERROR] {ex.Message}"); // Tambahan log
                    return StatusCode(500, $"Database error: {ex.Message}");
                }
            }

            if (student == null)
                return NotFound("Student not found");

            return Ok(student); // Mengembalikan siswa yang ditemukan
        }

        // 3. Endpoint untuk menambahkan siswa baru
        [HttpPost]
        public IActionResult AddStudent([FromBody] Student student)
        {
            if (student == null)
                return BadRequest("Invalid student data");

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO students (name, age, class) VALUES (@name, @age, @class)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", student.Name);
                    cmd.Parameters.AddWithValue("@age", student.Age);
                    cmd.Parameters.AddWithValue("@class", student.Class);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        return Ok("Student added successfully");
                    else
                        return BadRequest("Failed to add student");
                }
                    catch (MySqlException ex)
                {
                    Console.WriteLine($"[DB ERROR] {ex.Message}"); // Tambahan log
                    return StatusCode(500, $"Database error: {ex.Message}");
                }
            }
        }
        // 4. Endpoint untuk mengupdate seluruh data siswa (PUT)
        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id, [FromBody] Student updatedStudent)
        {
            if (updatedStudent == null || id != updatedStudent.Id)
                return BadRequest("Invalid student data");

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE students SET name = @name, age = @age, class = @class WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", updatedStudent.Id);
                    cmd.Parameters.AddWithValue("@name", updatedStudent.Name);
                    cmd.Parameters.AddWithValue("@age", updatedStudent.Age);
                    cmd.Parameters.AddWithValue("@class", updatedStudent.Class);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        return Ok("Student updated successfully");
                    else
                        return NotFound("Student not found");
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"[DB ERROR] {ex.Message}");
                    return StatusCode(500, $"Database error: {ex.Message}");
                }
            }
        }

        // 5. Endpoint untuk update sebagian data siswa (PATCH)
        [HttpPatch("{id}")]
        public IActionResult PatchStudent(int id, [FromBody] Student partialStudent)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    // Ambil data lama
                    string selectQuery = "SELECT * FROM students WHERE id = @id";
                    MySqlCommand selectCmd = new MySqlCommand(selectQuery, conn);
                    selectCmd.Parameters.AddWithValue("@id", id);

                    Student existingStudent = null;
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            existingStudent = new Student
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Age = reader.GetInt32("age"),
                                Class = reader.GetString("class")
                            };
                        }
                        else
                        {
                            return NotFound("Student not found");
                        }
                    }

                    // Gabungkan data lama + baru
                    existingStudent.Name = string.IsNullOrEmpty(partialStudent.Name) ? existingStudent.Name : partialStudent.Name;
                    existingStudent.Age = partialStudent.Age == 0 ? existingStudent.Age : partialStudent.Age;
                    existingStudent.Class = string.IsNullOrEmpty(partialStudent.Class) ? existingStudent.Class : partialStudent.Class;

                    // Update ke database
                    string updateQuery = "UPDATE students SET name = @name, age = @age, class = @class WHERE id = @id";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@id", id);
                    updateCmd.Parameters.AddWithValue("@name", existingStudent.Name);
                    updateCmd.Parameters.AddWithValue("@age", existingStudent.Age);
                    updateCmd.Parameters.AddWithValue("@class", existingStudent.Class);

                    int rowsAffected = updateCmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        return Ok("Student patched successfully");
                    else
                        return BadRequest("Failed to update student");
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"[DB ERROR] {ex.Message}");
                    return StatusCode(500, $"Database error: {ex.Message}");
                }
            }
        }

        // 6. Endpoint untuk menghapus siswa berdasarkan ID (DELETE)
        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM students WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        return Ok("Student deleted successfully");
                    else
                        return NotFound("Student not found");
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine($"[DB ERROR] {ex.Message}");
                    return StatusCode(500, $"Database error: {ex.Message}");
                }
            }
        }

    }
}
