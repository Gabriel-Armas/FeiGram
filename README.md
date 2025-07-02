# Feigram

> **Una red social inspirada en Instagram, creada especialmente para la comunidad de la Facultad de Estadística e Informática (FEI).**

Feigram es una aplicación pensada para la comunidad FEI. Desde compartir fotos hasta interactuar en tiempo real, Feigram sirvió para practicar el uso de microservicios, técnicas de despliegue y tecnologías modernas.

---

## Características principales

- **Inspiración en Instagram**: Interfaz amigable y centrada en la experiencia del usuario.
- **Identidad FEI**: Adaptada al contexto y cultura de la Facultad de Estadística e Informática.
- **Arquitectura de Microservicios**: Separación clara de responsabilidades para mejor mantenimiento y escalabilidad.
- **Contenedores Docker**: Orquestación de servicios mediante `docker-compose` para un despliegue local sencillo.
- **Clientes multiplataforma**:
  - `clients/desktop`: Cliente de escritorio.
  - `clients/web`: Cliente web (SPA).
  - `clients/mobile`: App móvil con Jetpack Compose para Android.

---

## Estructura del Proyecto

```plaintext
Feigram/
├── clients/
│   ├── desktop/       # Cliente de escritorio
│   ├── web/           # Cliente web (SPA)
│   └── mobile/        # App móvil Android
├── services/          # Microservicios independientes
│   ├── authentication-api/
│   ├── profile-api/
│   ├── follow-api/
│   └── ...            # Otros servicios como posts, messages, chart, etc.
├── docker-compose.yml # Orquestación de todos los servicios
└── README.md