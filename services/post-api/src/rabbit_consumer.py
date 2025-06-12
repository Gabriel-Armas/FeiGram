import json
import threading
import time
import pika
from datetime import datetime, timedelta, timezone
from src.db import posts_collection

def handle_request(ch, method, props, body):
    week = body.decode()
    print(f"Recibí solicitud de la semana: {week}")

    now = datetime.now(timezone.utc)
    one_week_ago = now - timedelta(days=7)

    posts = list(posts_collection.find({
        "fechaPublicacion": {"$gte": one_week_ago}
    }))

    result = [
        {
            "descripcion": p["descripcion"],
            "fechaPublicacion": p["fechaPublicacion"].isoformat()
        }
        for p in posts
    ]

    response = json.dumps(result)

    ch.basic_publish(
        exchange='',
        routing_key=props.reply_to,
        properties=pika.BasicProperties(correlation_id=props.correlation_id),
        body=response
    )
    ch.basic_ack(delivery_tag=method.delivery_tag)

def consume():
    connection = None
    while not connection:
        try:
            print("Intentando conectar a RabbitMQ...")
            connection = pika.BlockingConnection(pika.ConnectionParameters(host='rabbitmq'))
        except pika.exceptions.AMQPConnectionError:
            print("RabbitMQ no disponible. Reintentando en 5 segundos...")
            time.sleep(5)

    channel = connection.channel()
    channel.queue_declare(queue='get-weekly-posts')

    channel.basic_qos(prefetch_count=1)
    channel.basic_consume(queue='get-weekly-posts', on_message_callback=handle_request)

    print("Escuchando solicitudes RabbitMQ...")
    channel.start_consuming()

def start_feed_consumer_thread():
    thread = threading.Thread(target=consume, daemon=True)
    thread.start()
