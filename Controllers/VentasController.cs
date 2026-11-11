using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;
using proyectolibreria.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace proyectolibreria.Controllers
{
    public class VentasController : Controller
    {
        private readonly string _connectionString;

        public VentasController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDBConnection")!;
        }

        public async Task<IActionResult> Index()
        {
            var ventas = new List<Venta>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT v.id_venta, v.cantidad, v.fecha, p.nombre AS Libro, c.nombre AS Cliente 
                              FROM venta v 
                              JOIN producto p ON v.id_producto = p.id_producto 
                              JOIN cliente c ON v.id_cliente = c.id_cliente";

                using (var command = new MySqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ventas.Add(new Venta
                        {
                            Id = reader.GetInt32("id_venta"),
                            Cantidad = reader.GetInt32("cantidad"),
                            Fecha = reader.GetDateTime("fecha"),
                            NombreProducto = reader.GetString("Libro"),
                            NombreCliente = reader.GetString("Cliente")
                        });
                    }
                }
            }
            return View(ventas);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarListas();
            return View(new VentaForm { Cantidad = 1 });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(VentaForm venta)
        {
            if (venta.IdProducto == 0 || venta.IdCliente == 0 || venta.Cantidad <= 0)
            {
                ModelState.AddModelError("", "Elige un libro, un lector y una cantidad mayor a 0.");
                await CargarListas();
                return View(venta);
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(
                    "INSERT INTO venta (id_producto, id_cliente, cantidad, fecha) VALUES (@p, @c, @cant, NOW())", connection))
                {
                    command.Parameters.AddWithValue("@p", venta.IdProducto);
                    command.Parameters.AddWithValue("@c", venta.IdCliente);
                    command.Parameters.AddWithValue("@cant", venta.Cantidad);
                    await command.ExecuteNonQueryAsync();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            VentaForm? venta = null;
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(
                    "SELECT id_venta, id_producto, id_cliente, cantidad FROM venta WHERE id_venta = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            venta = new VentaForm
                            {
                                Id = reader.GetInt32("id_venta"),
                                IdProducto = reader.GetInt32("id_producto"),
                                IdCliente = reader.GetInt32("id_cliente"),
                                Cantidad = reader.GetInt32("cantidad")
                            };
                        }
                    }
                }
            }
            if (venta == null) return NotFound();
            await CargarListas();
            return View(venta);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(VentaForm venta)
        {
            if (venta.IdProducto == 0 || venta.IdCliente == 0 || venta.Cantidad <= 0)
            {
                ModelState.AddModelError("", "Elige un libro, un lector y una cantidad mayor a 0.");
                await CargarListas();
                return View(venta);
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(
                    "UPDATE venta SET id_producto = @p, id_cliente = @c, cantidad = @cant WHERE id_venta = @id", connection))
                {
                    command.Parameters.AddWithValue("@p", venta.IdProducto);
                    command.Parameters.AddWithValue("@c", venta.IdCliente);
                    command.Parameters.AddWithValue("@cant", venta.Cantidad);
                    command.Parameters.AddWithValue("@id", venta.Id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("DELETE FROM venta WHERE id_venta = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
            return RedirectToAction("Index");
        }

        private async Task CargarListas()
        {
            var libros = new List<SelectListItem>();
            var lectores = new List<SelectListItem>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("SELECT id_producto, nombre FROM producto ORDER BY nombre", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        libros.Add(new SelectListItem
                        {
                            Value = reader.GetInt32("id_producto").ToString(),
                            Text = reader.GetString("nombre")
                        });
                    }
                }

                using (var command = new MySqlCommand("SELECT id_cliente, nombre FROM cliente ORDER BY nombre", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lectores.Add(new SelectListItem
                        {
                            Value = reader.GetInt32("id_cliente").ToString(),
                            Text = reader.GetString("nombre")
                        });
                    }
                }
            }

                        ViewBag.Libros = libros;
            ViewBag.Lectores = lectores;
        }
    }
}