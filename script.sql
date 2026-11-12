CREATE DATABASE biblioteca_unifranz;
USE biblioteca_unifranz;

CREATE TABLE categoria (
    id_categoria INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100),
    descripcion VARCHAR(255)
);

CREATE TABLE libro (
    id_libro INT AUTO_INCREMENT PRIMARY KEY,
    titulo VARCHAR(255),
    autor VARCHAR(255),
    isbn VARCHAR(20),
    editorial VARCHAR(255),
    anio_publicacion INT,
    id_categoria INT,
    FOREIGN KEY (id_categoria) REFERENCES categoria(id_categoria)
);

CREATE TABLE ejemplar (
    id_ejemplar INT AUTO_INCREMENT PRIMARY KEY,
    codigo_barras VARCHAR(50),
    estado VARCHAR(50),
    ubicacion VARCHAR(100),
    id_libro INT,
    FOREIGN KEY (id_libro) REFERENCES libro(id_libro)
);

CREATE TABLE usuario (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100),
    apellido VARCHAR(100),
    correo VARCHAR(150),
    tipo_usuario VARCHAR(50),
    contrasena VARCHAR(255),
    estado VARCHAR(50)
);

CREATE TABLE prestamo (
    id_prestamo INT AUTO_INCREMENT PRIMARY KEY,
    fecha_prestamo DATE,
    fecha_limite DATE,
    fecha_devolucion DATE,
    estado VARCHAR(50),
    dias_mora INT,
    id_usuario INT,
    id_ejemplar INT,
    FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario),
    FOREIGN KEY (id_ejemplar) REFERENCES ejemplar(id_ejemplar)
);