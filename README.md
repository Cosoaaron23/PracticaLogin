# 🛡️ AKAY SYSTEM - Admin & Launcher

![.NET Core](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-Windows-blue?style=for-the-badge)
![MySQL](https://img.shields.io/badge/MySQL-00000F?style=for-the-badge&logo=mysql&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)

**AKAY SYSTEM** es una aplicación de escritorio moderna desarrollada en **C# y WPF** que simula una plataforma de gestión de videojuegos y administración de usuarios.

El proyecto destaca por su **Interfaz "Dark Gamer"**, eliminando los bordes estándar de Windows para ofrecer una experiencia inmersiva con ventanas personalizadas, efectos de neón y un sistema robusto de gestión de datos.

---

## ✨ Características Principales

### 🎨 Diseño y Experiencia de Usuario (UX)
* **Ventana Personalizada:** Sin bordes de sistema (WindowStyle None), con controles de ventana hechos a medida.
* **Alertas Propias:** Se ha eliminado el `MessageBox` nativo de Windows. Ahora el sistema usa **ventanas de diálogo personalizadas** (`CustomMessageBox`) con estética oscura y colores semánticos (Rojo para eliminar, Verde para éxito, Cian para info).
* **Dimmer Overlay:** Sistema de "velo negro" que oscurece el fondo al abrir ventanas modales.
* **Estética Gamer:** Paleta de colores Neón (Cian/Rojo) sobre fondos oscuros `#181818`.

### 🛡️ Sistema de Administración y Bans (Core)
* **Grados de Sanción:** Sistema escalonado de 5 niveles con cálculo automático de fechas:
    1. 1 Día
    2. 3 Días
    3. 1 Semana
    4. 1 Mes
    5. Permanente
* **Auto-Desbaneo (Lazy Update):** Al iniciar sesión, el sistema verifica matemáticamente si la sanción ha expirado y libera al usuario automáticamente.
* **Panel de Admin Avanzado:**
    * Buscador en tiempo real.
    * Visualización de fecha exacta de fin de baneo.
    * Edición rápida de roles y contraseñas.

### 📊 Reportes y Logs
* **Auditoría Completa:** Cada acción importante (banear, crear usuario, borrar) queda registrada en la base de datos `log_actividad`.
* **Visor de Reportes:** Ventana integrada estilo "Terminal" que genera un informe detallado de la actividad de los administradores.
* **Sistema de Apelaciones:** Buzón donde los usuarios baneados pueden solicitar el desbloqueo.

---

## 📷 Capturas de Pantalla

| Login Screen | Panel de Admin |
|:---:|:---:|
| ![Login](screenshots/login.png) | ![Admin](screenshots/admin.png) |

| Alertas Custom | Visor de Reportes |
|:---:|:---:|
| ![Alertas](screenshots/alert.png) | ![Reportes](screenshots/report.png) |

*(Asegúrate de subir tus imágenes a una carpeta 'screenshots' en el repo)*

---

## 🛠️ Tecnologías

* **Lenguaje:** C# (.NET Framework / Core)
* **Interfaz:** WPF (Windows Presentation Foundation)
* **Base de Datos:** MySQL
* **Librería:** `MySql.Data`
* **IDE:** Visual Studio 2022

---

## ⚙️ Instalación y Configuración

### 1. Clonar el Repositorio
```bash
git clone [https://github.com/TU_USUARIO/PracticaLogin.git](https://github.com/TU_USUARIO/PracticaLogin.git)

2. Configurar la Base de Datos
Este proyecto requiere una base de datos MySQL. Haz clic abajo para ver el script:

<details>
<summary>🔻 CLICK AQUÍ PARA VER EL SCRIPT SQL</summary>

DROP DATABASE IF EXISTS akay_data;
CREATE DATABASE akay_data;
USE akay_data;

-- 1. TABLA USUARIOS
CREATE TABLE usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100),
    apellidos VARCHAR(100),
    username VARCHAR(50) UNIQUE,
    password VARCHAR(255),
    email VARCHAR(100),
    telefono VARCHAR(20),
    cp VARCHAR(10),
    rol VARCHAR(20) DEFAULT 'USER',
    activo TINYINT(1) DEFAULT 1,
    grado_baneo INT DEFAULT 0,
    fin_baneo DATETIME NULL,
    suscripcion VARCHAR(20) DEFAULT 'FREE'
);

-- 2. TABLA LOGS (Para los reportes)
CREATE TABLE log_actividad (
    id_log INT AUTO_INCREMENT PRIMARY KEY,
    id_admin INT NOT NULL,
    accion VARCHAR(50),
    descripcion TEXT,
    fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_admin) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- 3. TABLA APELACIONES
CREATE TABLE apelaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    username VARCHAR(50),
    mensaje TEXT,
    fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- --- DATOS DE PRUEBA (SEEDERS) ---

-- Admin Principal
INSERT INTO usuarios (username, password, email, rol, activo, suscripcion)
VALUES ('admin', 'admin123', 'admin@akay.com', 'ADMIN', 1, 'VIP');

-- Usuario Normal
INSERT INTO usuarios (username, password, email, rol, activo)
VALUES ('user', '1234', 'user@email.com', 'USER', 1);

-- Usuario Baneado (Ejemplo)
INSERT INTO usuarios (username, password, email, rol, activo, grado_baneo, fin_baneo)
VALUES ('baneado', '1234', 'ban@email.com', 'USER', 0, 1, DATE_ADD(NOW(), INTERVAL 1 DAY));

</details>

3. Conexión
Abre el archivo DatabaseHelper.cs y asegúrate de que la cadena de conexión coincida con tu MySQL local:

private static string connectionString = "Server=localhost;Database=akay_data;Uid=root;Pwd=TU_CONTRASEÑA;";

Rol,Usuario,Contraseña
Admin,admin,admin123
User,user,1234
Baneado,baneado,1234



📄 Licencia
Este proyecto es una práctica académica y se distribuye bajo la licencia MIT.
