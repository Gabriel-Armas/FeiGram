FROM python:3.12-slim

WORKDIR /src

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY src/ .

EXPOSE 8086

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8086"]
