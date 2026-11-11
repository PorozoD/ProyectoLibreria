using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using proyectolibreria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace proyectolibreria.Controllers
{
    public class LibrosController : Controller
    {
        private readonly string _connectionString;

        public LibrosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDBConnection")!;
        }

        public async Task<IActionResult> Index()
        {
            var libros = new List<Libro>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT id_producto, nombre, precio, stock FROM producto", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            libros.Add(new Libro
                            {
                                Id = reader.GetInt32("id_producto"),
                                Titulo = reader.GetString("nombre"),
                                Precio = reader.GetInt32("precio"),
                                Stock = reader.GetInt32("stock")
                            });
                        }
                    }
                }
            }
            return View(libros);
        }

        public IActionResult Crear() => View();

        [HttpPost]
        public async Task<IActionResult> Crear(Libro nuevo)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("INSERT INTO producto (nombre, precio, stock) VALUES (@nombre, @precio, @stock)", connection))
                {
                    command.Parameters.AddWithValue("@nombre", nuevo.Titulo);
                    command.Parameters.AddWithValue("@precio", nuevo.Precio);
                    command.Parameters.AddWithValue("@stock", nuevo.Stock);
                    await command.ExecuteNonQueryAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            Libro? libro = null;
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT id_producto, nombre, precio, stock FROM producto WHERE id_producto = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            libro = new Libro
                            {
                                Id = reader.GetInt32("id_producto"),
                                Titulo = reader.GetString("nombre"),
                                Precio = reader.GetInt32("precio"),
                                Stock = reader.GetInt32("stock")
                            };
                        }
                    }
                }
            }
            return View(libro);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Libro modificado)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("UPDATE producto SET nombre=@nombre, precio=@precio, stock=@stock WHERE id_producto=@id", connection))
                {
                    command.Parameters.AddWithValue("@nombre", modificado.Titulo);
                    command.Parameters.AddWithValue("@precio", modificado.Precio);
                    command.Parameters.AddWithValue("@stock", modificado.Stock);
                    command.Parameters.AddWithValue("@id", modificado.Id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Borrar(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("DELETE FROM producto WHERE id_producto = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}