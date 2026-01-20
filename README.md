esta es el schema sql para que funcione la bbdd:


-- =======================================================
-- SCRIPT COMPLETO: PRÁCTICA 4 + EXTENSIÓN (LOGS)
-- =======================================================

-- 1. Crear la Base de Datos
CREATE DATABASE IF NOT EXISTS akay_data;
USE akay_data;

-- 2. LIMPIEZA: Borrar tablas antiguas para evitar errores
-- (Importante: Borrar primero log_actividad porque depende de usuarios)
DROP TABLE IF EXISTS log_actividad;
DROP TABLE IF EXISTS usuarios;

-- 3. TABLA USUARIOS (Modificada para Práctica 4)
-- Incluye Roles, Email y Estado (Activo/Baneado) [cite: 26, 29, 30, 31]
CREATE TABLE usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    
    -- Campos básicos de identidad
    nombre VARCHAR(100),
    apellidos VARCHAR(100),
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL, 
    email VARCHAR(100),              -- Requisito Práctica 4 [cite: 30]
    telefono VARCHAR(20),
    cp VARCHAR(10),
    
    -- Lógica de Negocio y Seguridad
    rol VARCHAR(20) DEFAULT 'USER',  -- 'ADMIN' o 'USER' [cite: 29]
    activo TINYINT(1) DEFAULT 1,     -- 1 = Activo, 0 = Baneado [cite: 31]
    suscripcion VARCHAR(20) DEFAULT 'FREE',
    
    -- Campos extra de seguridad (intentos fallidos)
    intentos_fallidos INT DEFAULT 0,
    bloqueado_hasta DATETIME NULL
);

-- 4. TABLA LOG DE ACTIVIDAD (La Extensión del PDF)
-- Registra qué admin hizo qué cambio y cuándo 
CREATE TABLE log_actividad (
    id_log INT AUTO_INCREMENT PRIMARY KEY,
    
    id_admin INT NOT NULL,           -- QUIÉN hizo la acción
    accion VARCHAR(50),              -- TIPO (LOGIN, UPDATE, BAN, DELETE)
    descripcion TEXT,                -- QUÉ hizo (Detalle legible)
    fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP, -- CUÁNDO
    
    -- Relación: Un log pertenece a un usuario (que debe ser admin)
    CONSTRAINT fk_log_admin FOREIGN KEY (id_admin) 
    REFERENCES usuarios(id) 
    ON DELETE CASCADE -- Si borras al admin, se borran sus logs (opcional)
);

-- =======================================================
-- DATOS DE PRUEBA (SEEDERS)
-- =======================================================

-- 1. CREAR ADMINISTRADOR
INSERT INTO usuarios (nombre, apellidos, username, password, email, rol, activo, suscripcion)
VALUES ('Jefe', 'Supremo', 'admin', 'admin123', 'admin@akay.com', 'ADMIN', 1, 'VIP');

-- 2. CREAR USUARIO NORMAL
INSERT INTO usuarios (nombre, apellidos, username, password, email, rol, activo, suscripcion)
VALUES ('Pepito', 'Jugador', 'user', '1234', 'pepe@correo.com', 'USER', 1, 'FREE');

-- 3. CREAR USUARIO PARA PRUEBAS DE BANEO
INSERT INTO usuarios (nombre, apellidos, username, password, email, rol, activo, suscripcion)
VALUES ('Usuario', 'Malo', 'baneado', '1234', 'banned@correo.com', 'USER', 0, 'FREE');

-- 4. CREAR UN LOG DE EJEMPLO
-- Para que cuando abras la ventana de reportes no salga vacía al principio
INSERT INTO log_actividad (id_admin, accion, descripcion, fecha_hora)
VALUES (1, 'INIT', 'Inicialización del sistema y creación de usuarios base', NOW());

-- =======================================================
-- COMPROBACIÓN FINAL
-- =======================================================
SELECT * FROM usuarios;
SELECT * FROM log_actividad;
