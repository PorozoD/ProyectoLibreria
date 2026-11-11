using System;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using proyectolibreria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace proyectolibreria.Controllers
{
    public class ClientesController : Controller
    {
        private readonly string _connectionString;

        public ClientesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDBConnection")!;
        }

        public async Task<IActionResult> Index()
        {
            var clientes = new List<Cliente>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT id_cliente, nombre, telefono FROM cliente", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        clientes.Add(new Cliente
                        {
                            Id = reader.GetInt32("id_cliente"),
                            Nombre = reader.GetString("nombre"),
                            Telefono = reader.GetString("telefono")
                        });
                    }
                }
            }
            return View(clientes);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Cliente());
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio");
                return View(cliente);
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(
                    "INSERT INTO cliente (nombre, telefono) VALUES (@nombre, @telefono)", connection))
                {
                    command.Parameters.AddWithValue("@nombre", cliente.Nombre);
                    command.Parameters.AddWithValue("@telefono", cliente.Telefono ?? "");
                    await command.ExecuteNonQueryAsync();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Cliente? cliente = null;
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(
                    "SELECT id_cliente, nombre, telefono FROM cliente WHERE id_cliente = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            cliente = new Cliente
                            {
                                Id = reader.GetInt32("id_cliente"),
                                Nombre = reader.GetString("nombre"),
                                Telefono = reader.GetString("telefono")
                            };
                        }
                    }
                }
            }
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio");
                return View(cliente);
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(
                    "UPDATE cliente SET nombre = @nombre, telefono = @telefono WHERE id_cliente = @id", connection))
                {
                    command.Parameters.AddWithValue("@nombre", cliente.Nombre);
                    command.Parameters.AddWithValue("@telefono", cliente.Telefono ?? "");
                    command.Parameters.AddWithValue("@id", cliente.Id);
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
                        "DELETE FROM cliente WHERE id_cliente = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (MySqlException)
            {
                TempData["Error"] = "No se puede borrar: este lector tiene ventas registradas.";
            }
            return RedirectToAction("Index");
        }
    }
}