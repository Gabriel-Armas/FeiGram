# Usa una imagen base con Python
FROM python:3.11

# Establece el directorio de trabajo en el contenedor
WORKDIR /app

# Copia los archivos de tu proyecto al contenedor
COPY . .

# Instala las dependencias
RUN pip install --no-cache-dir -r requirements.txt

# Expone el puerto que usará la app
EXPOSE 8000

# Comando para ejecutar la app
CMD ["uvicorn", "src.main:app", "--host", "0.0.0.0", "--port", "8000"]
