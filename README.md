<div align="center">

  # 🎮 AKAY LAUNCHER
  
  **Plataforma de Gestión y Distribución Digital de Videojuegos**

  ![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
  ![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
  ![WPF](https://img.shields.io/badge/WPF-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
  ![MySQL](https://img.shields.io/badge/MySQL-000000?style=for-the-badge&logo=mysql&logoColor=white)
  ![Status](https://img.shields.io/badge/Status-Finalizado-success?style=for-the-badge)

  <p align="center">
    Una experiencia de usuario inmersiva con estética <strong>Dark Cyberpunk</strong>.
    <br />
    Simula el ecosistema completo de una tienda digital: Compra, Biblioteca, Descarga y Administración.
  </p>

</div>

---

## 🚀 Sobre el Proyecto

**AKAY Launcher** es una aplicación de escritorio desarrollada como proyecto final del módulo de **Desarrollo de Interfaces**. El objetivo principal ha sido alejarse de los controles estándar de Windows para crear una interfaz personalizada, moderna y visualmente atractiva, similar a plataformas como Steam o Epic Games.

El proyecto implementa una arquitectura robusta separando la interfaz (XAML) de la lógica de negocio y la persistencia de datos.

---

## 📸 Galería Visual

| Pantalla de Login | Panel Principal (Home) |
|:---:|:---:|
| <img src="assets/login.png" alt="Login Screen" width="400"/> | <img src="assets/home.png" alt="Home Screen" width="400"/> |

| Gestión de Usuarios (Admin) | Detalles y Descarga |
|:---:|:---:|
| <img src="assets/admin.png" alt="Admin Panel" width="400"/> | <img src="assets/detalle.png" alt="Game Detail" width="400"/> |

---

## ✨ Características Principales

### 👤 Para el Usuario (Gamer)
* **Identidad Visual:** Modo oscuro profundo (`#121212`) con acentos Neón Cian y Rojo para reducir la fatiga visual.
* **Tienda Dinámica:** Exploración de catálogo con imágenes de portada y banners.
* **Sistema de Carrito:** Simulación de proceso de compra completo.
* **Biblioteca Inteligente:** Gestión de juegos adquiridos con simulación asíncrona de descarga (Barra de progreso real).
* **Feedback Visual:** Ventanas modales personalizadas (`CustomMessageBox`) para no romper la inmersión.

### 🛡️ Para el Administrador
* **Gestión de Usuarios:** Panel avanzado con `DataGrids` estilizados.
* **Buscador en Tiempo Real:** Filtrado instantáneo de usuarios mediante **LINQ**.
* **Sistema de Sanciones:**
    * Baneos temporales o permanentes.
    * Cálculo automático de la fecha de fin de sanción.
    * Visualización del tiempo restante en la tabla de usuarios.

---

## 🛠️ Stack Tecnológico

* **Lenguaje:** C# (.NET 6.0)
* **Interfaz:** Windows Presentation Foundation (WPF) + XAML Avanzado (ControlTemplates, Triggers, Styles).
* **Base de Datos:** MySQL 8.0 (Relacional).
* **Librerías:** `MySql.Data` (ADO.NET).
* **Patrones:** Navegación Modal, Event-Driven Programming.

---

## 💾 Base de Datos

El proyecto requiere una base de datos MySQL para funcionar.
El script de generación completo se encuentra en la carpeta raíz:

`📜 akay_data.sql`

Este script genera:
1.  La base de datos `akay_data`.
2.  Las tablas: `usuarios`, `videojuegos`, `biblioteca`, etc.
3.  Usuarios de prueba (Admin y User) y el catálogo de juegos inicial.

---

## ✒️ Autor

**Aaron Del Coso Ridocci**
* *Desarrollador Full Stack (En formación)*
* IES Rey Fernando VI - Desarrollo de Aplicaciones Multiplataforma

---
<div align="center">
  <sub>Hecho con ❤️ y mucho café / 2026</sub>
</div>
