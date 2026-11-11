using System;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using proyectolibreria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace proyectolibreria.Controllers
{
    public class EditorialesController : Controller
    {
        private readonly string _connectionString;

        public EditorialesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDBConnection")!;
        }

        public async Task<IActionResult> Index()
        {
            var editoriales = new List<Editorial>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT id_editorial, nombre, telefono, pais FROM editorial", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        editoriales.Add(new Editorial
                        {
                            Id = reader.GetInt32("id_editorial"),
                            Nombre = reader.GetString("nombre"),
                            Telefono = reader.GetString("telefono"),
                            Pais = reader.GetString("pais")
                        });
                    }
                }
            }
            return View(editoriales);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Editorial());
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Editorial editorial)
        {
            if (string.IsNullOrWhiteSpace(editorial.Nombre))
            {
                ModelState.AddModelError("Nombre", "La razón social es obligatoria");
                return View(editorial);
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(
                    "INSERT INTO editorial (nombre, telefono, pais) VALUES (@nombre, @telefono, @pais)", connection))
                {
                    command.Parameters.AddWithValue("@nombre", editorial.Nombre);
                    command.Parameters.AddWithValue("@telefono", editorial.Telefono ?? "");
                    command.Parameters.AddWithValue("@pais", editorial.Pais ?? "");
                    await command.ExecuteNonQueryAsync();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Editorial? editorial = null;
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(
                    "SELECT id_editorial, nombre, telefono, pais FROM editorial WHERE id_editorial = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            editorial = new Editorial
                            {
                                Id = reader.GetInt32("id_editorial"),
                                Nombre = reader.GetString("nombre"),
                                Telefono = reader.GetString("telefono"),
                                Pais = reader.GetString("pais")
                            };
                        }
                    }
                }
            }
            if (editorial == null) return NotFound();
            return View(editorial);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Editorial editorial)
        {
            if (string.IsNullOrWhiteSpace(editorial.Nombre))
            {
                ModelState.AddModelError("Nombre", "La razón social es obligatoria");
                return View(editorial);
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(
                    "UPDATE editorial SET nombre = @nombre, telefono = @telefono, pais = @pais WHERE id_editorial = @id", connection))
                {
                    command.Parameters.AddWithValue("@nombre", editorial.Nombre);
                    command.Parameters.AddWithValue("@telefono", editorial.Telefono ?? "");
                    command.Parameters.AddWithValue("@pais", editorial.Pais ?? "");
                    command.Parameters.AddWithValue("@id", editorial.Id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new MySqlCommand(
                        "DELETE FROM editorial WHERE id_editorial = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (MySqlException)
            {
                TempData["Error"] = "No se puede borrar: esta editorial tiene libros asociados.";
            }
            return RedirectToAction("Index");
        }
    }
}