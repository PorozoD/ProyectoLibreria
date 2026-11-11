namespace proyectolibreria.Models
{
    public class Libro
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int Precio { get; set; }
        public int Stock { get; set; }
    }
}
