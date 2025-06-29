import json
import threading
import time
import pika
from datetime import datetime, timedelta, timezone
from src.db import posts_collection
from src.CommentCountRpcClient import CommentCountRpcClient
from src.LikesCountRpcClient import LikesCountRpcClient

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

def handle_feed_posts_request(ch, method, props, body):
    print("Recibí solicitud para los 30 posts del feed 💫")

    # Obtener los 30 posts más recientes
    posts = list(posts_collection.find().sort("fechaPublicacion", -1).limit(30))

    # Instanciar clientes RPC
    comment_rpc = CommentCountRpcClient()
    likes_rpc = LikesCountRpcClient()

    result = []
    for p in posts:
        post_id = str(p["post_id"])

        # Llamadas RPC para contar comentarios y likes
        comment_response = comment_rpc.get_comment_count(post_id)
        like_response = likes_rpc.get_likes_count(post_id)

        comment_count = comment_response.get("count", 0)
        like_count = like_response.get("like_count", 0)

        result.append({
            "post_id": p["post_id"],
            "id_usuario": p["id_usuario"],
            "descripcion": p["descripcion"],
            "url_media": p.get("url_media", ""),
            "fechaPublicacion": p["fechaPublicacion"].isoformat(),
            "comentarios": comment_count,
            "likes": like_count
        })

    response = json.dumps(result)

    # Responder vía RabbitMQ
    ch.basic_publish(
        exchange='',
        routing_key=props.reply_to,
        properties=pika.BasicProperties(correlation_id=props.correlation_id),
        body=response
    )
    ch.basic_ack(delivery_tag=method.delivery_tag)



def consume_feed_posts():
    connection = None
    while not connection:
        try:
            print("Intentando conectar a RabbitMQ para feed posts...")
            connection = pika.BlockingConnection(pika.ConnectionParameters(host='rabbitmq'))
        except pika.exceptions.AMQPConnectionError:
            print("RabbitMQ no disponible (feed posts). Reintentando en 5 segundos...")
            time.sleep(5)

    channel = connection.channel()
    channel.queue_declare(queue='get-feed-posts')

    channel.basic_qos(prefetch_count=1)
    channel.basic_consume(queue='get-feed-posts', on_message_callback=handle_feed_posts_request)

    print("Escuchando solicitudes para 30 posts del feed...")
    channel.start_consuming()


def start_feed_posts_consumer_thread():
    thread = threading.Thread(target=consume_feed_posts, daemon=True)
    thread.start()