# 🎮 AKAY Launcher

**AKAY Launcher** es una aplicación de escritorio moderna desarrollada en **C# y WPF** que simula una plataforma de gestión de videojuegos (estilo Steam o Epic Games).

El proyecto destaca por su **Interfaz de Usuario (UI) personalizada "Dark Gamer"**, eliminando los bordes estándar de Windows para ofrecer una experiencia inmersiva con transparencias, efectos de desenfoque y controles personalizados. Además, cuenta con un robusto sistema de administración, gestión de usuarios y sistema de sanciones automatizado.

---

## ✨ Características Principales

### 🎨 Diseño y Experiencia de Usuario (UX)
* **Ventana Personalizada:** Sin bordes de sistema, con controles de ventana (Minimizar, Maximizar/Restaurar, Cerrar) hechos a medida.
* **Diseño Elástico:** La aplicación se adapta a cualquier resolución y arranca en pantalla completa.
* **Dimmer Overlay:** Sistema de "velo negro" que oscurece el fondo al abrir ventanas modales, evitando el uso de opacidad que transparenta el escritorio.
* **Estética Gamer:** Colores neón (Cian/Rojo), fondos oscuros y efectos visuales en Hover.

### 🔐 Autenticación y Seguridad
* **Login y Registro:** Conexión segura a base de datos MySQL.
* **Validación de Acceso:** Detección automática de credenciales incorrectas o cuentas baneadas.
* **Teclas Rápidas:** Soporte para tecla `ENTER` en formularios.

### 🛡️ Sistema de Administración y Bans (Core)
* **Grados de Sanción:** Sistema escalonado de 5 niveles:
    1.  1 Día
    2.  3 Días
    3.  1 Semana
    4.  1 Mes
    5.  Permanente
* **Auto-Desbaneo (Lazy Update):** El sistema comprueba automáticamente al iniciar sesión si el tiempo de castigo ha expirado. Si es así, libera al usuario y le permite entrar sin intervención manual.
* **Panel de Admin:**
    * Buscador de usuarios en tiempo real.
    * Edición de Roles (User/Admin) y Contraseñas.
    * Visualización del tiempo restante de baneo (con cuenta atrás legible).
    * Botones de acción rápida: Aplicar sanción, Levantar castigo.

### 📩 Sistema de Apelaciones
* **Buzón de Baneados:** Los usuarios bloqueados pueden enviar mensajes de apelación desde la pantalla de login.
* **Gestión de Mensajes:** El administrador puede leer las apelaciones en una tabla dedicada y eliminarlas tras su revisión.

---

## 📸 Capturas de Pantalla

*(Sube tus imágenes a una carpeta llamada 'Screenshots' y descomenta estas líneas)*

| Login Screen | Panel de Admin |
|:---:|:---:|
| | |

| Home Gamer | Buzón de Apelaciones |
|:---:|:---:|
| | |

---

## 🛠️ Tecnologías Utilizadas

* **Lenguaje:** C# (.NET Framework / .NET Core)
* **Framework UI:** WPF (Windows Presentation Foundation)
* **Base de Datos:** MySQL
* **Librería SQL:** MySql.Data
* **IDE:** Visual Studio 2022

---

## ⚙️ Instalación y Configuración

### 1. Requisitos Previos
* Visual Studio con la carga de trabajo de desarrollo de escritorio .NET.
* Servidor MySQL (XAMPP, MySQL Workbench, etc.).

### 2. Configurar la Base de Datos
Ejecuta el siguiente script SQL en tu gestor de base de datos para crear la estructura y los usuarios de prueba.

```sql
CREATE DATABASE IF NOT EXISTS akay_data;
USE akay_data;

-- Tabla de Usuarios
CREATE TABLE IF NOT EXISTS usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50),
    apellidos VARCHAR(50),
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(100),
    telefono VARCHAR(20),
    cp VARCHAR(10),
    rol VARCHAR(20) DEFAULT 'USER',
    activo TINYINT(1) DEFAULT 1,
    grado_baneo INT DEFAULT 0,
    fin_baneo DATETIME DEFAULT NULL
);

-- Tabla de Apelaciones
CREATE TABLE IF NOT EXISTS apelaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario VARCHAR(50),
    mensaje TEXT,
    fecha DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- --- DATOS DE PRUEBA (Dummies) ---

-- 1. Admin Supremo
INSERT INTO usuarios (nombre, username, password, email, rol, activo)
VALUES ('Admin', 'admin', 'admin123', 'admin@akay.com', 'ADMIN', 1);

-- 2. Usuario Normal
INSERT INTO usuarios (nombre, username, password, email, rol, activo)
VALUES ('Pepe', 'user', '1234', 'user@akay.com', 'USER', 1);

-- 3. Usuario Baneado (Grado 5 - Permanente)
INSERT INTO usuarios (nombre, username, password, email, rol, activo, grado_baneo, fin_baneo)
VALUES ('Hacker', 'baneado', '1234', 'ban@akay.com', 'USER', 0, 5, '9999-12-31 23:59:59');

