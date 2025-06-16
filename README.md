# 🦖 Feigram

**Proyecto desarrollado por el equipo Dinosoft**

Feigram es una red social moderna enfocada en la comunidad FEI. Su arquitectura distribuida y modular está diseñada para escalar fácilmente, facilitar la evolución de funcionalidades, y permitir la integración de múltiples clientes. Este repositorio es el resultado del esfuerzo colaborativo de Dinosoft.

---

## 📁 Estructura del Proyecto

```bash
feigram/
├── clients/             # Clientes multiplataforma (Desktop, Web, Móvil)
│   ├── feigram-desktop
│   ├── feigram-movil
│   └── feigram-web
├── infra/               # Infraestructura y recursos compartidos
├── k8s/                 # Manifiestos de Kubernetes (base y overlays)
│   ├── base/
│   ├── overlays/
│   │   ├── dev/
│   │   └── prod/
│   └── services/
├── nginx/               # Configuración de NGINX Proxy Manager y certificados
│   └── certs/
├── scripts/             # Scripts útiles para despliegue o administración
├── services/            # Microservicios que conforman el sistema principal
│   ├── authentication-api
│   ├── ban-api
│   ├── chart-api
│   ├── comments-api
│   ├── feed-api
│   ├── follow-api
│   ├── likes-api
│   ├── message-api
│   ├── post-api
│   └── profile-api
└── tests/               # Conjunto de pruebas para los servicios
```

---

## 🧠 Microservicios

| Servicio             | Descripción                                                 |
| -------------------- | ----------------------------------------------------------- |
| `authentication-api` | Autenticación, generación y validación de tokens JWT        |
| `profile-api`        | Gestión de perfiles de usuario (C# + PostgreSQL)            |
| `post-api`           | Publicación de contenido multimedia (FastAPI + MongoDB)     |
| `comments-api`       | Comentarios en publicaciones                                |
| `likes-api`          | Reacciones y "me gusta" a publicaciones                     |
| `follow-api`         | Relaciones entre seguidores (FastAPI + Neo4j)               |
| `feed-api`           | Generación del feed de publicaciones personalizado          |
| `chart-api`          | Análisis y estadísticas de publicaciones (gRPC)             |
| `message-api`        | Chat en tiempo real mediante WebSockets (FastAPI + MongoDB) |
| `ban-api`            | Control y registro de usuarios bloqueados                   |

Todos los microservicios se comunican mediante **RabbitMQ**, lo que facilita un desacoplamiento robusto entre servicios.

---

## 🌐 Clientes

Los clientes del sistema se dividen en:

* **Feigram Desktop**: Aplicación de escritorio moderna.
* **Feigram Web**: Aplicación web accesible desde cualquier navegador.
* **Feigram Móvil**: App móvil desarrollada con Jetpack Compose.

Todos se conectan a los microservicios mediante una capa de red gestionada por **NGINX Proxy Manager**.

---

## 🔧 Kubernetes (k8s)

Se utiliza Kustomize para manejar múltiples entornos:

* `base/`: configuración base de servicios
* `overlays/dev/`: entorno de desarrollo
* `overlays/prod/`: entorno de producción

---

## 🛡 Seguridad

* Autenticación mediante JWT
* Certificados SSL/TLS configurados con Let's Encrypt (o autofirmados para desarrollo)
* Seguridad en WebSockets usando `wss://.../ws/?token=...`

---

## 🚀 Ejecutar el sistema

```bash
docker compose -f docker-compose.dev.yml up -d
```

> También puedes usar los manifiestos de `k8s/` para despliegues en clúster.

---

## 🧪 Pruebas

El directorio `tests/` incluye pruebas en formato json, de integración listas para ser usadas en newman.

---

## 👨‍💻 Equipo Dinosoft

* Gabriel Armas Viveros
* Yael Alfredo Salazar Aguilar
* Juan Pablo Torres Ortiz

Gracias por visitar nuestro repositorio. ¡Feigram está construido para gamers, por desarrolladores que también son gamers! 🎮
